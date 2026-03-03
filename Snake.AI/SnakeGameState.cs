using Game.Core;
using Snake.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Snake.AI
{
    public class SnakeGameState
    {
        public SnakeGameState()
        {
        }

        public SnakeEntity Snake { get; set; }
        public SnakeWorld World { get; set; }
        public Boolean EatsFood { get; set; }
        public int Score { get; set; }
    }

}
