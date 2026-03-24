using MicroNEAT.Config;

namespace MicroNEAT.Core.Genome.Genes.NodeGene;

/// <summary>
/// Abstract base class for all node genes in the neural network.
/// Represents a neuron in the evolved network topology.
/// </summary>
public abstract class NodeGene
{
    /// <summary>
    /// Gets or sets the unique identifier for this node.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the configuration settings for the NEAT algorithm.
    /// </summary>
    public NEATConfig Config { get; set; }
    
    /// <summary>
    /// Gets or sets the most recent output value produced by this node.
    /// </summary>
    public double LastOutput { get; set; }
    
    /// <summary>
    /// Gets or sets the number of inputs this node expects to receive before activation.
    /// </summary>
    public int ExpectedInputs { get; set; }
    
    /// <summary>
    /// Gets or sets the number of inputs this node has received in the current propagation cycle.
    /// </summary>
    public int ReceivedInputs { get; set; }
    
    /// <summary>
    /// Gets the type of this node (INPUT, OUTPUT, HIDDEN, or BIAS).
    /// </summary>
    public abstract NodeType NodeType { get; }

    /// <summary>
    /// Initializes a new instance of the NodeGene class.
    /// </summary>
    /// <param name="id">The unique identifier for this node.</param>
    /// <param name="config">The NEAT configuration settings.</param>
    protected NodeGene(int id, NEATConfig config)
    {
        Id = id;
        Config = config;
        LastOutput = 0;
        ExpectedInputs = 0;
        ReceivedInputs = 0;
    }

    /// <summary>
    /// Resets the node's state to initial values.
    /// Called between propagation cycles to clear previous computations.
    /// </summary>
    public virtual void ResetState()
    {
        LastOutput = 0;
        ExpectedInputs = 0;
        ReceivedInputs = 0;
    }

    /// <summary>
    /// Determines whether this node can receive incoming connections.
    /// </summary>
    /// <returns>True if the node accepts incoming connections, false otherwise.</returns>
    public virtual bool AcceptsIncomingConnections() => false;
    
    /// <summary>
    /// Determines whether this node can send outgoing connections.
    /// </summary>
    /// <returns>True if the node accepts outgoing connections, false otherwise.</returns>
    public virtual bool AcceptsOutgoingConnections() => false;
}
