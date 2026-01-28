using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Api.Requests;

/// <summary>
/// R&amp;D (Law 5746) exemption settings for education-based income tax exemption.
/// </summary>
/// <remarks>
/// <para>
/// This setting only applies to employees with R&amp;D 5746 employee types:
/// <see cref="EmployeeTypeId.RnD5746"/> and <see cref="EmployeeTypeId.EmployerRnD5746"/>.
/// For other employee types, these settings will generate a warning and be ignored.
/// </para>
/// <para>
/// The education level affects the employer's income tax exemption rate:
/// <list type="bullet">
///   <item><see cref="EducationTypeId.Doctorate"/>: Highest exemption rate</item>
///   <item><see cref="EducationTypeId.MastersOrFundamentalSciences"/>: Medium exemption rate</item>
///   <item><see cref="EducationTypeId.OtherRnDPersonnel"/>: Base exemption rate</item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Education">The education level of the R&amp;D employee.</param>
public sealed record RnDSettings(EducationTypeId Education)
{
    /// <summary>
    /// Default R&amp;D settings for other R&amp;D personnel (non-doctorate, non-masters).
    /// </summary>
    public static RnDSettings Default { get; } = new(EducationTypeId.OtherRnDPersonnel);

    /// <summary>
    /// Creates R&amp;D settings for a doctorate degree holder.
    /// </summary>
    public static RnDSettings Doctorate() => new(EducationTypeId.Doctorate);

    /// <summary>
    /// Creates R&amp;D settings for a masters degree holder or bachelor's in fundamental sciences.
    /// </summary>
    public static RnDSettings MastersOrFundamentalSciences() => new(EducationTypeId.MastersOrFundamentalSciences);

    /// <summary>
    /// Creates R&amp;D settings for other R&amp;D personnel.
    /// </summary>
    public static RnDSettings Other() => new(EducationTypeId.OtherRnDPersonnel);
}
