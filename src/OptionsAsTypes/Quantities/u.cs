namespace OptionsAsTypes.Quantities;

/// <summary>
/// Cox-Ross-Rubinstein up-move factor: u = exp(sigma * sqrt(dt)).
/// The only public constructor is the formula itself.
/// </summary>
public readonly struct u
{
    public double Value { get; }

    public u(Volatility sigma, dt step) => Value = Math.Exp(sigma.Value * Math.Sqrt(step.Value));

    public static implicit operator double(u x) => x.Value;

    public override string ToString() => Value.ToString("F6");
}
