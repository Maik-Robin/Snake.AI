using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core
{
    /// <summary>
    /// Represents the state of a game, including whether the game has ended and if the player has achieved victory.
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// Gets a value indicating whether the game has ended.
        /// </summary>
        bool IsGameOver { get; set; }

        /// <summary>
        /// Gets a value indicating whether the game has been won.
        /// </summary>
        bool IsGameVictory { get; set; }
    }
}
