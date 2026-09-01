namespace OptionsAsTypes.Quantities;

/// <summary>Width of a single binomial step, in years. Only obtainable as T / N.</summary>
public readonly struct dt
{
    public double Value { get; }

    internal dt(double value)
    {
        if (value <= 0.0 || double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentOutOfRangeException(nameof(value), value, "Step width must be positive.");
        Value = value;
    }

    public static implicit operator double(dt x) => x.Value;

    public override string ToString() => Value.ToString("F6");
}
