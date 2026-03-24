namespace MicroNEAT.WeightInitialization;

/// <summary>
/// Implements random weight initialization with uniform distribution.
/// Generates weights uniformly distributed between specified minimum and maximum values.
/// </summary>
public class RandomWeightInitialization : IWeightInitialization
{
    private readonly double _min;
    private readonly double _max;
    private readonly Random _random = new();

    /// <summary>
    /// Initializes a new instance of the RandomWeightInitialization class.
    /// </summary>
    /// <param name="min">The minimum weight value (inclusive).</param>
    /// <param name="max">The maximum weight value (exclusive).</param>
    public RandomWeightInitialization(double min, double max)
    {
        _min = min;
        _max = max;
    }

    /// <summary>
    /// Generates a single random weight value within the specified range.
    /// </summary>
    /// <returns>A random weight value between min and max.</returns>
    public double InitializeWeight()
    {
        return _min + (_max - _min) * _random.NextDouble();
    }

    /// <summary>
    /// Generates an array of random weight values within the specified range.
    /// </summary>
    /// <param name="size">The number of weights to generate.</param>
    /// <returns>An array of random weight values.</returns>
    public double[] InitializeWeights(int size)
    {
        var weights = new double[size];
        for (int i = 0; i < size; i++)
        {
            weights[i] = InitializeWeight();
        }
        return weights;
    }
}
