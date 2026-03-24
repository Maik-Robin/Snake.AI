using MicroNEAT.Config;
using ConnectionGeneBase = MicroNEAT.Core.Genome.Genes.ConnectionGene.ConnectionGene;

namespace MicroNEAT.Core.Genome.Genes.NodeGene;

/// <summary>
/// Represents a hidden node in the neural network.
/// Hidden nodes are created during evolution through add-node mutations.
/// They accept inputs, apply activation functions, and propagate outputs to other nodes.
/// </summary>
public class HiddenNode : NodeGene
{
    /// <inheritdoc/>
    public override NodeType NodeType => NodeType.HIDDEN;
    
    /// <summary>
    /// Gets or sets the list of incoming (non-recurrent) connections to this hidden node.
    /// </summary>
    public List<ConnectionGeneBase> IncomingConnections { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of outgoing connections from this hidden node.
    /// </summary>
    public List<ConnectionGeneBase> OutgoingConnections { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of incoming recurrent connections to this hidden node.
    /// </summary>
    public List<ConnectionGeneBase> InComingRecurrentConnections { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the connection from the bias node to this hidden node, if one exists.
    /// </summary>
    public ConnectionGeneBase? BiasConnection { get; set; }
    
    private readonly List<double> _inputs = new();

    /// <summary>
    /// Initializes a new instance of the HiddenNode class.
    /// </summary>
    /// <param name="id">The unique identifier for this node.</param>
    /// <param name="config">The NEAT configuration settings.</param>
    public HiddenNode(int id, NEATConfig config) : base(id, config)
    {
    }

    /// <summary>
    /// Receives an input value from a connected node.
    /// Once all expected inputs are received, the node activates and propagates output.
    /// </summary>
    /// <param name="input">The input value to receive.</param>
    public void FeedInput(double input)
    {
        _inputs.Add(input);
        ReceivedInputs++;
        if (ReceivedInputs == ExpectedInputs)
        {
            Activate(_inputs);
        }
    }

    private void Activate(List<double> inputs)
    {
        double sum = inputs.Sum();

        foreach (var connection in InComingRecurrentConnections)
        {
            if (connection.Enabled)
            {
                sum += connection.InNode.LastOutput * connection.Weight;
            }
        }

        switch (Config.BiasMode)
        {
            case "WEIGHTED_NODE":
                if (BiasConnection != null && BiasConnection.Enabled)
                {
                    sum += BiasConnection.Weight * ((BiasNode)BiasConnection.InNode).Bias;
                }
                break;
            case "DIRECT_NODE":
                if (BiasConnection != null && BiasConnection.Enabled)
                {
                    sum += ((BiasNode)BiasConnection.InNode).Bias;
                }
                break;
            case "CONSTANT":
                sum += Config.Bias;
                break;
            case "DISABLED":
                break;
        }

        double output = Config.ActivationFunction.Apply(sum);
        LastOutput = output;

        foreach (var connection in OutgoingConnections)
        {
            connection.FeedForward(output);
        }

        _inputs.Clear();
        ReceivedInputs = 0;
    }

    /// <summary>
    /// Propagates the expected input count to downstream nodes.
    /// Recursively forwards through non-recurrent connections.
    /// </summary>
    public void ForwardExpectedInput()
    {
        ExpectedInputs++;
        foreach (var connection in OutgoingConnections)
        {
            if (connection.Enabled && !connection.Recurrent)
            {
                connection.ForwardExpectedInput();
            }
        }
    }

    /// <summary>
    /// Adds an incoming connection to this hidden node.
    /// Automatically categorizes recurrent connections and bias connections.
    /// </summary>
    /// <param name="connection">The connection to add.</param>
    public void AddIncomingConnection(ConnectionGeneBase connection)
    {
        IncomingConnections.Add(connection);
        if (connection.Recurrent)
        {
            InComingRecurrentConnections.Add(connection);
        }
        if (connection.InNode is BiasNode)
        {
            BiasConnection = connection;
        }
    }

    /// <summary>
    /// Adds an outgoing connection from this hidden node.
    /// </summary>
    /// <param name="connection">The connection to add.</param>
    public void AddOutgoingConnection(ConnectionGeneBase connection)
    {
        OutgoingConnections.Add(connection);
    }

    /// <inheritdoc/>
    public override bool AcceptsIncomingConnections() => true;

    /// <inheritdoc/>
    public override bool AcceptsOutgoingConnections() => true;
}
