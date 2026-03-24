namespace MicroNEAT.Core.Genome.Genes.NodeGene;

/// <summary>
/// Specifies the type of node in the neural network.
/// </summary>
public enum NodeType
{
    /// <summary>
    /// Input node that receives external data.
    /// </summary>
    INPUT,
    
    /// <summary>
    /// Output node that produces the network's result.
    /// </summary>
    OUTPUT,
    
    /// <summary>
    /// Hidden node that processes information between input and output layers.
    /// </summary>
    HIDDEN,
    
    /// <summary>
    /// Bias node that provides a constant offset value.
    /// </summary>
    BIAS
}
