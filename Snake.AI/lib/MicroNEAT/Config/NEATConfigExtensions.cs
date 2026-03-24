using MicroNEAT.ActivationFunctions;
using MicroNEAT.FitnessFunctions;
using MicroNEAT.WeightInitialization;

namespace MicroNEAT.Config;

/// <summary>
/// Provides fluent extension methods for configuring NEATConfig instances.
/// Enables method chaining for more readable configuration setup.
/// </summary>
public static class NEATConfigExtensions
{
    /// <summary>
    /// Sets the number of input nodes in the network.
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <param name="size">The number of input nodes.</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig WithInputSize(this NEATConfig config, int size)
    {
        config.InputSize = size;
        return config;
    }

    /// <summary>
    /// Sets the number of output nodes in the network.
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <param name="size">The number of output nodes.</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig WithOutputSize(this NEATConfig config, int size)
    {
        config.OutputSize = size;
        return config;
    }

    /// <summary>
    /// Sets the population size (number of genomes per generation).
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <param name="size">The population size.</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig WithPopulation(this NEATConfig config, int size)
    {
        config.PopulationSize = size;
        return config;
    }

    /// <summary>
    /// Sets the maximum number of generations to evolve.
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <param name="generations">The number of generations.</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig WithGenerations(this NEATConfig config, int generations)
    {
        config.Generations = generations;
        return config;
    }

    /// <summary>
    /// Sets the target fitness threshold. Evolution stops when reached.
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <param name="fitness">The target fitness value.</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig WithTargetFitness(this NEATConfig config, double fitness)
    {
        config.TargetFitness = fitness;
        return config;
    }

    /// <summary>
    /// Sets the activation function used by network nodes.
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <param name="activationFunction">The activation function to use.</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig WithActivationFunction(this NEATConfig config, IActivationFunction activationFunction)
    {
        config.ActivationFunction = activationFunction;
        return config;
    }

    /// <summary>
    /// Sets the fitness function used to evaluate genomes.
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <param name="fitnessFunction">The fitness function to use.</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig WithFitnessFunction(this NEATConfig config, IFitnessFunction fitnessFunction)
    {
        config.FitnessFunction = fitnessFunction;
        return config;
    }

    /// <summary>
    /// Sets the weight initialization strategy for new connections.
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <param name="weightInit">The weight initialization strategy.</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig WithWeightInitialization(this NEATConfig config, IWeightInitialization weightInit)
    {
        config.WeightInitialization = weightInit;
        return config;
    }

    /// <summary>
    /// Disables the use of bias nodes in the network.
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig DisableBias(this NEATConfig config)
    {
        config.BiasMode = "DISABLED";
        return config;
    }

    /// <summary>
    /// Sets the bias mode for the network.
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <param name="mode">The bias mode ("DEFAULT", "ALL_POSITIVE", "ALL_NEGATIVE", or "DISABLED").</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig WithBiasMode(this NEATConfig config, string mode)
    {
        config.BiasMode = mode;
        return config;
    }

    /// <summary>
    /// Disables recurrent connections between neurons.
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig DisableRecurrentConnections(this NEATConfig config)
    {
        config.AllowRecurrentConnections = false;
        return config;
    }

    /// <summary>
    /// Sets the mutation rates for various genetic operations.
    /// </summary>
    /// <param name="config">The configuration to modify.</param>
    /// <param name="weight">The mutation rate for connection weights.</param>
    /// <param name="addConnection">The mutation rate for adding new connections.</param>
    /// <param name="addNode">The mutation rate for adding new nodes.</param>
    /// <returns>The modified configuration for method chaining.</returns>
    public static NEATConfig WithMutationRates(this NEATConfig config, 
        double weight = 0.8, double addConnection = 0.05, double addNode = 0.03)
    {
        config.WeightMutationRate = weight;
        config.AddConnectionMutationRate = addConnection;
        config.AddNodeMutationRate = addNode;
        return config;
    }
}
