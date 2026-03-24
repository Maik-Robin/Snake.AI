using MicroNEAT.Config;

namespace MicroNEAT.Util.Trackers;

/// <summary>
/// Tracks and stores the configuration for a population.
/// Provides a wrapper for accessing NEAT configuration settings.
/// </summary>
public class ConfigTracker
{
    private readonly NEATConfig _config;

    /// <summary>
    /// Initializes a new instance of the ConfigTracker class.
    /// </summary>
    /// <param name="config">The NEAT configuration to track.</param>
    public ConfigTracker(NEATConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Retrieves the tracked configuration.
    /// </summary>
    /// <returns>The NEAT configuration instance.</returns>
    public NEATConfig GetConfig()
    {
        return _config;
    }
}
