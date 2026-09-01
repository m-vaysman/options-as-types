using OptionsAsTypes.Pricing;
using OptionsAsTypes.Quantities;

namespace OptionsAsTypes;

internal static class Program
{
    private static void Main()
    {
        Rule("OPTIONS AS TYPES");
        Console.WriteLine("Every symbol in the model is its own type. The notation survives; the");
        Console.WriteLine("compiler starts enforcing it.");
        Console.WriteLine();

        // The inputs read exactly like the parameter list of the model.
        var spot = new S(52.0);
        var strike = new K(50.0);
        var rate = new r(0.02);
        var yield = q.None;
        var sigma = Volatility.Create(0.30);
        var expiry = new Time(1.0);

        Console.WriteLine($"  S = {spot}    K = {strike}    r = {rate}    q = {yield}    sigma = {sigma}    T = {expiry}");
        Console.WriteLine();

        BlackScholes(spot, strike, rate, yield, sigma, expiry);
        Convergence(spot, strike, rate, yield, sigma, expiry);
        EarlyExercise(rate, sigma);
        RealisedVolatility();
        WhatDoesNotCompile();
    }

    private static void BlackScholes(S spot, K strike, r rate, q yield, Volatility sigma, Time expiry)
    {
        Rule("BLACK-SCHOLES-MERTON");

        var call = new BlackScholesCall(spot, strike, rate, yield, sigma, expiry);
        var put = new BlackScholesPut(spot, strike, rate, yield, sigma, expiry);

        // Each intermediate has a type of its own, so none of them can be confused
        // for a price on the way through.
        Console.WriteLine($"  d1        = {call.D1}");
        Console.WriteLine($"  d2        = {call.D2}");
        Console.WriteLine($"  N(d1)     = {call.Nd1}");
        Console.WriteLine($"  N(d2)     = {call.Nd2}");
        Console.WriteLine();

        C c = call.Price();
        P p = put.Price();

        Console.WriteLine($"  Call      = {c}   ({call.Moneyness}, intrinsic {call.IntrinsicValue})");
        Console.WriteLine($"  Put       = {p}   ({put.Moneyness}, intrinsic {put.IntrinsicValue})");
        Console.WriteLine();

        // Put-call parity: C - P = S*exp(-qT) - K*exp(-rT). If this does not hold, the
        // two pricers disagree and at least one of them is wrong.
        double lhs = c - p;
        double rhs = spot * Math.Exp(-yield * expiry) - strike * Math.Exp(-rate * expiry);
        Console.WriteLine($"  Parity    C - P = {lhs:F10}");
        Console.WriteLine($"            S*e^-qT - K*e^-rT = {rhs:F10}");
        Console.WriteLine($"            residual = {Math.Abs(lhs - rhs):E2}");
        Console.WriteLine();
    }

    private static void Convergence(S spot, K strike, r rate, q yield, Volatility sigma, Time expiry)
    {
        Rule("BINOMIAL CONVERGENCE TO BLACK-SCHOLES");
        Console.WriteLine("  A European CRR tree should walk onto the closed-form price as N grows.");
        Console.WriteLine("  This is the check the original sketch never had: it returned a hardcoded 2.0.");
        Console.WriteLine();

        double closedForm = new BlackScholesCall(spot, strike, rate, yield, sigma, expiry).Price();

        Console.WriteLine($"  {"steps",8}  {"binomial",12}  {"error",12}");
        Console.WriteLine($"  {new string('-', 8)}  {new string('-', 12)}  {new string('-', 12)}");

        foreach (int steps in new[] { 1, 2, 5, 10, 25, 50, 100, 250, 500, 1000, 2000 })
        {
            C priced = new BinomialCallOption(spot, strike, expiry, rate, yield, sigma, new N(steps)).Price();
            Console.WriteLine($"  {steps,8}  {priced.Value,12:F6}  {Math.Abs(priced - closedForm),12:F6}");
        }

        Console.WriteLine($"  {"closed",8}  {closedForm,12:F6}");
        Console.WriteLine();
        Console.WriteLine("  The error oscillates rather than falling monotonically. That is CRR");
        Console.WriteLine("  behaving normally: the strike sits between terminal nodes differently");
        Console.WriteLine("  for odd and even N. The envelope is what shrinks.");
        Console.WriteLine();
    }

    private static void EarlyExercise(r rate, Volatility sigma)
    {
        Rule("EARLY EXERCISE");
        Console.WriteLine("  An American put is worth at least the European one: the right to exercise");
        Console.WriteLine("  early cannot be a liability. That difference is the early exercise premium.");
        Console.WriteLine();

        var spot = new S(52.0);
        var strike = new K(60.0);      // deep in the money, where early exercise bites
        var expiry = new Time(1.0);
        var steps = new N(1000);

        P european = new BinomialPutOption(spot, strike, expiry, rate, q.None, sigma, steps).Price();
        P american = new BinomialPutOption(spot, strike, expiry, rate, q.None, sigma, steps,
            ExerciseStyle.American).Price();

        Console.WriteLine($"  S = {spot}   K = {strike}   European put = {european}");
        Console.WriteLine($"                        American put = {american}");
        Console.WriteLine($"                        premium      = {american - european:F4}");
        Console.WriteLine();
    }

    private static void RealisedVolatility()
    {
        Rule("REALISED VOLATILITY");
        Console.WriteLine("  Estimated from log returns and annualised. A per-period sigma fed into a");
        Console.WriteLine("  model whose T is in years is wrong by a factor of sqrt(252).");
        Console.WriteLine();

        // A deterministic walk, so the demo prints the same numbers every run.
        var prices = new List<double> { 100.0 };
        double[] shocks = { 0.012, -0.008, 0.015, -0.011, 0.006, 0.009, -0.014, 0.004, -0.003, 0.010,
                            -0.007, 0.013, -0.009, 0.005, 0.002, -0.012, 0.011, -0.006, 0.008, -0.004 };
        foreach (double shock in shocks)
            prices.Add(prices[^1] * (1.0 + shock));

        Volatility daily = Volatility.CreateFromPrices(prices, periodsPerYear: 1.0);
        Volatility annual = Volatility.CreateFromPrices(prices);

        Console.WriteLine($"  {prices.Count} daily closes");
        Console.WriteLine($"  per-period sigma  = {daily}");
        Console.WriteLine($"  annualised sigma  = {annual}   (x sqrt(252))");
        Console.WriteLine();
    }

    private static void WhatDoesNotCompile()
    {
        Rule("WHAT THE COMPILER REFUSES");
        Console.WriteLine("  Uncomment any of these in Program.cs and the build fails:");
        Console.WriteLine();
        Console.WriteLine("    new BlackScholesCall(strike, spot, ...)   // K where S belongs");
        Console.WriteLine("    new BlackScholesCall(spot, strike, sigma, ...)  // sigma where r belongs");
        Console.WriteLine("    new u(sigma, expiry)                     // Time is not a step width");
        Console.WriteLine("    new d(sigma)                             // d comes from u, nothing else");
        Console.WriteLine("    D2 d2 = new D2(sigma, expiry)            // d2 needs a real d1");
        Console.WriteLine();
        Console.WriteLine("  And what it still lets through, which is the honest half of the story:");
        Console.WriteLine();
        Console.WriteLine("    double leak = spot - strike + rate;      // implicit operator double");
        Console.WriteLine();
        Console.WriteLine("  Every quantity converts implicitly to double, so the type discipline is");
        Console.WriteLine("  one-way. Make those conversions explicit and the appliance closes up -");
        Console.WriteLine("  at the cost of ceremony on every line of arithmetic. That trade is the");
        Console.WriteLine("  whole experiment.");
        Console.WriteLine();
    }

    private static void Rule(string title)
    {
        Console.WriteLine(new string('=', 72));
        Console.WriteLine($"  {title}");
        Console.WriteLine(new string('=', 72));
        Console.WriteLine();
    }
}
