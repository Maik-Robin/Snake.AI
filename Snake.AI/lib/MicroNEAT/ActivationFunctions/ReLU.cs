namespace MicroNEAT.ActivationFunctions;

/// <summary>
/// Implements the Rectified Linear Unit (ReLU) activation function.
/// Returns the input if positive, otherwise returns 0: max(0, x)
/// </summary>
public class ReLU : IActivationFunction
{
    /// <summary>
    /// Applies the ReLU function to the input value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <returns>The input value if positive, 0 otherwise.</returns>
    public double Apply(double value)
    {
        return Math.Max(0, value);
    }
}
