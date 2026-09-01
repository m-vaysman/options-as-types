namespace OptionsAsTypes.Quantities;

/// <summary>
/// Continuously compounded risk-free rate, expressed as a decimal (0.05 = 5%).
/// Negative rates are permitted: EUR, JPY and CHF have all traded below zero.
/// </summary>
public readonly struct r
{
    public double Value { get; }

    public r(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Rate must be a real number.", nameof(value));
        if (value <= -1.0 || value >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(value), value,
                "Rate is a decimal, not a percentage: 5% is 0.05, not 5.");
        Value = value;
    }

    public static implicit operator double(r x) => x.Value;

    public override string ToString() => Value.ToString("P2");
}
