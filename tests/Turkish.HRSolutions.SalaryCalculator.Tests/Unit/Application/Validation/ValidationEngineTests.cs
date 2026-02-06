#pragma warning disable CA1707

using Turkish.HRSolutions.SalaryCalculator.Application.Providers;
using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Application.Validation;
using Turkish.HRSolutions.SalaryCalculator.Common.Results;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Identifiers;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Providers;
using Turkish.HRSolutions.SalaryCalculator.Infrastructure.Services;

namespace Turkish.HRSolutions.SalaryCalculator.Tests.Unit.Application.Validation;

/// <summary>
/// Tests for <see cref="ValidationEngine"/> capability rules and input validation.
/// </summary>
public sealed class ValidationEngineTests
{
    private static IYearParameterProvider YearProvider { get; } = new EmbeddedYearParameterProvider();
    private static ICalculationConstantsProvider ConstantsProvider { get; } = new EmbeddedCalculationConstantsProvider();

    private static CapabilityResolver CapabilityResolver { get; } = new(YearProvider, ConstantsProvider);

    private static ValidationEngine Engine { get; } = new(CapabilityResolver, YearProvider, ConstantsProvider);

    [Test]
    public async Task Validate_Should_FailWithYearNotSpecified_When_YearIsZero()
    {
        var context = CreateValidContext() with
        {
            Year = 0
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Code == ErrorCode.YearNotSpecified)).IsTrue();
    }

    [Test]
    public async Task Validate_Should_FailWithYearNotSupported_When_YearDoesNotExist()
    {
        var context = CreateValidContext() with
        {
            Year = 1900
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Code == ErrorCode.YearNotSupported)).IsTrue();
    }

    [Test]
    public async Task Validate_Should_FailWithMissingEmployeeType_When_EmployeeTypeIsZero()
    {
        var context = CreateValidContext() with
        {
            EmployeeType = EmployeeTypeId.FromId(0)
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Code == ErrorCode.InvalidEmployeeType)).IsTrue();
    }

    [Test]
    public async Task Validate_Should_FailWithInvalidMonthCount_When_NoMonthsProvided()
    {
        var context = CreateValidContext() with
        {
            Months = []
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Code == ErrorCode.InvalidMonthCount)).IsTrue();
    }

    [Test]
    public async Task Validate_Should_FailWithInvalidMonthCount_When_NotTwelveMonths()
    {
        var context = CreateValidContext() with
        {
            Months =
            [
                .. MonthsOfYear.AllMonths.Take(6).Select(m => new MonthlyInput(m, 30_000m)),
            ],
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Code == ErrorCode.InvalidMonthCount)).IsTrue();
    }

    [Test]
    public async Task Validate_Should_FailWithInvalidSalaryAmount_When_SalaryIsNegative()
    {
        // Negative salary should be rejected
        var months = MonthsOfYear.AllMonths
            .Select((m, i) => new MonthlyInput(m, i == 0 ? -1000m : 30_000m))
            .ToList();
        var context = CreateValidContext() with
        {
            Months = months
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Code == ErrorCode.InvalidSalaryAmount)).IsTrue();
    }

    [Test]
    public async Task Validate_Should_AllowZeroSalary_For_NewHireScenarios()
    {
        // Zero salary is allowed for months where employee didn't work (new hire scenarios)
        // This aligns with Angular behavior
        var months = MonthsOfYear.AllMonths
            .Select((m, i) => new MonthlyInput(m, i < 2 ? 0m : 30_000m, i < 2 ? 0 : 30))
            .ToList();
        var context = CreateValidContext() with
        {
            Months = months
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    public async Task Validate_Should_FailWithInvalidWorkedDays_When_WorkedDaysExceedsThirty()
    {
        var months = MonthsOfYear.AllMonths
            .Select((m, i) => new MonthlyInput(m, 30_000m, i == 0 ? 31 : 30))
            .ToList();
        var context = CreateValidContext() with
        {
            Months = months
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Code == ErrorCode.InvalidWorkedDays)).IsTrue();
    }

    [Test]
    public async Task Validate_Should_FailWithRnDDaysExceedWorkedDays_When_RnDDaysGreaterThanWorkedDays()
    {
        var months = MonthsOfYear.AllMonths
            .Select((m, i) => new MonthlyInput(m, 30_000m, i == 0 ? 15 : 30, i == 0 ? 20 : 0))
            .ToList();
        var context = CreateValidContext() with
        {
            Months = months
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Errors.Any(e => e.Code == ErrorCode.RnDDaysExceedWorkedDays)).IsTrue();
    }

    [Test]
    public async Task Validate_Should_Succeed_When_AllInputsAreValid()
    {
        var context = CreateValidContext();

        var result = Engine.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
    }

    [Test]
    [MethodDataSource(nameof(AgiSelectionDisabledScenarios))]
    public async Task Validate_Should_DisableAgiSelection_When_EmployeeTypeDoesNotSupportAgiOrYearHasMinWageExemption(
        int year, int employeeTypeId, bool expectedDisabled)
    {
        var context = CreateValidContext() with
        {
            Year = year,
            EmployeeType = EmployeeTypeId.FromId(employeeTypeId),
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
        var isDisabled = !result.Value.IsAvailable(Capability.AgiSelection);
        await Assert.That(isDisabled).IsEqualTo(expectedDisabled);
    }

    public static IEnumerable<Func<(int Year, int EmployeeTypeId, bool ExpectedDisabled)>> AgiSelectionDisabledScenarios()
    {
        // Standard employee (AGI applicable), pre-2022 (no min wage exemption) = AGI enabled
        yield return () => (2021, 1, false);
        // Standard employee, 2022+ (min wage exemption) = AGI disabled
        yield return () => (2024, 1, true);
        // Employer type (also AGI applicable), 2021 = AGI enabled
        yield return () => (2021, 5, false);
        // Employer type, 2024 (min wage exemption) = AGI disabled
        yield return () => (2024, 5, true);
    }

    [Test]
    [MethodDataSource(nameof(EducationTypeDisabledScenarios))]
    public async Task Validate_Should_DisableEducationType_When_EmployeeTypeDoesNotSupportEducationExemption(
        int employeeTypeId, bool expectedDisabled)
    {
        var context = CreateValidContext() with
        {
            EmployeeType = EmployeeTypeId.FromId(employeeTypeId),
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
        var isDisabled = !result.Value.IsAvailable(Capability.EducationType);
        await Assert.That(isDisabled).IsEqualTo(expectedDisabled);
    }

    public static IEnumerable<Func<(int EmployeeTypeId, bool ExpectedDisabled)>> EducationTypeDisabledScenarios()
    {
        // Standard employee = Education disabled
        yield return () => (1, true);
        // Teknokent 4691 = Education disabled (does NOT have education exemption)
        yield return () => (2, true);
        // 5746 R&D = Education enabled (has education exemption)
        yield return () => (3, false);
    }

    [Test]
    [MethodDataSource(nameof(Discount5746DisabledScenarios))]
    public async Task Validate_Should_Disable5746Discount_When_PensionerOrEmployeeTypeDoesNotSupport(
        int employeeTypeId, bool isPensioner, bool expectedDisabled)
    {
        var context = CreateValidContext() with
        {
            EmployeeType = EmployeeTypeId.FromId(employeeTypeId),
            IsPensioner = isPensioner,
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
        var isDisabled = !result.Value.IsAvailable(Capability.Discount5746);
        await Assert.That(isDisabled).IsEqualTo(expectedDisabled);
    }

    public static IEnumerable<Func<(int EmployeeTypeId, bool IsPensioner, bool ExpectedDisabled)>> Discount5746DisabledScenarios()
    {
        // Standard employee, not pensioner, supports 5746 = enabled
        yield return () => (1, false, false);
        // Standard employee, pensioner = disabled
        yield return () => (1, true, true);
        // Employer type (doesn't support 5746) = disabled
        yield return () => (5, false, true);
    }

    [Test]
    [MethodDataSource(nameof(RnDDaysDisabledScenarios))]
    public async Task Validate_Should_DisableRnDDaysInput_When_EmployeeTypeDoesNotSupportRnD(
        int employeeTypeId, bool expectedDisabled)
    {
        var context = CreateValidContext() with
        {
            EmployeeType = EmployeeTypeId.FromId(employeeTypeId),
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
        var isDisabled = !result.Value.IsAvailable(Capability.RnDDaysInput);
        await Assert.That(isDisabled).IsEqualTo(expectedDisabled);
    }

    public static IEnumerable<Func<(int EmployeeTypeId, bool ExpectedDisabled)>> RnDDaysDisabledScenarios()
    {
        // Standard employee = R&D disabled
        yield return () => (1, true);
        // Teknokent 4691 = R&D enabled
        yield return () => (2, false);
        // 5746 R&D = R&D enabled
        yield return () => (3, false);
        // Employer = R&D disabled
        yield return () => (5, true);
    }

    [Test]
    [MethodDataSource(nameof(AgiIncludedInNetDisabledScenarios))]
    public async Task Validate_Should_DisableAgiIncludedInNet_When_ModeIsNotNetToGross(
        CalculationMode mode, bool expectedDisabled)
    {
        var context = CreateValidContext() with
        {
            Mode = mode
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
        var isDisabled = !result.Value.IsAvailable(Capability.AgiIncludedInNet);
        await Assert.That(isDisabled).IsEqualTo(expectedDisabled);
    }

    public static IEnumerable<Func<(CalculationMode Mode, bool ExpectedDisabled)>> AgiIncludedInNetDisabledScenarios()
    {
        yield return () => (CalculationMode.GrossToNet, true);
        yield return () => (CalculationMode.NetToGross, false);
        yield return () => (CalculationMode.TotalToGross, true);
    }

    [Test]
    public async Task Validate_Should_AddWarning_When_AgiSettingsProvidedButAgiDisabled()
    {
        var context = CreateValidContext() with
        {
            Year = 2024, // Min wage exemption year = AGI disabled
            Agi = new AgiSettings(SpouseStatus.SpouseNotWorking, 2),
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Warnings.Any(w => w.Code == ErrorCode.AgiNotApplicable)).IsTrue();
    }

    [Test]
    public async Task Validate_Should_AddWarning_When_5746DiscountRequestedButDisabled()
    {
        var context = CreateValidContext() with
        {
            IsPensioner = true, // Pensioner = 5746 disabled
            Apply5746Discount = true,
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Warnings.Any(w => w.Code == ErrorCode.Discount5746NotApplicable)).IsTrue();
    }

    [Test]
    public async Task Validate_Should_AddWarning_When_RnDDaysProvidedButRnDDisabled()
    {
        var months = MonthsOfYear.AllMonths
            .Select(m => new MonthlyInput(m, 30_000m, 30, 15))
            .ToList();
        var context = CreateValidContext() with
        {
            EmployeeType = EmployeeTypeId.Standard, // Standard = R&D disabled
            Months = months,
        };

        var result = Engine.Validate(context);

        await Assert.That(result.IsSuccess).IsTrue();
        await Assert.That(result.Warnings.Any(w => w.Code == ErrorCode.RnDDaysNotApplicable)).IsTrue();
    }

    private static ValidationContext CreateValidContext() => new()
    {
        Year = 2024,
        Mode = CalculationMode.GrossToNet,
        EmployeeType = EmployeeTypeId.Standard,
        Disability = DisabilityDegreeId.None,
        IsPensioner = false,
        ApplyMinWageExemption = true,
        Apply5746Discount = false,
        Months = [.. MonthsOfYear.AllMonths.Select(m => new MonthlyInput(m, 30_000m))],
    };
}
