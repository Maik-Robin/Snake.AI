using Game.Core;
using MicroNEAT.Core.Genome;
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
        private readonly ISnakeGameEnvironment _envTemplate;

        public SnakeEvaluator(ISnakeGameEnvironment env)
        {
            _envTemplate = env;
        }

        private double[] BuildInputs(SnakeGameState s)
        {
            return StateEncoder.Encode(s);
        }

        public double CalculateFitness(Genome genome)
        {
            var env = _envTemplate.Clone();
            return RunSingleEpisode(genome, env).fitness;
        }
        private (double fitness, bool maxScore) RunSingleEpisode(Genome genome, ISnakeGameEnvironment env)
        {
            var state = env.Reset();
            double fitness = 0;
            int totalSteps = 0;
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
                var nextState = env.Step(dir);
                // Big food reward
                if (state.EatsFood)
                {
                    fitness += 10;
                    sinceFood = 0;
                }
                else
                {
                    sinceFood++;
                    if(sinceFood > maxSinceFood)
                    {
                        // Starvation penalty
                        //fitness -= 50;
                        break;
                    }
                }
                // Death penalty
                //if (state.IsGameOver)
                //    fitness -= 100;
                // Update
                state = nextState;
                totalSteps++;
            }

            if (state.IsGameVictory)
                fitness += 1000;
            fitness += totalSteps * 0.01; // Small reward for lasting longer
            return (fitness, state.IsGameVictory);
        }

        private static int ArgMax(double[] arr)
        {
            int idx = 0;
            double best = arr[0];
            for (int i = 1; i < arr.Length; i++)
                if (arr[i] > best) { best = arr[i]; idx = i; }
            return idx;
        }

        public (double fitness, bool maxScore) CalculateFitnessAndScore(Genome genome)
        {
            var env = _envTemplate.Clone();
            return RunSingleEpisode(genome, env);
        }
    }
}