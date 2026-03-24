using Game.Core;
using Snake.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Snake.AI
{
    /// <summary>
    /// Represents the current state of a snake game, including the snake, world, score, and game status.
    /// </summary>
    /// <remarks>This class encapsulates all relevant information about a single frame or step of the game,
    /// allowing game logic and rendering code to access the current snake position, world layout, score, and game
    /// outcome flags. It is typically used to track and update the game's progress after each move.</remarks>
    public class SnakeGameState : IGameState
    {
        /// <summary>
        /// Gets or sets the snake entity associated with the current game state.
        /// </summary>
        public SnakeEntity Snake { get; set; }

        /// <summary>
        /// Gets or sets the current state of the Snake game world.
        /// </summary>
        public SnakeWorld World { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity consumes food.
        /// </summary>
        public Boolean EatsFood { get; set; }

        /// <summary>
        /// Gets or sets the score value.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game has ended.
        /// </summary>
        public Boolean IsGameOver { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game has been won.
        /// </summary>
        public Boolean IsGameVictory { get; set; }
    }

}
