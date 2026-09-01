namespace OptionsAsTypes.Quantities;

/// <summary>
/// Risk-neutral probability of an up-move: p = (exp((r - q) * dt) - d) / (u - d).
/// </summary>
public readonly struct pbUpMovement
{
    public double Value { get; }

    public pbUpMovement(r riskFreeRate, q dividendYield, dt step, u upMove, d downMove)
    {
        double growth = Math.Exp((riskFreeRate.Value - dividendYield.Value) * step.Value);
        double p = (growth - downMove.Value) / (upMove.Value - downMove.Value);

        // d < exp((r-q)dt) < u is the no-arbitrage condition for the tree. Violate it and
        // p falls outside [0,1], which is not a probability and not a price.
        if (p < 0.0 || p > 1.0)
            throw new ArgumentOutOfRangeException(nameof(step), p,
                $"Risk-neutral probability {p:F6} is outside [0, 1]: the tree admits arbitrage. " +
                "This usually means dt is too large for the given volatility — increase the step count.");

        Value = p;
    }

    public static implicit operator double(pbUpMovement x) => x.Value;

    public override string ToString() => Value.ToString("F6");
}
