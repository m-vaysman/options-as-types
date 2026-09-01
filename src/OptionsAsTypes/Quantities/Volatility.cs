namespace OptionsAsTypes.Quantities;

/// <summary>
/// Annualised volatility (sigma), expressed as a decimal (0.30 = 30%).
/// Black-Scholes measures time in years, so sigma must be annualised to match.
/// </summary>
public readonly struct Volatility
{
    public double Value { get; }

    private Volatility(double value) => Value = value;

    /// <summary>Wrap an already-annualised volatility.</summary>
    public static Volatility Create(double annualisedVolatility)
    {
        if (double.IsNaN(annualisedVolatility) || double.IsInfinity(annualisedVolatility))
            throw new ArgumentException("Volatility must be a real number.", nameof(annualisedVolatility));
        if (annualisedVolatility <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(annualisedVolatility), annualisedVolatility,
                "Volatility must be positive.");
        return new Volatility(annualisedVolatility);
    }

    /// <summary>
    /// Realised volatility from a price series, annualised.
    /// Uses log returns (the diffusion Black-Scholes actually assumes) and scales by
    /// the square root of <paramref name="periodsPerYear"/> — 252 for daily closes,
    /// 52 for weekly, 12 for monthly.
    /// </summary>
    public static Volatility CreateFromPrices(IEnumerable<double> prices, double periodsPerYear = 252.0)
    {
        ArgumentNullException.ThrowIfNull(prices);
        if (periodsPerYear <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(periodsPerYear), periodsPerYear,
                "Periods per year must be positive.");

        double[] series = prices as double[] ?? prices.ToArray();
        if (series.Length < 3)
            throw new ArgumentException("At least three prices are needed to estimate volatility.", nameof(prices));
        if (series.Any(p => p <= 0.0))
            throw new ArgumentException("Prices must be positive to take log returns.", nameof(prices));

        double[] logReturns = new double[series.Length - 1];
        for (int i = 1; i < series.Length; i++)
            logReturns[i - 1] = Math.Log(series[i] / series[i - 1]);

        double mean = logReturns.Average();
        // Sample standard deviation (n-1): we are estimating from a sample, not a population.
        double variance = logReturns.Sum(x => (x - mean) * (x - mean)) / (logReturns.Length - 1);
        double perPeriod = Math.Sqrt(variance);

        return Create(perPeriod * Math.Sqrt(periodsPerYear));
    }

    public static implicit operator double(Volatility x) => x.Value;

    public override string ToString() => Value.ToString("P2");
}
