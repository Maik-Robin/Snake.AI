using System;
using System.Windows;

namespace Snake
{
    /// <summary>
    /// Window for visualizing the AI's neural network inputs and outputs in real-time.
    /// </summary>
    public partial class AIVisionWindow : Window
    {
        private readonly Random _random = new Random();

        public AIVisionWindow()
        {
            InitializeComponent();
            
            // Initialize with test data
            ShowTestData();
        }

        /// <summary>
        /// Updates the vision control with actual AI state data.
        /// </summary>
        /// <param name="inputs">Neural network input values.</param>
        /// <param name="outputs">Neural network output values.</param>
        /// <param name="selectedActionIndex">Index of the selected action.</param>
        public void UpdateVision(double[] inputs, double[] outputs, int selectedActionIndex = -1)
        {
            VisionControl.UpdateInputs(inputs, AIVisionLabels.GetSnakeInputLabels());
            VisionControl.UpdateOutputs(outputs, selectedActionIndex, AIVisionLabels.GetSnakeOutputLabels());
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTestData();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            VisionControl.Clear();
        }

        private void ShowTestData()
        {
            // Generate random test data
            double[] testInputs = new double[11];
            for (int i = 0; i < testInputs.Length; i++)
            {
                testInputs[i] = _random.NextDouble();
            }

            double[] testOutputs = new double[3];
            for (int i = 0; i < testOutputs.Length; i++)
            {
                testOutputs[i] = _random.NextDouble();
            }

            // Find max output for selection
            int selectedIndex = 0;
            double maxValue = testOutputs[0];
            for (int i = 1; i < testOutputs.Length; i++)
            {
                if (testOutputs[i] > maxValue)
                {
                    maxValue = testOutputs[i];
                    selectedIndex = i;
                }
            }

            UpdateVision(testInputs, testOutputs, selectedIndex);
        }
    }
}
