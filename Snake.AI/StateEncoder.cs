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
    public static class StateEncoder_Org
    {
        public static Int32 InputSize = 11;
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
            var blockingFunction = new Func<Vector2, bool>(pos => s.Snake.OccupiesCell(pos));

            //Distance to nearest tail segment in front, left, and right(3 normalized inputs, 1 / distance)
            state[3] = 1 / GetDistanceToTail(s, RelativeDirection2D.StraightForward, snakeHeadVector2);
            state[4] = 1 / GetDistanceToTail(s, RelativeDirection2D.TurnLeft, snakeHeadVector2);
            state[5] = 1 / GetDistanceToTail(s, RelativeDirection2D.TurnRight, snakeHeadVector2);


            //Food position relative to snake head(2 binary inputs: food right?, food below ?)
            state[6] = foodRight ? 1 : 0;
            state[7] = foodDown ? 1 : 0;

            //Current direction one - hot encoded(3 inputs for Up, Right, Down; Left is implicit)
            state[8] = dirUp ? 1 : 0;
            state[9] = dirRight ? 1 : 0;
            state[10] = dirDown ? 1 : 0;

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

        private static double GetDistanceToTail(SnakeGameState s, RelativeDirection2D relativeDirection, Vector2 startPosition)
        {
            var direction = Direction2DHelper.RelativeToDirection(s.Snake.CurrentDirection, relativeDirection);
            var delta = Direction2DHelper.DirectionDelta(direction);
            int distance = 1;
            int maxDistance = (int)MathF.Max(s.World.PositionMax.X, s.World.PositionMax.Y);

            var body = s.Snake.Body; // cache: avoids repeated LinkedList.ToArray() allocations

            var x = startPosition.X + delta.X;
            var y = startPosition.Y + delta.Y;

            while (distance <= maxDistance)
            {
                if (x < 0 || x >= maxDistance || y < 0 || y >= maxDistance)
                    break;

                for (int i = 1; i < body.Length; i++)
                {
                    if (body[i].X == x && body[i].Y == y)
                    {
                        return distance;
                    }
                }

                x += delta.X;
                y += delta.Y;
                distance++;
            }

            return maxDistance + 1;
        }

    }
}
