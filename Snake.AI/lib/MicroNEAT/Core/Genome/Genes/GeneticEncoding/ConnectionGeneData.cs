namespace MicroNEAT.Core.Genome.Genes.GeneticEncoding;

/// <summary>
/// Represents encoded data for a connection gene, used for genetic operations.
/// Contains all information needed to represent a connection during crossover and compatibility calculations.
/// </summary>
public class ConnectionGeneData
{
    /// <summary>
    /// Gets or sets the ID of the source (input) node.
    /// </summary>
    public int InNodeId { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the destination (output) node.
    /// </summary>
    public int OutNodeId { get; set; }
    
    /// <summary>
    /// Gets or sets the weight (strength) of the connection.
    /// </summary>
    public double Weight { get; set; }
    
    /// <summary>
    /// Gets or sets whether the connection is enabled (active).
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Gets or sets the innovation number that uniquely identifies this connection type.
    /// </summary>
    public int InnovationNumber { get; set; }
    
    /// <summary>
    /// Gets or sets whether this connection is recurrent (creates a feedback loop).
    /// </summary>
    public bool Recurrent { get; set; }

    /// <summary>
    /// Initializes a new instance of the ConnectionGeneData class.
    /// </summary>
    /// <param name="inNodeId">The source node ID.</param>
    /// <param name="outNodeId">The destination node ID.</param>
    /// <param name="weight">The connection weight.</param>
    /// <param name="enabled">Whether the connection is enabled.</param>
    /// <param name="innovationNumber">The innovation number.</param>
    /// <param name="recurrent">Whether the connection is recurrent.</param>
    public ConnectionGeneData(int inNodeId, int outNodeId, double weight, bool enabled, int innovationNumber, bool recurrent)
    {
        InNodeId = inNodeId;
        OutNodeId = outNodeId;
        Weight = weight;
        Enabled = enabled;
        InnovationNumber = innovationNumber;
        Recurrent = recurrent;
    }
}
