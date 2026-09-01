# Options as types

An experiment, not a pricing library: model Black-Scholes and the Cox-Ross-Rubinstein
binomial tree so that every symbol in the maths is its own C# type, and see how much of
the model the compiler will check for you.

The constraint is that the notation has to survive. Renaming `S` to `spotPrice` is the
usual advice and it breaks the correspondence with the derivation, which is the review
that actually catches pricing errors. So the types keep the letters.

```csharp
var spot   = new S(52.0);
var strike = new K(50.0);
var rate   = new r(0.02);
var sigma  = Volatility.Create(0.30);
var expiry = new Time(1.0);

C price = new BlackScholesCall(spot, strike, rate, q.None, sigma, expiry).Price();
```

Derived quantities are reachable only through the formula that defines them:

```csharp
dt step = expiry / steps;              // T / N is, by definition, a step width
var up   = new u(sigma, step);         // u = exp(sigma * sqrt(dt))
var down = new d(up);                  // d = 1 / u, and nothing else
```

## Run it

```bash
dotnet run --project src/OptionsAsTypes
```

```bash
dotnet test
```

.NET 9, no dependencies. The demo prices a call and a put, checks put-call parity, walks
the binomial tree onto the closed-form price as the step count grows, and shows the early
exercise premium on an American put.

## What the compiler refuses

```csharp
new BlackScholesCall(strike, spot, ...)     // K where S belongs
new u(sigma, expiry)                        // Time is not a step width
new d(sigma)                                // d is derived from u, full stop
new D2(sigma, expiry)                       // d2 needs a real d1
```

## What it lets through

```csharp
double leak = spot - strike + rate;         // implicit operator double
```

Every quantity converts implicitly to `double`, so the discipline is one-way. The escape
hatch is kept deliberately: it is what the original sketch had, and it is what let a real
bug through — `CalculateD1()` returning `new C(price)`, wrapping a log-moneyness in the
type meaning *call price*. Making the conversions `explicit` closes the hole and adds
ceremony to every line of arithmetic. That trade is the experiment.

## Quant corrections against the original sketch

The first pass was a design exercise with placeholder maths. All of the below is fixed
here and covered by tests.

| Original | Corrected |
|---|---|
| Binomial `CalculateOptionPrice()` returned a hardcoded `2.0` | Full backward induction, European and American |
| Terminal nodes looped to `N` | `N + 1` nodes — an N-step tree has N+1 leaves |
| `operator -(P, S)` as put intrinsic | `operator -(K, S)`; intrinsic is strike less spot |
| Intrinsic value could go negative | `max(S - K, 0)` and `max(K - S, 0)` |
| `CalculateD1()` returned `new C(price)` | `D1` and `D2` are their own types |
| Realised vol not annualised, simple returns | Log returns, scaled by `sqrt(periodsPerYear)` |
| Negative rates threw | Permitted — EUR, JPY and CHF have all traded below zero |
| No dividend yield | `q` carried through both models |
| Abramowitz-Stegun N(.), ~7.5e-8 | Hart's algorithm, ~1e-15 |
| No no-arbitrage check on the tree | `d < exp((r-q)dt) < u` enforced; p outside [0, 1] throws |
| `CompareTo` threw `NotImplementedException` | Removed — `implicit operator double` already gave comparison |
| ~60 lines of boilerplate per type | ~15 |

The suite checks put-call parity to 1e-10 with and without dividends, convergence of the
tree to the closed form, and that an American call on a non-dividend-paying underlying
prices identically to the European one — Merton's result, and a sharp check on the
induction.

## Where it hits a ceiling

`r`, `q`, `u`, `d` and `dt` all trip **CS8981**: C# reserves all-lowercase type names for
future keywords, so keeping the notation means suppressing that warning in the csproj.

More fundamentally, C# has no phantom types and no units of measure. F# has `[<Measure>]`.
Typed constructors get most of the way; the last stretch is not available in this language.
.NET 7 static abstract interface members would collapse the remaining boilerplate but do
not move that ceiling.

## Prior art

This lands in well-populated territory, reached from the domain side rather than the
language-theory side:

- **Primitive obsession** — the smell being reacted to (Fowler, Beck)
- **Value objects**, narrowed to *tiny types* — Evans, DDD
- **Newtype** — Haskell's `newtype`, Rust's newtype idiom
- **Make illegal states unrepresentable** — Yaron Minsky, argued for financial code in OCaml at Jane Street
- **Parse, don't validate** — Alexis King; the validating constructors here
- **Dimension types / units of measure** — Andrew Kennedy's 1996 thesis, later F# `[<Measure>]`
- **Curry-Howard** — the deep version of "if it compiles, the formula is right"

## Layout

```
src/OptionsAsTypes/
  Quantities/     S K r q Volatility Time N dt u d p 1-p disc D1 D2 Probability C P
  Pricing/        BlackScholesCall BlackScholesPut BinomialCallOption BinomialPutOption
  Numerics/       NormalDistribution
  Program.cs      the demo
tests/
```
