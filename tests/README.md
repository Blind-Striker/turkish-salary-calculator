# Testing Architecture

This document describes the testing strategy for the Turkish HR Solutions Salary Calculator library.

## Overview

The test suite consists of two main categories:

| Category | Purpose | Count | Run by Default |
|----------|---------|-------|----------------|
| **Unit Tests** | Test individual components in isolation | ~167 | ✅ Yes |
| **Parity Tests** | Verify .NET matches Angular (source of truth) | ~76 | ❌ No (`[Explicit]`) |

## Test Framework

We use [TUnit](https://tunit.dev/) - a modern, AOT-compatible test framework for .NET.

### Key Features Used

- **Source Generation Mode**: AOT-compatible, compile-time test discovery
- **`[MethodDataSource]`**: Data-driven tests with scenarios defined in C#
- **`[Explicit]`**: Parity tests are opt-in (not run on every build)
- **`[Category]`**: Filter tests by category (`Parity`, `Angular`)
- **`[DisplayName]`**: Readable test names with parameter interpolation
- **`[ClassDataSource]`**: Shared infrastructure across tests

## Directory Structure

```
tests/
├── README.md                                          # This file
├── artifacts/                                         # Generated test artifacts (gitignored)
│   └── parity-fixtures/                              # Angular-generated JSON fixtures
├── tools/
│   └── angular-parity-fixtures/                      # TypeScript CLI for Angular calculations
│       ├── package.json
│       ├── src/
│       │   ├── calculate.ts                          # CLI entry point (TODO: refactor)
│       │   ├── generate-fixtures.ts                  # Current batch generator
│       │   └── angular-types.ts                      # Type definitions
│       └── tsconfig.json
└── Turkish.HRSolutions.SalaryCalculator.Tests/
    ├── Unit/                                         # Unit tests by layer
    │   ├── Api/                                      # API layer tests
    │   ├── Common/                                   # Cross-cutting concern tests
    │   ├── Configuration/                            # Configurator tests
    │   ├── DependencyInjection/                      # DI extension tests
    │   ├── Domain/                                   # Domain model tests
    │   └── Infrastructure/                           # Provider tests
    └── Parity/                                       # Angular parity tests
        ├── AngularParityTests.cs                     # Main parity test class
        ├── FieldComparer.cs                          # Tolerance-based comparison
        └── FixtureModels.cs                          # JSON deserialization models
```

## Running Tests

### All Unit Tests (Default)

```bash
# Run all non-explicit tests
dotnet test

# Or using dotnet run for more options
dotnet run --project tests/Turkish.HRSolutions.SalaryCalculator.Tests
```

### Parity Tests (Explicit)

Parity tests are marked `[Explicit]` and excluded from default runs. To run them:

```bash
# Run parity tests specifically
dotnet test -- --treenode-filter "/*/*/AngularParityTests/*"

# Or run by category
dotnet test -- --treenode-filter "/*/*/*/*[Category=Parity]"
```

### Prerequisites for Parity Tests

1. **Node.js 20+** - Required to run Angular calculator
2. **Git submodule initialized** - Angular project at `external/maas-hesaplama`

```bash
# Initialize submodule (first time)
git submodule update --init --recursive

# Verify Node.js version
node --version  # Should be v20.x or higher
```

### Useful Commands

```bash
# List all tests without running
dotnet test -- --list-tests

# Run with detailed output
dotnet test -- --output Detailed

# Run with maximum parallelism
dotnet test -- --maximum-parallel-tests 8

# Run specific test by name
dotnet test -- --treenode-filter "/*/*/*/MethodName_Should_Behavior_When_Condition"
```

---

## Unit Tests

### Naming Convention

```
MethodName_Should_ExpectedBehavior_When_Condition
```

Examples:

- `FromId_Should_ReturnKnownType_When_IdIsValid`
- `Calculate_Should_ReturnFailure_When_YearNotFound`
- `Build_Should_ThrowInvalidOperation_When_NoProviderRegistered`

### Test Organization

Tests mirror the source structure:

| Source Layer | Test Location |
|--------------|---------------|
| `Api/Requests/` | `Unit/Api/Requests/` |
| `Api/Mappings/` | `Unit/Api/Mappings/` |
| `Common/Results/` | `Unit/Common/Results/` |
| `Configuration/` | `Unit/Configuration/` |
| `Domain/ValueObjects/` | `Unit/Domain/ValueObjects/` |
| `Infrastructure/Providers/` | `Unit/Infrastructure/Providers/` |
| `DependencyInjection/` | `Unit/DependencyInjection/` |

### Example Unit Test

```csharp
public class EmployeeTypeIdTests
{
    [Test]
    [Arguments(1, "Standard")]
    [Arguments(2, "Teknokent")]
    [Arguments(5, "Employer")]
    public async Task FromId_Should_ReturnKnownType_When_IdIsValid(int id, string expectedName)
    {
        var result = EmployeeTypeId.FromId(id);

        await Assert.That(result.IsKnown).IsTrue();
        await Assert.That(result.Id).IsEqualTo(id);
    }
}
```

---

## Parity Tests

### Purpose

The .NET library is a port of the Angular salary calculator (`external/maas-hesaplama`). Parity tests ensure behavioral equivalence between the two implementations.

**Angular is the source of truth** - when discrepancies are found, the .NET implementation should be adjusted to match Angular's behavior (unless Angular has a confirmed bug).

### Architecture

The parity testing architecture uses C# as the single source of truth for test scenarios, with the Angular CLI as a pure calculation tool.

```
┌─────────────────────────────────────────────────────────────────┐
│ C# Test Suite (Source of Truth for Scenarios)                    │
│                                                                  │
│ [Test]                                                           │
│ [MethodDataSource(typeof(ParityScenarios), nameof(GetAllScenarios))]
│ [DisplayName("Parity: $scenario")]                               │
│ public async Task Scenario_Should_Match_Angular(ParityScenario) │
│                                                                  │
│     ┌─────────────┐                                              │
│     │ 1. Call CLI │ → npx ts-node calculate.ts < input.json     │
│     │ 2. Parse    │ ← stdout JSON (Angular result)              │
│     │ 3. Calculate│ → ISalaryCalculator.Calculate(request)      │
│     │ 4. Compare  │ → FieldComparer with tolerance              │
│     └─────────────┘                                              │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│ calculate.ts (Pure CLI Tool)                                     │
│                                                                  │
│ stdin:  { year, mode, employeeTypeId, salary, ... }             │
│ stdout: { success, output: { months: [...], totals: {...} } }   │
│ stderr: validation errors, warnings                              │
│ exit:   0=success, 1=validation error, 2=calculation error      │
└─────────────────────────────────────────────────────────────────┘
```

**Key Components:**

| File | Purpose |
|------|---------|
| `ParityScenarios.cs` | Provider-driven data source with ~76 test scenarios |
| `AngularCalculatorClient.cs` | CLI wrapper (handles init, npm install) |
| `calculate.ts` | Pure Angular CLI tool (stdin JSON → stdout JSON) |
| `AngularParityTests.cs` | TUnit test class using `[MethodDataSource]` |

**Benefits:**

- Single source of truth for scenarios (C#)
- Each scenario = individual TUnit test (better IDE integration)
- Parallel execution with `[ParallelLimiter]` (4 concurrent)
- Reusable Angular CLI tool
- Easy to add/remove scenarios
- Clear test failure reporting

**Performance:**

- ~1.7s per test (CLI startup overhead)
- 4 parallel tests = ~33s for 76 tests
- Acceptable for explicit parity tests

### Comparison Tolerance

Values are compared with tolerance to account for floating-point precision:

| Value Range | Tolerance Type | Value |
|-------------|----------------|-------|
| < 1 TRY | Absolute | 0.01 TRY (1 kuruş) |
| ≥ 1 TRY | Relative | 0.01% |

### Scenario Coverage

Current parity scenarios cover (all derived from library providers):

- **Years**: 2016-2026 (from `YearProvider.AvailableYears`)
- **Calculation Modes**: GrossToNet, NetToGross, TotalToGross
- **Employee Types**: All 10 types (from `ConstantsProvider.AllEmployeeTypes`)
- **Disability Degrees**: 1st, 2nd, 3rd (from `ConstantsProvider.GetDisability()`)
- **Partial Months**: 1, 15, 20, 30 days
- **R&D Days**: Various proportions for R&D employee types
- **AGI Scenarios**: Pre-2022 years with rates from `ConstantsProvider.AllAgiOptions`
- **Pensioner**: Tests with `isPensioner: true` for core employee types
- **5746 Discount**: Employee types where `EmployerSgkDiscount5746Applicable` is true
- **Education Exemption**: R&D types with rates from `ConstantsProvider.AllEducationTypes`
- **High Salary**: Tax bracket boundary testing

### Adding New Parity Scenarios

Scenarios are derived from library providers. To add a new category, edit `ParityScenarios.cs`:

```csharp
// Use provider data instead of hardcoded values
private static IEnumerable<ParityScenario> GetMyNewScenarios()
{
    var year = LatestYear;  // From YearProvider

    // Loop through provider data
    foreach (var empType in AllEmployeeTypes.Where(t => t.SomeProperty))
    {
        yield return CreateScenario(
            year + "-" + empType.Text.ToLowerInvariant() + "-my-scenario",
            "Year " + year + ", " + empType.Text + ", My Scenario",
            year,
            "GROSS_TO_NET",
            empType.Id,
            salaryAmount: 50000m,
            workedDays: 30);
    }
}

// Then include in GetAllScenarios():
public static IEnumerable<Func<ParityScenario>> GetAllScenarios()
{
    // ... existing scenarios ...

    foreach (var scenario in GetMyNewScenarios())
    {
        yield return () => scenario;
    }
}
```

**Key points:**
- Use `AvailableYears`, `AllEmployeeTypes`, `AllAgiOptions`, `AllEducationTypes` from providers
- Check properties like `ResearchAndDevelopmentTaxExemption`, `EmployerSgkDiscount5746Applicable`
- Use `empType.Text` for names instead of hardcoded switch statements

Run the tests to verify the new scenario passes.

---

## NativeAOT Compatibility

Both the library and test framework are NativeAOT compatible:

- **Library**: Uses source-generated JSON serialization, no reflection
- **TUnit**: Source-generated test discovery, AOT-first design
- **Tests**: Use static method data sources, avoid dynamic reflection

### AOT Test Guidelines

```csharp
// ✅ Good - Static method data source (AOT compatible)
[Test]
[MethodDataSource(nameof(GetScenarios))]
public async Task MyTest(TestData data) { }

public static IEnumerable<Func<TestData>> GetScenarios()
{
    yield return () => new TestData(...);
}

// ❌ Bad - Instance method data source (not AOT compatible)
[Test]
[InstanceMethodDataSource(nameof(scenarios))]
public async Task MyTest(TestData data) { }

private IEnumerable<TestData> scenarios => ...;
```

---

## CI/CD Integration

### GitHub Actions

```yaml
- name: Run Unit Tests
  run: dotnet test --configuration Release

- name: Run Parity Tests (optional, explicit)
  if: github.event_name == 'workflow_dispatch'
  run: |
    git submodule update --init --recursive
    dotnet test -- --treenode-filter "/*/*/AngularParityTests/*"
```

### Test Reports

TUnit supports TRX reports for CI integration:

```bash
dotnet test -- --report-trx --report-trx-filename results.trx
```

---

## Troubleshooting

### Parity Test Failures

1. **Submodule not initialized**:

   ```
   Angular submodule not found at external/maas-hesaplama
   ```

   Fix: `git submodule update --init --recursive`

2. **Node.js not found**:

   ```
   Required command failed: node --version
   ```

   Fix: Install Node.js 20+

3. **npm dependencies missing**:

   ```
   Cannot find module '...'
   ```

   Fix: `cd tests/tools/angular-parity-fixtures && npm ci`

4. **Tolerance exceeded**:

   ```
   Month 01.NetSalary: expected 50000.0000, actual 50000.0123, diff 0.0123
   ```

   This indicates a calculation discrepancy. Investigate the specific field.

### Debug Tips

```bash
# Run single parity scenario
dotnet test -- --treenode-filter "/*/*/*/*[TestId=2026-standard-grosstonet-50000-30days]"

# Verbose output
dotnet test -- --output Detailed --log-level Debug

# Generate fixtures manually
cd tests/tools/angular-parity-fixtures
npm run generate
```

---

## References

- [TUnit Documentation](https://tunit.dev/)
- [Angular Source (maas-hesaplama)](https://github.com/nuryagdym/maas-hesaplama)
- [Architecture V2](../docs/ARCHITECTURE-V2.md)
- [Parity Testing Handover](../docs/ANGULAR-PARITY-TESTING-HANDOVER.md)
