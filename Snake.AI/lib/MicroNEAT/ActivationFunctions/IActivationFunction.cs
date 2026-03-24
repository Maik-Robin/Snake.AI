namespace MicroNEAT.ActivationFunctions;

/// <summary>
/// Defines the contract for activation functions used in neural network nodes.
/// Activation functions introduce non-linearity into the network.
/// </summary>
public interface IActivationFunction
{
    /// <summary>
    /// Applies the activation function to the given input value.
    /// </summary>
    /// <param name="value">The input value to transform.</param>
    /// <returns>The transformed output value.</returns>
    double Apply(double value);
}
