namespace MicroNEAT.ActivationFunctions;

/// <summary>
/// Implements the Sigmoid (logistic) activation function.
/// Maps input values to the range (0, 1) using the formula: 1 / (1 + e^(-x))
/// </summary>
public class Sigmoid : IActivationFunction
{
    /// <summary>
    /// Applies the sigmoid function to the input value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <returns>A value between 0 and 1.</returns>
    public double Apply(double value)
    {
        return 1.0 / (1.0 + Math.Exp(-value));
    }
}
