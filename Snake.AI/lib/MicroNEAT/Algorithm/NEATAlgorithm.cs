using MicroNEAT.Config;
using MicroNEAT.Core.Population;

namespace MicroNEAT.Algorithm;

/// <summary>
/// Implements the main NEAT (NeuroEvolution of Augmenting Topologies) algorithm.
/// Orchestrates the evolutionary process including population initialization, evaluation, and evolution.
/// </summary>
public class NEATAlgorithm
{
    private readonly NEATConfig _config;
    private readonly Population _population;

    /// <summary>
    /// Initializes a new instance of the NEATAlgorithm class.
    /// Creates the initial population of genomes based on the provided configuration.
    /// </summary>
    /// <param name="config">The configuration settings for the NEAT algorithm.</param>
    public NEATAlgorithm(NEATConfig config)
    {
        _config = config;
        _population = new Population(config);
    }

    /// <summary>
    /// Runs the NEAT algorithm for the configured number of generations.
    /// Evolves the population until the target fitness is reached or max generations is hit.
    /// Outputs generation progress to the console.
    /// </summary>
    public void Run()
    {
        _population.EvaluatePopulation();
        
        for (int i = 0; i < _config.Generations; i++)
        {
            _population.Evolve();
            _population.EvaluatePopulation();
            double bestFitness = _population.GetBestGenome().Fitness;

            Console.WriteLine($"Generation: {_population.Generation} best fitness: {bestFitness:F6}");
            
            if (bestFitness >= _config.TargetFitness)
            {
                Console.WriteLine("Target fitness reached");
                break;
            }
        }
    }

    /// <summary>
    /// Gets the genome with the highest fitness from the current population.
    /// </summary>
    /// <returns>The best performing genome.</returns>
    public Core.Genome.Genome GetBestGenome()
    {
        return _population.GetBestGenome();
    }
}
