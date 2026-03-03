using Game.Core;
using Snake.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnakeEngine.AI
{
    public class SnakeCheater1000 : ISnakeGameController
    {
        public Direction2D GetNextMove(SnakeGameState state)
        {
            var maxPoint = state.World.PositionMax;
            var snakeHead = state.Snake.Head;
            var xMod = (Convert.ToInt32(snakeHead.X) & 1) == 0;
            var yMod = (Convert.ToInt32(snakeHead.Y) & 1) == 0;

            if (snakeHead.X == maxPoint.X && snakeHead.Y == maxPoint.Y)
                return Direction2D.Up; //Up on max
            if (snakeHead.X == 0 && snakeHead.Y == 0)
                return Direction2D.Down; // Down on 0/0
            if (snakeHead.X == 0 && snakeHead.Y == maxPoint.Y)
                return Direction2D.Right; //Left on Max/0
            if (snakeHead.X == maxPoint.X && snakeHead.Y == 0)
                return Direction2D.Left; //Right on 0/Max

            //Some directions chance in case of xmod / ymod
            if (snakeHead.Y == maxPoint.Y && snakeHead.X > 0 && !xMod)
                return Direction2D.Up;
            if (xMod == false && snakeHead.Y == 1 && snakeHead.X < maxPoint.X)
                return Direction2D.Right;
            if (xMod == true && snakeHead.X > 0 && snakeHead.Y == 1)
                return Direction2D.Down;

            //Check hit wall on right wall
            if (snakeHead.X == maxPoint.X && snakeHead.Y > 0 && state.Snake.CurrentDirection != Direction2D.Up)
                return Direction2D.Up;
            //Check hit wall on top wall
            if(snakeHead.Y == 0 &&snakeHead.X > 0 && state.Snake.CurrentDirection != Direction2D.Left)
                return Direction2D.Left;
            //Check hit wall in down wall
            if(snakeHead.Y == maxPoint.Y && snakeHead.X > 0 && state.Snake.CurrentDirection != Direction2D.Right)
                return Direction2D.Right;
            return state.Snake.CurrentDirection;

            
        }
    }
}
