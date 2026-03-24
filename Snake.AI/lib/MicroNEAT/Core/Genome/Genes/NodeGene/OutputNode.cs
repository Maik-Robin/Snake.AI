using MicroNEAT.Config;
using ConnectionGeneBase = MicroNEAT.Core.Genome.Genes.ConnectionGene.ConnectionGene;

namespace MicroNEAT.Core.Genome.Genes.NodeGene;

/// <summary>
/// Represents an output node in the neural network.
/// Output nodes collect inputs from connected nodes, apply activation functions, and produce the network's final output.
/// They can handle both regular and recurrent connections, and support various bias modes.
/// </summary>
public class OutputNode : NodeGene
{
    /// <inheritdoc/>
    public override NodeType NodeType => NodeType.OUTPUT;
    
    /// <summary>
    /// Gets or sets the list of incoming (non-recurrent) connections to this output node.
    /// </summary>
    public List<ConnectionGeneBase> IncomingConnections { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of incoming recurrent connections to this output node.
    /// Recurrent connections are processed differently to handle feedback loops.
    /// </summary>
    public List<ConnectionGeneBase> InComingRecurrentConnections { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of outgoing connections from this output node.
    /// Typically empty for output nodes in feedforward networks.
    /// </summary>
    public List<ConnectionGeneBase> OutgoingConnections { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the connection from the bias node to this output node, if one exists.
    /// </summary>
    public ConnectionGeneBase? BiasConnection { get; set; }
    
    private readonly List<double> _inputs = new();

    /// <summary>
    /// Initializes a new instance of the OutputNode class.
    /// </summary>
    /// <param name="id">The unique identifier for this node.</param>
    /// <param name="config">The NEAT configuration settings.</param>
    public OutputNode(int id, NEATConfig config) : base(id, config)
    {
    }

    /// <summary>
    /// Receives an input value from a connected node.
    /// Once all expected inputs are received, the node activates and produces output.
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

        LastOutput = Config.ActivationFunction.Apply(sum);
        _inputs.Clear();
        ReceivedInputs = 0;
    }

    /// <summary>
    /// Increments the expected input count for this node.
    /// Called during the propagation setup phase.
    /// </summary>
    public void ForwardExpectedInput()
    {
        ExpectedInputs++;
    }

    /// <summary>
    /// Adds an incoming connection to this output node.
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
    /// Adds an outgoing connection from this output node.
    /// </summary>
    /// <param name="connection">The connection to add.</param>
    public void AddOutgoingConnection(ConnectionGeneBase connection)
    {
        OutgoingConnections.Add(connection);
    }

    /// <inheritdoc/>
    public override bool AcceptsIncomingConnections() => true;
    
    /// <inheritdoc/>
    public override bool AcceptsOutgoingConnections() => false;
}
