namespace OptionsAsTypes.Pricing;

public enum ExerciseStyle
{
    /// <summary>Exercisable only at expiry.</summary>
    European,

    /// <summary>Exercisable at any node. Worth at least as much as the European equivalent.</summary>
    American
}
