using System.Text.Json;

namespace Snake.AI.QLearning
{
    /// <summary>
    /// Stores and manages Q-values for state-action pairs using a dictionary.
    /// The state is represented as a string key derived from the encoded game state.
    /// Actions are indexed 0 = TurnLeft, 1 = StraightForward, 2 = TurnRight.
    /// </summary>
    public class QTable
    {
        /// <summary>The number of discrete actions available to the agent.</summary>
        public const int ActionCount = 3;

        private readonly Dictionary<string, double[]> _table = new();

        /// <summary>
        /// Returns the Q-values for all actions in the given state.
        /// If the state has not been seen before, it is initialised with zeros.
        /// </summary>
        public double[] GetQValues(string stateKey)
        {
            if (!_table.TryGetValue(stateKey, out var values))
            {
                values = new double[ActionCount];
                _table[stateKey] = values;
            }
            return values;
        }

        /// <summary>
        /// Updates the Q-value for a specific state-action pair using the Bellman equation.
        /// </summary>
        /// <param name="stateKey">Encoded current state.</param>
        /// <param name="action">Action index taken (0-2).</param>
        /// <param name="reward">Reward received after taking the action.</param>
        /// <param name="nextStateKey">Encoded next state.</param>
        /// <param name="isTerminal">Whether the next state is terminal.</param>
        /// <param name="config">Hyperparameter configuration.</param>
        public void Update(string stateKey, int action, double reward, string nextStateKey, bool isTerminal, QLearningConfig config)
        {
            var current = GetQValues(stateKey);
            double maxNextQ = isTerminal ? 0.0 : GetQValues(nextStateKey).Max();
            double target = reward + config.DiscountFactor * maxNextQ;
            current[action] += config.LearningRate * (target - current[action]);
        }

        /// <summary>Returns the action index with the highest Q-value for the given state.</summary>
        public int BestAction(string stateKey)
        {
            var values = GetQValues(stateKey);
            int best = 0;
            for (int i = 1; i < ActionCount; i++)
                if (values[i] > values[best])
                    best = i;
            return best;
        }

        /// <summary>Number of unique states currently stored in the table.</summary>
        public int StateCount => _table.Count;

        /// <summary>Serializes the Q-table to a JSON string.</summary>
        public string Serialize() => JsonSerializer.Serialize(_table);

        /// <summary>Replaces the current table contents with values deserialized from <paramref name="json"/>.</summary>
        public void Deserialize(string json)
        {
            var loaded = JsonSerializer.Deserialize<Dictionary<string, double[]>>(json)
                         ?? throw new InvalidDataException("Could not deserialize Q-table.");
            _table.Clear();
            foreach (var kv in loaded)
                _table[kv.Key] = kv.Value;
        }

        /// <summary>Saves the Q-table to <paramref name="filePath"/>.</summary>
        public void Save(string filePath) => File.WriteAllText(filePath, Serialize());

        /// <summary>Loads the Q-table from <paramref name="filePath"/>.</summary>
        public void Load(string filePath) => Deserialize(File.ReadAllText(filePath));
    }
}
