namespace MicroNEAT.Util.Trackers;

/// <summary>
/// Tracks and assigns unique IDs to nodes within a population.
/// Ensures each node has a globally unique identifier.
/// </summary>
public class NodeTracker
{
    private int _nodeId = 0;

    /// <summary>
    /// Gets the next available node ID and increments the counter.
    /// </summary>
    /// <returns>A unique node ID.</returns>
    public int GetNextNodeId()
    {
        return _nodeId++;
    }
}
