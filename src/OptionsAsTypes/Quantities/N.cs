namespace OptionsAsTypes.Quantities;

/// <summary>Number of steps in the binomial tree.</summary>
public readonly struct N
{
    public int Value { get; }

    public N(int steps)
    {
        if (steps < 1)
            throw new ArgumentOutOfRangeException(nameof(steps), steps,
                "A binomial tree needs at least one step.");
        Value = steps;
    }

    /// <summary>An N-step tree has N+1 terminal nodes, not N.</summary>
    public int TerminalNodeCount => Value + 1;

    public static implicit operator int(N x) => x.Value;
    public static implicit operator double(N x) => x.Value;

    public override string ToString() => Value.ToString();
}
