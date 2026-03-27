using Game.Core;
using Snake.AI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Core
{
    /// <summary>
    /// Represents the main controller for the Snake game, managing game state, player actions, and world updates.
    /// </summary>
    /// <remarks>The SnakeGame class coordinates the snake entity, world grid, scoring, and game progression.
    /// It provides methods to advance the game state, handle user input for direction changes, and reset the game.
    /// Events are raised to notify subscribers when the game updates or ends. This class is not thread-safe; all
    /// interactions should occur on the same thread.</remarks>
    public class SnakeGame : ISnakeGameEnvironment
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
        /// Gets or sets the size of the game world, which determines the dimensions of the playing area.
        /// </summary>
        public SnakeWorld.SnakeWorldSize WorldSize { get; set; }

        /// <summary>
        /// Gets or sets the score value.
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game has ended.>
        /// </summary>
        public bool IsGameOver { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the game has been won.
        /// </summary>
        public bool IsGameVictory { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity consumes food.
        /// </summary>
        public bool EatsFood { get; set; }

        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Event raised when the game state is updated, allowing subscribers to react to changes in the game.
        /// </summary>
        public event Action? GameUpdated;

        /// <summary>
        /// Occurs when the game has ended.
        /// </summary>
        /// <remarks>Subscribe to this event to be notified when the game reaches a terminal state, such
        /// as a win, loss, or draw. The event provides no additional data; use other properties or methods to determine
        /// the outcome if needed.</remarks>
        public event Action? GameOver;

        /// <summary>
        /// Creates a new snake entity at the midpoint of the world and assigns it to the Snake property.
        /// </summary>
        public void SpawnSnake()
        {
            Snake = new SnakeEntity(World.PositionMidPoint);
        }

        /// <summary>
        /// Initializes a new instance of the SnakeGame class with the specified world size.
        /// </summary>
        /// <remarks>The game starts with the snake and food spawned in the world. The initial score is
        /// set to 0, and the game is not over or won at initialization.</remarks>
        /// <param name="size">The size of the game world to create. Determines the dimensions of the playing area.</param>
        public SnakeGame(SnakeWorld.SnakeWorldSize size) : this( Convert.ToInt32(size), Convert.ToInt32(size))
        {
            WorldSize = size;
        }

        /// <summary>
        /// Initializes a new instance of the SnakeGame class with the specified world dimensions.
        /// </summary>
        /// <remarks>The game starts with a new snake and a food item placed in the world. The initial
        /// score is set to zero, and the game is not over or won at initialization.</remarks>
        /// <param name="sizeX">The width of the game world, in cells. Must be greater than zero.</param>
        /// <param name="sizeY">The height of the game world, in cells. Must be greater than zero.</param>
        private SnakeGame(Int32 sizeX, Int32 sizeY)
        {
            World = new SnakeWorld(sizeX,sizeY);
            Score = 0;
            IsGameOver = false;
            IsGameVictory = false;
            SpawnSnake();
            World.SpawnFood(Snake);
        }

        /// <summary>
        /// Raises the GameUpdated event to notify subscribers that the game state has changed.
        /// </summary>
        /// <remarks>Call this method to signal that the game has been updated. Subscribers to the
        /// GameUpdated event will be notified. If there are no subscribers, this method has no effect.</remarks>
        public void OnGameUpdated()
        {
            if (GameUpdated != null)
                GameUpdated.Invoke();
        }

        public void OnGameOver()
        {
            if (GameOver != null)
                GameOver.Invoke();
        }

        /// <summary>
        /// Gets a snapshot of the current state of the snake game, including the snake's position, the world layout,
        /// whether the snake has just eaten food, and the current score.
        /// </summary>
        /// <remarks>The returned game state is a copy and is not affected by subsequent changes to the
        /// game. Modifying the returned object does not alter the actual game state.</remarks>
        /// <returns>A <see cref="SnakeGameState"/> object representing the current state of the game.</returns>
        public SnakeGameState GetGameState()
        {
            return new SnakeGameState
            {
                Snake = this.Snake,
                World = this.World,
                EatsFood = this.EatsFood,
                Score = this.Score,
                IsGameOver = this.IsGameOver,
                IsGameVictory = this.IsGameVictory
            };
        }

        /// <summary>
        /// Advances the game state by one tick, moving the snake and handling collisions, food consumption, and game
        /// over conditions.
        /// </summary>
        /// <remarks>Call this method to progress the game by a single step. If the snake collides with a
        /// wall or itself, the game ends and the game over event is triggered. If the snake eats food, its length
        /// increases and the score is updated. This method has no effect if the game is already over.</remarks>
        public void Tick()
        {
            if (IsGameOver) return;

            var nextHead = Snake.NextHeadPosition();

            // Check wall collision
            if (World.IsOutOfBounds(nextHead))
            {
                IsGameOver = true;
                GameOver?.Invoke();
                return;
            }

            // Check self collision (exclude tail if not growing, since tail will move)
            EatsFood = World.IsFoodSpawned
                && nextHead.X == World.FoodPosition.X
                && nextHead.Y == World.FoodPosition.Y;

            // When not growing the tail moves away, so we temporarily check without tail
            bool hitsBody;
            if (!EatsFood)
            {
                // Tail will be removed, so exclude it from collision
                var tail = Snake.Body.Last();
                hitsBody = Snake.OccupiesCell(nextHead)
                    && nextHead != tail;
            }
            else
            {
                hitsBody = Snake.OccupiesCell(nextHead);
            }

            if (hitsBody)
            {
                IsGameOver = true;
                GameOver?.Invoke();
                return;
            }

            Snake.Move(EatsFood);

            if (EatsFood)
            {
                Score += 10;
                var canFoodSpawn = World.SpawnFood(Snake);
                if (!canFoodSpawn)
                {
                    IsGameOver = true;
                    IsGameVictory = true;
                    GameOver?.Invoke();
                }

            }
            OnGameUpdated();
        }

        /// <summary>
        /// Changes the current movement direction of the snake to the specified direction, if the game is not over.
        /// </summary>
        /// <remarks>This method has no effect if the game is over. Use this method to control the snake's
        /// movement during active gameplay.</remarks>
        /// <param name="direction">The new direction to set for the snake's movement.</param>
        public void ChangeDirection(Direction2D direction)
        {
            if (!IsGameOver)
            {
                Snake.ChangeDirection(direction);
            }
        }

        /// <summary>
        /// Resets the game world to its initial state, restarting the game and clearing the current score.
        /// </summary>
        /// <remarks>This method reinitializes the world using its current size, resets the score to zero,
        /// sets the game state to not over, and respawns both the snake and food. Call this method to start a new game
        /// session or to restart after a game over.</remarks>
        public void ResetWorld()
        {
            var oldWorld = World;
            World = new SnakeWorld(Convert.ToInt32(oldWorld.Size.X+1), Convert.ToInt32(oldWorld.Size.Y+1));
            Score = 0;
            IsGameOver = false;
            SpawnSnake();
            World.SpawnFood(Snake);
        }

        /// <inheritdoc/>
        public SnakeGameState Step(Direction2D action)
        {
            ChangeDirection(action);
            Tick();
            return GetGameState();
        }

        /// <inheritdoc/>
        public SnakeGameState Reset()
        {
            ResetWorld();
            return GetGameState();
        }

        public ISnakeGameEnvironment Clone()
        {
            var clone = new SnakeGame(this.WorldSize);
            clone.Reset();
            return clone;
        }
    }
}
