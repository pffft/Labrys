using UnityEngine;

namespace Labrys.Generation.Selectors 
{
    public class RandomPositionSelector : IPositionSelector
    {
        public void Initialize(ReadOnlyGrid grid)
        {
        }

        public Vector2Int Select(ReadOnlyGrid currentGrid)
        {
            Vector2Int[] positions = currentGrid.GetBoundary();
            return positions[Random.Range(0, positions.Length)];
        }
    }
}