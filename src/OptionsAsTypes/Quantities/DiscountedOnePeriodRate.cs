namespace OptionsAsTypes.Quantities;

/// <summary>One-period discount factor: exp(-r * dt).</summary>
public readonly struct DiscountedOnePeriodRate
{
    public double Value { get; }

    public DiscountedOnePeriodRate(r riskFreeRate, dt step) =>
        Value = Math.Exp(-riskFreeRate.Value * step.Value);

    public static implicit operator double(DiscountedOnePeriodRate x) => x.Value;

    public override string ToString() => Value.ToString("F6");
}
