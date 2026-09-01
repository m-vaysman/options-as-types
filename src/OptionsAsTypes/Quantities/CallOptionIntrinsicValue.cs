namespace OptionsAsTypes.Quantities;

/// <summary>
/// max(S - K, 0). Intrinsic value is floored at zero by definition: nobody exercises
/// a call into a loss. The original sketch let this go negative, which quietly made
/// "intrinsic value" mean "S minus K" instead.
/// </summary>
public readonly struct CallOptionIntrinsicValue
{
    public double Value { get; }

    public CallOptionIntrinsicValue(double spotLessStrike) => Value = Math.Max(0.0, spotLessStrike);

    public static implicit operator double(CallOptionIntrinsicValue x) => x.Value;

    public override string ToString() => Value.ToString("F4");
}
