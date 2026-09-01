namespace OptionsAsTypes.Quantities;

/// <summary>
/// Continuous dividend yield, expressed as a decimal (0.02 = 2%).
/// Use <see cref="None"/> for a non-dividend-paying underlying.
/// </summary>
public readonly struct q
{
    public static q None => new(0.0);

    public double Value { get; }

    public q(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Dividend yield must be a real number.", nameof(value));
        if (value < 0.0 || value >= 1.0)
            throw new ArgumentOutOfRangeException(nameof(value), value,
                "Dividend yield is a decimal in [0, 1): 2% is 0.02, not 2.");
        Value = value;
    }

    public static implicit operator double(q x) => x.Value;

    public override string ToString() => Value.ToString("P2");
}
