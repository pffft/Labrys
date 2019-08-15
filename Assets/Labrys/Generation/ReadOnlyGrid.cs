using System.Collections;
using System.Collections.Generic;
using Labrys.Generation;
using UnityEngine;

namespace Labrys.Generation
{
    /// <summary>
    /// A decorator around a Grid object that enforces readonly access.
    /// </summary>
    public class ReadOnlyGrid
    {
        private readonly Grid backingGrid;

        public ReadOnlyGrid(Grid grid)
        {
            this.backingGrid = grid;
        }

        public Section? this[Vector2Int pos] => backingGrid[pos];

        public Section? this[int x, int y] => backingGrid[x, y];

        public Vector2Int[] GetBoundary() => backingGrid.GetBoundary();

        public Vector2Int[] GetOccupiedCells() => backingGrid.GetOccupiedCells();

        public Connection GetPhysicalAdjacencies(Vector2Int position) => backingGrid.GetPhysicalAdjacencies(position);
    }
}
