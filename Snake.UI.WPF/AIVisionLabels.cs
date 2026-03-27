namespace Snake;

/// <summary>
/// Provides human-readable labels for AI neural network inputs and outputs.
/// </summary>
public static class AIVisionLabels
{
    /// <summary>
    /// Gets the standard input labels for the Snake AI (11 inputs).
    /// </summary>
    public static string[] GetSnakeInputLabels()
    {
        return new[]
        {
            "Danger Straight",
            "Danger Left",
            "Danger Right",
            "Distance to Tail (Front)",
            "Distance to Tail (Left)",
            "Distance to Tail (Right)",
            "Food Right",
            "Food Down",
            "Direction: Up",
            "Direction: Right",
            "Direction: Down",
            "Distance to Wall (Front)",
            "Distance to Wall (Left)",
            "Distance to Wall (Right)",
        };
    }

    /// <summary>
    /// Gets the standard output labels for the Snake AI (3 outputs).
    /// </summary>
    public static string[] GetSnakeOutputLabels()
    {
        return new[]
        {
            "Turn Left",
            "Go Straight",
            "Turn Right"
        };
    }

    /// <summary>
    /// Gets a detailed description for a specific input index.
    /// </summary>
    /// <param name="index">The input index (0-10).</param>
    /// <returns>A description of what the input represents.</returns>
    public static string GetInputDescription(int index)
    {
        return index switch
        {
            0 => "1 if danger (wall/body) straight ahead, 0 otherwise",
            1 => "1 if danger (wall/body) to the left, 0 otherwise",
            2 => "1 if danger (wall/body) to the right, 0 otherwise",
            3 => "Normalized distance to nearest tail segment straight ahead (0-1)",
            4 => "Normalized distance to nearest tail segment to the left (0-1)",
            5 => "Normalized distance to nearest tail segment to the right (0-1)",
            6 => "1 if food is to the right, 0 otherwise",
            7 => "1 if food is below, 0 otherwise",
            8 => "1 if current direction is Up, 0 otherwise",
            9 => "1 if current direction is Right, 0 otherwise",
            10 => "1 if current direction is Down, 0 otherwise",
            _ => $"Input {index}"
        };
    }

    /// <summary>
    /// Gets a detailed description for a specific output index.
    /// </summary>
    /// <param name="index">The output index (0-2).</param>
    /// <returns>A description of what the output represents.</returns>
    public static string GetOutputDescription(int index)
    {
        return index switch
        {
            0 => "Turn left relative to current direction",
            1 => "Continue straight in current direction",
            2 => "Turn right relative to current direction",
            _ => $"Output {index}"
        };
    }
}
