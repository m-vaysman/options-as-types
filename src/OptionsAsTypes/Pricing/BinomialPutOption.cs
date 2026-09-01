using OptionsAsTypes.Quantities;

namespace OptionsAsTypes.Pricing;

/// <summary>Put priced on a CRR tree. Returns a <see cref="P"/>, never a bare double.</summary>
public sealed class BinomialPutOption : BinomialOption
{
    public BinomialPutOption(S spot, K strike, Time expiry, r riskFreeRate, q dividendYield,
        Volatility sigma, N steps, ExerciseStyle exercise = ExerciseStyle.European)
        : base(spot, strike, expiry, riskFreeRate, dividendYield, sigma, steps, exercise)
    {
    }

    protected override double Payoff(double underlyingPrice) =>
        Math.Max(Strike.Value - underlyingPrice, 0.0);

    public P Price() => new(RollBack());
}
