using MicroNEAT.Config;
using ConnectionGeneBase = MicroNEAT.Core.Genome.Genes.ConnectionGene.ConnectionGene;

namespace MicroNEAT.Core.Genome.Genes.NodeGene;

/// <summary>
/// Represents a bias node in the neural network.
/// Bias nodes provide a constant offset value to help the network learn thresholds.
/// They only have outgoing connections and do not receive inputs.
/// </summary>
public class BiasNode : NodeGene
{
    /// <inheritdoc/>
    public override NodeType NodeType => NodeType.BIAS;
    
    /// <summary>
    /// Gets or sets the constant bias value this node provides.
    /// </summary>
    public double Bias { get; set; }
    
    /// <summary>
    /// Gets or sets the list of outgoing connections from this bias node.
    /// </summary>
    public List<ConnectionGeneBase> OutgoingConnections { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the BiasNode class.
    /// Sets the bias value and LastOutput to the configured bias value.
    /// </summary>
    /// <param name="id">The unique identifier for this node.</param>
    /// <param name="config">The NEAT configuration settings.</param>
    public BiasNode(int id, NEATConfig config) : base(id, config)
    {
        Bias = config.Bias;
        LastOutput = Bias;
    }

    /// <summary>
    /// Adds an outgoing connection from this bias node.
    /// </summary>
    /// <param name="connection">The connection to add.</param>
    public void AddOutgoingConnection(ConnectionGeneBase connection)
    {
        OutgoingConnections.Add(connection);
    }

    /// <inheritdoc/>
    public override bool AcceptsOutgoingConnections() => true;
}
