using MicroNEAT.Config;
using MicroNEAT.Core.Genome;
using MicroNEAT.Util;
using MicroNEAT.Util.Trackers;

namespace MicroNEAT.Core.Population;

/// <summary>
/// Represents a population of genomes that evolves over generations.
/// Manages speciation, reproduction, mutation, and fitness evaluation.
/// </summary>
public class Population
{
    /// <summary>
    /// Gets or sets the current generation of genomes in the population.
    /// </summary>
    public List<Genome.Genome> Genomes { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of species in the population.
    /// Genomes are grouped into species based on structural similarity.
    /// </summary>
    public List<Species> Species { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of genomes for the next generation (work in progress during evolution).
    /// </summary>
    public List<Genome.Genome> NewGeneration { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of elite genomes preserved from the previous generation.
    /// </summary>
    public List<Genome.Genome> EliteGenomes { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the NEAT configuration settings.
    /// </summary>
    public NEATConfig Config { get; set; }
    
    /// <summary>
    /// Gets or sets whether all species in the population are stagnated.
    /// </summary>
    public bool AllStagnated { get; set; }
    
    /// <summary>
    /// Gets or sets whether the population is stale (not improving).
    /// </summary>
    public bool Stale { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier for this population.
    /// </summary>
    public int PopulationId { get; set; }
    
    /// <summary>
    /// Gets or sets the innovation tracker for this population.
    /// </summary>
    public InnovationTracker InnovationTracker { get; set; }
    
    /// <summary>
    /// Gets or sets the current generation number.
    /// </summary>
    public int Generation { get; set; }
    
    private int _speciesCounter = 0;
    
    /// <summary>
    /// Gets or sets the best fitness achieved by any genome in the population's history.
    /// </summary>
    public double BestFitness { get; set; }
    
    /// <summary>
    /// Gets or sets the number of generations since the last fitness improvement.
    /// </summary>
    public int AgeSinceLastImprovement { get; set; }
    
    private bool _speciated = false;

    /// <summary>
    /// Initializes a new instance of the Population class.
    /// Creates an initial population of genomes with random weights.
    /// </summary>
    /// <param name="config">The NEAT configuration settings.</param>
    public Population(NEATConfig config)
    {
        Config = config;
        AllStagnated = false;
        Stale = false;
        PopulationId = PopulationTracker.GetNextPopulationId();
        InnovationTracker = StaticManager.Instance.GetInnovationTracker(PopulationId);
        Generation = 0;
        BestFitness = 0;
        AgeSinceLastImprovement = 0;

        var baseGenome = GenomeBuilder.BuildGenome(config, PopulationId);

        for (int i = 0; i < config.PopulationSize; i++)
        {
            Genomes.Add(baseGenome.Copy());
        }

        foreach (var genome in Genomes)
        {
            genome.ReinitializeWeights();
        }
    }

    /// <summary>
    /// Divides the population into species based on genetic similarity.
    /// Genomes within a compatibility threshold are grouped into the same species.
    /// </summary>
    public void Speciate()
    {
        foreach (var species in Species)
        {
            species.Genomes.Clear();
        }

        foreach (var genome in Genomes)
        {
            bool speciesFound = false;
            foreach (var species in Species)
            {
                var representative = species.Representative;
                if (representative == null)
                    continue;

                if (genome.GetGeneticEncoding().CalculateCompatibilityDistance(
                    representative.GetGeneticEncoding()) < Config.CompatibilityThreshold)
                {
                    species.AddGenome(genome);
                    speciesFound = true;
                    break;
                }
            }

            if (!speciesFound)
            {
                var newSpecies = new Species(_speciesCounter++, Config);
                newSpecies.AddGenome(genome);
                Species.Add(newSpecies);
            }
        }

        Species = Species.Where(s => s.Genomes.Count > 0).ToList();
        foreach (var species in Species)
        {
            species.SetRandomRepresentative();
        }

        _speciated = true;
    }

    /// <summary>
    /// Evolves the population by performing selection, crossover, and mutation.
    /// </summary>
    public void Evolve()
    {
        Stale = false;
        AllStagnated = false;
        NewGeneration = new List<Genome.Genome>();
        EliteGenomes = new List<Genome.Genome>();

        if (!_speciated)
        {
            Speciate();
        }

        InnovationTracker.Reset();
        SaveEliteGenomes();
        HandleStagnation();
        RemoveWorstGenomes();
        CalculateOffspring();
        GenerateOffspring();
        PutBackElite();

        Genomes.Clear();
        Genomes.AddRange(NewGeneration);

        Generation++;
        _speciated = false;
    }

    /// <summary>
    /// Evaluates the fitness of each genome in the population.
    /// </summary>
    public void EvaluatePopulation()
    {
        foreach (var genome in Genomes)
        {
            genome.EvaluateFitness();
        }
    }

    private void SaveEliteGenomes()
    {
        Genomes = Genomes.OrderByDescending(g => g.Fitness).ToList();

        foreach (var species in Species)
        {
            if (species.Genomes.Count > 5)
            {
                EliteGenomes.Add(species.GetBestGenome().Copy());
            }
        }

        int index = 0;
        while (EliteGenomes.Count < Config.NumOfElite && index < Genomes.Count)
        {
            var candidate = Genomes[index];
            bool isDuplicate = EliteGenomes.Any(elite => 
                elite == candidate || elite.EqualsGenome(candidate));

            if (!isDuplicate)
            {
                EliteGenomes.Add(candidate.Copy());
            }
            index++;
        }
    }

    private void HandleStagnation()
    {
        UpdateFitnessAndStagnation();
        foreach (var species in Species)
        {
            species.UpdateFitnessAndStagnation();
        }

        AllStagnated = Species.All(s => s.Stagnated);
        RemoveStale();
    }

    private void UpdateFitnessAndStagnation()
    {
        double currentBest = GetBestGenome().Fitness;
        if (currentBest > BestFitness)
        {
            BestFitness = currentBest;
            AgeSinceLastImprovement = 0;
        }
        else
        {
            AgeSinceLastImprovement++;
        }

        if (AgeSinceLastImprovement > Config.PopulationStagnationLimit)
        {
            Stale = true;
        }
    }

    private void RemoveStale()
    {
        if (AllStagnated)
        {
            var bestSpecies = SelectBestSpecies();
            Species.Clear();
            Species.Add(bestSpecies);
        }
        else
        {
            Species = Species.Where(s => !s.Stagnated).ToList();
        }
    }

    private void RemoveWorstGenomes()
    {
        foreach (var species in Species)
        {
            species.RemoveBadGenomes();
        }
    }

    private void CalculateOffspring()
    {
        double totalAdjustedFitness = 0;
        int remainingPopulation = Config.PopulationSize - EliteGenomes.Count;

        foreach (var species in Species)
        {
            species.SetAdjustedFitness();
            totalAdjustedFitness += species.GetTotalAdjustedFitness();
        }

        var percentageOfOffspring = Species.Select(s =>
            (s.GetTotalAdjustedFitness() / totalAdjustedFitness) * 100
        ).ToList();

        for (int i = 0; i < Species.Count; i++)
        {
            int count = (int)Math.Floor((percentageOfOffspring[i] / 100) * remainingPopulation);
            Species[i].OffspringCount = count;
        }

        int totalOffspring = Species.Sum(s => s.OffspringCount);

        if (totalOffspring > remainingPopulation)
        {
            int difference = totalOffspring - remainingPopulation;
            var worstSpecies = Species.OrderBy(s => s.GetTotalAdjustedFitness()).First();
            worstSpecies.OffspringCount -= difference;
        }

        if (totalOffspring < remainingPopulation)
        {
            int difference = remainingPopulation - totalOffspring;
            for (int i = 0; i < difference; i++)
            {
                var bestSpecies = SelectBestSpecies();
                bestSpecies.OffspringCount++;
            }
        }
    }

    private Species SelectBestSpecies()
    {
        return Species.OrderByDescending(s => s.BestFitness).First();
    }

    private void GenerateOffspring()
    {
        foreach (var species in Species)
        {
            int offspringCount = species.OffspringCount;
            var mutatedOnlyGenomes = new List<Genome.Genome>();

            for (int i = 0; i < offspringCount; i++)
            {
                int genomesInSpecies = species.Genomes.Count;

                if (Random.Shared.NextDouble() < Config.MutateOnlyProb)
                {
                    var selectedGenome = species.Genomes[Random.Shared.Next(genomesInSpecies)];
                    while (genomesInSpecies > 1 && 
                           mutatedOnlyGenomes.Contains(selectedGenome) && 
                           i < genomesInSpecies)
                    {
                        selectedGenome = species.Genomes[Random.Shared.Next(genomesInSpecies)];
                    }
                    mutatedOnlyGenomes.Add(selectedGenome);
                    var offspring = selectedGenome.Copy();
                    offspring.Mutate();
                    NewGeneration.Add(offspring);
                    continue;
                }

                if (Random.Shared.NextDouble() < Config.InterspeciesMatingRate && Species.Count > 1)
                {
                    var randomSpecies = Species[Random.Shared.Next(Species.Count)];
                    while (randomSpecies == species)
                    {
                        randomSpecies = Species[Random.Shared.Next(Species.Count)];
                    }
                    var parent1 = species.Genomes[Random.Shared.Next(species.Genomes.Count)];
                    var parent2 = randomSpecies.Genomes[Random.Shared.Next(randomSpecies.Genomes.Count)];
                    var offspring = parent1.Crossover(parent2);
                    if (Random.Shared.NextDouble() <= Config.MutationRate)
                    {
                        offspring.Mutate();
                    }
                    NewGeneration.Add(offspring);
                    continue;
                }

                Genome.Genome offspring2;
                if (species.Genomes.Count > 1)
                {
                    bool parentsFound = false;
                    Genome.Genome parent1, parent2;
                    do
                    {
                        parent1 = species.Genomes[Random.Shared.Next(species.Genomes.Count)];
                        parent2 = species.Genomes[Random.Shared.Next(species.Genomes.Count)];
                        if (parent1 != parent2)
                        {
                            parentsFound = true;
                        }
                    } while (!parentsFound);

                    offspring2 = parent1.Crossover(parent2);
                    if (Random.Shared.NextDouble() <= Config.MutationRate)
                    {
                        offspring2.Mutate();
                    }
                }
                else
                {
                    offspring2 = species.Genomes[0].Copy();
                    offspring2.Mutate();
                }
                NewGeneration.Add(offspring2);
            }
        }
    }

    private void PutBackElite()
    {
        foreach (var eliteGenome in EliteGenomes)
        {
            NewGeneration.Add(eliteGenome);
        }
    }

    /// <summary>
    /// Gets the genome with the highest fitness in the population.
    /// </summary>
    /// <returns>The best genome.</returns>
    public Genome.Genome GetBestGenome()
    {
        return Genomes.OrderByDescending(g => g.Fitness).First();
    }
}
