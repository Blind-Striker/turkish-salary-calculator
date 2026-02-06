#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Services;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Tests for <see cref="CapabilityResolver"/> capability resolution and provider validation.
/// </summary>
public sealed class CapabilityResolverTests
{
    private static IYearParameterProvider YearProvider { get; } = new EmbeddedYearParameterProvider();
    private static ICalculationConstantsProvider ConstantsProvider { get; } = new EmbeddedCalculationConstantsProvider();
    private static CapabilityResolver Resolver { get; } = new(YearProvider, ConstantsProvider);

    // ═══════════════════════════════════════════════════════════════
    // ValidateProviders Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ValidateProviders_Should_Succeed_When_ProvidersAreValid()
    {
        var result = Resolver.ValidateProviders();

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task ValidateProviders_Should_Fail_When_YearProviderHasNoYears()
    {
        var emptyYearProvider = new EmptyYearParameterProvider();
        var resolver = new CapabilityResolver(emptyYearProvider, ConstantsProvider);

        var result = resolver.ValidateProviders();

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Code == ErrorCode.InvalidYearParameterProvider)).IsTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // ResolveCapabilities - Basic Validation Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    public async Task ResolveCapabilities_Should_ReturnCapabilities_When_ValidInput()
    {
        var result = Resolver.ResolveCapabilities(
            2024, EmployeeTypeId.Standard, false, CalculationMode.GrossToNet);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Value).IsNotNull();
    }

    [Test]
    public async Task ResolveCapabilities_Should_Fail_When_YearNotSupported()
    {
        var result = Resolver.ResolveCapabilities(
            1900, EmployeeTypeId.Standard, false, CalculationMode.GrossToNet);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Code == ErrorCode.YearNotSupported)).IsTrue();
    }

    [Test]
    public async Task ResolveCapabilities_Should_Fail_When_EmployeeTypeNotFound()
    {
        var result = Resolver.ResolveCapabilities(
            2024, EmployeeTypeId.FromId(99), false, CalculationMode.GrossToNet);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Code == ErrorCode.InvalidEmployeeType)).IsTrue();
    }

    // ═══════════════════════════════════════════════════════════════
    // AGI Selection Capability Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [MethodDataSource(nameof(AgiSelectionScenarios))]
    public async Task ResolveCapabilities_Should_DetermineAgiSelection_BasedOnEmployeeTypeAndYear(
        int year, int employeeTypeId, bool expectedDisabled)
    {
        var result = Resolver.ResolveCapabilities(
            year, EmployeeTypeId.FromId(employeeTypeId), false, CalculationMode.GrossToNet);

        await Assert.That(result.IsSuccess).IsTrue();
        var isDisabled = !result.Value.IsAvailable(Capability.AgiSelection);
        await Assert.That(isDisabled).IsEqualTo(expectedDisabled);
    }

    public static IEnumerable<Func<(int Year, int EmployeeTypeId, bool ExpectedDisabled)>> AgiSelectionScenarios()
    {
        // Pre-2022 (no min wage exemption), AGI applicable employee = enabled
        yield return () => (2021, 1, false); // Standard
        yield return () => (2021, 5, false); // Employer

        // Post-2022 (min wage exemption) = AGI disabled regardless of employee type
        yield return () => (2024, 1, true);
        yield return () => (2024, 5, true);
    }

    // ═══════════════════════════════════════════════════════════════
    // Education Type Capability Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [MethodDataSource(nameof(EducationTypeScenarios))]
    public async Task ResolveCapabilities_Should_DetermineEducationType_BasedOnEmployeeType(
        int employeeTypeId, bool expectedDisabled)
    {
        var result = Resolver.ResolveCapabilities(
            2024, EmployeeTypeId.FromId(employeeTypeId), false, CalculationMode.GrossToNet);

        await Assert.That(result.IsSuccess).IsTrue();
        var isDisabled = !result.Value.IsAvailable(Capability.EducationType);
        await Assert.That(isDisabled).IsEqualTo(expectedDisabled);
    }

    public static IEnumerable<Func<(int EmployeeTypeId, bool ExpectedDisabled)>> EducationTypeScenarios()
    {
        yield return () => (1, true);  // Standard = disabled
        yield return () => (2, true);  // Teknokent 4691 = disabled
        yield return () => (3, false); // 5746 R&D = enabled (has education exemption)
        yield return () => (5, true);  // Employer = disabled
    }

    // ═══════════════════════════════════════════════════════════════
    // 5746 Discount Capability Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [MethodDataSource(nameof(Discount5746Scenarios))]
    public async Task ResolveCapabilities_Should_Determine5746Discount_BasedOnPensionerAndEmployeeType(
        int employeeTypeId, bool isPensioner, bool expectedDisabled)
    {
        var result = Resolver.ResolveCapabilities(
            2024, EmployeeTypeId.FromId(employeeTypeId), isPensioner, CalculationMode.GrossToNet);

        await Assert.That(result.IsSuccess).IsTrue();
        var isDisabled = !result.Value.IsAvailable(Capability.Discount5746);
        await Assert.That(isDisabled).IsEqualTo(expectedDisabled);
    }

    public static IEnumerable<Func<(int EmployeeTypeId, bool IsPensioner, bool ExpectedDisabled)>> Discount5746Scenarios()
    {
        yield return () => (1, false, false); // Standard, not pensioner = enabled
        yield return () => (1, true, true);   // Standard, pensioner = disabled
        yield return () => (5, false, true);  // Employer (doesn't support 5746) = disabled
    }

    // ═══════════════════════════════════════════════════════════════
    // R&D Days Input Capability Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [MethodDataSource(nameof(RnDDaysScenarios))]
    public async Task ResolveCapabilities_Should_DetermineRnDDaysInput_BasedOnEmployeeType(
        int employeeTypeId, bool expectedDisabled)
    {
        var result = Resolver.ResolveCapabilities(
            2024, EmployeeTypeId.FromId(employeeTypeId), false, CalculationMode.GrossToNet);

        await Assert.That(result.IsSuccess).IsTrue();
        var isDisabled = !result.Value.IsAvailable(Capability.RnDDaysInput);
        await Assert.That(isDisabled).IsEqualTo(expectedDisabled);
    }

    public static IEnumerable<Func<(int EmployeeTypeId, bool ExpectedDisabled)>> RnDDaysScenarios()
    {
        yield return () => (1, true);  // Standard = disabled
        yield return () => (2, false); // Teknokent 4691 = enabled
        yield return () => (3, false); // 5746 R&D = enabled
        yield return () => (5, true);  // Employer = disabled
    }

    // ═══════════════════════════════════════════════════════════════
    // AGI Included In Net Capability Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [MethodDataSource(nameof(AgiIncludedInNetScenarios))]
    public async Task ResolveCapabilities_Should_DetermineAgiIncludedInNet_BasedOnMode(
        CalculationMode mode, bool expectedDisabled)
    {
        var result = Resolver.ResolveCapabilities(
            2024, EmployeeTypeId.Standard, false, mode);

        await Assert.That(result.IsSuccess).IsTrue();
        var isDisabled = !result.Value.IsAvailable(Capability.AgiIncludedInNet);
        await Assert.That(isDisabled).IsEqualTo(expectedDisabled);
    }

    public static IEnumerable<Func<(CalculationMode Mode, bool ExpectedDisabled)>> AgiIncludedInNetScenarios()
    {
        yield return () => (CalculationMode.GrossToNet, true);
        yield return () => (CalculationMode.NetToGross, false); // Only enabled for NetToGross
        yield return () => (CalculationMode.TotalToGross, true);
    }

    // ═══════════════════════════════════════════════════════════════
    // Min Wage Exemption & AGI Calculation Capability Tests
    // ═══════════════════════════════════════════════════════════════

    [Test]
    [MethodDataSource(nameof(MinWageExemptionScenarios))]
    public async Task ResolveCapabilities_Should_DetermineMinWageExemption_BasedOnYear(
        int year, bool minWageExemptionDisabled, bool agiCalculationDisabled)
    {
        var result = Resolver.ResolveCapabilities(
            year, EmployeeTypeId.Standard, false, CalculationMode.GrossToNet);

        await Assert.That(result.IsSuccess).IsTrue();

        var minWageDisabled = !result.Value.IsAvailable(Capability.MinWageExemption);
        var agiCalcDisabled = !result.Value.IsAvailable(Capability.AgiCalculation);

        await Assert.That(minWageDisabled).IsEqualTo(minWageExemptionDisabled);
        await Assert.That(agiCalcDisabled).IsEqualTo(agiCalculationDisabled);
    }

    public static IEnumerable<Func<(int Year, bool MinWageExemptionDisabled, bool AgiCalculationDisabled)>> MinWageExemptionScenarios()
    {
        // Pre-2022: MinWageExemption disabled, AGI calculation enabled
        yield return () => (2021, true, false);

        // 2022+: MinWageExemption enabled, AGI calculation disabled
        yield return () => (2024, false, true);
        yield return () => (2026, false, true);
    }

    // ═══════════════════════════════════════════════════════════════
    // Test Helpers
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Empty provider for testing validation failures.
    /// </summary>
    private sealed class EmptyYearParameterProvider : IYearParameterProvider
    {
        public IReadOnlyList<int> AvailableYears => [];

        public YearParameter? GetParameter(int year) => null;

        public bool HasYear(int year) => false;
    }
}
