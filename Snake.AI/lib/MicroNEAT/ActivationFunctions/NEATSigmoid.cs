namespace MicroNEAT.ActivationFunctions;

/// <summary>
/// Implements a steeper variant of the Sigmoid activation function.
/// This version is commonly used in NEAT (NeuroEvolution of Augmenting Topologies) implementations.
/// </summary>
public class NEATSigmoid : IActivationFunction
{
    private readonly double _steepness;

    /// <summary>
    /// Initializes a new instance of the NEATSigmoid activation function.
    /// </summary>
    /// <param name="steepness">The steepness parameter that controls the slope of the sigmoid. Default is 4.9.</param>
    public NEATSigmoid(double steepness = 4.9)
    {
        _steepness = steepness;
    }

    /// <summary>
    /// Applies the NEAT sigmoid function to the input value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <returns>A value between 0 and 1, with steeper transition than standard sigmoid.</returns>
    public double Apply(double value)
    {
        return 1.0 / (1.0 + Math.Exp(-_steepness * value));
    }
}
