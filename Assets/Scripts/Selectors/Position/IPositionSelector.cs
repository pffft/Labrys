using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Selectors
{
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
