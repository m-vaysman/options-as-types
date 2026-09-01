namespace OptionsAsTypes.Quantities;

/// <summary>A probability in [0, 1] — the output of N(.), the standard normal CDF.</summary>
public readonly struct Probability
{
    public double Value { get; }

    public Probability(double value)
    {
        if (double.IsNaN(value) || value < 0.0 || value > 1.0)
            throw new ArgumentOutOfRangeException(nameof(value), value,
                "A probability must lie in [0, 1].");
        Value = value;
    }

    public static implicit operator double(Probability x) => x.Value;

    public override string ToString() => Value.ToString("F6");
}
