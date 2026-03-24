using Game.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake.AI
{
    public interface ISnakeGameController
    {
        Direction2D GetNextMove(SnakeGameState state);
    }

}
