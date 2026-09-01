using OptionsAsTypes.Quantities;

namespace OptionsAsTypes.Pricing;

/// <summary>Call priced on a CRR tree. Returns a <see cref="C"/>, never a bare double.</summary>
public sealed class BinomialCallOption : BinomialOption
{
    public BinomialCallOption(S spot, K strike, Time expiry, r riskFreeRate, q dividendYield,
        Volatility sigma, N steps, ExerciseStyle exercise = ExerciseStyle.European)
        : base(spot, strike, expiry, riskFreeRate, dividendYield, sigma, steps, exercise)
    {
    }

    protected override double Payoff(double underlyingPrice) =>
        Math.Max(underlyingPrice - Strike.Value, 0.0);

    public C Price() => new(RollBack());
}
