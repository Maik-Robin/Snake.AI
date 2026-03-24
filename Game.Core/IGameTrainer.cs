using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeEngine.AI
{
    public interface IGameTrainer
    {
        void Train();
        void SaveTrainingData(string path);
    }
}
