using Turkish.HRSolutions.SalaryCalculator.Application.Requests;
using Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects;

namespace Turkish.HRSolutions.SalaryCalculator.Application.Builders.Internal;

/// <inheritdoc />
internal sealed class AgiBuilder : IAgiBuilder
{
    private SpouseStatus _spouseStatus = SpouseStatus.Unmarried;
    private int _numberOfChildren;
    private bool _includeInTax = true;

    /// <inheritdoc />
    public IAgiBuilder Unmarried()
    {
        _spouseStatus = SpouseStatus.Unmarried;
        return this;
    }

    /// <inheritdoc />
    public IAgiBuilder SpouseWorking()
    {
        _spouseStatus = SpouseStatus.SpouseWorking;
        return this;
    }

    /// <inheritdoc />
    public IAgiBuilder SpouseNotWorking()
    {
        _spouseStatus = SpouseStatus.SpouseNotWorking;
        return this;
    }

    /// <inheritdoc />
    public IAgiBuilder WithChildren(int count)
    {
        _numberOfChildren = count;
        return this;
    }

    /// <inheritdoc />
    public IAgiBuilder IncludeInTax(bool include = true)
    {
        _includeInTax = include;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="AgiSettings"/> from accumulated configuration.
    /// </summary>
    internal AgiSettings Build() => new(_spouseStatus, _numberOfChildren, _includeInTax);
}
