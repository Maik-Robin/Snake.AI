using Game.Core;

namespace Snake.AI.QLearning
{
    /// <summary>
    /// Plays Snake using a pre-trained Q-table.
    /// Implements <see cref="ISnakeGameController"/> so it can be plugged directly
    /// into any existing game runner or UI.
    /// </summary>
    public class QLearningController : ISnakeGameController
    {
        private readonly QTable _qTable;

        /// <param name="qTable">A trained Q-table. Pass a new <see cref="QTable"/> and call
        /// <see cref="QTable.Load"/> before use, or supply one returned by
        /// <see cref="QLearningTrainer"/> after training.</param>
        public QLearningController(QTable qTable)
        {
            _qTable = qTable;
        }

        /// <summary>
        /// Creates a controller and immediately loads the Q-table from <paramref name="filePath"/>.
        /// </summary>
        public static QLearningController FromFile(string filePath)
        {
            var table = new QTable();
            table.Load(filePath);
            return new QLearningController(table);
        }

        /// <inheritdoc/>
        (Direction2D dirOut, double[] rawOutput) ISnakeGameController.GetNextMove(SnakeGameState state)
        {
            string key = QLearningTrainer.EncodeState(state);
            var qValues = _qTable.GetQValues(key);
            int bestAction = _qTable.BestAction(key);
            var rel = (RelativeDirection2D)bestAction;
            var dir = Direction2DHelper.RelativeToDirection(state.Snake.CurrentDirection, rel);
            return (dir, qValues);
        }
    }
}
