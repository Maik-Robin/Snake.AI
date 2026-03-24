using MicroNEAT.ActivationFunctions;
using MicroNEAT.FitnessFunctions;
using MicroNEAT.WeightInitialization;

namespace MicroNEAT.Config;

/// <summary>
/// Configuration class for the NEAT (NeuroEvolution of Augmenting Topologies) algorithm.
/// Contains all parameters needed to configure network topology, evolution, and training.
/// </summary>
public class NEATConfig
{
    /// <summary>
    /// Gets or sets the number of input nodes in the network.
    /// </summary>
    public int InputSize { get; set; }
    
    /// <summary>
    /// Gets or sets the number of output nodes in the network.
    /// </summary>
    public int OutputSize { get; set; }
    
    /// <summary>
    /// Gets or sets the activation function used by network nodes.
    /// </summary>
    public IActivationFunction ActivationFunction { get; set; }
    
    /// <summary>
    /// Gets or sets the bias value used in the network.
    /// </summary>
    public double Bias { get; set; }
    
    /// <summary>
    /// Gets or sets whether the bias node should be connected to the network.
    /// </summary>
    public bool ConnectBias { get; set; }
    
    /// <summary>
    /// Gets or sets the bias mode. Options: "WEIGHTED_NODE", "DIRECT_NODE", "CONSTANT", "DISABLED".
    /// </summary>
    public string BiasMode { get; set; }
    
    /// <summary>
    /// Gets or sets whether recurrent (feedback) connections are allowed.
    /// </summary>
    public bool AllowRecurrentConnections { get; set; }
    
    /// <summary>
    /// Gets or sets the rate at which recurrent connections are created when allowed.
    /// </summary>
    public double RecurrentConnectionRate { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum allowed weight value.
    /// </summary>
    public double MinWeight { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum allowed weight value.
    /// </summary>
    public double MaxWeight { get; set; }
    
    /// <summary>
    /// Gets or sets the strategy for initializing connection weights.
    /// </summary>
    public IWeightInitialization WeightInitialization { get; set; }
    
    /// <summary>
    /// Gets or sets the number of genomes in each generation.
    /// </summary>
    public int PopulationSize { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum number of generations to evolve.
    /// </summary>
    public int Generations { get; set; }
    
    /// <summary>
    /// Gets or sets the fitness function used to evaluate genomes.
    /// </summary>
    public IFitnessFunction FitnessFunction { get; set; }
    
    /// <summary>
    /// Gets or sets the target fitness value. Evolution stops when reached.
    /// </summary>
    public double TargetFitness { get; set; }
    
    /// <summary>
    /// Gets or sets the proportion of each species that survives to reproduce (0-1).
    /// </summary>
    public double SurvivalRate { get; set; }
    
    /// <summary>
    /// Gets or sets the number of elite genomes preserved unchanged each generation.
    /// </summary>
    public int NumOfElite { get; set; }
    
    /// <summary>
    /// Gets or sets the number of generations without improvement before population is considered stale.
    /// </summary>
    public int PopulationStagnationLimit { get; set; }
    
    /// <summary>
    /// Gets or sets the probability of mating between different species (0-1).
    /// </summary>
    public double InterspeciesMatingRate { get; set; }
    
    /// <summary>
    /// Gets or sets the probability of mutation without crossover (0-1).
    /// </summary>
    public double MutateOnlyProb { get; set; }
    
    /// <summary>
    /// Gets or sets the coefficient for excess genes in compatibility distance calculation.
    /// </summary>
    public double C1 { get; set; }
    
    /// <summary>
    /// Gets or sets the coefficient for disjoint genes in compatibility distance calculation.
    /// </summary>
    public double C2 { get; set; }
    
    /// <summary>
    /// Gets or sets the coefficient for weight differences in compatibility distance calculation.
    /// </summary>
    public double C3 { get; set; }
    
    /// <summary>
    /// Gets or sets the threshold for species compatibility. Lower values create more species.
    /// </summary>
    public double CompatibilityThreshold { get; set; }
    
    /// <summary>
    /// Gets or sets the number of generations without improvement before a species is considered stagnant.
    /// </summary>
    public int DropOffAge { get; set; }
    
    /// <summary>
    /// Gets or sets the overall probability that a genome will be mutated (0-1).
    /// </summary>
    public double MutationRate { get; set; }
    
    /// <summary>
    /// Gets or sets the probability of weight mutation occurring (0-1).
    /// </summary>
    public double WeightMutationRate { get; set; }
    
    /// <summary>
    /// Gets or sets the probability of adding a new connection mutation (0-1).
    /// </summary>
    public double AddConnectionMutationRate { get; set; }
    
    /// <summary>
    /// Gets or sets the probability of adding a new node mutation (0-1).
    /// </summary>
    public double AddNodeMutationRate { get; set; }
    
    /// <summary>
    /// Gets or sets the probability of completely reinitializing a weight during mutation (0-1).
    /// </summary>
    public double ReinitializeWeightRate { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum perturbation value for weight mutation.
    /// </summary>
    public double MinPerturb { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum perturbation value for weight mutation.
    /// </summary>
    public double MaxPerturb { get; set; }
    
    /// <summary>
    /// Gets or sets the probability of keeping disabled genes during crossover (0-1).
    /// </summary>
    public double KeepDisabledOnCrossOverRate { get; set; }

    /// <summary>
    /// Initializes a new instance of the NEATConfig class with default values.
    /// Default configuration is suitable for solving the e.g. XOR problem.
    /// </summary>
    public NEATConfig()
    {
        InputSize = 2;
        OutputSize = 1;
        ActivationFunction = new Sigmoid();
        Bias = 1.0;
        ConnectBias = true;
        BiasMode = "WEIGHTED_NODE";
        AllowRecurrentConnections = true;
        RecurrentConnectionRate = 1.0;
        MinWeight = -4.0;
        MaxWeight = 4.0;
        WeightInitialization = new RandomWeightInitialization(-1, 1);
        PopulationSize = 150;
        Generations = 100;
        TargetFitness = 0.95;
        SurvivalRate = 0.2;
        NumOfElite = 10;
        PopulationStagnationLimit = 20;
        InterspeciesMatingRate = 0.001;
        MutateOnlyProb = 0.25;
        C1 = 1.0;
        C2 = 1.0;
        C3 = 0.4;
        CompatibilityThreshold = 3.0;
        DropOffAge = 15;
        MutationRate = 1.0;
        WeightMutationRate = 0.8;
        AddConnectionMutationRate = 0.05;
        AddNodeMutationRate = 0.03;
        ReinitializeWeightRate = 0.1;
        MinPerturb = -0.5;
        MaxPerturb = 0.5;
        KeepDisabledOnCrossOverRate = 0.75;
    }
}
