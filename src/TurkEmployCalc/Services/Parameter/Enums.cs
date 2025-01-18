#pragma warning disable MA0048 // File name must match type name (Models.cs) - disabled because of the enum types

namespace TurkHRSolutions.TurkEmployCalc.Services.Parameter;

public enum SpouseWorkStatus
{
    Unmarried,
    Working,
    Unemployed,
}

public enum ChildrenCompareType
{
    Eq,
    Gt,
    Gte,
    Lt,
    Lte,
}