using OptionsAsTypes.Pricing;
using OptionsAsTypes.Quantities;
using Xunit;

namespace OptionsAsTypes.Tests;

public class BinomialTests
{
    private static readonly S Spot = new(52.0);
    private static readonly K Strike = new(50.0);
    private static readonly r Rate = new(0.02);
    private static readonly Volatility Sigma = Volatility.Create(0.30);
    private static readonly Time Expiry = new(1.0);

    [Fact]
    public void EuropeanCallConvergesToBlackScholes()
    {
        double closedForm = new BlackScholesCall(Spot, Strike, Rate, q.None, Sigma, Expiry).Price();
        C tree = new BinomialCallOption(Spot, Strike, Expiry, Rate, q.None, Sigma, new N(5000)).Price();

        Assert.Equal(closedForm, tree.Value, 2);
    }

    [Fact]
    public void EuropeanPutConvergesToBlackScholes()
    {
        double closedForm = new BlackScholesPut(Spot, Strike, Rate, q.None, Sigma, Expiry).Price();
        P tree = new BinomialPutOption(Spot, Strike, Expiry, Rate, q.None, Sigma, new N(5000)).Price();

        Assert.Equal(closedForm, tree.Value, 2);
    }

    [Fact]
    public void ConvergenceImprovesWithMoreSteps()
    {
        double closedForm = new BlackScholesCall(Spot, Strike, Rate, q.None, Sigma, Expiry).Price();

        double coarse = Math.Abs(
            new BinomialCallOption(Spot, Strike, Expiry, Rate, q.None, Sigma, new N(10)).Price() - closedForm);
        double fine = Math.Abs(
            new BinomialCallOption(Spot, Strike, Expiry, Rate, q.None, Sigma, new N(2000)).Price() - closedForm);

        Assert.True(fine < coarse, $"expected {fine} < {coarse}");
    }

    [Fact]
    public void TreeHasNPlusOneTerminalNodes()
    {
        var option = new BinomialCallOption(Spot, Strike, Expiry, Rate, q.None, Sigma, new N(3));

        // The bug this replaces: looping to N produced 3 terminal prices, dropping S*u^N.
        Assert.Equal(4, option.TerminalAssetPrices().Length);
        Assert.Equal(4, option.TerminalPayoffs().Length);
    }

    [Fact]
    public void TerminalPricesRunFromAllDownToAllUp()
    {
        var option = new BinomialCallOption(Spot, Strike, Expiry, Rate, q.None, Sigma, new N(4));
        double[] prices = option.TerminalAssetPrices();

        Assert.Equal(Spot.Value * Math.Pow(option.DownMove.Value, 4), prices[0], 10);
        Assert.Equal(Spot.Value * Math.Pow(option.UpMove.Value, 4), prices[^1], 10);
        Assert.True(prices.Zip(prices.Skip(1)).All(pair => pair.First < pair.Second));
    }

    [Fact]
    public void AmericanPutIsWorthAtLeastTheEuropeanPut()
    {
        var strike = new K(60.0);
        var steps = new N(500);

        P european = new BinomialPutOption(Spot, strike, Expiry, Rate, q.None, Sigma, steps).Price();
        P american = new BinomialPutOption(Spot, strike, Expiry, Rate, q.None, Sigma, steps,
            ExerciseStyle.American).Price();

        Assert.True(american.Value > european.Value, $"{american} should exceed {european}");
    }

    [Fact]
    public void AmericanCallOnNonDividendPayerEqualsEuropean()
    {
        // Merton: it is never optimal to exercise an American call early on a
        // non-dividend-paying underlying, so the early exercise premium must be zero.
        var steps = new N(500);

        C european = new BinomialCallOption(Spot, Strike, Expiry, Rate, q.None, Sigma, steps).Price();
        C american = new BinomialCallOption(Spot, Strike, Expiry, Rate, q.None, Sigma, steps,
            ExerciseStyle.American).Price();

        Assert.Equal(european.Value, american.Value, 10);
    }

    [Fact]
    public void CoxRossRubinsteinTreeRecombines()
    {
        var option = new BinomialCallOption(Spot, Strike, Expiry, Rate, q.None, Sigma, new N(50));

        // u * d == 1 is what makes an up-then-down move land back on the start price.
        Assert.Equal(1.0, option.UpMove.Value * option.DownMove.Value, 12);
    }

    [Fact]
    public void RiskNeutralProbabilitiesSumToOne()
    {
        var option = new BinomialCallOption(Spot, Strike, Expiry, Rate, q.None, Sigma, new N(50));

        Assert.Equal(1.0, option.UpProbability.Value + option.DownProbability.Value, 12);
        Assert.InRange(option.UpProbability.Value, 0.0, 1.0);
    }

    [Fact]
    public void ArbitrageableTreeIsRejected()
    {
        // d < exp(r*dt) < u must hold. A single step over a 400-year horizon breaks it.
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new BinomialCallOption(Spot, Strike, new Time(400.0), new r(0.05), q.None, Sigma, new N(1)));
    }
}
