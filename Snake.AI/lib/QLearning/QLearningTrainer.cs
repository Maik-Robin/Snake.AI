using Game.Core;
using Snake.Core;

namespace Snake.AI.QLearning
{
    /// <summary>
    /// Trains a Q-Learning agent to play Snake.
    /// Implements the <see cref="IGameTrainer"/> interface so it integrates with
    /// the rest of the project's training pipeline.
    /// </summary>
    public class QLearningTrainer : IGameTrainer
    {
        private readonly ISnakeGameEnvironment _env;
        private readonly QLearningConfig _config;

        /// <summary>The Q-table produced (or updated) during training.</summary>
        public QTable QTable { get; } = new QTable();

        /// <param name="env">Snake game environment used for simulation.</param>
        /// <param name="config">Hyperparameters; uses defaults when <see langword="null"/>.</param>
        public QLearningTrainer(ISnakeGameEnvironment env, QLearningConfig? config = null)
        {
            _env = env;
            _config = config ?? new QLearningConfig();
        }

        /// <summary>
        /// Runs the full Q-Learning training loop and stores results in <see cref="QTable"/>.
        /// </summary>
        public void Train()
        {
            var rng = new Random();
            double epsilon = _config.EpsilonStart;

            for (int episode = 0; episode < _config.Episodes; episode++)
            {
                var state = _env.Reset();
                string stateKey = EncodeState(state);

                for (int step = 0; step < _config.MaxStepsPerEpisode; step++)
                {
                    // Epsilon-greedy action selection
                    int action = rng.NextDouble() < epsilon
                        ? rng.Next(QTable.ActionCount)
                        : QTable.BestAction(stateKey);

                    var dir = ActionToDirection(state, action);
                    var nextState = _env.Step(dir);
                    string nextKey = EncodeState(nextState);

                    double reward = ComputeReward(state, nextState);
                    bool terminal = nextState.IsGameOver || nextState.IsGameVictory;

                    QTable.Update(stateKey, action, reward, nextKey, terminal, _config);

                    state = nextState;
                    stateKey = nextKey;

                    if (terminal)
                        break;
                }

                // Decay epsilon
                epsilon = Math.Max(_config.EpsilonMin, epsilon * _config.EpsilonDecay);
            }
        }

        /// <summary>
        /// Saves the trained Q-table to the specified file path as JSON.
        /// </summary>
        public void SaveTrainingData(string path) => QTable.Save(path);

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private double ComputeReward(SnakeGameState current, SnakeGameState next)
        {
            if (next.IsGameVictory)
                return _config.RewardVictory;
            if (next.IsGameOver)
                return _config.PenaltyDeath;
            if (current.EatsFood)
                return _config.RewardFood;
            return _config.PenaltyStep;
        }

        private static Direction2D ActionToDirection(SnakeGameState state, int action)
        {
            var rel = (RelativeDirection2D)action;
            return Direction2DHelper.RelativeToDirection(state.Snake.CurrentDirection, rel);
        }

        /// <summary>
        /// Converts the continuous double[] encoding from <see cref="StateEncoder"/> into a
        /// compact binary string that can be used as a dictionary key.
        /// Each element is rounded to the nearest integer so that very close floating-point
        /// values map to the same key.
        /// </summary>
        public static string EncodeState(SnakeGameState state)
        {
            var encoded = StateEncoder.Encode(state);
            return string.Join(",", encoded.Select(v => ((int)Math.Round(v)).ToString()));
        }
    }
}
