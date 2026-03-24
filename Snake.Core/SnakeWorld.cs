using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Snake.Core
{
 
    /// <summary>
    /// Represents the game world for a snake game, providing properties and methods for managing the world size,
    /// boundaries, and food spawning logic.
    /// </summary>
    /// <remarks>The SnakeWorld class defines the playable area for a snake game, including the dimensions,
    /// valid position ranges, and food management. It offers utility properties for determining world boundaries and
    /// supports spawning food at random, unoccupied positions. Use the provided constructors to initialize the world
    /// with custom dimensions or predefined sizes. This class is not thread-safe.</remarks>
    public class SnakeWorld : Game.Core.WorldBase
    {
        /// <summary>
        /// Gets the width and height of the object as a two-dimensional vector.
        /// </summary>
        public Vector2 Size { get; }

        /// <summary>
        /// Gets the minimum allowed position value as a two-dimensional vector.
        /// </summary>
        public Vector2 PositionMin { get { return new Vector2(0f, 0f);  } }

        /// <summary>
        /// Gets the maximum position value allowed within the current bounds.
        /// </summary>
        public Vector2 PositionMax { get { return Size; } }

        Vector2 _positionMidPoint;

        /// <summary>
        /// Gets the coordinates of the midpoint of the object's size.
        /// </summary>
        public Vector2 PositionMidPoint { get { return _positionMidPoint; } }

        Vector2 _foodPosition;
        /// <summary>
        /// Gets the current position of the food item in the game world.
        /// </summary>
        public Vector2 FoodPosition { get { return _foodPosition; } }

        private bool _isFoodSpawned;

        /// <summary>
        /// Gets or sets a value indicating whether food is currently present in the game.
        /// </summary>
        public bool IsFoodSpawned { get { return _isFoodSpawned; } }

        private Int32 _maxFields;

        /// <summary>
        /// Gets the maximum number of fields that can be processed / occupied
        /// </summary>
        public Int32 MaxFields { get { return _maxFields; } }

        /// <summary>
        /// Specifies the available world sizes for a snake game environment.
        /// </summary>
        /// <remarks>Each value represents a predefined world size, typically corresponding to the width
        /// or height of the game grid. The numeric values can be used to configure the dimensions of the game
        /// area.</remarks>
        public enum SnakeWorldSize
        {
            Small = 10,
            Medium = 20,
            Large = 30
        }

        private void CalculateVariables()
        {
            var xMod = (Convert.ToInt32(Size.X) & 1) == 0;
            var yMod = (Convert.ToInt32(Size.Y) & 1) == 0;
            var xMid = xMod ? Convert.ToInt32(Size.X) / 2 : (Convert.ToInt32(Size.X) - 1) / 2;
            var yMid = yMod ? Convert.ToInt32(Size.Y) / 2 : (Convert.ToInt32(Size.Y) - 1) / 2;
            _positionMidPoint = new Vector2(xMid, yMid);
            _maxFields = Convert.ToInt32((PositionMax.X+1 / 1) * (PositionMax.Y+1 / 1));
        }

        /// <summary>
        /// Initializes a new instance of the SnakeWorld class with the specified horizontal and vertical dimensions.
        /// </summary>
        /// <param name="sizeX">The width of the world, in units. Must be a positive value.</param>
        /// <param name="sizeY">The height of the world, in units. Must be a positive value.</param>
        public SnakeWorld(Int32 sizeX, Int32 sizeY)
        {
            this.Name = "SnakeWorld";
            Size = new Vector2(sizeX-1, sizeY-1);
            CalculateVariables();
        }

        /// <summary>
        /// Initializes a new instance of the SnakeWorld class with the specified world size.
        /// </summary>
        /// <param name="worldSize">The size of the world, specified as a value of the SnakeWorldSize enumeration.</param>
        public SnakeWorld(SnakeWorldSize worldSize) : this (Convert.ToInt32(worldSize), Convert.ToInt32(worldSize))
        { 

        }

        /// <summary>
        /// Attempts to spawn a food item at a random unoccupied position on the game board, avoiding the cells
        /// currently occupied by the specified snake.
        /// </summary>
        /// <param name="snake">The snake entity whose occupied cells will be avoided when selecting a spawn position for the food. Cannot
        /// be null.</param>
        /// <returns>true if the food was successfully spawned at a valid position; otherwise, false.</returns>
        public bool SpawnFood(SnakeEntity snake)
        {
            if (snake.Body.Count() >= MaxFields)
                return false;

            var newRandomVector = new Vector2(0, 0);
            do
            {
                // the +1 is needed because the upper bound of Random.Next is exclusive, and we want to include PositionMax as a valid spawn point
                var randomX = Random.Shared.Next(Convert.ToInt32(PositionMin.X), Convert.ToInt32(PositionMax.X+1)); 
                var randomY = Random.Shared.Next(Convert.ToInt32(PositionMin.Y), Convert.ToInt32(PositionMax.Y+1));
                newRandomVector = new Vector2(randomX, randomY);
            }
            while (snake.OccupiesCell(newRandomVector));
            _foodPosition = newRandomVector;
            _isFoodSpawned = true;
            return true;
        }

        /// <summary>
        /// Determines whether the specified vector is outside the valid bounds of the grid.
        /// </summary>
        /// <param name="">The vector to check for boundary validity.</param>
        /// <returns>true if the vector is outside the grid boundaries; otherwise, false.</returns>
        public bool IsOutOfBounds(Vector2 v)
        {
            return v.X < PositionMin.X || v.X > PositionMax.X  || v.Y < PositionMin.Y || v.Y > PositionMax.Y;
        }

    }
}
