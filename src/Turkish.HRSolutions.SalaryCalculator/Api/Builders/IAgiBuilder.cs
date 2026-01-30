namespace Turkish.HRSolutions.SalaryCalculator.Api.Builders;

/// <summary>
/// Fluent builder for AGI (Minimum Living Allowance) settings.
/// </summary>
/// <remarks>
/// AGI was replaced by the minimum wage income tax exemption starting from 2022.
/// These settings are only relevant for calculations before 2022.
/// </remarks>
public interface IAgiBuilder
{
    /// <summary>Employee is unmarried.</summary>
    public IAgiBuilder Unmarried();

    /// <summary>Employee's spouse is working.</summary>
    public IAgiBuilder SpouseWorking();

    /// <summary>Employee's spouse is not working.</summary>
    public IAgiBuilder SpouseNotWorking();

    /// <summary>Number of children for AGI calculation.</summary>
    public IAgiBuilder WithChildren(int count);

    /// <summary>Include AGI in tax calculations.</summary>
    public IAgiBuilder IncludeInTax(bool include = true);

    /// <summary>Include AGI in net salary (for Net-to-Gross binary search target).</summary>
    public IAgiBuilder IncludeInNet(bool include = true);
}
