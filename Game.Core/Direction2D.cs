using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core
{

    /// <summary>
    /// Specifies the possible directions for movement or orientation.
    /// </summary>
    /// <remarks>Use this enumeration to indicate a direction in scenarios such as navigation, user interface
    /// controls, or spatial calculations. The values represent the four cardinal directions: up, down, left, and
    /// right.</remarks>
    public enum Direction2D
    {
        Up,
        Down,
        Left,
        Right
    }

    /// <summary>
    /// Specifies the relative direction for movement or turning actions.
    /// </summary>
    /// <remarks>Use this enumeration to indicate whether an object should turn left, move straight forward,
    /// or turn right relative to its current orientation. This is commonly used in navigation, robotics, or
    /// command-processing scenarios where relative movement is required.</remarks>
    public enum RelativeDirection2D
    {
        /// <summary>
        /// Turns the object to the left.
        /// </summary>
        TurnLeft,
        /// <summary>
        /// Represents the action or command to move straight forward.
        /// </summary>
        StraightForward,
        /// <summary>
        /// Turns the object to the right.
        /// </summary>
        TurnRight,
    }

}
