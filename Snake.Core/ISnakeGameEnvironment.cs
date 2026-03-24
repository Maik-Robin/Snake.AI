using Game.Core;
using Snake.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Core
{
    /// <summary>
    /// Defines the interface for a snake game environment that supports resetting and advancing the game state.
    /// </summary>
    /// <remarks>Implementations of this interface provide the core mechanics for interacting with a snake
    /// game simulation, such as initializing the environment and applying player actions. This interface is suitable
    /// for use in reinforcement learning scenarios or custom game logic.</remarks>
    public interface ISnakeGameEnvironment : IGameEnvironment
    {
        /// <summary>
        /// Resets the game to its initial state and returns a new game state instance.
        /// </summary>
        /// <remarks>Use this method to restart the game from the beginning, clearing any progress or
        /// changes made during previous gameplay.</remarks>
        /// <returns>A <see cref="SnakeGameState"/> representing the initial state of the game after the reset.</returns>
        SnakeGameState Reset();

        /// <summary>
        /// Advances the game state by one step using the specified movement direction.
        /// </summary>
        /// <remarks>If the action results in a collision or the game ending, the returned state will
        /// reflect these changes. This method does not modify the current state; it returns a new instance representing
        /// the next state.</remarks>
        /// <param name="action">The direction in which the snake should move during this step. Must be a valid value of <see
        /// cref="Direction2D"/>.</param>
        /// <returns>A new <see cref="SnakeGameState"/> representing the updated state of the game after applying the specified
        /// action.</returns>
        SnakeGameState Step(Direction2D action);
    }
}
