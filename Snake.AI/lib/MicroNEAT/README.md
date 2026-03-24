# MicroNEAT

MicroNEAT: A minimal, efficient C# library for evolving neural network topologies. Genetic algorithms meet .NET.

## Overview

NEAT is a genetic algorithm for evolving artificial neural networks. This implementation includes:

- **Activation Functions**: Sigmoid, Tanh, ReLU, LeakyReLU, Gaussian, NEATSigmoid, SELU
- **Fitness Functions**: XOR (extensible for custom fitness functions)
- **Weight Initialization**: Random weight initialization
- **Genetic Operations**: Mutation (weight, add connection, add node), Crossover, Speciation
- **Population Management**: Elite preservation, species tracking, stagnation detection

## Features

- **Fully functional NEAT algorithm** - Complete implementation of the NEAT paper  
- **Multiple activation functions** - 7 built-in activation functions  
- **Speciation with compatibility distance** - Automatic species formation  
- **Structural and weight mutations** - Evolve network topology and weights  
- **Crossover between genomes** - Reproduction with innovation tracking  
- **Elite preservation** - Keep best genomes across generations  
- **Stagnation detection** - Automatic handling of stagnant species  
- **Recurrent connections support** - Optional recurrent neural networks  
- **Configurable bias modes** - Multiple bias handling strategies  

## Project Structure

```
MicroNEAT/
- ActivationFunctions       # Neural network activation functions
- Algorithm                 # Main NEAT algorithm
- Config                    # Configuration settings
- Core                      # Genome representation and operations & Population and species management
- FitnessFunctions          # Fitness evaluation functions
- Util                      # Utilities and trackers
- WeightInitialization      # Weight initialization strategies

MicroNEAT/Examples          # Example usage of the NEAT algorithm

MicroNEAT/Tests             # Unit tests for various components
```


## Quick Start

```csharp
using MicroNEAT.Algorithm;
using MicroNEAT.Config;
using MicroNEAT.ActivationFunctions;
using MicroNEAT.FitnessFunctions;

// Create configuration
var config = new NEATConfig
{
    InputSize = 2,
    OutputSize = 1,
    PopulationSize = 150,
    Generations = 100,
    TargetFitness = 0.95,
    ActivationFunction = new Sigmoid(),
    FitnessFunction = new XOR()
};

// Run the algorithm
var algorithm = new NEATAlgorithm(config);
algorithm.Run();

// Get the best genome
var bestGenome = algorithm.GetBestGenome();

// Test the genome
bestGenome.ResetState();
var output = bestGenome.Propagate(new[] { 1.0, 0.0 });
Console.WriteLine($"Output: {output[0]:F4}");
```

## Using Extension Methods (Fluent API)

```csharp
var config = new NEATConfig()
    .WithInputSize(2)
    .WithOutputSize(1)
    .WithPopulation(100)
    .WithGenerations(50)
    .WithTargetFitness(0.95)
    .WithActivationFunction(new Tanh())
    .WithFitnessFunction(new XOR())
    .WithMutationRates(weight: 0.8, addConnection: 0.1, addNode: 0.05);
```

## Configuration Options

| Parameter | Default | Description |
|-----------|---------|-------------|
| `InputSize` | 2 | Number of input nodes |
| `OutputSize` | 1 | Number of output nodes |
| `PopulationSize` | 150 | Number of genomes in population |
| `Generations` | 100 | Maximum generations to evolve |
| `TargetFitness` | 0.95 | Target fitness to reach (stops early if achieved) |
| `ActivationFunction` | Sigmoid | Activation function for neurons |
| `Bias` | 1.0 | Bias value |
| `ConnectBias` | true | Connect bias to output nodes initially |
| `BiasMode` | WEIGHTED_NODE | Bias handling: WEIGHTED_NODE, DIRECT_NODE, CONSTANT, DISABLED |
| `AllowRecurrentConnections` | true | Allow recurrent connections |
| `RecurrentConnectionRate` | 1.0 | Probability of accepting recurrent connections |
| `MinWeight` | -4.0 | Minimum connection weight |
| `MaxWeight` | 4.0 | Maximum connection weight |
| `MutationRate` | 1.0 | Overall mutation probability |
| `WeightMutationRate` | 0.8 | Probability of weight mutation |
| `AddConnectionMutationRate` | 0.05 | Probability of adding connection |
| `AddNodeMutationRate` | 0.03 | Probability of adding node |
| `ReinitializeWeightRate` | 0.1 | Probability of completely reinitializing a weight |
| `MinPerturb` | -0.5 | Minimum weight perturbation |
| `MaxPerturb` | 0.5 | Maximum weight perturbation |
| `CompatibilityThreshold` | 3.0 | Threshold for speciation |
| `C1` | 1.0 | Excess genes coefficient |
| `C2` | 1.0 | Disjoint genes coefficient |
| `C3` | 0.4 | Weight difference coefficient |
| `SurvivalRate` | 0.2 | Survival rate per species (20% survive) |
| `NumOfElite` | 10 | Number of elite genomes to preserve |
| `PopulationStagnationLimit` | 20 | Generations before population is considered stagnant |
| `DropOffAge` | 15 | Generations before species is considered stagnant |
| `InterspeciesMatingRate` | 0.001 | Probability of mating between different species |
| `MutateOnlyProb` | 0.25 | Probability of mutation without crossover |
| `KeepDisabledOnCrossOverRate` | 0.75 | Probability of keeping disabled genes |

## Custom Fitness Functions

Implement the `IFitnessFunction` interface:

```csharp
using MicroNEAT.FitnessFunctions;
using MicroNEAT.Core.Genome;

public class MyCustomFitness : IFitnessFunction
{
    public double CalculateFitness(Genome genome)
    {
        // Evaluate your genome here
        // Return a fitness score (higher is better)
        
        double totalError = 0;
        for (int i = 0; i < testCases.Length; i++)
        {
            genome.ResetState();
            var output = genome.Propagate(testCases[i].Input);
            totalError += Math.Pow(output[0] - testCases[i].Expected, 2);
        }
        
        return 1.0 / (1.0 + totalError);
    }
}

// Use it in config
config.FitnessFunction = new MyCustomFitness();
```

## Custom Activation Functions

Implement the `IActivationFunction` interface:

```csharp
using MicroNEAT.ActivationFunctions;

public class CustomActivation : IActivationFunction
{
    public double Apply(double value)
    {
        // Your activation function logic
        return Math.Sin(value); // Example: sine activation
    }
}

// Use it in config
config.ActivationFunction = new CustomActivation();
```

## Activation Functions

### Built-in Activation Functions

- **Sigmoid**: `1 / (1 + e^(-x))` - Standard sigmoid (0 to 1)
- **NEATSigmoid**: `1 / (1 + e^(-4.9x))` - Steeper sigmoid as used in original NEAT
- **Tanh**: `tanh(x)` - Hyperbolic tangent (-1 to 1)
- **ReLU**: `max(0, x)` - Rectified Linear Unit
- **LeakyReLU**: `x > 0 ? x : 0.01x` - Leaky ReLU with configurable alpha
- **Gaussian**: `e^(-((x-c)/w)^2)` - Gaussian with configurable center and width
- **SELU**: Scaled Exponential Linear Unit

## How NEAT Works

1. **Initialize Population**: Creates a population of simple neural networks (genomes)
2. **Evaluate Fitness**: Each genome is tested using the fitness function
3. **Speciate**: Genomes are grouped into species based on structural similarity
4. **Select for Reproduction**: Better genomes have higher chance of reproduction
5. **Crossover**: Combine two parent genomes to create offspring
6. **Mutate**: Randomly modify weights or add new nodes/connections
7. **Repeat**: Continue evolution until target fitness is reached or max generations

### Key NEAT Concepts

- **Innovation Numbers**: Track historical origin of genes for crossover alignment
- **Speciation**: Protect innovation by grouping similar topologies
- **Structural Mutation**: Networks can grow in complexity (add nodes/connections)
- **Complexification**: Start simple and add complexity as needed

## Examples

### Basic XOR

```csharp
var config = new NEATConfig
{
    InputSize = 2,
    OutputSize = 1,
    TargetFitness = 0.95,
    FitnessFunction = new XOR()
};

var algorithm = new NEATAlgorithm(config);
algorithm.Run();
```

### With Custom Settings

```csharp
var config = new NEATConfig
{
    InputSize = 4,
    OutputSize = 2,
    PopulationSize = 200,
    Generations = 150,
    ActivationFunction = new Tanh(),
    BiasMode = "DIRECT_NODE",
    AllowRecurrentConnections = false,
    WeightMutationRate = 0.9,
    AddConnectionMutationRate = 0.08,
    AddNodeMutationRate = 0.05
};
```

## Troubleshooting

### Stack Overflow
If you encounter stack overflow errors, check:
- Ensure `ResetState()` is called before each `Propagate()` call
- Check if recurrent connections are properly marked
- Verify fitness function properly resets genome state

### Slow Evolution
If evolution is slow:
- Increase mutation rates
- Reduce population size for faster generations
- Adjust compatibility threshold for better speciation
- Increase survival rate to preserve more diversity

### Not Reaching Target Fitness
If target fitness is not reached:
- Increase population size
- Increase number of generations
- Adjust mutation rates (more structural mutations)
- Lower target fitness threshold
- Check fitness function implementation

## Performance Tips

- Use smaller populations for faster experimentation (50-100)
- Start with higher mutation rates for more exploration
- Use elite preservation to maintain good solutions
- Monitor species count (5-15 species is typically good)

## Credits

Original NEAT algorithm by Kenneth O. Stanley: [Evolving Neural Networks through Augmenting Topologies](http://nn.cs.utexas.edu/downloads/papers/stanley.ec02.pdf)

