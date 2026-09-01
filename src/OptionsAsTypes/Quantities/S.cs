namespace OptionsAsTypes.Quantities;

/// <summary>Spot price of the underlying security.</summary>
public readonly struct S
{
    public double Value { get; }

    public S(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Spot price must be a real number.", nameof(value));
        if (value <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Spot price must be positive.");
        Value = value;
    }

    /// <summary>S - K. The amount a call is in the money, floored at zero.</summary>
    public static CallOptionIntrinsicValue operator -(S spot, K strike) =>
        new(spot.Value - strike.Value);

    public static implicit operator double(S x) => x.Value;

    public override string ToString() => Value.ToString("F4");
}
