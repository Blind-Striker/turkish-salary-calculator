namespace Turkish.HRSolutions.SalaryCalculator.Domain.ValueObjects.Enums;

public readonly struct MonthsOfYear : IEquatable<MonthsOfYear>
{
    public static readonly MonthsOfYear January = new("January", 1, "Winter");
    public static readonly MonthsOfYear February = new("February", 2, "Winter");
    public static readonly MonthsOfYear March = new("March", 3, "Spring");
    public static readonly MonthsOfYear April = new("April", 4, "Spring");
    public static readonly MonthsOfYear May = new("May", 5, "Spring");
    public static readonly MonthsOfYear June = new("June", 6, "Summer");
    public static readonly MonthsOfYear July = new("July", 7, "Summer");
    public static readonly MonthsOfYear August = new("August", 8, "Summer");
    public static readonly MonthsOfYear September = new("September", 9, "Autumn");
    public static readonly MonthsOfYear October = new("October", 10, "Autumn");
    public static readonly MonthsOfYear November = new("November", 11, "Autumn");
    public static readonly MonthsOfYear December = new("December", 12, "Winter");

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name { get; }

    /// <summary>
    /// Gets the index.
    /// </summary>
    /// <value>The index.</value>
    public short Number { get; }

    /// <summary>
    /// Gets the season.
    /// </summary>
    /// <value>The season.</value>
    public string Season { get; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="MonthsOfYear"/> is default.
    /// </summary>
    /// <value><c>true</c> if is default; otherwise, <c>false</c>.</value>
    public readonly bool IsDefault => Name == null && Number == 0 && string.Equals(Season, b: null, StringComparison.Ordinal);

    /// <summary>
    /// Initializes a new instance of the <see cref="MonthsOfYear"/> struct.
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="number">Index.</param>
    /// <param name="season">Season.</param>
    private MonthsOfYear(string name, short number, string season)
    {
        Name = name;
        Number = number;
        Season = season;
    }

    public static bool operator ==(MonthsOfYear left, MonthsOfYear right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(MonthsOfYear left, MonthsOfYear right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        return obj is MonthsOfYear other && Equals(other);
    }

    public bool Equals(MonthsOfYear other)
    {
        return string.Equals(Name, other.Name, StringComparison.Ordinal)
               && Number == other.Number
               && string.Equals(Season, other.Season, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Number, Season);
    }
}
