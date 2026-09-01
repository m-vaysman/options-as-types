using OptionsAsTypes.Pricing;
using OptionsAsTypes.Quantities;
using Xunit;

namespace OptionsAsTypes.Tests;

public class BlackScholesTests
{
    private static readonly S Spot = new(52.0);
    private static readonly K Strike = new(50.0);
    private static readonly r Rate = new(0.02);
    private static readonly Volatility Sigma = Volatility.Create(0.30);
    private static readonly Time Expiry = new(1.0);

    [Fact]
    public void CallMatchesTextbookValue()
    {
        var call = new BlackScholesCall(Spot, Strike, Rate, q.None, Sigma, Expiry);

        Assert.Equal(0.347402, call.D1.Value, 6);
        Assert.Equal(0.047402, call.D2.Value, 6);
        Assert.Equal(7.633047, call.Price().Value, 5);
    }

    [Fact]
    public void PutCallParityHolds()
    {
        C call = new BlackScholesCall(Spot, Strike, Rate, q.None, Sigma, Expiry).Price();
        P put = new BlackScholesPut(Spot, Strike, Rate, q.None, Sigma, Expiry).Price();

        double lhs = call - put;
        double rhs = Spot.Value - Strike.Value * Math.Exp(-Rate.Value * Expiry.Value);

        Assert.Equal(rhs, lhs, 10);
    }

    [Fact]
    public void PutCallParityHoldsWithDividendYield()
    {
        var yield = new q(0.03);

        C call = new BlackScholesCall(Spot, Strike, Rate, yield, Sigma, Expiry).Price();
        P put = new BlackScholesPut(Spot, Strike, Rate, yield, Sigma, Expiry).Price();

        double lhs = call - put;
        double rhs = Spot.Value * Math.Exp(-yield.Value * Expiry.Value)
                     - Strike.Value * Math.Exp(-Rate.Value * Expiry.Value);

        Assert.Equal(rhs, lhs, 10);
    }

    [Fact]
    public void DeepInTheMoneyCallApproachesDiscountedForward()
    {
        var deepSpot = new S(500.0);
        C call = new BlackScholesCall(deepSpot, Strike, Rate, q.None, Sigma, Expiry).Price();

        double floor = deepSpot.Value - Strike.Value * Math.Exp(-Rate.Value * Expiry.Value);

        Assert.Equal(floor, call.Value, 4);
    }

    [Fact]
    public void PriceIsNeverBelowIntrinsic()
    {
        var itmSpot = new S(80.0);
        var call = new BlackScholesCall(itmSpot, Strike, Rate, q.None, Sigma, Expiry);

        Assert.True(call.Price().Value >= call.IntrinsicValue.Value);
    }

    [Fact]
    public void NegativeRatesArePriceable()
    {
        var negative = new r(-0.005);
        C call = new BlackScholesCall(Spot, Strike, negative, q.None, Sigma, Expiry).Price();

        Assert.True(call.Value > 0.0);
    }
}
