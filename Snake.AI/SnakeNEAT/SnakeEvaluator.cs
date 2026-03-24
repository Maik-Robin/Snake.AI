using Game.Core;
using MicroNEAT.FitnessFunctions;
using Snake.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Snake.AI.SnakeNEAT
{
    public class SnakeEvaluator : IFitnessFunction
    {
        private readonly ISnakeGameEnvironment _env;

        public SnakeEvaluator(ISnakeGameEnvironment env)
        {
            _env = env;
        }

        private double[] BuildInputs(SnakeGameState s)
        {
            return StateEncoder.Encode(s);
        }

        public double CalculateFitness(global::MicroNEAT.Core.Genome.Genome genome)
        {
            return RunSingleEpisode(genome);
        }
        private double RunSingleEpisode(global::MicroNEAT.Core.Genome.Genome genome)
        {
            var state = _env.Reset();
            double fitness = 0;
            int steps = 0;
            int sinceFood = 0;
            var maxSinceFood = 400; 
            while (!state.IsGameOver)
            {
                // Build inputs
                double[] inputs = BuildInputs(state);
                // Get action from NEAT
                double[] output = genome.Propagate(inputs);
                int bestIdx = ArgMax(output);
                var relDir = (RelativeDirection2D)bestIdx;
                var dir = Direction2DHelper.RelativeToDirection(state.Snake.CurrentDirection, relDir);
                // Step the game
                var nextState = _env.Step(dir);
                // Big food reward
                if (state.EatsFood)
                {
                    fitness += 50;
                    sinceFood = 0;
                }
                else
                {
                    sinceFood++;
                    if(sinceFood > maxSinceFood)
                    {
                        // Starvation penalty
                        fitness -= 50;
                        break;
                    }
                }
                // Death penalty
                if (state.IsGameOver)
                    fitness -= 100;
                // Update
                state = nextState;
                steps++;
            }

            if (state.IsGameVictory)
                fitness += 1000;

            return fitness;
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