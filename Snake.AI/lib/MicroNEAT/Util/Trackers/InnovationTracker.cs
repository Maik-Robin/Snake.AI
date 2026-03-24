namespace MicroNEAT.Util.Trackers;

/// <summary>
/// Tracks innovation numbers for structural mutations across the population.
/// Ensures that identical structural mutations receive the same innovation number,
/// which is critical for proper gene alignment during crossover.
/// </summary>
public class InnovationTracker
{
    /// <summary>
    /// Specifies the type of structural mutation.
    /// </summary>
    public enum InnovationType
    {
        /// <summary>
        /// A new connection was added between two existing nodes.
        /// </summary>
        AddConnection,
        
        /// <summary>
        /// A new node was inserted by splitting an existing connection.
        /// </summary>
        AddNode
    }

    /// <summary>
    /// Contains information about a specific innovation (structural mutation).
    /// </summary>
    public class InnovationData
    {
        /// <summary>
        /// Gets or sets the ID of the source node.
        /// </summary>
        public int InNodeId { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the destination node.
        /// </summary>
        public int OutNodeId { get; set; }
        
        /// <summary>
        /// Gets or sets the unique innovation number for this connection type.
        /// </summary>
        public int InnovationNumber { get; set; }
    }

    /// <summary>
    /// Contains information about an add-node mutation, which creates two new connections.
    /// </summary>
    public class AddNodeInnovationData
    {
        /// <summary>
        /// Gets or sets the innovation data for the connection from the original input node to the new node.
        /// </summary>
        public required InnovationData InToNew { get; set; }
        
        /// <summary>
        /// Gets or sets the innovation data for the connection from the new node to the original output node.
        /// </summary>
        public required InnovationData NewToOut { get; set; }
        
        /// <summary>
        /// Gets or sets the ID assigned to the newly created node.
        /// </summary>
        public int NewNodeId { get; set; }
    }

    private readonly Dictionary<string, InnovationData> _innovationMap = new();
    private readonly Dictionary<string, int> _nodeInnovationMap = new();
    private int _innovationCounter = 0;

    /// <summary>
    /// Resets the innovation tracker, clearing all tracked innovations.
    /// Called at the start of each generation.
    /// </summary>
    public void Reset()
    {
        _innovationMap.Clear();
    }

    /// <summary>
    /// Tracks an innovation (new connection) and assigns or retrieves its innovation number.
    /// If this connection type has been seen before in the current generation, returns the existing innovation number.
    /// Otherwise, assigns a new innovation number.
    /// </summary>
    /// <param name="inNodeId">The ID of the source node.</param>
    /// <param name="outNodeId">The ID of the destination node.</param>
    /// <returns>The innovation data including the innovation number.</returns>
    public InnovationData TrackInnovation(int inNodeId, int outNodeId)
    {
        string mutationKey = GenerateMutationKey(InnovationType.AddConnection, inNodeId, outNodeId);

        if (_innovationMap.TryGetValue(mutationKey, out var innovationData))
        {
            return innovationData;
        }
        else
        {
            var newInnovationData = new InnovationData
            {
                InNodeId = inNodeId,
                OutNodeId = outNodeId,
                InnovationNumber = _innovationCounter
            };

            _innovationMap[mutationKey] = newInnovationData;
            _innovationCounter++;
            return newInnovationData;
        }
    }

    /// <summary>
    /// Tracks an add-node mutation and manages the innovation numbers for the two new connections created.
    /// Ensures that identical add-node mutations in the same generation use consistent node IDs.
    /// </summary>
    /// <param name="existingConnection">The connection being split by the new node.</param>
    /// <param name="populationId">The ID of the population.</param>
    /// <returns>Innovation data for the add-node mutation, including the new node ID.</returns>
    public AddNodeInnovationData TrackAddNodeInnovation(Core.Genome.Genes.ConnectionGene.ConnectionGene existingConnection, int populationId)
    {
        var nodeTracker = StaticManager.Instance.GetNodeTracker(populationId);
        string mutationKey = GenerateMutationKey(InnovationType.AddNode, existingConnection.InNode.Id, existingConnection.OutNode.Id);
        
        int newNodeId;
        if (_nodeInnovationMap.TryGetValue(mutationKey, out var existingNodeId))
        {
            newNodeId = existingNodeId;
        }
        else
        {
            newNodeId = nodeTracker.GetNextNodeId();
            _nodeInnovationMap[mutationKey] = newNodeId;
        }

        int sourceNodeId = existingConnection.InNode.Id;
        int targetNodeId = existingConnection.OutNode.Id;

        var inToNewInnovation = TrackInnovation(sourceNodeId, newNodeId);
        var newToOutInnovation = TrackInnovation(newNodeId, targetNodeId);

        return new AddNodeInnovationData
        {
            InToNew = inToNewInnovation,
            NewToOut = newToOutInnovation,
            NewNodeId = newNodeId
        };
    }

    private string GenerateMutationKey(InnovationType innovationType, int inNodeId, int outNodeId)
    {
        return $"{innovationType}-{inNodeId}-{outNodeId}";
    }
}
