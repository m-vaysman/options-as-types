namespace OptionsAsTypes.Quantities;

/// <summary>Time to expiry (T), in years.</summary>
public readonly struct Time
{
    public double Value { get; }

    public Time(double years)
    {
        if (double.IsNaN(years) || double.IsInfinity(years))
            throw new ArgumentException("Time to expiry must be a real number.", nameof(years));
        if (years <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(years), years,
                "Time to expiry must be positive. An expired option has no time value to price.");
        Value = years;
    }

    /// <summary>Calendar days expressed in years, on a 365-day basis.</summary>
    public static Time FromDays(double days) => new(days / 365.0);

    public static Time operator +(Time left, Time right) => new(left.Value + right.Value);

    /// <summary>T / N is, by definition, the width of one binomial step.</summary>
    public static dt operator /(Time total, N steps) => new(total.Value / steps.Value);

    public static implicit operator double(Time x) => x.Value;

    public override string ToString() => $"{Value:F4}y";
}
