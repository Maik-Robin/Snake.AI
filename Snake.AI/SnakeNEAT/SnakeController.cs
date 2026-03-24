using Game.Core;
using MicroNEAT.ActivationFunctions;
using MicroNEAT.Algorithm;
using MicroNEAT.Config;
using MicroNEAT.Core.Genome;
using MicroNEAT.FitnessFunctions;
using MicroNEAT.WeightInitialization;
using Snake.AI;
using System.Text.Json;

namespace Snake.AI.SnakeNEAT
{
    public class SnakeController : ISnakeGameController
    {

        public Genome BestGen { get; set; }
        

        public void LoadBestGenome(string path)
        {
            var savedData = System.IO.File.ReadAllText(path);
            BestGen = GenomeBuilder.LoadGenome(savedData, SnakeTrainer.GetConfig());
        }

        public SnakeController()
        {
            
        }

        public Direction2D GetNextMove(SnakeGameState state)
        {
            var input = StateEncoder.Encode(state);
            var output = BestGen.Propagate(input);
            int bestIdx = ArgMax(output);
            var rel = (RelativeDirection2D)bestIdx;
            var dir = Direction2DHelper.RelativeToDirection(state.Snake.CurrentDirection, rel);
            return dir;
        }

        private static int ArgMax(double[] arr)
        {
            int idx = 0;
            double best = arr[0];
            for (int i = 1; i < arr.Length; i++)
                if (arr[i] > best) { best = arr[i]; idx = i; }
            return idx;
        }
    }
}
