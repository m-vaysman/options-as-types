namespace OptionsAsTypes.Quantities;

/// <summary>
/// Cox-Ross-Rubinstein down-move factor: d = 1 / u.
/// A down-move can only be derived from the matching up-move, which is what keeps
/// the tree recombining.
/// </summary>
public readonly struct d
{
    public double Value { get; }

    public d(u upMove) => Value = 1.0 / upMove.Value;

    public static implicit operator double(d x) => x.Value;

    public override string ToString() => Value.ToString("F6");
}
