namespace Snake.AI.QLearning
{
    /// <summary>
    /// Hyperparameter configuration for the Q-Learning algorithm.
    /// </summary>
    public class QLearningConfig
    {
        /// <summary>
        /// Learning rate (alpha): controls how quickly the agent updates Q-values.
        /// Range [0, 1]. Higher values lead to faster but less stable learning.
        /// </summary>
        public double LearningRate { get; set; } = 0.1;

        /// <summary>
        /// Discount factor (gamma): determines the importance of future rewards.
        /// Range [0, 1]. 0 = myopic, 1 = considers all future rewards.
        /// </summary>
        public double DiscountFactor { get; set; } = 0.95;

        /// <summary>
        /// Initial exploration rate (epsilon) for the epsilon-greedy policy.
        /// </summary>
        public double EpsilonStart { get; set; } = 1.0;

        /// <summary>
        /// Minimum exploration rate after decay.
        /// </summary>
        public double EpsilonMin { get; set; } = 0.01;

        /// <summary>
        /// Multiplicative decay factor applied to epsilon after each episode.
        /// </summary>
        public double EpsilonDecay { get; set; } = 0.995;

        /// <summary>
        /// Total number of training episodes.
        /// </summary>
        public int Episodes { get; set; } = 10_000;

        /// <summary>
        /// Maximum steps per episode to prevent infinite loops.
        /// </summary>
        public int MaxStepsPerEpisode { get; set; } = 2_000;

        /// <summary>
        /// Reward given when the snake eats a food item.
        /// </summary>
        public double RewardFood { get; set; } = 10.0;

        /// <summary>
        /// Penalty applied when the snake dies (collision with wall or self).
        /// </summary>
        public double PenaltyDeath { get; set; } = -10.0;

        /// <summary>
        /// Small penalty applied each step to encourage efficiency.
        /// </summary>
        public double PenaltyStep { get; set; } = -0.01;

        /// <summary>
        /// Reward given when the snake wins the game (fills the board).
        /// </summary>
        public double RewardVictory { get; set; } = 100.0;
    }
}
