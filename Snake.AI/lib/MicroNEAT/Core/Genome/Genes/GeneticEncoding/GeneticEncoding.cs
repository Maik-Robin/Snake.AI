using MicroNEAT.Config;
using MicroNEAT.Core.Genome.Genes.NodeGene;
using MicroNEAT.Core.Genome.Genes.ConnectionGene;

namespace MicroNEAT.Core.Genome.Genes.GeneticEncoding;

/// <summary>
/// Represents the genetic encoding of a genome in a format suitable for crossover and compatibility calculations.
/// Separates the genome's structure and weights from its execution logic.
/// </summary>
public class GeneticEncoding
{
    private readonly NEATConfig _config;
    private readonly int _populationId;
    private readonly Dictionary<int, NodeGeneData> _nodeGenesMap = new();
    private readonly Dictionary<int, ConnectionGeneData> _connectionGenesMap = new();
    private readonly List<NodeGeneData> _inputNodes = new();
    private readonly List<NodeGeneData> _outputNodes = new();
    private NodeGeneData? _biasNode;

    /// <summary>
    /// Initializes a new instance of the GeneticEncoding class.
    /// </summary>
    /// <param name="config">The NEAT configuration settings.</param>
    /// <param name="populationId">The ID of the population.</param>
    public GeneticEncoding(NEATConfig config, int populationId)
    {
        _config = config;
        _populationId = populationId;
    }

    /// <summary>
    /// Loads the structure and weights from a genome into this genetic encoding.
    /// </summary>
    /// <param name="genome">The genome to encode.</param>
    public void LoadGenome(Genome genome)
    {
        this._nodeGenesMap.Clear();
        this._connectionGenesMap.Clear();

        foreach (var node in genome.NodeGenes)
        {
            var nodeData = new NodeGeneData(node.Id, node.NodeType);
            AddNode(nodeData);
        }

        foreach (var connection in genome.ConnectionGenes)
        {
            var connectionData = new ConnectionGeneData(
                connection.InNode.Id,
                connection.OutNode.Id,
                connection.Weight,
                connection.Enabled,
                connection.InnovationNumber,
                connection.Recurrent
            );
            AddConnection(connectionData);
        }
    }

    /// <summary>
    /// Performs crossover with another genetic encoding to produce offspring encoding.
    /// Implements the NEAT crossover algorithm, inheriting matching genes randomly from both parents
    /// and excess/disjoint genes from the fitter parent.
    /// </summary>
    /// <param name="otherParent">The other parent's genetic encoding.</param>
    /// <returns>A new genetic encoding representing the offspring.</returns>
    public GeneticEncoding Crossover(GeneticEncoding otherParent)
    {
        var offspring = new GeneticEncoding(_config, _populationId);

        var thisEncoding = this;
        var thisFitness = thisEncoding._connectionGenesMap.Count;
        var otherFitness = otherParent._connectionGenesMap.Count;

        GeneticEncoding bestParent;
        GeneticEncoding worstParent;

        if (thisFitness > otherFitness)
        {
            bestParent = thisEncoding;
            worstParent = otherParent;
        }
        else if (thisFitness < otherFitness)
        {
            bestParent = otherParent;
            worstParent = thisEncoding;
        }
        else
        {
            bestParent = thisEncoding._connectionGenesMap.Count < otherParent._connectionGenesMap.Count 
                ? thisEncoding 
                : otherParent;
            worstParent = thisEncoding._connectionGenesMap.Count < otherParent._connectionGenesMap.Count 
                ? otherParent 
                : thisEncoding;
        }

        foreach (var (innovationNumber, currentGene) in bestParent._connectionGenesMap)
        {
            if (worstParent.HasInnovationNumber(innovationNumber))
            {
                var parent2Gene = worstParent.GetConnectionByInnovationNumber(innovationNumber);
                var selectedParent = Random.Shared.NextDouble() < 0.5 ? bestParent : worstParent;
                var selectedGene = selectedParent.GetConnectionByInnovationNumber(innovationNumber);
                bool isEnabled = !currentGene.Enabled || !parent2Gene.Enabled 
                    ? Random.Shared.NextDouble() > _config.KeepDisabledOnCrossOverRate 
                    : true;

                if (_config.KeepDisabledOnCrossOverRate == -1)
                {
                    isEnabled = selectedGene.Enabled;
                }
                offspring.AddConnectionAndNodes(selectedGene, isEnabled, selectedParent, bestParent);
            }
            else
            {
                bool isEnabled = !currentGene.Enabled 
                    ? Random.Shared.NextDouble() > _config.KeepDisabledOnCrossOverRate 
                    : true;
                if (_config.KeepDisabledOnCrossOverRate == -1)
                {
                    isEnabled = currentGene.Enabled;
                }
                offspring.AddConnectionAndNodes(currentGene, isEnabled, bestParent, bestParent);
            }
        }

        foreach (var inputNode in _inputNodes)
        {
            offspring.AddNode(inputNode);
        }
        foreach (var outputNode in _outputNodes)
        {
            offspring.AddNode(outputNode);
        }
        if (_biasNode != null)
        {
            offspring.AddNode(_biasNode);
        }

        return offspring;
    }

    /// <summary>
    /// Builds a genome from this genetic encoding's structure and weights.
    /// </summary>
    /// <returns>A genome representing the encoded structure and weights.</returns>
    public Genome BuildGenome()
    {
        var newNodeGenesMap = new Dictionary<int, NodeGene.NodeGene>();
        var newNodeGenes = new List<NodeGene.NodeGene>();

        foreach (var oldNode in _nodeGenesMap.Values)
        {
            NodeGene.NodeGene? newNode = oldNode.NodeType switch
            {
                NodeType.INPUT => new InputNode(oldNode.Id, _config),
                NodeType.HIDDEN => new HiddenNode(oldNode.Id, _config),
                NodeType.OUTPUT => new OutputNode(oldNode.Id, _config),
                NodeType.BIAS => new BiasNode(oldNode.Id, _config),
                _ => null
            };

            if (newNode != null)
            {
                newNodeGenesMap[newNode.Id] = newNode;
                newNodeGenes.Add(newNode);
            }
        }

        var newConnectionGenes = new List<ConnectionGene.ConnectionGene>();

        foreach (var oldConnection in _connectionGenesMap.Values)
        {
            var newInNode = newNodeGenesMap[oldConnection.InNodeId];
            var newOutNode = newNodeGenesMap[oldConnection.OutNodeId];

            var newConnection = new ConnectionGene.ConnectionGene(
                newInNode,
                newOutNode,
                oldConnection.Weight,
                oldConnection.Enabled,
                oldConnection.InnovationNumber,
                oldConnection.Recurrent,
                _config
            );
            newConnectionGenes.Add(newConnection);
        }

        var genome = new Genome(newNodeGenes, newConnectionGenes, _config, _populationId);
        genome.CheckForRecurrentConnections();
        return genome;
    }

    private void AddConnectionAndNodes(ConnectionGeneData connection, bool enabled, 
        GeneticEncoding parent, GeneticEncoding bestParent)
    {
        var outNodeType = parent.GetNodeById(connection.OutNodeId).NodeType;
        if (outNodeType == NodeType.INPUT)
        {
            throw new InvalidOperationException($"Invalid connection: Input node {connection.OutNodeId} cannot be used as out node.");
        }

        var bestParentConnection = bestParent.GetConnectionByInnovationNumber(connection.InnovationNumber);
        var newConnection = new ConnectionGeneData(
            connection.InNodeId,
            connection.OutNodeId,
            connection.Weight,
            enabled,
            connection.InnovationNumber,
            bestParentConnection.Recurrent
        );

        AddConnection(newConnection);

        var inNode = new NodeGeneData(connection.InNodeId, parent.GetNodeById(connection.InNodeId).NodeType);
        var outNode = new NodeGeneData(connection.OutNodeId, parent.GetNodeById(connection.OutNodeId).NodeType);

        AddNode(inNode);
        AddNode(outNode);
    }

    /// <summary>
    /// Calculates the compatibility distance between this genetic encoding and another.
    /// Used to determine the compatibility of genomes for speciation.
    /// </summary>
    /// <param name="otherParent">The other genetic encoding to compare with.</param>
    /// <returns>A double representing the compatibility distance.</returns>
    public double CalculateCompatibilityDistance(GeneticEncoding otherParent)
    {
        int disjointGenes = GetNumberOfDisjointGenes(otherParent);
        int excessGenes = GetNumberOfExcessGenes(otherParent);
        int maxGenes = Math.Max(_connectionGenesMap.Count, otherParent._connectionGenesMap.Count);
        maxGenes = maxGenes < 20 ? 1 : maxGenes;

        return ((_config.C1 * excessGenes) / maxGenes) + 
               ((_config.C2 * disjointGenes) / maxGenes) + 
               (_config.C3 * CalculateAverageWeightDifference(otherParent));
    }

    private int GetNumberOfDisjointGenes(GeneticEncoding otherParent)
    {
        int disjointGenes = 0;
        int maxInnovationSelf = GetHighestInnovationNumber();
        int maxInnovationOther = otherParent.GetHighestInnovationNumber();
        int comparisonLimit = Math.Min(maxInnovationSelf, maxInnovationOther);

        foreach (var innovationNumber in _connectionGenesMap.Keys)
        {
            if (innovationNumber <= comparisonLimit && !otherParent.HasInnovationNumber(innovationNumber))
            {
                disjointGenes++;
            }
        }

        foreach (var innovationNumber in otherParent._connectionGenesMap.Keys)
        {
            if (innovationNumber <= comparisonLimit && !HasInnovationNumber(innovationNumber))
            {
                disjointGenes++;
            }
        }

        return disjointGenes;
    }

    private int GetNumberOfExcessGenes(GeneticEncoding otherParent)
    {
        int excessGenes = 0;
        int maxInnovationSelf = GetHighestInnovationNumber();
        int maxInnovationOther = otherParent.GetHighestInnovationNumber();
        var largerParent = maxInnovationSelf > maxInnovationOther ? this : otherParent;
        int minInnovation = Math.Min(maxInnovationSelf, maxInnovationOther);

        foreach (var innovationNumber in largerParent._connectionGenesMap.Keys)
        {
            if (innovationNumber > minInnovation)
            {
                excessGenes++;
            }
        }

        return excessGenes;
    }

    private double CalculateAverageWeightDifference(GeneticEncoding otherParent)
    {
        double totalWeightDifference = 0;
        int matchingGenesCount = 0;

        foreach (var (innovationNumber, connection) in _connectionGenesMap)
        {
            if (otherParent.HasInnovationNumber(innovationNumber))
            {
                var otherConnection = otherParent.GetConnectionByInnovationNumber(innovationNumber);
                totalWeightDifference += Math.Abs(connection.Weight - otherConnection.Weight);
                matchingGenesCount++;
            }
        }

        return matchingGenesCount == 0 ? 0 : totalWeightDifference / matchingGenesCount;
    }

    private int GetHighestInnovationNumber()
    {
        return _connectionGenesMap.Count == 0 ? 0 : _connectionGenesMap.Keys.Max();
    }

    public bool HasInnovationNumber(int innovationNumber)
    {
        return _connectionGenesMap.ContainsKey(innovationNumber);
    }

    public NodeGeneData GetNodeById(int id)
    {
        if (!_nodeGenesMap.TryGetValue(id, out var node))
        {
            throw new InvalidOperationException($"Error: Node with ID {id} does not exist.");
        }
        return node;
    }

    public ConnectionGeneData GetConnectionByInnovationNumber(int innovationNumber)
    {
        return _connectionGenesMap[innovationNumber];
    }

    private void AddConnection(ConnectionGeneData connection)
    {
        foreach (var existingConnection in _connectionGenesMap.Values)
        {
            if (existingConnection.InNodeId == connection.InNodeId &&
                existingConnection.OutNodeId == connection.OutNodeId)
            {
                return;
            }
        }
        _connectionGenesMap[connection.InnovationNumber] = connection;
    }

    public bool HasNodeId(int nodeId)
    {
        return _nodeGenesMap.ContainsKey(nodeId);
    }

    private void AddNode(NodeGeneData node)
    {
        if (_nodeGenesMap.ContainsKey(node.Id))
        {
            return;
        }
        _nodeGenesMap[node.Id] = node;

        switch (node.NodeType)
        {
            case NodeType.INPUT:
                _inputNodes.Add(node);
                break;
            case NodeType.OUTPUT:
                _outputNodes.Add(node);
                break;
            case NodeType.BIAS:
                _biasNode = node;
                break;
        }
    }
}
