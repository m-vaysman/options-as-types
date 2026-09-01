namespace OptionsAsTypes.Quantities;

/// <summary>Put option price.</summary>
public readonly struct P
{
    public double Value { get; }

    public P(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Put price must be a real number.", nameof(value));
        if (value < -1e-9)
            throw new ArgumentOutOfRangeException(nameof(value), value, "A put price cannot be negative.");
        Value = Math.Max(0.0, value);
    }

    public static implicit operator double(P x) => x.Value;

    public override string ToString() => Value.ToString("F4");
}
