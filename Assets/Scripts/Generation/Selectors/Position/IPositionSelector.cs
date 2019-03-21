using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Generation.Selectors
{
    /// <summary>
    /// Chooses which position we will place a Feature in the next generation step.
    /// </summary>
    public interface IPositionSelector
    {
        /// <summary>
        /// Initialize this PositionSelector.
        /// </summary>
        /// <param name="grid">The initial state of the dungeon.</param>
        void Initialize(Grid grid);

        /// <summary>
        /// Select the next position to place a Feature.
        /// </summary>
        /// <returns>The chosen position.</returns>
        /// <param name="currentGrid">A reference to the current state of the dungeon.</param>
        Vector2Int Select(Grid currentGrid);
    }
}
