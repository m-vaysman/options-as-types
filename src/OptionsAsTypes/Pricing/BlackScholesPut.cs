using OptionsAsTypes.Numerics;
using OptionsAsTypes.Quantities;

namespace OptionsAsTypes.Pricing;

/// <summary>
/// European put under Black-Scholes-Merton with a continuous dividend yield:
/// P = K * exp(-rT) * N(-d2) - S * exp(-qT) * N(-d1)
/// </summary>
public sealed class BlackScholesPut
{
    public S Spot { get; }
    public K Strike { get; }
    public r RiskFreeRate { get; }
    public q DividendYield { get; }
    public Volatility Sigma { get; }
    public Time Expiry { get; }

    public BlackScholesPut(S spot, K strike, r riskFreeRate, q dividendYield, Volatility sigma, Time expiry)
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

    public Probability NMinusD1 => NormalDistribution.N(-D1.Value);

    public Probability NMinusD2 => NormalDistribution.N(-D2.Value);

    /// <summary>max(K - S, 0).</summary>
    public PutOptionIntrinsicValue IntrinsicValue => Strike - Spot;

    public Moneyness Moneyness =>
        Math.Abs(Spot.Value - Strike.Value) < 1e-12 ? Moneyness.AtTheMoney
        : Spot.Value < Strike.Value ? Moneyness.InTheMoney
        : Moneyness.OutOfTheMoney;

    public P Price() => new(
        Strike.Value * Math.Exp(-RiskFreeRate.Value * Expiry.Value) * NMinusD2.Value
        - Spot.Value * Math.Exp(-DividendYield.Value * Expiry.Value) * NMinusD1.Value);
}
