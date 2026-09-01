# I'm not a quant. I tried to make the compiler check the option pricer anyway.

Some years ago I opened a pricing file and found about forty `double`s.

They were named `s`, `k`, `r`, `v`, `t`, `d1`, `d2`, `nd1`. There was a comment at the top naming the paper it came from. That comment was the only specification in the file. Everything else was arithmetic on interchangeable numbers.

Nothing in that file stopped you passing volatility where the rate belonged. Nothing stopped a helper returning `d1` into a variable that meant a price. The compiler had no opinion about any of it, because every quantity in the model had the same type.

Let me get the disclaimer out of the way, because it matters for how you read the rest. **I am not a quant.** I have never derived a model. I could not defend a choice of numeraire and I'm not going to pretend otherwise. I've spent seventeen years in and around capital markets, mostly on the engineering side, and my job has generally been the code that still has to work on the Monday after the person who wrote it moved desks.

So I don't look at that file and think about the mathematics. I look at it and think about risk surface. How many ways can this be wrong without anybody noticing? That's the only lens I have, and it's the lens this whole thing came out of.

This wasn't a language experiment for its own sake. It was a genuine attempt to put a better idea on the table for maintainability — to push as much of the checking as possible onto the compiler, because the compiler is the only reviewer that never gets tired, never gets rushed the day before a release, and never assumes somebody else already looked at it. Everything a type can enforce is something a human no longer has to remember to check. That seemed worth trying to prove out rather than just assert.

## The fix that doesn't work

The standard engineering answer is: rename everything. `s` becomes `spotPrice`. `k` becomes `strikePrice`. `v` becomes `annualisedVolatility`. Self-documenting code. Ship it.

Quants hate this, and after watching the argument a few times I came around to their side. The single letters aren't laziness. They're the correspondence with the derivation. When the numbers come out wrong, the review that finds it is someone putting the paper next to the screen and reading down both. Rename the variables and you've destroyed the only review that reliably works, in exchange for names that were never the actual problem.

And `spotPrice` and `strikePrice` are still both `double`. You can still swap them. All the renaming bought was a longer line.

## So keep the letters, and make them types

That's the whole idea. It took me an embarrassingly long time to get to something this obvious.

```csharp
var spot   = new S(52.0);
var strike = new K(50.0);
var rate   = new r(0.02);
var sigma  = Volatility.Create(0.30);
var expiry = new Time(1.0);

C price = new BlackScholesCall(spot, strike, rate, q.None, sigma, expiry).Price();
```

`S` isn't a `double` holding a spot price. It is a spot price. Hand the constructor a `K` where it wants an `S` and the build fails. The notation is intact — you can still read this next to the paper — and the swap that used to be silent is now a compile error.

Then it gets more interesting, because the derived quantities can be made reachable *only* through the formula that defines them:

```csharp
dt step = expiry / steps;        // T / N is, by definition, a step width
var up   = new u(sigma, step);   // u = exp(sigma * sqrt(dt))
var down = new d(up);            // d = 1 / u, and nothing else
```

There is no other way to get a `d`. It comes from a `u` or it doesn't exist. That's the part I found genuinely satisfying: the constructor signature *is* the formula, and the type system is carrying a piece of the model rather than just labelling it.

Feed the binomial tree its inputs and it assembles itself — step width from time and step count, up-move from volatility and step width, down-move from up-move, risk-neutral probability from all of the above. The constructor ends up reading like the paper's parameter list.

## Where it stopped working

Two walls, and they're the useful part of this.

**Black-Scholes isn't algebraic.** I started by reaching for operator overloading, because that's the obvious tool: define `+`, `-`, `/` between the symbol types and let the price fall out of an expression. It works right up until you need `d1`, which needs `ln`. Then it needs `exp`, then `N(·)`. Binary operators can't express any of that. The chain breaks, you drop back to raw `double` arithmetic, and everything you built stops applying at exactly the point the formula gets interesting.

**Operators don't scale across a vocabulary this size.** With fifteen symbol types you have a large surface of type pairs that are legal purely because both types exist. I wrote `operator -(P, S)` — put price minus spot — and used it as put intrinsic value. The compiler took it happily. It doesn't denote anything. Put intrinsic is strike minus spot, `K - S`. I'd hung the operator off the wrong type and nothing objected, because operator overloading has no way to know which of the available pairings mean something.

What survived both walls was constructors. `new u(sigma, step)` is a total function with a typed domain. The signature is the formula. Unwanted combinations don't need to be forbidden, because they were never written. Only the genuinely binary relations kept operators: `Time / N -> dt`, `S - K -> call intrinsic`, `K - S -> put intrinsic`. Three of them. The rest was scaffolding I'd built out of enthusiasm.

## The part where I proved my own point by accident

Every one of these types had an `implicit operator double`. Without it the arithmetic is unreadable — you're casting on every line — so it went in early and I stopped thinking about it.

Here's what I wrote, months later, in the method that computes `d1`:

```csharp
public double CalculateD1()
{
    var price = (Math.Log(SecurityPrice / StrikePrice) + ...) / ...;
    return new C(price);
}
```

`C` is the type that means **call option price**. `d1` is a standardised log-moneyness. Those are not the same kind of thing, not remotely, and I wrapped one in the other and shipped it.

It compiled. It produced correct numbers, because the implicit conversion quietly unwrapped it back to a `double` on the way out. And it was precisely the class of mistake the entire design existed to prevent, waved through by the convenience I'd added to make the design pleasant to use.

I don't think I could have written a better argument for the thing than making the error myself, inside the codebase built to stop it. Convenience and enforcement turned out to be the same switch, and I'd left it in the convenient position.

The fix isn't subtle: make the conversions `explicit` and the hole closes. The cost is ceremony on every line of arithmetic, forever. That trade — not the type design, the trade — is the actual finding.

## What the language thinks of all this

C# reserves all-lowercase type names for future keywords, so `r`, `q`, `u`, `d` and `dt` all raise **CS8981**. Keeping the notation from the papers means explicitly suppressing a compiler warning. The language is mildly hostile to the premise.

And there's a ceiling: C# has no units of measure. F# does, via `[<Measure>]`, which descends from Andrew Kennedy's 1996 work on dimension types. Typed constructors get you most of the way. The last stretch isn't available in this language, and no amount of cleverness changes that.

## This has names, which I only learned afterwards

I got here on my own, staring at that file. I wasn't first, and it turns out I wasn't close to first.

It's **primitive obsession** as the smell, **value objects** or *tiny types* as the shape, **newtype** if you come from Haskell or Rust, **parse, don't validate** for the validating constructors, and **make illegal states unrepresentable** — a phrase from Yaron Minsky, who argued it for financial code in OCaml at a trading firm. Which is to say: people with far better credentials than mine hit the same pressure and reached the same conclusion.

I find that reassuring rather than deflating. When people arrive at the same answer from opposite directions — one from language theory, one from being annoyed at a file — the answer is probably tracking something real.

## What I actually don't know

I rebuilt the whole thing properly to write this. Real backward induction instead of the stub I'd left, `N+1` terminal nodes instead of `N`, dividend yield carried through both models, negative rates allowed because EUR and JPY have both traded there, volatility annualised from log returns, and a test suite that checks put-call parity to 1e-10 and confirms an American call on a non-dividend payer prices identically to the European one. It's here: [github.com/m-vaysman/options-as-types](https://github.com/m-vaysman/options-as-types). Clone it and `dotnet run`.

What I can't tell you is whether this is a good idea at scale.

A five-thousand-line pricer nobody dares touch, against two hundred lines of types that make the wrong call fail to compile. On paper that's an obvious trade for anyone whose job is reducing the number of ways a thing can be silently wrong. In practice I've never had to live with it past a demo, and ceremony compounds in ways that are easy to underestimate from here.

I'd genuinely like to hear from someone who has. Not whether the types are clever — whether, two years in, anybody still wanted them.
