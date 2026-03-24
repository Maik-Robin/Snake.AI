using MicroNEAT.Config;
using MicroNEAT.Core.Genome.Genes.NodeGene;
using MicroNEAT.Core.Genome.Genes.ConnectionGene;
using MicroNEAT.Core.Genome.Genes.GeneticEncoding;
using MicroNEAT.Util;
using MicroNEAT.Util.Trackers;
using NodeGeneBase = MicroNEAT.Core.Genome.Genes.NodeGene.NodeGene;
using ConnectionGeneBase = MicroNEAT.Core.Genome.Genes.ConnectionGene.ConnectionGene;

namespace MicroNEAT.Core.Genome;

/// <summary>
/// Represents a complete neural network genome in the NEAT algorithm.
/// A genome consists of nodes (neurons) and connections (synapses) with associated weights.
/// Genomes can be mutated, crossed over with other genomes, and evaluated for fitness.
/// </summary>
public class Genome
{
    /// <summary>
    /// Gets or sets the list of all nodes in this genome.
    /// </summary>
    public List<NodeGeneBase> NodeGenes { get; set; }
    
    /// <summary>
    /// Gets or sets the list of all connections in this genome.
    /// </summary>
    public List<ConnectionGeneBase> ConnectionGenes { get; set; }
    
    /// <summary>
    /// Gets or sets the list of input nodes (cached for quick access).
    /// </summary>
    public List<InputNode> InputNodes { get; set; }
    
    /// <summary>
    /// Gets or sets the list of output nodes (cached for quick access).
    /// </summary>
    public List<OutputNode> OutputNodes { get; set; }
    
    /// <summary>
    /// Gets or sets the bias node, if one exists in this genome.
    /// </summary>
    public BiasNode? BiasNode { get; set; }
    
    /// <summary>
    /// Gets or sets the configuration settings for this genome.
    /// </summary>
    public NEATConfig Config { get; set; }
    
    /// <summary>
    /// Gets or sets the genome tracker for managing genome IDs.
    /// </summary>
    public GenomeTracker GenomeTracker { get; set; }
    
    /// <summary>
    /// Gets or sets the node tracker for managing node IDs.
    /// </summary>
    public NodeTracker NodeTracker { get; set; }
    
    /// <summary>
    /// Gets or sets the innovation tracker for tracking structural mutations.
    /// </summary>
    public InnovationTracker InnovationTracker { get; set; }
    
    /// <summary>
    /// Gets or sets the unique identifier for this genome.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the raw fitness score of this genome.
    /// </summary>
    public double Fitness { get; set; }
    
    /// <summary>
    /// Gets or sets the fitness adjusted for species size (shared fitness).
    /// </summary>
    public double AdjustedFitness { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the population this genome belongs to.
    /// </summary>
    public int PopulationId { get; set; }

    /// <summary>
    /// Initializes a new instance of the Genome class.
    /// </summary>
    /// <param name="nodeGenes">The list of nodes in the genome.</param>
    /// <param name="connectionGenes">The list of connections in the genome.</param>
    /// <param name="config">The NEAT configuration settings.</param>
    /// <param name="populationId">The ID of the population this genome belongs to.</param>
    public Genome(List<NodeGeneBase> nodeGenes, List<ConnectionGeneBase> connectionGenes, 
        NEATConfig config, int populationId)
    {
        NodeGenes = nodeGenes;
        ConnectionGenes = connectionGenes;
        InputNodes = nodeGenes.OfType<InputNode>().ToList();
        OutputNodes = nodeGenes.OfType<OutputNode>().ToList();
        BiasNode = nodeGenes.OfType<BiasNode>().FirstOrDefault();
        Config = config;
        GenomeTracker = StaticManager.Instance.GetGenomeTracker(populationId);
        NodeTracker = StaticManager.Instance.GetNodeTracker(populationId);
        InnovationTracker = StaticManager.Instance.GetInnovationTracker(populationId);
        Id = GenomeTracker.GetNextGenomeId();
        Fitness = 0;
        AdjustedFitness = 0;
        PopulationId = populationId;
    }

    /// <summary>
    /// Propagates input values through the network and returns output values.
    /// Implements the forward pass of the neural network.
    /// </summary>
    /// <param name="inputs">The input values to feed to the network.</param>
    /// <returns>The output values produced by the network.</returns>
    public double[] Propagate(double[] inputs)
    {
        CalculateExpectedInputs();
        
        for (int i = 0; i < inputs.Length; i++)
        {
            var inputNode = GetNodeById(i);
            if (inputNode is InputNode iNode)
            {
                iNode.FeedInput(inputs[i]);
            }
        }

        var outputs = new double[OutputNodes.Count];
        for (int i = 0; i < OutputNodes.Count; i++)
        {
            var outputNode = GetNodeById(i + InputNodes.Count);
            outputs[i] = outputNode!.LastOutput;
        }

        return outputs;
    }

    /// <summary>
    /// Resets all node states to prepare for a new propagation cycle.
    /// Clears LastOutput and input counters for all nodes.
    /// </summary>
    public void ResetState()
    {
        foreach (var node in NodeGenes)
        {
            node.ResetState();
        }
    }

    private void CalculateExpectedInputs()
    {
        foreach (var node in NodeGenes)
        {
            node.ExpectedInputs = 0;
            node.ReceivedInputs = 0;
        }

        foreach (var connection in ConnectionGenes)
        {
            connection.ForwardedExpectedInput = false;
        }

        foreach (var node in InputNodes)
        {
            node.CalculateExpectedInputs();
        }
    }

    /// <summary>
    /// Retrieves a node by its unique ID.
    /// </summary>
    /// <param name="nodeId">The ID of the node to find.</param>
    /// <returns>The node with the specified ID, or null if not found.</returns>
    public NodeGeneBase? GetNodeById(int nodeId)
    {
        return NodeGenes.FirstOrDefault(node => node.Id == nodeId);
    }

    /// <summary>
    /// Applies random mutations to the genome, including weight mutations, 
    /// and structural mutations (adding connections or nodes).
    /// </summary>
    public void Mutate()
    {
        if (Random.Shared.NextDouble() < Config.WeightMutationRate)
        {
            MutateWeights();
        }
        if (Random.Shared.NextDouble() < Config.AddConnectionMutationRate)
        {
            MutateAddConnection();
        }
        if (Random.Shared.NextDouble() < Config.AddNodeMutationRate)
        {
            MutateAddNode();
        }
    }

    private void MutateWeights()
    {
        foreach (var connection in ConnectionGenes)
        {
            if (Random.Shared.NextDouble() < Config.ReinitializeWeightRate)
            {
                connection.ReinitializeWeight();
            }
            else
            {
                double weight = connection.Weight;
                double perturb = Config.MinPerturb + (Config.MaxPerturb - Config.MinPerturb) * Random.Shared.NextDouble();
                double newWeight = weight + perturb;
                newWeight = Math.Max(Config.MinWeight, Math.Min(newWeight, Config.MaxWeight));
                connection.Weight = newWeight;
            }
        }
    }

    private void MutateAddConnection()
    {
        NodeGeneBase? fromNode = null;
        NodeGeneBase? toNode = null;
        const int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            fromNode = NodeGenes[Random.Shared.Next(NodeGenes.Count)];
            toNode = NodeGenes[Random.Shared.Next(NodeGenes.Count)];

            if (!fromNode.AcceptsOutgoingConnections() || !toNode.AcceptsIncomingConnections())
            {
                attempts++;
                continue;
            }

            bool connectionExists = ConnectionGenes.Any(c => c.InNode == fromNode && c.OutNode == toNode);

            if (connectionExists)
            {
                attempts++;
                continue;
            }

            bool isRecurrent = CheckIfRecurrent(fromNode, toNode);
            
            if (isRecurrent)
            {
                if (!Config.AllowRecurrentConnections || Random.Shared.NextDouble() > Config.RecurrentConnectionRate)
                {
                    attempts++;
                    continue;
                }
            }

            var innovationData = InnovationTracker.TrackInnovation(fromNode.Id, toNode.Id);

            var newConnection = new ConnectionGeneBase(
                fromNode, toNode, 
                Config.WeightInitialization.InitializeWeight(), 
                true, 
                innovationData.InnovationNumber, 
                isRecurrent, 
                Config
            );
            ConnectionGenes.Add(newConnection);
            break;
        }
    }

    private void MutateAddNode()
    {
        if (ConnectionGenes.Count < 1)
            return;

        ConnectionGeneBase? selectedConnection = null;
        const int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            var potentialConnection = ConnectionGenes[Random.Shared.Next(ConnectionGenes.Count)];
            if (potentialConnection.Enabled)
            {
                selectedConnection = potentialConnection;
                break;
            }
            attempts++;
        }

        if (selectedConnection == null)
            return;

        selectedConnection.Enabled = false;

        var innovations = InnovationTracker.TrackAddNodeInnovation(selectedConnection, PopulationId);
        var newNode = new HiddenNode(innovations.NewNodeId, Config);
        NodeGenes.Add(newNode);

        var connection1 = new ConnectionGeneBase(
            selectedConnection.InNode, newNode, 1, true, 
            innovations.InToNew.InnovationNumber, false, Config
        );
        var connection2 = new ConnectionGeneBase(
            newNode, selectedConnection.OutNode, selectedConnection.Weight, true, 
            innovations.NewToOut.InnovationNumber, selectedConnection.Recurrent, Config
        );

        ConnectionGenes.Add(connection1);
        ConnectionGenes.Add(connection2);
    }

    private bool CheckIfRecurrent(NodeGeneBase fromNode, NodeGeneBase toNode)
    {
        if (fromNode == toNode)
            return true;

        if (fromNode is OutputNode)
            return true;

        var stack = new Stack<NodeGeneBase>();
        var visited = new HashSet<NodeGeneBase>();

        stack.Push(toNode);

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();
            if (currentNode == fromNode)
                return true;

            if (!currentNode.AcceptsOutgoingConnections())
                continue;

            visited.Add(currentNode);

            var outgoingConnections = currentNode switch
            {
                HiddenNode hn => hn.OutgoingConnections,
                InputNode inn => inn.OutgoingConnections,
                BiasNode bn => bn.OutgoingConnections,
                _ => new List<ConnectionGeneBase>()
            };

            foreach (var connection in outgoingConnections)
            {
                var nextNode = connection.OutNode;
                if (!connection.Recurrent && !visited.Contains(nextNode))
                {
                    stack.Push(nextNode);
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks all connections in the genome and updates their Recurrent property 
    /// if they form a recurrent loop with the given nodes.
    /// </summary>
    public void CheckForRecurrentConnections()
    {
        foreach (var connection in ConnectionGenes)
        {
            connection.Recurrent = CheckIfRecurrent(connection.InNode, connection.OutNode);
        }
    }

    /// <summary>
    /// Reinitializes the weights of all connections in the genome.
    /// Typically used after a crossover or mutation event.
    /// </summary>
    public void ReinitializeWeights()
    {
        foreach (var connection in ConnectionGenes)
        {
            connection.ReinitializeWeight();
        }
    }

    /// <summary>
    /// Creates a deep copy of the genome, including all nodes and connections.
    /// </summary>
    /// <returns>A new Genome instance that is a copy of the current genome.</returns>
    public Genome Copy()
    {
        var newNodes = new List<NodeGeneBase>();
        var nodeMapping = new Dictionary<int, NodeGeneBase>();

        foreach (var node in NodeGenes)
        {
            NodeGeneBase? newNode = node switch
            {
                InputNode inode => new InputNode(inode.Id, Config),
                HiddenNode hnode => new HiddenNode(hnode.Id, Config),
                OutputNode onode => new OutputNode(onode.Id, Config),
                BiasNode bnode => new BiasNode(bnode.Id, Config),
                _ => null
            };

            if (newNode != null)
            {
                newNodes.Add(newNode);
                nodeMapping[newNode.Id] = newNode;
            }
        }

        var newConnections = new List<ConnectionGeneBase>();

        foreach (var connection in ConnectionGenes)
        {
            var newInNode = nodeMapping[connection.InNode.Id];
            var newOutNode = nodeMapping[connection.OutNode.Id];

            var newConnection = new ConnectionGeneBase(
                newInNode,
                newOutNode,
                connection.Weight,
                connection.Enabled,
                connection.InnovationNumber,
                connection.Recurrent,
                Config
            );
            newConnections.Add(newConnection);
        }

        return new Genome(newNodes, newConnections, Config, PopulationId);
    }

    /// <summary>
    /// Compares this genome to another genome for equality.
    /// Two genomes are considered equal if they have the same number of nodes and connections,
    /// and all corresponding connection weights are equal.
    /// </summary>
    /// <param name="genome">The genome to compare to this genome.</param>
    /// <returns>True if the genomes are equal, false otherwise.</returns>
    public bool EqualsGenome(Genome genome)
    {
        if (NodeGenes.Count != genome.NodeGenes.Count ||
            ConnectionGenes.Count != genome.ConnectionGenes.Count)
        {
            return false;
        }

        for (int i = 0; i < ConnectionGenes.Count; i++)
        {
            if (Math.Abs(ConnectionGenes[i].Weight - genome.ConnectionGenes[i].Weight) > 1e-10)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Encodes the genome's structure and weights into a GeneticEncoding object,
    /// which can be used for reproduction and crossover with other genomes.
    /// </summary>
    /// <returns>A GeneticEncoding object representing the genome.</returns>
    public GeneticEncoding GetGeneticEncoding()
    {
        var geneticEncoding = new GeneticEncoding(Config, PopulationId);
        geneticEncoding.LoadGenome(this);
        return geneticEncoding;
    }

    /// <summary>
    /// Performs crossover with another genome to produce an offspring genome.
    /// The resulting genome inherits traits from both parent genomes.
    /// </summary>
    /// <param name="parent2">The second parent genome to crossover with.</param>
    /// <returns>A new Genome instance representing the offspring.</returns>
    public Genome Crossover(Genome parent2)
    {
        var parent1Encoding = GetGeneticEncoding();
        var parent2Encoding = parent2.GetGeneticEncoding();

        var offspring = parent1Encoding.Crossover(parent2Encoding);
        var offspringGenome = offspring.BuildGenome();

        return offspringGenome;
    }

    /// <summary>
    /// Evaluates the fitness of the genome by calculating its performance
    /// on the given task or benchmark. The fitness value is used to rank
    /// and select genomes for reproduction in the evolutionary algorithm.
    /// </summary>
    public void EvaluateFitness()
    {
        Fitness = Config.FitnessFunction.CalculateFitness(this);
    }

    /// <summary>
    /// Serializes the genome to a JSON string.
    /// Includes all nodes, connections, fitness, and population ID for complete reconstruction.
    /// </summary>
    /// <returns>A JSON string representation of the genome.</returns>
    public string ToJson()
    {
        var genomeData = new
        {
            id = Id,
            nodeGenes = NodeGenes.Select(node => new
            {
                id = node.Id,
                type = node.NodeType.ToString()
            }).ToList(),
            connectionGenes = ConnectionGenes.Select(connection => new
            {
                innovationNumber = connection.InnovationNumber,
                inNodeId = connection.InNode.Id,
                outNodeId = connection.OutNode.Id,
                enabled = connection.Enabled,
                weight = connection.Weight,
                recurrent = connection.Recurrent
            }).ToList(),
            fitness = Fitness,
            populationId = PopulationId
        };

        return System.Text.Json.JsonSerializer.Serialize(genomeData, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}
