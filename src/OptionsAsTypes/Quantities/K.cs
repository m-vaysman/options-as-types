namespace OptionsAsTypes.Quantities;

/// <summary>Strike price.</summary>
public readonly struct K
{
    public double Value { get; }

    public K(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Strike must be a real number.", nameof(value));
        if (value <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Strike must be positive.");
        Value = value;
    }

    /// <summary>K - S. The amount a put is in the money, floored at zero.</summary>
    public static PutOptionIntrinsicValue operator -(K strike, S spot) =>
        new(strike.Value - spot.Value);

    public static implicit operator double(K x) => x.Value;

    public override string ToString() => Value.ToString("F4");
}
