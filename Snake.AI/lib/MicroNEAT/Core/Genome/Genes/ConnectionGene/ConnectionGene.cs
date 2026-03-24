using MicroNEAT.Config;
using NodeGeneBase = MicroNEAT.Core.Genome.Genes.NodeGene.NodeGene;

namespace MicroNEAT.Core.Genome.Genes.ConnectionGene;

/// <summary>
/// Represents a connection (synapse) between two nodes in the neural network.
/// Connections have weights and can be enabled/disabled or marked as recurrent.
/// Each connection has a unique innovation number for tracking across populations.
/// </summary>
public class ConnectionGene
{
    /// <summary>
    /// Gets or sets the source (input) node for this connection.
    /// </summary>
    public NodeGeneBase InNode { get; set; }
    
    /// <summary>
    /// Gets or sets the destination (output) node for this connection.
    /// </summary>
    public NodeGeneBase OutNode { get; set; }
    
    /// <summary>
    /// Gets or sets the weight (strength) of this connection.
    /// </summary>
    public double Weight { get; set; }
    
    /// <summary>
    /// Gets or sets whether this connection is enabled (active) in the network.
    /// Disabled connections do not propagate signals.
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// Gets or sets the innovation number that uniquely identifies when this connection type first appeared.
    /// Used for aligning genes during crossover.
    /// </summary>
    public int InnovationNumber { get; set; }
    
    /// <summary>
    /// Gets or sets whether this connection is recurrent (creates a feedback loop).
    /// </summary>
    public bool Recurrent { get; set; }
    
    /// <summary>
    /// Gets or sets the NEAT configuration settings.
    /// </summary>
    public NEATConfig Config { get; set; }
    
    /// <summary>
    /// Gets or sets whether this connection has already forwarded its expected input count.
    /// Used during propagation setup to avoid double-counting.
    /// </summary>
    public bool ForwardedExpectedInput { get; set; }

    /// <summary>
    /// Initializes a new instance of the ConnectionGene class.
    /// Automatically registers this connection with its input and output nodes.
    /// </summary>
    /// <param name="inNode">The source node.</param>
    /// <param name="outNode">The destination node.</param>
    /// <param name="weight">The initial weight value.</param>
    /// <param name="enabled">Whether the connection is initially enabled.</param>
    /// <param name="innovationNumber">The innovation number for this connection type.</param>
    /// <param name="recurrent">Whether this connection is recurrent.</param>
    /// <param name="config">The NEAT configuration settings.</param>
    public ConnectionGene(NodeGeneBase inNode, NodeGeneBase outNode, double weight, 
        bool enabled, int innovationNumber, bool recurrent, NEATConfig config)
    {
        InNode = inNode;
        OutNode = outNode;
        Weight = weight;
        Enabled = enabled;
        InnovationNumber = innovationNumber;
        Recurrent = recurrent;
        Config = config;
        ForwardedExpectedInput = false;

        if (inNode is NodeGene.InputNode inputNode)
            inputNode.AddOutgoingConnection(this);
        else if (inNode is NodeGene.HiddenNode hiddenInNode)
            hiddenInNode.AddOutgoingConnection(this);
        else if (inNode is NodeGene.BiasNode biasNode)
            biasNode.AddOutgoingConnection(this);

        if (outNode is NodeGene.OutputNode outputNode)
            outputNode.AddIncomingConnection(this);
        else if (outNode is NodeGene.HiddenNode hiddenOutNode)
            hiddenOutNode.AddIncomingConnection(this);
    }

    
    /// <summary>
    /// Propagates the input value through this connection to the output node.
    /// Only propagates if the connection is enabled and not recurrent.
    /// </summary>
    /// <param name="input">The input value to propagate.</param>
    public void FeedForward(double input)
    {
        if (Enabled && !Recurrent)
        {
            FeedInputToNode(input * Weight);
        }
    }

    /// <summary>
    /// Forwards the expected input count to the output node.
    /// Ensures the count is only forwarded once per propagation cycle.
    /// </summary>
    public void ForwardExpectedInput()
    {
        if (ForwardedExpectedInput)
            return;

        ForwardedExpectedInput = true;

        if (OutNode is NodeGene.HiddenNode hiddenNode)
        {
            hiddenNode.ForwardExpectedInput();
        }
        else if (OutNode is NodeGene.OutputNode outputNode)
        {
            outputNode.ForwardExpectedInput();
        }
    }

    /// <summary>
    /// Reinitializes this connection's weight using the configured weight initialization strategy.
    /// </summary>
    public void ReinitializeWeight()
    {
        Weight = Config.WeightInitialization.InitializeWeight();
    }

    private void FeedInputToNode(double input)
    {
        if (OutNode is NodeGene.HiddenNode hiddenNode)
            hiddenNode.FeedInput(input);
        else if (OutNode is NodeGene.OutputNode outputNode)
            outputNode.FeedInput(input);
    }
}
