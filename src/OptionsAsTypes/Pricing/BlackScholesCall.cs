using OptionsAsTypes.Numerics;
using OptionsAsTypes.Quantities;

namespace OptionsAsTypes.Pricing;

/// <summary>
/// European call under Black-Scholes-Merton with a continuous dividend yield:
/// C = S * exp(-qT) * N(d1) - K * exp(-rT) * N(d2)
/// </summary>
public sealed class BlackScholesCall
{
    public S Spot { get; }
    public K Strike { get; }
    public r RiskFreeRate { get; }
    public q DividendYield { get; }
    public Volatility Sigma { get; }
    public Time Expiry { get; }

    public BlackScholesCall(S spot, K strike, r riskFreeRate, q dividendYield, Volatility sigma, Time expiry)
    {
        Spot = spot;
        Strike = strike;
        RiskFreeRate = riskFreeRate;
        DividendYield = dividendYield;
        Sigma = sigma;
        Expiry = expiry;
    }

    public D1 D1 => new(Spot, Strike, RiskFreeRate, DividendYield, Sigma, Expiry);

    public D2 D2 => new(D1, Sigma, Expiry);

    public Probability Nd1 => NormalDistribution.N(D1);

    public Probability Nd2 => NormalDistribution.N(D2);

    /// <summary>max(S - K, 0) — what the option is worth if exercised right now.</summary>
    public CallOptionIntrinsicValue IntrinsicValue => Spot - Strike;

    public Moneyness Moneyness =>
        Math.Abs(Spot.Value - Strike.Value) < 1e-12 ? Moneyness.AtTheMoney
        : Spot.Value > Strike.Value ? Moneyness.InTheMoney
        : Moneyness.OutOfTheMoney;

    public C Price() => new(
        Spot.Value * Math.Exp(-DividendYield.Value * Expiry.Value) * Nd1.Value
        - Strike.Value * Math.Exp(-RiskFreeRate.Value * Expiry.Value) * Nd2.Value);
}
