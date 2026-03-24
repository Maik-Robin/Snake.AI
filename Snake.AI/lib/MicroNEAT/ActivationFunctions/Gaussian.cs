namespace MicroNEAT.ActivationFunctions;

/// <summary>
/// Implements the Gaussian (bell curve) activation function.
/// Produces a bell-shaped curve centered at a specified point.
/// </summary>
public class Gaussian : IActivationFunction
{
    private readonly double _center;
    private readonly double _width;

    /// <summary>
    /// Initializes a new instance of the Gaussian activation function.
    /// </summary>
    /// <param name="center">The center point of the Gaussian curve. Default is 0.</param>
    /// <param name="width">The width (standard deviation) of the Gaussian curve. Default is 1.</param>
    public Gaussian(double center = 0, double width = 1)
    {
        _center = center;
        _width = width;
    }

    /// <summary>
    /// Applies the Gaussian function to the input value.
    /// </summary>
    /// <param name="value">The input value.</param>
    /// <returns>A value between 0 and 1, with maximum at the center point.</returns>
    public double Apply(double value)
    {
        return Math.Exp(-Math.Pow((value - _center) / _width, 2));
    }
}
