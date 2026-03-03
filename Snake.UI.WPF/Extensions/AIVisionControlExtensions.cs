using System;
using Snake.Controls;

namespace Snake.Extensions
{
    /// <summary>
    /// Extension methods for integrating AI visualization with the Snake game.
    /// </summary>
    public static class AIVisionControlExtensions
    {
        /// <summary>
        /// Updates the AI vision control with Snake game state.
        /// Automatically applies proper labels and highlights the selected action.
        /// </summary>
        /// <param name="control">The AIVisionControl to update.</param>
        /// <param name="inputs">Neural network input values.</param>
        /// <param name="outputs">Neural network output values.</param>
        /// <param name="autoSelectMax">If true, automatically highlights the maximum output.</param>
        public static void UpdateSnakeVision(this AIVisionControl control, 
            double[] inputs, 
            double[] outputs, 
            bool autoSelectMax = true)
        {
            if (control == null)
                return;

            //int selectedIndex = autoSelectMax ? outputs.GetMaxIndex() : -1;
            
            //control.UpdateInputs(inputs, AIVisionLabels.GetSnakeInputLabels());
            //control.UpdateOutputs(outputs, selectedIndex, AIVisionLabels.GetSnakeOutputLabels());
        }
    }
}
