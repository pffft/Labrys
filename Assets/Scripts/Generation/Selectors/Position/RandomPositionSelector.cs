using UnityEngine;

namespace Labrys.Generation.Selectors 
{
    public class RandomPositionSelector : IPositionSelector
    {
        public void Initialize(Grid grid)
        {
        }

        public Vector2Int Select(Grid currentGrid)
        {
            //return currentGrid.GetFullCells()[Random.Range(0, currentGrid.NumFullCells)];
            Vector2Int[] positions = currentGrid.GetBoundary();
            return positions[Random.Range(0, positions.Length)];
        }
    }
}