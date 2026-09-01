using OptionsAsTypes.Quantities;

namespace OptionsAsTypes.Pricing;

/// <summary>
/// Cox-Ross-Rubinstein recombining tree. Every parameter below is derived, not supplied:
/// dt comes from T / N, u from sigma and dt, d from u, p from r, q, dt, u and d. Get the
/// inputs into the constructor and the model builds itself.
/// </summary>
public abstract class BinomialOption
{
    public S Spot { get; }
    public K Strike { get; }
    public Time Expiry { get; }
    public r RiskFreeRate { get; }
    public q DividendYield { get; }
    public Volatility Sigma { get; }
    public N Steps { get; }
    public ExerciseStyle Exercise { get; }

    public dt StepWidth { get; }
    public u UpMove { get; }
    public d DownMove { get; }
    public pbUpMovement UpProbability { get; }
    public pbDownMovement DownProbability { get; }
    public DiscountedOnePeriodRate OnePeriodDiscount { get; }

    protected BinomialOption(S spot, K strike, Time expiry, r riskFreeRate, q dividendYield,
        Volatility sigma, N steps, ExerciseStyle exercise)
    {
        Spot = spot;
        Strike = strike;
        Expiry = expiry;
        RiskFreeRate = riskFreeRate;
        DividendYield = dividendYield;
        Sigma = sigma;
        Steps = steps;
        Exercise = exercise;

        StepWidth = expiry / steps;
        UpMove = new u(sigma, StepWidth);
        DownMove = new d(UpMove);
        UpProbability = new pbUpMovement(riskFreeRate, dividendYield, StepWidth, UpMove, DownMove);
        DownProbability = new pbDownMovement(UpProbability);
        OnePeriodDiscount = new DiscountedOnePeriodRate(riskFreeRate, StepWidth);
    }

    /// <summary>Payoff of this option at a given underlying price.</summary>
    protected abstract double Payoff(double underlyingPrice);

    /// <summary>Underlying price after <paramref name="step"/> steps, of which upMoves were up.</summary>
    public double AssetPriceAt(int step, int upMoves) =>
        Spot.Value * Math.Pow(UpMove.Value, upMoves) * Math.Pow(DownMove.Value, step - upMoves);

    /// <summary>
    /// The N+1 terminal underlying prices, lowest first. An N-step tree has N+1 terminal
    /// nodes — the original sketch looped to N and silently dropped the top node.
    /// </summary>
    public double[] TerminalAssetPrices()
    {
        var prices = new double[Steps.TerminalNodeCount];
        for (int j = 0; j < prices.Length; j++)
            prices[j] = AssetPriceAt(Steps.Value, j);
        return prices;
    }

    /// <summary>Payoff at each of the N+1 terminal nodes.</summary>
    public double[] TerminalPayoffs() => TerminalAssetPrices().Select(Payoff).ToArray();

    /// <summary>Backward induction through the tree to today's value.</summary>
    protected double RollBack()
    {
        int n = Steps.Value;
        double pUp = UpProbability.Value;
        double pDown = DownProbability.Value;
        double df = OnePeriodDiscount.Value;

        double[] values = TerminalPayoffs();

        for (int step = n - 1; step >= 0; step--)
        {
            for (int j = 0; j <= step; j++)
            {
                double continuation = df * (pUp * values[j + 1] + pDown * values[j]);

                values[j] = Exercise == ExerciseStyle.American
                    ? Math.Max(continuation, Payoff(AssetPriceAt(step, j)))
                    : continuation;
            }
        }

        return values[0];
    }
}
