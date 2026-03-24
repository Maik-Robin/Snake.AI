namespace MicroNEAT.WeightInitialization;

/// <summary>
/// Defines the contract for weight initialization strategies in neural networks.
/// </summary>
public interface IWeightInitialization
{
    /// <summary>
    /// Initializes a single weight value.
    /// </summary>
    /// <returns>A randomly initialized weight value.</returns>
    double InitializeWeight();
    
    /// <summary>
    /// Initializes multiple weight values.
    /// </summary>
    /// <param name="size">The number of weights to initialize.</param>
    /// <returns>An array of randomly initialized weight values.</returns>
    double[] InitializeWeights(int size);
}
