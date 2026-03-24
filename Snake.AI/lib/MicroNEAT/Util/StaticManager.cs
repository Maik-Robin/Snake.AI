namespace MicroNEAT.Util;

/// <summary>
/// Singleton manager that maintains trackers for multiple populations.
/// Provides centralized access to innovation, genome, and node trackers.
/// Each population has its own set of trackers to maintain independent ID spaces.
/// </summary>
public class StaticManager
{
    private static StaticManager? _instance;

    /// <summary>
    /// Gets the singleton instance of the StaticManager.
    /// </summary>
    public static StaticManager Instance => _instance ??= new StaticManager();

    private readonly Dictionary<int, Trackers.InnovationTracker> _innovationTrackerMap = new();
    private readonly Dictionary<int, Trackers.GenomeTracker> _genomeTrackerMap = new();
    private readonly Dictionary<int, Trackers.NodeTracker> _nodeTrackerMap = new();

    private StaticManager()
    {
    }

    /// <summary>
    /// Gets or creates an innovation tracker for the specified population.
    /// </summary>
    /// <param name="populationId">The ID of the population.</param>
    /// <returns>The innovation tracker for the population.</returns>
    public Trackers.InnovationTracker GetInnovationTracker(int populationId)
    {
        if (_innovationTrackerMap.TryGetValue(populationId, out var tracker))
        {
            return tracker;
        }

        var newTracker = new Trackers.InnovationTracker();
        _innovationTrackerMap[populationId] = newTracker;
        return newTracker;
    }

    /// <summary>
    /// Gets or creates a genome tracker for the specified population.
    /// </summary>
    /// <param name="populationId">The ID of the population.</param>
    /// <returns>The genome tracker for the population.</returns>
    public Trackers.GenomeTracker GetGenomeTracker(int populationId)
    {
        if (_genomeTrackerMap.TryGetValue(populationId, out var tracker))
        {
            return tracker;
        }

        var newTracker = new Trackers.GenomeTracker();
        _genomeTrackerMap[populationId] = newTracker;
        return newTracker;
    }

    /// <summary>
    /// Gets or creates a node tracker for the specified population.
    /// </summary>
    /// <param name="populationId">The ID of the population.</param>
    /// <returns>The node tracker for the population.</returns>
    public Trackers.NodeTracker GetNodeTracker(int populationId)
    {
        if (_nodeTrackerMap.TryGetValue(populationId, out var tracker))
        {
            return tracker;
        }

        var newTracker = new Trackers.NodeTracker();
        _nodeTrackerMap[populationId] = newTracker;
        return newTracker;
    }
}
