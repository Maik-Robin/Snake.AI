using Game.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.Serialization.Formatters;

namespace Snake.Core
{
    /// <summary>
    /// Represents a snake entity in the game, maintaining its body segments, movement direction, and logic for movement
    /// and growth.
    /// </summary>
    /// <remarks>The SnakeEntity manages the position and direction of a snake within the game grid. It
    /// provides methods to move, grow, and change direction, while preventing invalid direction reversals. The head and
    /// body segments are tracked as a sequence of coordinates, allowing for collision detection and rendering. This
    /// class is typically used as the player-controlled or AI-controlled snake in a classic snake game
    /// implementation.</remarks>
    public class SnakeEntity : Game.Core.Entity
    {

        LinkedList<Vector2> body;

        /// <summary>
        /// Gets the position of the head (which is first segment of the body).
        /// </summary>
        public Vector2 Head { get { return body.ToArray()[0]; } }

        /// <summary>
        /// Gets an array containing the points that define the body of the shape.
        /// </summary>
        public Vector2[] Body { get { return body.ToArray(); } }


        private Direction2D _currentDirection;

        /// <summary>
        /// Gets the current movement direction as a two-dimensional vector.
        /// </summary>
        public Direction2D CurrentDirection { get { return _currentDirection; } }

        private Direction2D _lastProcessedDirection;

        /// <summary>
        /// Gets the last processed direction as a two-dimensional value.
        /// </summary>
        private Direction2D LastProcessedDirection { get { return Direction2D.Right; } }

        /// <summary>
        /// Initializes a new instance of the SnakeEntity class with the specified starting position, initial length,
        /// and direction.
        /// </summary>
        /// <remarks>The snake's body is initialized with consecutive segments extending from the starting
        /// position in the opposite direction of movement. The head is placed at the specified coordinates.</remarks>
        /// <param name="startX">The X-coordinate of the snake's starting head position.</param>
        /// <param name="startY">The Y-coordinate of the snake's starting head position.</param>
        /// <param name="initialLength">The initial number of segments in the snake's body. Must be greater than zero. Defaults to 3.</param>
        /// <param name="initialDir">The initial movement direction of the snake. Defaults to Direction2D.Right.</param>
        public SnakeEntity(Vector2 startPosition, int initialLength = 3, Direction2D initialDir = Direction2D.Right)
        {
            body = new LinkedList<Vector2>();
            _currentDirection = initialDir;
            _lastProcessedDirection = initialDir;

            //TODO: determine the initial dirction and initialize the body segments accordingly. For example, if the initial direction is right, the body should extend to the left of the head.
            for (int i = 0; i < initialLength; i++)
            {
                if(initialDir == Direction2D.Right)
                    body.AddLast(new Vector2(startPosition.X-i, startPosition.Y));
            }
        }

        /// <summary>
        /// Calculates the next position of the head based on the current direction.
        /// </summary>
        /// <returns>A <see cref="Vector2"/> representing the coordinates of the next head position.</returns>
        public Vector2 NextHeadPosition()
        {
            var delta = Direction2DHelper.DirectionDelta(_currentDirection, 1);
            var deltaX = Head.X + delta.X;
            var deltaY = Head.Y + delta.Y;
            return new Vector2(deltaX, deltaY);
        }

        /// <summary>
        /// Advances the object to its next position, optionally increasing its length.
        /// </summary>
        /// <remarks>Call this method to update the object's position based on its current direction. If
        /// grow is set to true, the object will become longer after the move, which is typically used when consuming an
        /// item or achieving a growth condition.</remarks>
        /// <param name="grow">true to increase the length during the move; false to maintain the current length.</param>
        public void Move(bool grow)
        {
            var next = NextHeadPosition();
            body.AddFirst(next);
            if (!grow)
            {
                body.RemoveLast();
            }
            _lastProcessedDirection = _currentDirection;
        }

        /// <summary>
        /// Determines whether the object occupies the specified cell.
        /// </summary>
        /// <param name="cell">The coordinates of the cell to check for occupancy.</param>
        /// <returns>true if the object occupies the specified cell; otherwise, false.</returns>
        public bool OccupiesCell(Vector2 cell)
        {
            return body.Contains(cell);
        }

        /// <summary>
        /// Changes the current movement direction to the specified direction, unless the new direction is directly
        /// opposite to the current one.
        /// </summary>
        /// <remarks>This method prevents changing direction to the direct opposite (for example, from Up
        /// to Down) to avoid invalid or abrupt movement reversals. If the specified direction is directly opposite to
        /// the current direction, the change is ignored.</remarks>
        /// <param name="newDirection">The new direction to set. Cannot be directly opposite to the current direction.</param>
        public void ChangeDirection(Direction2D newDirection)
        {
            // Prevent changing to the opposite direction to avoid invalid movement reversals
            if (_lastProcessedDirection == Direction2D.Up && newDirection == Direction2D.Down) return;
            if (_lastProcessedDirection == Direction2D.Down && newDirection == Direction2D.Up) return;
            if (_lastProcessedDirection == Direction2D.Left && newDirection == Direction2D.Right) return;
            if (_lastProcessedDirection == Direction2D.Right && newDirection == Direction2D.Left) return;
            _currentDirection = newDirection;
        }

        /// <summary>
        /// Changes the current movement direction to the specified direction, unless the new direction is directly
        /// opposite to the current one.
        /// </summary>
        /// <remarks>This method prevents changing direction to the direct opposite (for example, from Up
        /// to Down) to avoid invalid or abrupt movement reversals. If the specified direction is directly opposite to
        /// the current direction, the change is ignored.</remarks>
        /// <param name="newDirection">The new direction to set. Cannot be directly opposite to the current direction.</param>
        public void ChangeDirectionRelative(RelativeDirection2D relDirection)
        {
            var direction = Direction2DHelper.RelativeToDirection(_currentDirection, relDirection);
            ChangeDirection(direction);
        }

    }
}
