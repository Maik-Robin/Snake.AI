using MicroNEAT.Config;
using MicroNEAT.Core.Genome.Genes.NodeGene;
using MicroNEAT.Core.Genome.Genes.ConnectionGene;
using MicroNEAT.Util;
using MicroNEAT.Util.Trackers;
using System.Text.Json;
using NodeGeneBase = MicroNEAT.Core.Genome.Genes.NodeGene.NodeGene;
using ConnectionGeneBase = MicroNEAT.Core.Genome.Genes.ConnectionGene.ConnectionGene;

namespace MicroNEAT.Core.Genome;

/// <summary>
/// Factory class for creating initial genome structures.
/// Builds minimal genomes with input and output nodes fully connected.
/// </summary>
public class GenomeBuilder
{
    /// <summary>
    /// Builds a new genome with the specified configuration.
    /// Creates input nodes, output nodes, optional bias node, and fully connects inputs to outputs.
    /// This creates the minimal starting topology for the NEAT algorithm.
    /// </summary>
    /// <param name="config">The NEAT configuration specifying network structure and parameters.</param>
    /// <param name="populationId">Optional population ID. If null, a new population ID is generated.</param>
    /// <returns>A new Genome instance with minimal topology.</returns>
    public static Genome BuildGenome(NEATConfig config, int? populationId = null)
    {
        int popId = populationId ?? PopulationTracker.GetNextPopulationId();
        int numInputs = config.InputSize;
        int numOutputs = config.OutputSize;

        var nodeGenes = new List<NodeGeneBase>();
        var connectionGenes = new List<ConnectionGeneBase>();

        for (int i = 0; i < numInputs; i++)
        {
            nodeGenes.Add(new InputNode(StaticManager.Instance.GetNodeTracker(popId).GetNextNodeId(), config));
        }

        for (int i = 0; i < numOutputs; i++)
        {
            nodeGenes.Add(new OutputNode(StaticManager.Instance.GetNodeTracker(popId).GetNextNodeId(), config));
        }

        BiasNode? biasNode = null;
        if (config.BiasMode != "DISABLED" && config.BiasMode != "CONSTANT")
        {
            biasNode = new BiasNode(StaticManager.Instance.GetNodeTracker(popId).GetNextNodeId(), config);
            nodeGenes.Add(biasNode);
        }

        for (int inputIdx = 0; inputIdx < numInputs; inputIdx++)
        {
            var inputNode = (InputNode)nodeGenes[inputIdx];

            for (int outputIdx = numInputs; outputIdx < numInputs + numOutputs; outputIdx++)
            {
                var outputNode = (OutputNode)nodeGenes[outputIdx];
                var innovationData = StaticManager.Instance.GetInnovationTracker(popId)
                    .TrackInnovation(inputNode.Id, outputNode.Id);

                connectionGenes.Add(new ConnectionGeneBase(
                    inputNode,
                    outputNode,
                    config.WeightInitialization.InitializeWeight(),
                    true,
                    innovationData.InnovationNumber,
                    false,
                    config
                ));
            }
        }

        if (config.ConnectBias && config.BiasMode != "DISABLED" && config.BiasMode != "CONSTANT")
        {
            for (int outputIdx = numInputs; outputIdx < numInputs + numOutputs; outputIdx++)
            {
                var outputNode = (OutputNode)nodeGenes[outputIdx];
                var innovationData = StaticManager.Instance.GetInnovationTracker(popId)
                    .TrackInnovation(biasNode!.Id, outputNode.Id);

                connectionGenes.Add(new ConnectionGeneBase(
                    biasNode,
                    outputNode,
                    config.WeightInitialization.InitializeWeight(),
                    true,
                    innovationData.InnovationNumber,
                    false,
                    config
                ));
            }
        }

        return new Genome(nodeGenes, connectionGenes, config, popId);
    }

    /// <summary>
    /// Loads a genome from JSON data.
    /// Reconstructs the complete genome structure including all nodes and connections from serialized data.
    /// </summary>
    /// <param name="jsonData">JSON string containing the serialized genome data.</param>
    /// <param name="config">The NEAT configuration to use for the loaded genome.</param>
    /// <returns>A reconstructed Genome instance.</returns>
    /// <exception cref="JsonException">Thrown when the JSON data is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when connection refers to non-existing nodes.</exception>
    public static Genome LoadGenome(string jsonData, NEATConfig config)
    {
        var parsedData = JsonSerializer.Deserialize<GenomeData>(jsonData, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new JsonException("Failed to parse genome JSON data");

        var nodeGenes = new List<NodeGeneBase>();

        foreach (var nodeData in parsedData.NodeGenes)
        {
            NodeGeneBase node = nodeData.Type.ToUpperInvariant() switch
            {
                "INPUT" => new InputNode(nodeData.Id, config),
                "HIDDEN" => new HiddenNode(nodeData.Id, config),
                "OUTPUT" => new OutputNode(nodeData.Id, config),
                "BIAS" => new BiasNode(nodeData.Id, config),
                _ => throw new InvalidOperationException($"Unknown node type: {nodeData.Type}")
            };
            nodeGenes.Add(node);
        }

        var connectionGenes = new List<ConnectionGeneBase>();

        foreach (var connData in parsedData.ConnectionGenes)
        {
            var inNode = nodeGenes.FirstOrDefault(node => node.Id == connData.InNodeId)
                ?? throw new InvalidOperationException($"Connection refers to non-existing input node with ID {connData.InNodeId}");
            var outNode = nodeGenes.FirstOrDefault(node => node.Id == connData.OutNodeId)
                ?? throw new InvalidOperationException($"Connection refers to non-existing output node with ID {connData.OutNodeId}");

            var connection = new ConnectionGeneBase(
                inNode, 
                outNode, 
                connData.Weight, 
                connData.Enabled,
                connData.InnovationNumber, 
                connData.Recurrent, 
                config
            );
            connectionGenes.Add(connection);
        }

        var genome = new Genome(nodeGenes, connectionGenes, config, parsedData.PopulationId);
        genome.Fitness = parsedData.Fitness;

        return genome;
    }

    /// <summary>
    /// Saves a genome to a JSON file.
    /// Convenience method that serializes the genome and writes it to the specified file path.
    /// </summary>
    /// <param name="genome">The genome to save.</param>
    /// <param name="filePath">The file path where the genome should be saved.</param>
    /// <exception cref="IOException">Thrown when file write operation fails.</exception>
    public static void SaveGenome(Genome genome, string filePath)
    {
        var jsonData = genome.ToJson();
        File.WriteAllText(filePath, jsonData);
    }

    /// <summary>
    /// Loads a genome from a JSON file.
    /// Convenience method that reads the file and deserializes the genome.
    /// </summary>
    /// <param name="filePath">The file path to load the genome from.</param>
    /// <param name="config">The NEAT configuration to use for the loaded genome.</param>
    /// <returns>A reconstructed Genome instance.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    /// <exception cref="JsonException">Thrown when the JSON data is invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when connection refers to non-existing nodes.</exception>
    public static Genome LoadGenomeFromFile(string filePath, NEATConfig config)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Genome file not found: {filePath}");
        }

        var jsonData = File.ReadAllText(filePath);
        return LoadGenome(jsonData, config);
    }

    /// <summary>
    /// Data structure for deserializing genome JSON.
    /// </summary>
    private class GenomeData
    {
        public int Id { get; set; }
        public List<NodeData> NodeGenes { get; set; } = new();
        public List<ConnectionData> ConnectionGenes { get; set; } = new();
        public double Fitness { get; set; }
        public int PopulationId { get; set; }
    }

    /// <summary>
    /// Data structure for deserializing node JSON.
    /// </summary>
    private class NodeData
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Data structure for deserializing connection JSON.
    /// </summary>
    private class ConnectionData
    {
        public int InnovationNumber { get; set; }
        public int InNodeId { get; set; }
        public int OutNodeId { get; set; }
        public bool Enabled { get; set; }
        public double Weight { get; set; }
        public bool Recurrent { get; set; }
    }
}
