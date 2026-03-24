using MicroNEAT.Config;
using ConnectionGeneBase = MicroNEAT.Core.Genome.Genes.ConnectionGene.ConnectionGene;

namespace MicroNEAT.Core.Genome.Genes.NodeGene;

/// <summary>
/// Represents an input node in the neural network.
/// Input nodes receive external data and propagate it through their outgoing connections.
/// They do not perform activation function calculations on their inputs.
/// </summary>
public class InputNode : NodeGene
{
    /// <inheritdoc/>
    public override NodeType NodeType => NodeType.INPUT;
    
    /// <summary>
    /// Gets or sets the list of outgoing connections from this input node.
    /// </summary>
    public List<ConnectionGeneBase> OutgoingConnections { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the InputNode class.
    /// </summary>
    /// <param name="id">The unique identifier for this node.</param>
    /// <param name="config">The NEAT configuration settings.</param>
    public InputNode(int id, NEATConfig config) : base(id, config)
    {
    }

    /// <summary>
    /// Feeds an input value to this node and propagates it to connected nodes.
    /// </summary>
    /// <param name="input">The input value to feed.</param>
    public void FeedInput(double input)
    {
        Activate(input);
    }

    private void Activate(double input)
    {
        foreach (var connection in OutgoingConnections)
        {
            connection.FeedForward(input);
            LastOutput = input;
        }
    }

    /// <summary>
    /// Calculates and propagates the expected number of inputs for downstream nodes.
    /// Used to determine when nodes have received all their inputs and can activate.
    /// </summary>
    public void CalculateExpectedInputs()
    {
        foreach (var connection in OutgoingConnections)
        {
            if (connection.Enabled && !connection.Recurrent)
            {
                connection.ForwardExpectedInput();
            }
        }
    }

    /// <summary>
    /// Adds an outgoing connection from this input node.
    /// </summary>
    /// <param name="connection">The connection to add.</param>
    public void AddOutgoingConnection(ConnectionGeneBase connection)
    {
        OutgoingConnections.Add(connection);
    }

    /// <inheritdoc/>
    public override bool AcceptsOutgoingConnections() => true;
}
