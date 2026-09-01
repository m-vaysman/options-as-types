using OptionsAsTypes.Quantities;

namespace OptionsAsTypes.Numerics;

/// <summary>
/// Standard normal cumulative distribution, N(.).
///
/// This uses Hart's double-precision algorithm (as published by Graeme West), accurate to
/// roughly 1e-15. The Abramowitz and Stegun 26.2.17 approximation that Excel's NORMSDIST
/// is usually written against tops out near 7.5e-8 — fine for a spreadsheet, visible in
/// the tails when you are pricing wings or differencing for greeks.
/// </summary>
public static class NormalDistribution
{
    private const double SqrtTwoPi = 2.506628274631;

    /// <summary>N(x): the probability that a standard normal draw is at most x.</summary>
    public static Probability N(double x)
    {
        double absX = Math.Abs(x);
        double tail;

        if (absX > 37.0)
        {
            tail = 0.0;
        }
        else
        {
            double e = Math.Exp(-absX * absX / 2.0);

            if (absX < 7.07106781186547)
            {
                double numerator = 3.52624965998911e-02 * absX + 0.700383064443688;
                numerator = numerator * absX + 6.37396220353165;
                numerator = numerator * absX + 33.912866078383;
                numerator = numerator * absX + 112.079291497871;
                numerator = numerator * absX + 221.213596169931;
                numerator = numerator * absX + 220.206867912376;

                double denominator = 8.83883476483184e-02 * absX + 1.75566716318264;
                denominator = denominator * absX + 16.064177579207;
                denominator = denominator * absX + 86.7807322029461;
                denominator = denominator * absX + 296.564248779674;
                denominator = denominator * absX + 637.333633378831;
                denominator = denominator * absX + 793.826512519948;
                denominator = denominator * absX + 440.413735824752;

                tail = e * numerator / denominator;
            }
            else
            {
                // Continued fraction for the far tail.
                double build = absX + 0.65;
                build = absX + 4.0 / build;
                build = absX + 3.0 / build;
                build = absX + 2.0 / build;
                build = absX + 1.0 / build;
                tail = e / (build * SqrtTwoPi);
            }
        }

        return new Probability(x > 0.0 ? 1.0 - tail : tail);
    }

    public static Probability N(D1 d1) => N(d1.Value);

    public static Probability N(D2 d2) => N(d2.Value);
}
