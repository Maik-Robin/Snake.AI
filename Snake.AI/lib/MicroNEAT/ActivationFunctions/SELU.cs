namespace MicroNEAT.ActivationFunctions;

/// <summary>
/// Implements the Scaled Exponential Linear Unit (SELU) activation function.
/// Self-normalizing activation function that maintains mean and variance across layers.
/// Uses specific alpha and scale values for self-normalization properties.
/// </summary>
public class SELU : IActivationFunction
{
    private readonly double _alpha = 1.6732632423543772848170429916717;
    private readonly double _scale = 1.0507009873554804934193349852946;

    /// <summary>
    /// Applies the SELU function to the input value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <returns>Scaled linear value if positive, scaled exponential if negative.</returns>
    public double Apply(double value)
    {
        return _scale * (value > 0 ? value : _alpha * (Math.Exp(value) - 1));
    }
}
