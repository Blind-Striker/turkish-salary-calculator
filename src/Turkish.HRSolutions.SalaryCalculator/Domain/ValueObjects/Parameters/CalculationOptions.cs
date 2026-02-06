namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Parameters;

internal record CalculationOptions(
    bool ApplyMinWageTaxExemption = true,
    bool ApplyEmployerDiscount5746 = true,
    bool IsAgiCalculationEnabled = false,
    bool IsAgiIncludedTax = false,
    bool IsAgiIncludedNet = false);
