using MicroNEAT.ActivationFunctions;
using MicroNEAT.Algorithm;
using MicroNEAT.Config;
using MicroNEAT.Core.Genome;
using MicroNEAT.FitnessFunctions;
using MicroNEAT.WeightInitialization;
using Snake.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Snake.AI.SnakeNEAT
{
    public class SnakeTrainer : IGameTrainer
    {
        public SnakeTrainer()
        {           
  
        }

        public void Train()
        {
            Genome bestGen = null;
            var bestFitess = double.MinValue;

            var config = GetConfig();
            var algo = new NEATAlgorithm(config);
            
            algo.OnGenerationComplete += (gen, fitness) =>
            {
                Console.WriteLine($"Generation {gen} - Best Fitness: {fitness}");
                if(fitness > bestFitess)
                {
                    bestFitess = fitness;
                    bestGen = algo.GetBestGenomeFromPopulation();
                    Console.WriteLine($"New best genome found with fitness {bestFitess}");
                }
            };
            algo.Run();
            //var best = algo.GetBestGenome();
            GenomeBuilder.SaveGenome(bestGen, "best_neat_snake.json");
        }

        public void SaveTrainingData(string path)
        {

        }

        public static NEATConfig GetConfig()
        {
            IFitnessFunction fitnessFunction = new SnakeEvaluator(new SnakeGame(SnakeWorld.SnakeWorldSize.Small));
            var config = new NEATConfig()
            {
                // Network topology for Snake game
                InputSize = StateEncoder.InputSize,
                OutputSize = StateEncoder.OutputSize,

                ActivationFunction = new Tanh(),

                Bias = 1.0,
                ConnectBias = true,

                PopulationSize = 200,
                Generations = 200,
                TargetFitness = 5000.0,
                SurvivalRate = 0.2,

                MutationRate = 0.8,
                AddConnectionMutationRate = 0.05,
                AddNodeMutationRate = 0.03,

                // Speciation parameters
                C1 = 1.0,
                C2 = 1.0,
                C3 = 0.4,
                CompatibilityThreshold = 3.0,

                AllowRecurrentConnections = true,
            };
            config.FitnessFunction = fitnessFunction;
            return config;
        }

        public static NEATConfig GetConfigNew()
        {
            IFitnessFunction fitnessFunction = new SnakeEvaluator(new SnakeGame(SnakeWorld.SnakeWorldSize.Small));
            var config = new NEATConfig()
            {
                // Network topology for Snake game
                InputSize = StateEncoder.InputSize,
                OutputSize = StateEncoder.OutputSize,
                ActivationFunction = new Tanh(),
                Bias = 1.0,
                ConnectBias = true,
                BiasMode = "WEIGHTED_NODE",

                // Recurrent connections can help the snake remember recent patterns
                AllowRecurrentConnections = true,
                RecurrentConnectionRate = 0.3,

                // Weight ranges
                MinWeight = -4.0,
                MaxWeight = 4.0,
                WeightInitialization = new RandomWeightInitialization(-1, 1),

                // Population parameters - larger for complex task
                PopulationSize = 200,
                Generations = 200,
                TargetFitness = 5000.0,  // High fitness target for Snake

                // Selection parameters
                SurvivalRate = 0.2,
                NumOfElite = 15,
                PopulationStagnationLimit = 30,
                InterspeciesMatingRate = 0.01,
                MutateOnlyProb = 0.25,

                // Speciation parameters
                C1 = 1.0,  // Excess genes coefficient
                C2 = 1.0,  // Disjoint genes coefficient
                C3 = 0.4,  // Weight difference coefficient
                CompatibilityThreshold = 3.0,
                DropOffAge = 20,

                // Mutation parameters - balanced for exploration and exploitation
                MutationRate = 1.0,
                WeightMutationRate = 0.8,
                AddConnectionMutationRate = 0.1,  // Higher for Snake complexity
                AddNodeMutationRate = 0.05,  // Higher for Snake complexity
                ReinitializeWeightRate = 0.1,
                MinPerturb = -0.5,
                MaxPerturb = 0.5,
                KeepDisabledOnCrossOverRate = 0.75,
            };
            config.FitnessFunction = fitnessFunction;
            return config;
        }
    }
}
