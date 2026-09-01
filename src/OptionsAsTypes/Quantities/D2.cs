namespace OptionsAsTypes.Quantities;

/// <summary>Black-Scholes d2 = d1 - sigma * sqrt(T). Derivable only from a real d1.</summary>
public readonly struct D2
{
    public double Value { get; }

    public D2(D1 d1, Volatility sigma, Time expiry) =>
        Value = d1.Value - sigma.Value * Math.Sqrt(expiry.Value);

    public static implicit operator double(D2 x) => x.Value;

    public override string ToString() => Value.ToString("F6");
}
