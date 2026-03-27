using MicroNEAT.Config;
using MicroNEAT.Core.Genome;
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
    private Genome? _bestGenomeFromRun;
    private Genome? _bestGenomeFromEvaluation;
    private Double _bestFitnessFromRun;
    private Double _bestFitnessFromEvaluation;

    /// <summary>
    /// Raised after each generation has been evaluated.
    /// Parameters are the generation number and the best fitness value.
    /// </summary>
    public event Action<int, double>? OnGenerationComplete;

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
        _bestGenomeFromRun = null;
        _bestGenomeFromEvaluation = null;
        _bestFitnessFromRun = 0.0;
        _bestFitnessFromEvaluation = 0.0;

        _population.EvaluatePopulation();


        for (int i = 0; i < _config.Generations; i++)
        {
            _population.Evolve();
            _population.EvaluatePopulation();
            double bestFitness = _population.GetBestGenome().Fitness;
            if(bestFitness > _bestFitnessFromRun)
            {
                _bestFitnessFromRun = bestFitness;
                _bestGenomeFromRun = GetBestGenomeFromPopulation().Copy();
            }
            Console.WriteLine($"Generation: {_population.Generation} best fitness: {bestFitness:F6}");
            OnGenerationComplete?.Invoke(_population.Generation, bestFitness);

            if (bestFitness >= _config.TargetFitness)
            {
                Console.WriteLine("Target fitness reached");
                break;
            }
        }
    }

    /// <summary>
    /// Runs the NEAT algorithm for the configured number of generations.
    /// Evolves the population until the target fitness is reached or max generations is hit.
    /// Outputs generation progress to the console.
    /// </summary>
    public void RunAdvancedEvaluation(bool breakOnTargetFitness = false, Int32 evaluationRuns = 10)
    {
        _bestGenomeFromRun = null;
        _bestGenomeFromEvaluation = null;
        _bestFitnessFromRun = 0.0;
        _bestFitnessFromEvaluation = 0.0;

        _population.EvaluatePopulation();


        for (int i = 0; i < _config.Generations; i++)
        {
            _population.Evolve();
            _population.EvaluatePopulation();
            double bestFitness = _population.GetBestGenome().Fitness;
            if (bestFitness > _bestFitnessFromRun)
            {
                _bestFitnessFromRun = bestFitness;
                _bestGenomeFromRun = GetBestGenomeFromPopulation().Copy();
            }
            if(bestFitness >= _config.TargetFitness)
            {
                var fitnessFromEvaluation = EvaluateGenome(GetBestGenomeFromPopulation(), evaluationRuns);
                if(fitnessFromEvaluation > _bestFitnessFromEvaluation)
                {
                    _bestFitnessFromEvaluation = fitnessFromEvaluation;
                    _bestGenomeFromEvaluation = GetBestGenomeFromPopulation().Copy();
                }
            }
            Console.WriteLine($"Generation: {_population.Generation} best fitness: {bestFitness:F6}");
            OnGenerationComplete?.Invoke(_population.Generation, bestFitness);

            if (breakOnTargetFitness && bestFitness >= _config.TargetFitness)
            {
                Console.WriteLine("Target fitness reached");
                break;
            }
        }
    }

    public double EvaluateGenome(Genome genome, Int32 runs)
    {
        var sumFitness = 0.0;
        for (int i = 0; i < runs+1; i++)
        {
            var result = _config.FitnessFunction.CalculateFitnessAndScore(genome);
            sumFitness += result.fitness;
            if (result.maxScore)
            {
                sumFitness += 1000;
            }
        }
        return sumFitness / runs;
    }

    /// <summary>
    /// Gets the best genome found across all generations of the run after evaluation.
    /// </summary>
    /// <returns>The best performing genome.</returns>
    public Genome GetBestGenomeFromEvaluation()
    {
        if (_bestGenomeFromEvaluation == null)
        {
            if (_bestGenomeFromRun == null)
            {
                return GetBestGenomeFromPopulation();
            }
            return _bestGenomeFromRun;
        }
        return _bestGenomeFromEvaluation;
    }

    /// <summary>
    /// Gets the best genome found across all generations of the run by fitness.
    /// </summary>
    /// <returns>The best performing genome.</returns>
    public Genome GetBestGenomeFromRun()
    {
        if(_bestGenomeFromRun == null)
        {
            return GetBestGenomeFromPopulation();
        }
        return _bestGenomeFromRun;
    }

    /// <summary>
    /// Gets the genome with the highest fitness from the current population.
    /// </summary>
    /// <returns>The best performing genome.</returns>
    public Genome GetBestGenomeFromPopulation()
    {
        return _population.GetBestGenome();
    }
}
