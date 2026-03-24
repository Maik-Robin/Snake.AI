namespace MicroNEAT.ActivationFunctions;

/// <summary>
/// Implements the Leaky Rectified Linear Unit (Leaky ReLU) activation function.
/// Similar to ReLU but allows a small gradient when the input is negative.
/// Formula: x if x > 0, else alpha * x
/// </summary>
public class LeakyReLU : IActivationFunction
{
    private readonly double _alpha;

    /// <summary>
    /// Initializes a new instance of the LeakyReLU activation function.
    /// </summary>
    /// <param name="alpha">The slope for negative values. Default is 0.01.</param>
    public LeakyReLU(double alpha = 0.01)
    {
        _alpha = alpha;
    }

    /// <summary>
    /// Applies the Leaky ReLU function to the input value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <returns>The input value if positive, otherwise alpha times the input value.</returns>
    public double Apply(double value)
    {
        return value > 0 ? value : _alpha * value;
    }
}
