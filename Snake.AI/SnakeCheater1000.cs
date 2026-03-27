using Game.Core;
using Snake.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Snake.AI
{
    public class SnakeCheater1000 : ISnakeGameController
    {
        (Direction2D dirOut, double[] rawOutput) ISnakeGameController.GetNextMove(SnakeGameState state)
        {
            var maxPoint = state.World.PositionMax;
            var snakeHead = state.Snake.Head;
            var xMod = (Convert.ToInt32(snakeHead.X) & 1) == 0;
            var yMod = (Convert.ToInt32(snakeHead.Y) & 1) == 0;
            var outputDefault = new double[] { 0, 0, 0 };

            if (snakeHead.X == maxPoint.X && snakeHead.Y == maxPoint.Y)
                return (Direction2D.Up, outputDefault); //Up on max
            if (snakeHead.X == 0 && snakeHead.Y == 0)
                return (Direction2D.Down, outputDefault); // Down on 0/0
            if (snakeHead.X == 0 && snakeHead.Y == maxPoint.Y)
                return (Direction2D.Right, outputDefault); //Left on Max/0
            if (snakeHead.X == maxPoint.X && snakeHead.Y == 0)
                return (Direction2D.Left, outputDefault); //Right on 0/Max

            //Some directions chance in case of xmod / ymod
            if (snakeHead.Y == maxPoint.Y && snakeHead.X > 0 && !xMod)
                return (Direction2D.Up, outputDefault);
            if (xMod == false && snakeHead.Y == 1 && snakeHead.X < maxPoint.X)
                return (Direction2D.Right, outputDefault);
            if (xMod == true && snakeHead.X > 0 && snakeHead.Y == 1)
                return (Direction2D.Down, outputDefault);

            //Check hit wall on right wall
            if (snakeHead.X == maxPoint.X && snakeHead.Y > 0 && state.Snake.CurrentDirection != Direction2D.Up)
                return (Direction2D.Up, outputDefault);
            //Check hit wall on top wall
            if (snakeHead.Y == 0 && snakeHead.X > 0 && state.Snake.CurrentDirection != Direction2D.Left)
                return (Direction2D.Left, outputDefault);
            //Check hit wall in down wall
            if (snakeHead.Y == maxPoint.Y && snakeHead.X > 0 && state.Snake.CurrentDirection != Direction2D.Right)
                return (Direction2D.Right, outputDefault);
            return (state.Snake.CurrentDirection, outputDefault);
        }
    }
}
