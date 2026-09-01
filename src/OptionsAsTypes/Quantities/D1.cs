namespace OptionsAsTypes.Quantities;

/// <summary>
/// Black-Scholes d1 = (ln(S/K) + (r - q + sigma^2 / 2) * T) / (sigma * sqrt(T)).
///
/// This type exists because of a bug. In the original sketch, CalculateD1() ended with
/// `return new C(price)` — d1, a standardised log-moneyness, wrapped in the type meaning
/// "call option price". It compiled, because every quantity had an implicit conversion to
/// double. A quantity with no type of its own has nowhere to be wrong.
/// </summary>
public readonly struct D1
{
    public double Value { get; }

    public D1(S spot, K strike, r riskFreeRate, q dividendYield, Volatility sigma, Time expiry)
    {
        double numerator = Math.Log(spot.Value / strike.Value)
                           + (riskFreeRate.Value - dividendYield.Value + 0.5 * sigma.Value * sigma.Value) * expiry.Value;
        Value = numerator / (sigma.Value * Math.Sqrt(expiry.Value));
    }

    public static implicit operator double(D1 x) => x.Value;

    public override string ToString() => Value.ToString("F6");
}
