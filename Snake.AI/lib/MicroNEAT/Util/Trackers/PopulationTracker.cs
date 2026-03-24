namespace MicroNEAT.Util.Trackers;

/// <summary>
/// Tracks and assigns unique IDs to populations.
/// Provides a static counter for population identification across the application.
/// </summary>
public class PopulationTracker
{
    private static int _populationId = 0;

    /// <summary>
    /// Gets the next available population ID and increments the static counter.
    /// </summary>
    /// <returns>A unique population ID.</returns>
    public static int GetNextPopulationId()
    {
        return _populationId++;
    }
}
