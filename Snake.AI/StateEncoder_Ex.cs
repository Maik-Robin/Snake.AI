using Game.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Snake.AI
{
    public static class StateEncoder
    {
        public static Int32 InputSize = 14;
        public static Int32 OutputSize = 3;

        public static double[] Encode(SnakeGameState s)
        {
            var state = new double[InputSize];
            //Inputs(11 total):
            var currentDirection = s.Snake.CurrentDirection;
            var snakeHeadVector2= s.Snake.Head;
            var foodPositionVector2 = s.World.FoodPosition;

            // Direction bits
            bool dirUp = currentDirection == Direction2D.Up;
            bool dirDown = currentDirection == Direction2D.Down;
            bool dirLeft = currentDirection == Direction2D.Left;
            bool dirRight = currentDirection == Direction2D.Right;

            // Food relative position (can be multi-true if aligned both axes false)
            bool foodLeft = foodPositionVector2.X < snakeHeadVector2.X;
            bool foodRight = foodPositionVector2.X > snakeHeadVector2.X;
            bool foodUp = foodPositionVector2.Y < snakeHeadVector2.Y;
            bool foodDown = foodPositionVector2.Y > snakeHeadVector2.Y;

            //Immediate obstacles(wall or tail) in front, left, and right(3 binary inputs)
            state[0] = IsDanger(s, RelativeDirection2D.StraightForward, snakeHeadVector2) ? 1 : 0;
            state[1] = IsDanger(s, RelativeDirection2D.TurnLeft, snakeHeadVector2) ? 1 : 0;
            state[2] = IsDanger(s, RelativeDirection2D.TurnRight, snakeHeadVector2) ? 1 : 0;

            var maxDistance = Math.Max(s.World.PositionMax.X, s.World.PositionMax.Y); // Max possible distance on the board
            var blockBody = new Func<Vector2, bool>(pos => s.Snake.OccupiesCell(pos));
            var blockWall = new Func<Vector2, bool>(pos => s.World.IsWall(pos));


            var distanceStraightBody = Raycast(s, snakeHeadVector2, RelativeDirection2D.StraightForward, blockBody);
            var distanceLeftBody = Raycast(s, snakeHeadVector2, RelativeDirection2D.TurnLeft, blockBody);
            var distanceRightBody = Raycast(s, snakeHeadVector2, RelativeDirection2D.TurnRight, blockBody);

            var distanceStraightWall = Raycast(s, snakeHeadVector2, RelativeDirection2D.StraightForward, blockWall);
            var distanceLeftWall = Raycast(s, snakeHeadVector2, RelativeDirection2D.TurnLeft, blockWall);
            var distanceRightWall = Raycast(s, snakeHeadVector2, RelativeDirection2D.TurnRight, blockWall);


            state[3] = distanceStraightBody.hit ? distanceStraightBody.distance / maxDistance : 1;
            state[4] = distanceLeftBody.hit ? distanceLeftBody.distance / maxDistance : 1;
            state[5] = distanceRightBody.hit ? distanceRightBody.distance / maxDistance : 1;


            //var distanceStraight = Raycast(s, snakeHeadVector2, RelativeDirection2D.StraightForward, blockingFunction);
            //var distanceLeft = Raycast(s, snakeHeadVector2, RelativeDirection2D.TurnLeft, blockingFunction);
            //var distanceRight = Raycast(s, snakeHeadVector2, RelativeDirection2D.TurnRight, blockingFunction);


            //state[3] =  distanceStraight.hit ? distanceStraight.distance / maxDistance  : -0.1;
            //state[4] =  distanceLeft.hit ? distanceLeft.distance / maxDistance : -0.1;
            //state[5] = distanceRight.hit ? distanceRight.distance / maxDistance : -0.1;


            //Food position relative to snake head(2 binary inputs: food right?, food below ?)
            state[6] = foodRight ? 1 : 0;
            state[7] = foodDown ? 1 : 0;

            //Current direction one - hot encoded(3 inputs for Up, Right, Down; Left is implicit)
            state[8] = dirUp ? 1 : 0;
            state[9] = dirRight ? 1 : 0;
            state[10] = dirDown ? 1 : 0;

            state[11] = distanceStraightWall.hit ? distanceStraightWall.distance / maxDistance : 1;
            state[12] = distanceLeftWall.hit ? distanceLeftWall.distance / maxDistance : 1;
            state[13] = distanceRightWall.hit ? distanceRightWall.distance / maxDistance : 1;

            return state;
        }

        /// <summary>Check for boarders for the given direction</summary>
        private static bool IsDanger(SnakeGameState s, RelativeDirection2D relDir, Vector2 head)
        {
            var dir = Direction2DHelper.RelativeToDirection(s.Snake.CurrentDirection, relDir);
            var delta = Direction2DHelper.DirectionDelta(dir);
            var newVector2 = new Vector2(head.X + delta.X, head.Y + delta.Y);
            if (s.Snake.OccupiesCell(newVector2)) return true;
            if(s.World.IsOutOfBounds(newVector2)) return true;
            return false;
        }

        /// <summary>
        /// Cast a ray from the given position in the specified direction until hitting an obstacle.
        /// Returns the distance (number of cells) to the nearest obstacle (wall or snake body).
        /// </summary>
        /// <param name="s">The current game state</param>
        /// <param name="startPosition">Starting position of the ray</param>
        /// <param name="direction">Direction to cast the ray</param>
        /// <param name="isBlocking">Function that determines if a position is blocking.</param>
        /// <returns>Distance to the nearest obstacle in cells</returns>
        public static (bool hit, int distance) Raycast(SnakeGameState s, Vector2 startPosition, Direction2D direction, Func<Vector2, bool>? isBlocking)
        {
            var maxRange = Math.Max(s.World.PositionMax.X, s.World.PositionMax.Y);
            var delta = Direction2DHelper.DirectionDelta(direction);
            var rayCastPos = new Vector2(startPosition.X + delta.X, startPosition.Y + delta.Y);
            int distance = 1;
            var hitObstacle = false;
            do
            {
                if (isBlocking(rayCastPos))
                {
                    hitObstacle = true;
                    break;
                }
                rayCastPos = new Vector2(rayCastPos.X + delta.X, rayCastPos.Y + delta.Y);
                distance++;
            } while (distance < maxRange);
            return (hitObstacle, distance);
        }

        /// <summary>
        /// Cast a ray from the given position in the specified relative direction until hitting an obstacle.
        /// Returns the distance (number of cells) to the nearest obstacle (wall or snake body).
        /// </summary>
        /// <param name="s">The current game state</param>
        /// <param name="startPosition">Starting position of the ray</param>
        /// <param name="relativeDirection">Relative direction to cast the ray</param>
        /// <param name="isBlocking">Function that determines if a position is blocking. If null, checks for walls and snake body.</param>
        /// <returns>Distance to the nearest obstacle in cells</returns>
        public static (bool hit, int distance) Raycast(SnakeGameState s, Vector2 startPosition, RelativeDirection2D relativeDirection, Func<Vector2, bool>? isBlocking)
        {
            var direction = Direction2DHelper.RelativeToDirection(s.Snake.CurrentDirection, relativeDirection);
            return Raycast(s, startPosition, direction, isBlocking);
        }
    }
}
