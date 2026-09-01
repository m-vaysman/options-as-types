namespace OptionsAsTypes.Quantities;

/// <summary>Call option price.</summary>
public readonly struct C
{
    public double Value { get; }

    public C(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Call price must be a real number.", nameof(value));
        if (value < -1e-9)
            throw new ArgumentOutOfRangeException(nameof(value), value, "A call price cannot be negative.");
        Value = Math.Max(0.0, value);
    }

    public static implicit operator double(C x) => x.Value;

    public override string ToString() => Value.ToString("F4");
}
