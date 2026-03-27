namespace MicroNEAT.FitnessFunctions;

/// <summary>
/// Defines the contract for fitness functions used to evaluate genome performance.
/// Fitness functions determine how well a genome solves a given problem.
/// </summary>
public interface IFitnessFunction
{
    /// <summary>
    /// Calculates the fitness score for a given genome.
    /// </summary>
    /// <param name="genome">The genome to evaluate.</param>
    /// <returns>A fitness score, where higher is better.</returns>
    double CalculateFitness(Core.Genome.Genome genome);


    /// <summary>
    /// Calculates the fitness score for a given genome and determines if it has achieved the maximum possible score.
    /// </summary>
    /// <param name="genome">The genome to evaluate.</param>
    /// <returns>A fitness score, where higher is better and if maxScore was achived</returns>
    (double fitness, bool maxScore) CalculateFitnessAndScore(Core.Genome.Genome genome);
}
