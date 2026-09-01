namespace OptionsAsTypes.Quantities;

/// <summary>
/// max(K - S, 0). Note this hangs off K, not off P: intrinsic value is strike less spot,
/// never "put price less spot". The original sketch had `operator -(P, S)`, which is not
/// a quantity that means anything.
/// </summary>
public readonly struct PutOptionIntrinsicValue
{
    public double Value { get; }

    public PutOptionIntrinsicValue(double strikeLessSpot) => Value = Math.Max(0.0, strikeLessSpot);

    public static implicit operator double(PutOptionIntrinsicValue x) => x.Value;

    public override string ToString() => Value.ToString("F4");
}
