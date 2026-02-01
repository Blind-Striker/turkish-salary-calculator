using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Services;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Parity;

/// <summary>
/// Represents a parity test scenario with input parameters.
/// </summary>
public sealed record ParityScenario
{
    /// <summary>
    /// Gets the unique test identifier.
    /// </summary>
    public required string TestId { get; init; }

    /// <summary>
    /// Gets the human-readable test description.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the calculation input parameters.
    /// </summary>
    public required TestInput Input { get; init; }

    /// <inheritdoc />
    public override string ToString() => TestId;
}

/// <summary>
/// Static data source for parity test scenarios.
/// AOT-compatible: uses static methods returning IEnumerable{Func{T}}.
/// All scenario data is driven by library providers (no hardcoded values).
/// </summary>
public static class ParityScenarios
{
    /// <summary>Calculator instance for accessing providers.</summary>
    private static readonly ISalaryCalculator Calculator = SalaryCalculatorBuilder.Create();

    /// <summary>Year parameter provider from calculator.</summary>
    private static readonly IYearParameterProvider YearProvider = Calculator.YearProvider.Value;

    /// <summary>Calculation constants provider from calculator.</summary>
    private static readonly ICalculationConstantsProvider ConstantsProvider = Calculator.ConstantsProvider.Value;

    /// <summary>All available years from provider.</summary>
    private static IReadOnlyList<int> AvailableYears => YearProvider.AvailableYears;

    /// <summary>All employee types from provider.</summary>
    private static IReadOnlyList<EmployeeTypeConstant> AllEmployeeTypes => ConstantsProvider.AllEmployeeTypes;

    /// <summary>All AGI options from provider.</summary>
    private static IReadOnlyList<AgiConstant> AllAgiOptions => ConstantsProvider.AllAgiOptions;

    /// <summary>All education types from provider.</summary>
    private static IReadOnlyList<EmployeeEducationTypeConstant> AllEducationTypes => ConstantsProvider.AllEducationTypes;

    /// <summary>Most recent available year.</summary>
    private static int LatestYear => AvailableYears.Max();

    /// <summary>Recent 6 years for testing.</summary>
    private static IEnumerable<int> RecentYears => AvailableYears.OrderByDescending(y => y).Take(6);

    /// <summary>Years where AGI was applicable (discontinued in 2022).</summary>
    private static IEnumerable<int> YearsWithAgi => AvailableYears.Where(y => y <= 2021);

    /// <summary>Years with mid-year minimum wage changes.</summary>
    private static IEnumerable<int> MidYearChangeYears => AvailableYears.Where(y => y is 2022 or 2023);

    /// <summary>Core employee types for focused testing (Standard, Teknokent, ARGE, Employer).</summary>
    private static IEnumerable<EmployeeTypeConstant> CoreEmployeeTypes =>
        AllEmployeeTypes.Where(t => t.Id is 1 or 2 or 3 or 5);

    /// <summary>RnD employee types (those with ResearchAndDevelopmentTaxExemption).</summary>
    private static IEnumerable<EmployeeTypeConstant> RnDEmployeeTypes =>
        AllEmployeeTypes.Where(t => t.ResearchAndDevelopmentTaxExemption);

    /// <summary>Employee types that support 5746 discount.</summary>
    private static IEnumerable<EmployeeTypeConstant> Types5746Applicable =>
        AllEmployeeTypes.Where(t => t.EmployerSgkDiscount5746Applicable);

    /// <summary>
    /// Returns all parity scenarios as Func{ParityScenario} for TUnit's MethodDataSource.
    /// Each scenario is wrapped in Func to ensure fresh instances per test.
    /// </summary>
    /// <returns>Enumerable of scenario factory functions.</returns>
    public static IEnumerable<Func<ParityScenario>> GetAllScenarios()
    {
        return GetAllScenariosCore().Concat(GetAllScenariosFeatures()).Concat(GetAllScenariosComprehensive());
    }

    private static IEnumerable<Func<ParityScenario>> GetAllScenariosCore()
    {
        return WrapScenarios(
            GetBasicYearScenarios(),
            GetEmployeeTypeScenarios(),
            GetCalculationModeScenarios(),
            GetPartialDaysScenarios(),
            GetDisabilityScenarios(),
            GetHighSalaryScenarios(),
            GetRnDScenarios(),
            GetMidYearWageChangeScenarios());
    }

    private static IEnumerable<Func<ParityScenario>> GetAllScenariosFeatures()
    {
        return WrapScenarios(
            GetPensionerScenarios(),
            Get5746DiscountScenarios(),
            GetAgiScenarios(),
            GetEducationExemptionScenarios());
    }

    private static IEnumerable<Func<ParityScenario>> GetAllScenariosComprehensive()
    {
        return WrapScenarios(
            GetAllModesAllTypesScenarios(),
            GetMixedDaysRnDScenarios(),
            GetMinWageExemptionOffScenarios(),
            GetCombinationScenarios(),
            GetStressTestScenarios(),
            GetNewHireScenarios());
    }

    private static IEnumerable<Func<ParityScenario>> WrapScenarios(params IEnumerable<ParityScenario>[] scenarioSets)
    {
        return scenarioSets.SelectMany(set => set.Select<ParityScenario, Func<ParityScenario>>(s => () => s));
    }

    private static IEnumerable<ParityScenario> GetBasicYearScenarios()
    {
        var standardType = GetEmployeeType(1);

        foreach (var year in RecentYears)
        {
            yield return CreateScenario(
                year + "-" + standardType.Text.ToLowerInvariant() + "-grosstonet-50000-30days",
                "Year " + year + ", " + standardType.Text + ", GrossToNet, 50000 TRY, 30 days",
                year,
                "GROSS_TO_NET",
                employeeTypeId: standardType.Id,
                salaryAmount: 50000m,
                workedDays: 30);
        }
    }

    private static IEnumerable<ParityScenario> GetEmployeeTypeScenarios()
    {
        var year = LatestYear;
        const decimal salary = 50000m;

        foreach (var empType in AllEmployeeTypes)
        {
            yield return CreateScenario(
                year + "-" + empType.Text.ToLowerInvariant() + "-grosstonet-" + salary + "-30days",
                "Year " + year + ", " + empType.Text + ", GrossToNet, " + salary + " TRY, 30 days",
                year,
                "GROSS_TO_NET",
                empType.Id,
                salary,
                workedDays: 30);
        }
    }

    private static IEnumerable<ParityScenario> GetCalculationModeScenarios()
    {
        var year = LatestYear;

        var modes = new[]
        {
            ("GROSS_TO_NET", 50000m, "grosstonet"),
            ("NET_TO_GROSS", 35000m, "nettogross"),
            ("TOTAL_TO_GROSS", 80000m, "totaltogross"),
        };

        foreach (var (mode, salary, modeName) in modes)
        {
            foreach (var empType in CoreEmployeeTypes)
            {
                yield return CreateScenario(
                    year + "-" + empType.Text.ToLowerInvariant() + "-" + modeName + "-" + salary + "-30days",
                    "Year " + year + ", " + empType.Text + ", " + mode + ", " + salary + " TRY, 30 days",
                    year,
                    mode,
                    empType.Id,
                    salary,
                    workedDays: 30);
            }
        }
    }

    private static IEnumerable<ParityScenario> GetPartialDaysScenarios()
    {
        var year = LatestYear;
        var standardType = GetEmployeeType(1);

        foreach (var days in new[] { 1, 15, 20 })
        {
            yield return CreateScenario(
                year + "-" + standardType.Text.ToLowerInvariant() + "-grosstonet-50000-" + days + "days",
                "Year " + year + ", " + standardType.Text + ", GrossToNet, 50000 TRY, " + days + " days",
                year,
                "GROSS_TO_NET",
                employeeTypeId: standardType.Id,
                salaryAmount: 50000m,
                workedDays: days);

            yield return CreateScenario(
                year + "-" + standardType.Text.ToLowerInvariant() + "-nettogross-35000-" + days + "days",
                "Year " + year + ", " + standardType.Text + ", NetToGross, 35000 TRY, " + days + " days",
                year,
                "NET_TO_GROSS",
                employeeTypeId: standardType.Id,
                salaryAmount: 35000m,
                workedDays: days);
        }
    }

    private static IEnumerable<ParityScenario> GetDisabilityScenarios()
    {
        var year = LatestYear;
        var standardType = GetEmployeeType(1);

        // Disability degrees 1, 2, 3 (skip 0 = no disability)
        foreach (var degree in new[] { 1, 2, 3 })
        {
            var disability = ConstantsProvider.GetDisability(degree);
            if (disability is null)
            {
                continue;
            }

            yield return CreateScenario(
                year + "-" + standardType.Text.ToLowerInvariant() + "-grosstonet-50000-disability" + degree,
                "Year " + year + ", " + standardType.Text + ", GrossToNet, 50000 TRY, Disability degree " + degree,
                year,
                "GROSS_TO_NET",
                employeeTypeId: standardType.Id,
                salaryAmount: 50000m,
                workedDays: 30,
                disabilityDegree: degree);
        }
    }

    private static IEnumerable<ParityScenario> GetHighSalaryScenarios()
    {
        var year = LatestYear;
        var standardType = GetEmployeeType(1);

        foreach (var salary in new[] { 100000m, 200000m, 500000m })
        {
            yield return CreateScenario(
                year + "-" + standardType.Text.ToLowerInvariant() + "-grosstonet-" + salary + "-highsalary",
                "Year " + year + ", " + standardType.Text + ", GrossToNet, " + salary + " TRY (high salary)",
                year,
                "GROSS_TO_NET",
                employeeTypeId: standardType.Id,
                salaryAmount: salary,
                workedDays: 30);
        }
    }

    private static IEnumerable<ParityScenario> GetRnDScenarios()
    {
        var year = LatestYear;

        // Get a sample education type for exemption rate (Master's degree)
        var mastersDegree = AllEducationTypes.FirstOrDefault(e => e.Id == 4);
        var educationRate = mastersDegree?.ExemptionRate ?? 0.23;

        foreach (var rndDays in new[] { 10, 15, 20, 30 })
        {
            foreach (var empType in RnDEmployeeTypes.Take(2)) // Teknokent and ARGE
            {
                yield return CreateScenario(
                    year + "-" + empType.Text.ToLowerInvariant() + "-grosstonet-50000-rnd" + rndDays,
                    "Year " + year + ", " + empType.Text + ", GrossToNet, 50000 TRY, " + rndDays + " R&D days",
                    year,
                    "GROSS_TO_NET",
                    employeeTypeId: empType.Id,
                    salaryAmount: 50000m,
                    workedDays: 30,
                    rndDays: rndDays,
                    educationExemptionRate: (decimal)educationRate);
            }
        }
    }

    private static IEnumerable<ParityScenario> GetMidYearWageChangeScenarios()
    {
        foreach (var year in MidYearChangeYears)
        {
            foreach (var empType in CoreEmployeeTypes)
            {
                yield return CreateScenario(
                    year + "-" + empType.Text.ToLowerInvariant() + "-grosstonet-50000-midyear",
                    "Year " + year + " (mid-year change), " + empType.Text + ", GrossToNet, 50000 TRY",
                    year,
                    "GROSS_TO_NET",
                    empType.Id,
                    salaryAmount: 50000m,
                    workedDays: 30);

                // Additional modes for employer-related types
                if (empType.Id is 5 or 8 or 9)
                {
                    yield return CreateScenario(
                        year + "-" + empType.Text.ToLowerInvariant() + "-nettogross-35000-midyear",
                        "Year " + year + " (mid-year change), " + empType.Text + ", NetToGross, 35000 TRY",
                        year,
                        "NET_TO_GROSS",
                        empType.Id,
                        salaryAmount: 35000m,
                        workedDays: 30);

                    yield return CreateScenario(
                        year + "-" + empType.Text.ToLowerInvariant() + "-totaltogross-80000-midyear",
                        "Year " + year + " (mid-year change), " + empType.Text + ", TotalToGross, 80000 TRY",
                        year,
                        "TOTAL_TO_GROSS",
                        empType.Id,
                        salaryAmount: 80000m,
                        workedDays: 30);
                }
            }
        }
    }

    private static IEnumerable<ParityScenario> GetPensionerScenarios()
    {
        var year = LatestYear;

        // Test pensioner flag with core employee types
        foreach (var empType in CoreEmployeeTypes)
        {
            yield return CreateScenario(
                year + "-" + empType.Text.ToLowerInvariant() + "-grosstonet-50000-pensioner",
                "Year " + year + ", " + empType.Text + ", GrossToNet, 50000 TRY, Pensioner",
                year,
                "GROSS_TO_NET",
                empType.Id,
                salaryAmount: 50000m,
                workedDays: 30,
                isPensioner: true);
        }
    }

    private static IEnumerable<ParityScenario> Get5746DiscountScenarios()
    {
        var year = LatestYear;

        // Test 5746 employer discount for applicable employee types
        foreach (var empType in Types5746Applicable.Take(3))
        {
            yield return CreateScenario(
                year + "-" + empType.Text.ToLowerInvariant() + "-grosstonet-50000-5746discount",
                "Year " + year + ", " + empType.Text + ", GrossToNet, 50000 TRY, 5746 Discount",
                year,
                "GROSS_TO_NET",
                empType.Id,
                salaryAmount: 50000m,
                workedDays: 30,
                applyEmployerDiscount5746: true);
        }
    }

    private static IEnumerable<ParityScenario> GetAgiScenarios()
    {
        // AGI was discontinued in 2022, only test for earlier years
        var yearsToTest = YearsWithAgi.OrderByDescending(y => y).Take(2);
        var agiOptionsToTest = AllAgiOptions.Take(3);

        foreach (var year in yearsToTest)
        {
            foreach (var agi in agiOptionsToTest)
            {
                var standardType = GetEmployeeType(1);
                yield return CreateScenario(
                    year + "-" + standardType.Text.ToLowerInvariant() + "-grosstonet-50000-agi" + agi.Id,
                    "Year " + year + ", " + standardType.Text + ", GrossToNet, 50000 TRY, AGI " + agi.Text,
                    year,
                    "GROSS_TO_NET",
                    standardType.Id,
                    salaryAmount: 50000m,
                    workedDays: 30,
                    agiRate: (decimal)agi.Rate,
                    isAgiCalculationEnabled: true);
            }
        }
    }

    private static IEnumerable<ParityScenario> GetEducationExemptionScenarios()
    {
        var year = LatestYear;

        // Test education exemption rates for R&D employee types
        var rndType = RnDEmployeeTypes.FirstOrDefault();
        if (rndType is null)
        {
            yield break;
        }

        foreach (var eduType in AllEducationTypes)
        {
            yield return CreateScenario(
                year + "-" + rndType.Text.ToLowerInvariant() + "-grosstonet-50000-edu" + eduType.Id,
                "Year " + year + ", " + rndType.Text + ", GrossToNet, 50000 TRY, Education: " + eduType.Text,
                year,
                "GROSS_TO_NET",
                rndType.Id,
                salaryAmount: 50000m,
                workedDays: 30,
                rndDays: 30,
                educationExemptionRate: (decimal)eduType.ExemptionRate);
        }
    }

    /// <summary>
    /// All calculation modes crossed with all employee types.
    /// Tests comprehensive mode/type coverage.
    /// </summary>
    private static IEnumerable<ParityScenario> GetAllModesAllTypesScenarios()
    {
        var year = LatestYear;

        var modes = new[]
        {
            ("NET_TO_GROSS", 40000m, "nettogross"),
            ("TOTAL_TO_GROSS", 90000m, "totaltogross"),
        };

        // Skip types already covered in GetCalculationModeScenarios (core types)
        var nonCoreTypes = AllEmployeeTypes.Where(t => t.Id is not (1 or 2 or 3 or 5));

        foreach (var (mode, salary, modeName) in modes)
        {
            foreach (var empType in nonCoreTypes)
            {
                yield return CreateScenario(
                    year + "-" + empType.Text.ToLowerInvariant() + "-" + modeName + "-" + salary + "-allmodes",
                    "Year " + year + ", " + empType.Text + ", " + mode + ", " + salary + " TRY (all modes coverage)",
                    year,
                    mode,
                    empType.Id,
                    salary,
                    workedDays: 30);
            }
        }
    }

    /// <summary>
    /// Mixed worked days and RnD days combinations for RnD employee types.
    /// Tests partial month scenarios with varying RnD proportions.
    /// </summary>
    private static IEnumerable<ParityScenario> GetMixedDaysRnDScenarios()
    {
        var year = LatestYear;
        var rndType = RnDEmployeeTypes.FirstOrDefault();
        if (rndType is null)
        {
            yield break;
        }

        var mastersDegree = AllEducationTypes.FirstOrDefault(e => e.Id == 4);
        var educationRate = mastersDegree?.ExemptionRate ?? 0.23;

        // Various worked/RnD combinations
        var daysCombinations = new[]
        {
            (workedDays: 15, rndDays: 10),
            (workedDays: 20, rndDays: 15),
            (workedDays: 25, rndDays: 20),
            (workedDays: 20, rndDays: 5),
            (workedDays: 10, rndDays: 10),
        };

        foreach (var (workedDays, rndDays) in daysCombinations)
        {
            yield return CreateScenario(
                year + "-" + rndType.Text.ToLowerInvariant() + "-grosstonet-50000-" + workedDays + "w" + rndDays + "r",
                "Year " + year + ", " + rndType.Text + ", GrossToNet, " + workedDays + " worked, " + rndDays + " RnD days",
                year,
                "GROSS_TO_NET",
                rndType.Id,
                salaryAmount: 50000m,
                workedDays: workedDays,
                rndDays: rndDays,
                educationExemptionRate: (decimal)educationRate);

            // Also test NetToGross with mixed days
            yield return CreateScenario(
                year + "-" + rndType.Text.ToLowerInvariant() + "-nettogross-35000-" + workedDays + "w" + rndDays + "r",
                "Year " + year + ", " + rndType.Text + ", NetToGross, " + workedDays + " worked, " + rndDays + " RnD days",
                year,
                "NET_TO_GROSS",
                rndType.Id,
                salaryAmount: 35000m,
                workedDays: workedDays,
                rndDays: rndDays,
                educationExemptionRate: (decimal)educationRate);
        }
    }

    /// <summary>
    /// Tests with minimum wage tax exemption disabled.
    /// </summary>
    private static IEnumerable<ParityScenario> GetMinWageExemptionOffScenarios()
    {
        var year = LatestYear;

        foreach (var empType in CoreEmployeeTypes.Take(2))
        {
            yield return CreateScenario(
                year + "-" + empType.Text.ToLowerInvariant() + "-grosstonet-50000-nominwageexemption",
                "Year " + year + ", " + empType.Text + ", GrossToNet, 50000 TRY, No MinWage Exemption",
                year,
                "GROSS_TO_NET",
                empType.Id,
                salaryAmount: 50000m,
                workedDays: 30,
                applyMinWageTaxExemption: false);

            yield return CreateScenario(
                year + "-" + empType.Text.ToLowerInvariant() + "-nettogross-35000-nominwageexemption",
                "Year " + year + ", " + empType.Text + ", NetToGross, 35000 TRY, No MinWage Exemption",
                year,
                "NET_TO_GROSS",
                empType.Id,
                salaryAmount: 35000m,
                workedDays: 30,
                applyMinWageTaxExemption: false);
        }
    }

    /// <summary>
    /// Combination scenarios: multiple flags enabled together.
    /// </summary>
    private static IEnumerable<ParityScenario> GetCombinationScenarios()
    {
        var year = LatestYear;
        var standardType = GetEmployeeType(1);

        // Disability + Pensioner
        foreach (var degree in new[] { 1, 2 })
        {
            yield return CreateScenario(
                year + "-" + standardType.Text.ToLowerInvariant() + "-grosstonet-50000-disability" + degree + "-pensioner",
                "Year " + year + ", " + standardType.Text + ", Disability " + degree + " + Pensioner",
                year,
                "GROSS_TO_NET",
                standardType.Id,
                salaryAmount: 50000m,
                workedDays: 30,
                disabilityDegree: degree,
                isPensioner: true);
        }

        // RnD + Disability
        var rndType = RnDEmployeeTypes.FirstOrDefault();
        if (rndType is not null)
        {
            var mastersDegree = AllEducationTypes.FirstOrDefault(e => e.Id == 4);
            var educationRate = mastersDegree?.ExemptionRate ?? 0.23;

            yield return CreateScenario(
                year + "-" + rndType.Text.ToLowerInvariant() + "-grosstonet-50000-rnd20-disability1",
                "Year " + year + ", " + rndType.Text + ", RnD 20 days + Disability 1",
                year,
                "GROSS_TO_NET",
                rndType.Id,
                salaryAmount: 50000m,
                workedDays: 30,
                rndDays: 20,
                disabilityDegree: 1,
                educationExemptionRate: (decimal)educationRate);
        }

        // Pensioner + 5746 Discount
        var type5746 = Types5746Applicable.FirstOrDefault();
        if (type5746 is not null)
        {
            yield return CreateScenario(
                year + "-" + type5746.Text.ToLowerInvariant() + "-grosstonet-50000-pensioner-5746",
                "Year " + year + ", " + type5746.Text + ", Pensioner + 5746 Discount",
                year,
                "GROSS_TO_NET",
                type5746.Id,
                salaryAmount: 50000m,
                workedDays: 30,
                isPensioner: true,
                applyEmployerDiscount5746: true);
        }

        // Partial days + Pensioner
        yield return CreateScenario(
            year + "-" + standardType.Text.ToLowerInvariant() + "-grosstonet-50000-15days-pensioner",
            "Year " + year + ", " + standardType.Text + ", 15 days + Pensioner",
            year,
            "GROSS_TO_NET",
            standardType.Id,
            salaryAmount: 50000m,
            workedDays: 15,
            isPensioner: true);

        // Disability + No MinWage Exemption
        yield return CreateScenario(
            year + "-" + standardType.Text.ToLowerInvariant() + "-grosstonet-50000-disability2-nominwage",
            "Year " + year + ", " + standardType.Text + ", Disability 2 + No MinWage Exemption",
            year,
            "GROSS_TO_NET",
            standardType.Id,
            salaryAmount: 50000m,
            workedDays: 30,
            disabilityDegree: 2,
            applyMinWageTaxExemption: false);
    }

    /// <summary>
    /// Stress test scenarios with extreme salaries.
    /// Pushes tax brackets, SGK ceiling, and binary search limits.
    /// </summary>
    private static IEnumerable<ParityScenario> GetStressTestScenarios()
    {
        var year = LatestYear;
        var standardType = GetEmployeeType(1);

        var extremeSalaries = new[] { 300_000m, 500_000m, 1_000_000m };
        var modes = new[]
        {
            ("GROSS_TO_NET", "grosstonet"),
            ("NET_TO_GROSS", "nettogross"),
            ("TOTAL_TO_GROSS", "totaltogross"),
        };

        foreach (var salary in extremeSalaries)
        {
            foreach (var (mode, modeName) in modes)
            {
                // Adjust salary for reverse calculations (net/total are lower than gross)
                var adjustedSalary = mode switch
                {
                    "NET_TO_GROSS" => salary * 0.6m, // Approximate net is 60% of gross for high salaries
                    "TOTAL_TO_GROSS" => salary * 1.3m, // Total cost is ~130% of gross
                    _ => salary,
                };

                yield return CreateScenario(
                    year + "-" + standardType.Text.ToLowerInvariant() + "-" + modeName + "-" + salary + "-stress",
                    "Year " + year + ", " + standardType.Text + ", " + mode + ", " + salary + " TRY (stress test)",
                    year,
                    mode,
                    standardType.Id,
                    adjustedSalary,
                    workedDays: 30);
            }
        }

        // Also test with partial days at extreme salaries
        yield return CreateScenario(
            year + "-" + standardType.Text.ToLowerInvariant() + "-grosstonet-500000-15days-stress",
            "Year " + year + ", " + standardType.Text + ", GrossToNet, 500K TRY, 15 days (stress)",
            year,
            "GROSS_TO_NET",
            standardType.Id,
            salaryAmount: 500_000m,
            workedDays: 15);

        // Extreme with disability
        yield return CreateScenario(
            year + "-" + standardType.Text.ToLowerInvariant() + "-grosstonet-300000-disability1-stress",
            "Year " + year + ", " + standardType.Text + ", GrossToNet, 300K TRY, Disability 1 (stress)",
            year,
            "GROSS_TO_NET",
            standardType.Id,
            salaryAmount: 300_000m,
            workedDays: 30,
            disabilityDegree: 1);
    }

    private static ParityScenario CreateScenario(
        string testId,
        string description,
        int year,
        string calculationMode,
        int employeeTypeId,
        decimal salaryAmount,
        int workedDays,
        int rndDays = 0,
        int disabilityDegree = 0,
        decimal agiRate = 0m,
        decimal educationExemptionRate = 0m,
        bool isPensioner = false,
        bool applyEmployerDiscount5746 = false,
        bool isAgiIncludedNet = false,
        bool isAgiIncludedTax = false,
        bool applyMinWageTaxExemption = true,
        bool isAgiCalculationEnabled = false)
    {
        return new ParityScenario
        {
            TestId = testId,
            Description = description,
            Input = new TestInput
            {
                Year = year,
                CalculationMode = calculationMode,
                EmployeeTypeId = employeeTypeId,
                SalaryAmount = salaryAmount,
                WorkedDays = workedDays,
                ResearchAndDevelopmentWorkedDays = rndDays,
                DisabilityDegree = disabilityDegree,
                AgiRate = agiRate,
                EducationExemptionRate = educationExemptionRate,
                IsPensioner = isPensioner,
                ApplyEmployerDiscount5746 = applyEmployerDiscount5746,
                IsAgiIncludedNet = isAgiIncludedNet,
                IsAgiIncludedTax = isAgiIncludedTax,
                ApplyMinWageTaxExemption = applyMinWageTaxExemption,
                IsAgiCalculationEnabled = isAgiCalculationEnabled,
            },
        };
    }

    /// <summary>
    /// New hire scenarios where employee starts mid-year.
    /// Tests zero salary/worked days for non-working months.
    /// </summary>
    private static IEnumerable<ParityScenario> GetNewHireScenarios()
    {
        return GetNewHireBasicScenarios()
            .Concat(GetNewHireAdvancedScenarios())
            .Concat(GetNewHireReverseModeScenarios());
    }

    private static IEnumerable<ParityScenario> GetNewHireBasicScenarios()
    {
        var year = LatestYear;
        var standardType = GetEmployeeType(1);

        // New hire starting in March
        yield return CreateNewHireScenario(year, standardType.Id, "march-50000",
            "New hire starting March, 50000 TRY", "GROSS_TO_NET", startMonth: 3, salary: 50_000m);

        // New hire starting in July (second half)
        yield return CreateNewHireScenario(year, standardType.Id, "july-60000",
            "New hire starting July, 60000 TRY", "GROSS_TO_NET", startMonth: 7, salary: 60_000m);

        // New hire starting in November (late year)
        yield return CreateNewHireScenario(year, standardType.Id, "november-55000",
            "New hire starting November, 55000 TRY", "GROSS_TO_NET", startMonth: 11, salary: 55_000m);
    }

    private static IEnumerable<ParityScenario> GetNewHireAdvancedScenarios()
    {
        var year = LatestYear;
        var standardType = GetEmployeeType(1);
        var teknokentType = GetEmployeeType(2);

        // New hire with partial first month
        yield return CreateScenarioWithMonthlyArrays(
            year + "-newhire-march-partial",
            "Year " + year + ", New hire starting mid-March (15 days), 50000 TRY",
            year, "GROSS_TO_NET", standardType.Id,
            [0, 0, 50_000, 50_000, 50_000, 50_000, 50_000, 50_000, 50_000, 50_000, 50_000, 50_000],
            [0, 0, 15, 30, 30, 30, 30, 30, 30, 30, 30, 30],
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

        // New hire with mid-year salary raise
        yield return CreateScenarioWithMonthlyArrays(
            year + "-newhire-raise",
            "Year " + year + ", New hire April with raise in September, 40K→60K TRY",
            year, "GROSS_TO_NET", standardType.Id,
            [0, 0, 0, 40_000, 40_000, 40_000, 40_000, 40_000, 60_000, 60_000, 60_000, 60_000],
            [0, 0, 0, 30, 30, 30, 30, 30, 30, 30, 30, 30],
            [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]);

        // Teknokent new hire with R&D days
        yield return CreateScenarioWithMonthlyArrays(
            year + "-newhire-teknokent-may",
            "Year " + year + ", Teknokent new hire starting May, 80000 TRY, 20 R&D days",
            year, "GROSS_TO_NET", teknokentType.Id,
            [0, 0, 0, 0, 80_000, 80_000, 80_000, 80_000, 80_000, 80_000, 80_000, 80_000],
            [0, 0, 0, 0, 30, 30, 30, 30, 30, 30, 30, 30],
            [0, 0, 0, 0, 20, 20, 20, 20, 20, 20, 20, 20],
            educationExemptionRate: 0.23m);
    }

    private static IEnumerable<ParityScenario> GetNewHireReverseModeScenarios()
    {
        var year = LatestYear;
        var standardType = GetEmployeeType(1);

        // Net to Gross for new hire (binary search with zeros)
        yield return CreateNewHireScenario(year, standardType.Id, "nettogross-june",
            "New hire June, NetToGross 35000 TRY", "NET_TO_GROSS", startMonth: 6, salary: 35_000m);

        // Total to Gross for new hire
        yield return CreateNewHireScenario(year, standardType.Id, "totaltogross-august",
            "New hire August, TotalToGross 90000 TRY", "TOTAL_TO_GROSS", startMonth: 8, salary: 90_000m);
    }

    private static ParityScenario CreateNewHireScenario(
        int year, int employeeTypeId, string testIdSuffix, string description,
        string mode, int startMonth, decimal salary)
    {
        var salaries = Enumerable.Range(1, 12).Select(m => m >= startMonth ? salary : 0m).ToList();
        var workedDays = Enumerable.Range(1, 12).Select(m => m >= startMonth ? 30 : 0).ToList();
        var rndDays = Enumerable.Repeat(0, 12).ToList();

        return CreateScenarioWithMonthlyArrays(
            year + "-newhire-" + testIdSuffix,
            "Year " + year + ", " + description,
            year, mode, employeeTypeId, salaries, workedDays, rndDays);
    }

    private static ParityScenario CreateScenarioWithMonthlyArrays(
        string testId,
        string description,
        int year,
        string calculationMode,
        int employeeTypeId,
        IReadOnlyList<decimal> monthlySalaries,
        IReadOnlyList<int> monthlyWorkedDays,
        IReadOnlyList<int> monthlyRnDDays,
        int disabilityDegree = 0,
        decimal agiRate = 0m,
        decimal educationExemptionRate = 0m,
        bool isPensioner = false,
        bool applyEmployerDiscount5746 = false,
        bool isAgiIncludedNet = false,
        bool isAgiIncludedTax = false,
        bool applyMinWageTaxExemption = true,
        bool isAgiCalculationEnabled = false)
    {
        // Use first non-zero values as the "uniform" fallback values
        var firstNonZeroSalary = monthlySalaries.FirstOrDefault(s => s > 0);
        var firstNonZeroWorkedDays = monthlyWorkedDays.FirstOrDefault(d => d > 0);
        var firstNonZeroRndDays = monthlyRnDDays.FirstOrDefault(d => d > 0);

        return new ParityScenario
        {
            TestId = testId,
            Description = description,
            Input = new TestInput
            {
                Year = year,
                CalculationMode = calculationMode,
                EmployeeTypeId = employeeTypeId,
                // Fallback uniform values (required by TestInput)
                SalaryAmount = firstNonZeroSalary,
                WorkedDays = firstNonZeroWorkedDays,
                ResearchAndDevelopmentWorkedDays = firstNonZeroRndDays,
                // Per-month arrays
                MonthlySalaryAmounts = monthlySalaries,
                MonthlyWorkedDays = monthlyWorkedDays,
                MonthlyRnDDays = monthlyRnDDays,
                // Other settings
                DisabilityDegree = disabilityDegree,
                AgiRate = agiRate,
                EducationExemptionRate = educationExemptionRate,
                IsPensioner = isPensioner,
                ApplyEmployerDiscount5746 = applyEmployerDiscount5746,
                IsAgiIncludedNet = isAgiIncludedNet,
                IsAgiIncludedTax = isAgiIncludedTax,
                ApplyMinWageTaxExemption = applyMinWageTaxExemption,
                IsAgiCalculationEnabled = isAgiCalculationEnabled,
            },
        };
    }

    private static EmployeeTypeConstant GetEmployeeType(int id) =>
        ConstantsProvider.GetEmployeeType(id) ?? throw new InvalidOperationException("Employee type " + id + " not found");
}
