using System;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.FeatureEditor
{
    [Serializable]
    public class Link
    {
        public static void GetMinMaxSubjectTileCount(Vector2Int gridPos, out int min, out int max)
        {
            if (gridPos.x % 2 != 0)
            {
                if (gridPos.y % 2 != 0)
                {
                    min = max = 4;
                }
                else
                {
                    min = 1;
                    max = 2;
                }
            }
            else if (gridPos.y % 2 != 0)
            {
                min = 1;
                max = 2;
            }
            else
            {
                min = max = -1;
            }
        }

        public static IEnumerable<Vector2Int> GetSubjectTileGridPositions(Vector2Int gridPos)
        {
            HashSet<Vector2Int> uniquePositions = new HashSet<Vector2Int>();

            int lowerX = Mathf.FloorToInt(gridPos.x / (float)FeatureAsset.GRID_DENSITY) * FeatureAsset.GRID_DENSITY;
            int upperX = Mathf.FloorToInt((gridPos.x / (float)FeatureAsset.GRID_DENSITY) + 0.5f) * FeatureAsset.GRID_DENSITY;
            int lowerY = Mathf.FloorToInt(gridPos.y / (float)FeatureAsset.GRID_DENSITY) * FeatureAsset.GRID_DENSITY;
            int upperY = Mathf.FloorToInt((gridPos.y / (float)FeatureAsset.GRID_DENSITY) + 0.5f) * FeatureAsset.GRID_DENSITY;
            uniquePositions.Add(new Vector2Int(lowerX, lowerY));
            uniquePositions.Add(new Vector2Int(lowerX, upperY));
            uniquePositions.Add(new Vector2Int(upperX, lowerY));
            uniquePositions.Add(new Vector2Int(upperX, upperY));

            Vector2Int[] finalPositions = new Vector2Int[uniquePositions.Count];
            uniquePositions.CopyTo(finalPositions);
            return finalPositions;
        }

        public bool open;
        public bool external;
    }
}
