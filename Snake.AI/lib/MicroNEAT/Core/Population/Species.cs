using MicroNEAT.Config;

namespace MicroNEAT.Core.Population;

/// <summary>
/// Represents a species within the population.
/// A species is a group of genomes with similar structures that compete primarily with each other.
/// This helps maintain diversity by protecting innovative structures from competition with established solutions.
/// </summary>
public class Species
{
    /// <summary>
    /// Gets or sets the unique identifier for this species.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the NEAT configuration settings.
    /// </summary>
    public NEATConfig Config { get; set; }
    
    /// <summary>
    /// Gets or sets the best fitness achieved by any genome in this species' history.
    /// </summary>
    public double BestFitness { get; set; }
    
    /// <summary>
    /// Gets or sets the number of generations since this species last improved its best fitness.
    /// </summary>
    public int GenerationsSinceImprovement { get; set; }
    
    /// <summary>
    /// Gets or sets whether this species is stagnated (no improvement for too long).
    /// </summary>
    public bool Stagnated { get; set; }
    
    /// <summary>
    /// Gets or sets the list of genomes belonging to this species.
    /// </summary>
    public List<Genome.Genome> Genomes { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the representative genome used to determine if new genomes belong to this species.
    /// </summary>
    public Genome.Genome? Representative { get; set; }
    
    /// <summary>
    /// Gets or sets the number of offspring this species is allocated for the next generation.
    /// </summary>
    public int OffspringCount { get; set; }

    /// <summary>
    /// Initializes a new instance of the Species class.
    /// </summary>
    /// <param name="id">The unique identifier for this species.</param>
    /// <param name="config">The NEAT configuration settings.</param>
    public Species(int id, NEATConfig config)
    {
        Id = id;
        Config = config;
        BestFitness = 0;
        GenerationsSinceImprovement = 0;
        Stagnated = false;
        OffspringCount = 0;
    }

    /// <summary>
    /// Adds a genome to this species. If this is the first genome, it becomes the representative.
    /// </summary>
    /// <param name="genome">The genome to add.</param>
    public void AddGenome(Genome.Genome genome)
    {
        if (Genomes.Count == 0)
        {
            Representative = genome;
        }
        Genomes.Add(genome);
    }

    /// <summary>
    /// Adjusts the fitness of all genomes in this species by dividing by species size.
    /// This implements fitness sharing to maintain diversity.
    /// </summary>
    public void SetAdjustedFitness()
    {
        foreach (var genome in Genomes)
        {
            genome.AdjustedFitness = genome.Fitness / Genomes.Count;
        }
    }

    /// <summary>
    /// Removes the worst performing genomes from this species based on the survival rate.
    /// Only the fittest individuals survive to reproduce.
    /// </summary>
    public void RemoveBadGenomes()
    {
        Genomes = Genomes.OrderByDescending(g => g.Fitness).ToList();

        int totalGenomes = Genomes.Count;
        int numberToSurvive = Math.Max(1, (int)Math.Floor(totalGenomes * Config.SurvivalRate));

        if (Genomes.Count > numberToSurvive)
        {
            Genomes = Genomes.Take(numberToSurvive).ToList();
        }
    }

    /// <summary>
    /// Updates the species' best fitness and tracks stagnation.
    /// Marks the species as stagnant if no improvement for DropOffAge generations.
    /// </summary>
    public void UpdateFitnessAndStagnation()
    {
        double currentBest = GetBestGenome().Fitness;
        if (currentBest > BestFitness)
        {
            BestFitness = currentBest;
            GenerationsSinceImprovement = 0;
        }
        else
        {
            GenerationsSinceImprovement++;
        }

        if (GenerationsSinceImprovement > Config.DropOffAge)
        {
            Stagnated = true;
        }
    }

    /// <summary>
    /// Calculates the sum of all adjusted fitness values in this species.
    /// </summary>
    /// <returns>The total adjusted fitness.</returns>
    public double GetTotalAdjustedFitness()
    {
        return Genomes.Sum(g => g.AdjustedFitness);
    }

    /// <summary>
    /// Gets the genome with the highest fitness in this species.
    /// </summary>
    /// <returns>The best genome in the species.</returns>
    public Genome.Genome GetBestGenome()
    {
        return Genomes.OrderByDescending(g => g.Fitness).First();
    }

    /// <summary>
    /// Selects a random genome from this species to serve as the representative.
    /// The representative is used for determining species membership of new genomes.
    /// </summary>
    public void SetRandomRepresentative()
    {
        if (Genomes.Count == 0)
        {
            Representative = null;
        }
        else
        {
            Representative = Genomes[Random.Shared.Next(Genomes.Count)];
        }
    }
}
