using MicroNEAT.Config;
using MicroNEAT.Core.Genome.Genes.NodeGene;

namespace MicroNEAT.Core.Genome.Genes.GeneticEncoding;

/// <summary>
/// Represents encoded data for a node gene, used for genetic operations.
/// Contains the minimal information needed to represent a node in genetic encoding.
/// </summary>
public class NodeGeneData
{
    /// <summary>
    /// Gets or sets the unique identifier of the node.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the type of the node (INPUT, OUTPUT, HIDDEN, or BIAS).
    /// </summary>
    public NodeType NodeType { get; set; }

    /// <summary>
    /// Initializes a new instance of the NodeGeneData class.
    /// </summary>
    /// <param name="id">The node ID.</param>
    /// <param name="nodeType">The type of node.</param>
    public NodeGeneData(int id, NodeType nodeType)
    {
        Id = id;
        NodeType = nodeType;
    }
}
