namespace MicroNEAT.ActivationFunctions;

/// <summary>
/// Implements the Hyperbolic Tangent (Tanh) activation function.
/// Maps input values to the range (-1, 1).
/// </summary>
public class Tanh : IActivationFunction
{
    /// <summary>
    /// Applies the tanh function to the input value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <returns>A value between -1 and 1.</returns>
    public double Apply(double value)
    {
        return Math.Tanh(value);
    }
}
