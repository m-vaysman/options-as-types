namespace OptionsAsTypes.Quantities;

/// <summary>Risk-neutral probability of a down-move: 1 - p.</summary>
public readonly struct pbDownMovement
{
    public double Value { get; }

    public pbDownMovement(pbUpMovement up) => Value = 1.0 - up.Value;

    public static implicit operator double(pbDownMovement x) => x.Value;

    public override string ToString() => Value.ToString("F6");
}
