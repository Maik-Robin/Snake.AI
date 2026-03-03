using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core
{
 
    /// <summary>
    /// Provides helper methods for working with 2D directions and relative movements.
    /// </summary>
    public class Direction2DHelper
    {

        /// <summary>
        /// Determines the absolute direction resulting from applying a relative movement to a given starting direction.
        /// </summary>
        /// <param name="startDir">The current absolute direction before applying the relative movement.</param>
        /// <param name="a">The relative direction to apply, such as turning left, right, or moving straight forward.</param>
        /// <returns>The absolute direction after applying the specified relative movement to the current direction.</returns>
        /// <exception cref="ArgumentException">Thrown if either the current direction or the relative direction is not a valid value of their respective
        /// enumerations.</exception>
        public static Direction2D RelativeToDirection(Direction2D startDir, RelativeDirection2D a)
        {
            switch (a)
            {
                case RelativeDirection2D.StraightForward:
                    return startDir; // if we go straight forward, we keep the same direction
                case RelativeDirection2D.TurnLeft:
                    return startDir switch
                    {
                        Direction2D.Up => Direction2D.Left, //if direction is up ^, and we turn left, we will be facing left < 
                        Direction2D.Down => Direction2D.Right,// if direction is down ˅, and we turn left , we will be facing right >
                        Direction2D.Left => Direction2D.Down, //if direction is left < , and we turn left , we will be facing down ˅
                        Direction2D.Right => Direction2D.Up, //if direction is right >, and we turn left, we will be facing up ^
                        _ => throw new ArgumentException("Invalid direction")
                    };
                case RelativeDirection2D.TurnRight:
                    return startDir switch
                    {
                        Direction2D.Up => Direction2D.Right, //if direction is up ^, and we turn right, we will be facing right >
                        Direction2D.Down => Direction2D.Left, //if direction is down ˅, and we turn right , we will be facing left <
                        Direction2D.Left => Direction2D.Up, // if direction is left < , and we turn right , we will be facing up ^
                        Direction2D.Right => Direction2D.Down, // if direction is right >, and we turn right, we will be facing down ˅
                        _ => throw new ArgumentException("Invalid direction")
                    };
                default:
                    throw new ArgumentException("Invalid relative action");
            }
        }

        /// <summary>
        /// Calculates the change in position as a 2D vector for a given direction and distance.
        /// </summary>
        /// <param name="dir">The direction in which to calculate the movement delta.</param>
        /// <param name="distance">The distance to move in the specified direction. Defaults to 1. Must be a finite number.</param>
        /// <returns>A <see cref="Vector2"/> representing the change in position for the specified direction and distance.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="dir"/> is not a valid <see cref="Direction2D"/> value.</exception>
        public static Vector2 DirectionDelta(Direction2D dir, float distance = 1)
        {
            return dir switch
            {
                Direction2D.Up => new Vector2(0, -distance), // Up corresponds to a negative change in the y-coordinate
                Direction2D.Down => new Vector2(0, distance), // Down corresponds to a positive change in the y-coordinate
                Direction2D.Left => new Vector2(-distance, 0), // Left corresponds to a negative change in the x-coordinate
                Direction2D.Right => new Vector2(distance, 0), // Right corresponds to a positive change in the x-coordinate
                _ => throw new ArgumentException("Invalid direction")
            };
        }

    }
}
