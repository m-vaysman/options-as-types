# Options as types

**This is not a pricing library. It is an argument with a compiler attached.**

Open almost any option pricing code and you find the same file: thirty `double`s named
`s`, `k`, `r`, `v`, `t`, `d1`, `d2`, and a comment pointing at the paper it came from.
Nothing stops you passing volatility where the rate belongs. Nothing stops a function
returning `d1` into a variable holding a price. The compiler has no opinion, because
every quantity in the model has the same type.

The usual software-engineering fix is to rename everything — `s` becomes `spotPrice`,
`k` becomes `strikePrice`. Quants reject that, and they are right to. The single letters
are not laziness; they are the correspondence with the derivation. Break it and you can
no longer read the code beside the paper, which is the review that actually catches
pricing errors.

So: keep the letters, and make them **types**.

```csharp
var spot   = new S(52.0);
var strike = new K(50.0);
var rate   = new r(0.02);
var sigma  = Volatility.Create(0.30);
var expiry = new Time(1.0);

C price = new BlackScholesCall(spot, strike, rate, q.None, sigma, expiry).Price();
```

`S` is not a `double` that happens to hold a spot price. It is a spot price. And the
model builds itself out of the pieces:

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

Zero dependencies. .NET 9. The demo prices a call and a put, checks put-call parity,
walks a binomial tree onto the closed-form price as the step count grows, and shows the
early exercise premium on an American put.

## What the compiler refuses

```csharp
new BlackScholesCall(strike, spot, ...)     // K where S belongs
new u(sigma, expiry)                        // Time is not a step width
new d(sigma)                                // d is derived from u, full stop
new D2(sigma, expiry)                       // d2 needs a real d1
```

None of those compile. That is the whole idea working.

## What it still lets through

```csharp
double leak = spot - strike + rate;         // fine, because of implicit operator double
```

Every quantity has an `implicit operator double`, so the discipline is one-way: typed
values flow freely into untyped arithmetic and never come back. That escape hatch is
kept deliberately in this repo, because it is what the original sketch did and it is
what the writeup is about.

It is not hypothetical. In the first version, `CalculateD1()` ended with
`return new C(price)` — d1, a standardised log-moneyness, wrapped in the type meaning
*call option price*. It compiled. It produced correct numbers. And it was exactly the
class of error the type system had been built to prevent, waved straight through by the
conversion that made the arithmetic readable.

Make the conversions `explicit` and the appliance closes. The cost is ceremony on every
line of arithmetic. That trade is the experiment.

## What the language pushes back on

`r`, `q`, `u`, `d` and `dt` all trip **CS8981** — C# reserves all-lowercase type names
for future keywords. Keeping the notation means explicitly suppressing that warning in
the csproj. The language is mildly hostile to the thing this design depends on.

The deeper ceiling: C# has no phantom types and no units of measure. F# has
`[<Measure>]`. You can get most of the way with typed constructors; the last stretch is
not available. .NET 7's static abstract interface members would collapse the remaining
boilerplate, but they do not move that ceiling.

## Where operators stopped working

Operator overloading is a natural first reach, and it fails for two reasons.

Black-Scholes is not algebraic. `d1` needs `ln`, the price needs `exp` and `N(.)`, and
no amount of binary operator overloading expresses that. The chain breaks and you fall
back to raw doubles — which is exactly where the `new C(price)` bug got in.

And operators do not scale across a vocabulary this size. With fifteen symbol types you
get a combinatorial surface of legal-but-meaningless pairings. The original had
`operator -(P, S)` — put price minus spot — which the compiler was happy to accept
because the two types existed. It does not denote anything.

What survived is **constructors**. `new u(sigma, step)` is a total function with a typed
domain, and its signature is the formula. Only genuinely binary relations kept their
operators: `Time / N -> dt`, `S - K -> CallOptionIntrinsicValue`,
`K - S -> PutOptionIntrinsicValue`.

## Quant corrections against the original sketch

The first pass was a design experiment and the maths was placeholder. Everything below
is fixed here and covered by tests.

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
| No no-arbitrage check on the tree | `d < exp((r-q)dt) < u` enforced; p outside [0,1] throws |
| `CompareTo` threw `NotImplementedException` | Removed — `implicit operator double` already gave comparison |
| ~60 lines of boilerplate per type | ~15 |

Verified by the test suite: put-call parity holds to 1e-10 with and without dividends,
the tree converges to the closed form, and an American call on a non-dividend-paying
underlying prices identically to the European one — Merton's result, and a sharp check
on the induction.

## Layout

```
src/OptionsAsTypes/
  Quantities/     S K r q Volatility Time N dt u d p 1-p disc D1 D2 Probability C P
  Pricing/        BlackScholesCall BlackScholesPut BinomialCallOption BinomialPutOption
  Numerics/       NormalDistribution
  Program.cs      the demo
tests/
```

## The open question

A five-thousand-line pricer that nobody dares touch, versus two hundred lines of types
that make the wrong call fail to compile. Is that a trade worth making on a real book,
or is the ceremony just a new kind of unmaintainable?

I genuinely do not know. That is why this is a demo.
