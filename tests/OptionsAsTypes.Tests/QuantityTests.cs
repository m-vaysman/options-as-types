using OptionsAsTypes.Quantities;
using Xunit;

namespace OptionsAsTypes.Tests;

public class QuantityTests
{
    [Fact]
    public void CallIntrinsicValueIsFlooredAtZero()
    {
        var spot = new S(45.0);
        var strike = new K(50.0);

        CallOptionIntrinsicValue intrinsic = spot - strike;

        // The original let this be -5.0 and still called it "intrinsic value".
        Assert.Equal(0.0, intrinsic.Value);
    }

    [Fact]
    public void PutIntrinsicValueIsStrikeLessSpot()
    {
        var spot = new S(45.0);
        var strike = new K(50.0);

        // Hangs off K, not off P. "put price less spot" is not a quantity.
        PutOptionIntrinsicValue intrinsic = strike - spot;

        Assert.Equal(5.0, intrinsic.Value);
    }

    [Fact]
    public void StepWidthIsTimeDividedBySteps()
    {
        dt step = new Time(2.0) / new N(8);

        Assert.Equal(0.25, step.Value, 12);
    }

    [Fact]
    public void DownMoveIsTheReciprocalOfTheUpMove()
    {
        dt step = new Time(1.0) / new N(4);
        var up = new u(Volatility.Create(0.2), step);
        var down = new d(up);

        Assert.Equal(1.0 / up.Value, down.Value, 12);
    }

    [Fact]
    public void TerminalNodeCountIsStepsPlusOne()
    {
        Assert.Equal(101, new N(100).TerminalNodeCount);
    }

    [Fact]
    public void NegativeRatesAreAllowed()
    {
        var negative = new r(-0.0075);
        Assert.Equal(-0.0075, negative.Value);
    }

    [Theory]
    [InlineData(5.0)]      // 5, meaning 500%: almost certainly a percentage in the wrong units
    [InlineData(-1.5)]
    public void RatesOutsideTheDecimalRangeAreRejected(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new r(value));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void NonPositiveSpotIsRejected(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new S(value));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-1.0)]
    public void NonPositiveStrikeIsRejected(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new K(value));
    }

    [Fact]
    public void ExpiredOptionsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Time(0.0));
    }

    [Fact]
    public void NonPositiveVolatilityIsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Volatility.Create(0.0));
    }

    [Fact]
    public void TreeNeedsAtLeastOneStep()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new N(0));
    }

    [Fact]
    public void ProbabilityMustLieInTheUnitInterval()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Probability(1.5));
    }

    [Fact]
    public void OptionPricesCannotBeNegative()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new C(-1.0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new P(-1.0));
    }
}
