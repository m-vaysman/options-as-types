using OptionsAsTypes.Quantities;
using Xunit;

namespace OptionsAsTypes.Tests;

public class VolatilityTests
{
    private static double[] Series()
    {
        var prices = new List<double> { 100.0 };
        double[] shocks = { 0.012, -0.008, 0.015, -0.011, 0.006, 0.009, -0.014, 0.004, -0.003, 0.010 };
        foreach (double shock in shocks)
            prices.Add(prices[^1] * (1.0 + shock));
        return prices.ToArray();
    }

    [Fact]
    public void AnnualisingScalesByTheSquareRootOfPeriodsPerYear()
    {
        double[] prices = Series();

        Volatility perPeriod = Volatility.CreateFromPrices(prices, periodsPerYear: 1.0);
        Volatility annual = Volatility.CreateFromPrices(prices, periodsPerYear: 252.0);

        // The original returned the per-period number and fed it to a model measuring
        // time in years, understating sigma by a factor of sqrt(252).
        Assert.Equal(perPeriod.Value * Math.Sqrt(252.0), annual.Value, 12);
    }

    [Fact]
    public void UsesLogReturnsNotSimpleReturns()
    {
        double[] prices = Series();

        double[] logReturns = prices.Skip(1)
            .Zip(prices, (current, previous) => Math.Log(current / previous))
            .ToArray();

        double mean = logReturns.Average();
        double expected = Math.Sqrt(logReturns.Sum(x => (x - mean) * (x - mean)) / (logReturns.Length - 1));

        Assert.Equal(expected, Volatility.CreateFromPrices(prices, periodsPerYear: 1.0).Value, 12);
    }

    [Fact]
    public void ConstantPriceSeriesHasNoVolatilityAndIsRejected()
    {
        double[] flat = { 100.0, 100.0, 100.0, 100.0 };

        Assert.Throws<ArgumentOutOfRangeException>(() => Volatility.CreateFromPrices(flat));
    }

    [Fact]
    public void NonPositivePricesAreRejected()
    {
        double[] prices = { 100.0, 0.0, 101.0 };

        Assert.Throws<ArgumentException>(() => Volatility.CreateFromPrices(prices));
    }

    [Fact]
    public void TooFewPricesAreRejected()
    {
        Assert.Throws<ArgumentException>(() => Volatility.CreateFromPrices(new[] { 100.0, 101.0 }));
    }
}
