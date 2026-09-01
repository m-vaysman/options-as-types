using OptionsAsTypes.Numerics;
using Xunit;

namespace OptionsAsTypes.Tests;

public class NormalDistributionTests
{
    [Theory]
    [InlineData(0.0, 0.5)]
    [InlineData(1.0, 0.841344746068543)]
    [InlineData(-1.0, 0.158655253931457)]
    [InlineData(1.959963984540054, 0.975)]
    [InlineData(3.0, 0.998650101968370)]
    [InlineData(-3.0, 0.001349898031630)]
    public void MatchesKnownValues(double x, double expected)
    {
        Assert.Equal(expected, NormalDistribution.N(x).Value, 12);
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(2.5)]
    [InlineData(6.0)]
    [InlineData(9.0)]
    public void IsSymmetric(double x)
    {
        Assert.Equal(1.0, NormalDistribution.N(x).Value + NormalDistribution.N(-x).Value, 12);
    }

    [Fact]
    public void FarTailsSaturateWithoutOverflow()
    {
        Assert.Equal(1.0, NormalDistribution.N(40.0).Value);
        Assert.Equal(0.0, NormalDistribution.N(-40.0).Value);
    }
}
