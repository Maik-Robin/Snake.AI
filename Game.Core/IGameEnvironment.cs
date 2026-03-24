using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Core
{
    /// <summary>
    /// Represents the interface for a game environment, which can be implemented to define specific game mechanics and interactions.
    /// </summary>
    public interface IGameEnvironment
    {
        /// <summary>
        /// Gets or sets the name associated with the object.
        /// </summary>
        public String Name { get; set; }
    }
}
