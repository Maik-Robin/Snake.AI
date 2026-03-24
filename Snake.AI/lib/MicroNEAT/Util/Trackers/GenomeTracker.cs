namespace MicroNEAT.Util.Trackers;

/// <summary>
/// Tracks and assigns unique IDs to genomes within a population.
/// Ensures each genome has a unique identifier for tracking and debugging.
/// </summary>
public class GenomeTracker
{
    private int _genomeId = 0;

    /// <summary>
    /// Gets the next available genome ID and increments the counter.
    /// </summary>
    /// <returns>A unique genome ID.</returns>
    public int GetNextGenomeId()
    {
        return _genomeId++;
    }
}
