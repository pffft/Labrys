using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// A database for looking up specific individual tiles.
/// </summary>
public class TileSet
{
    // Roughly a 2D array; TileType is the x axis, variant is the y axis.
    // TODO: figure out how to index this. Also, Tiles will know their TileType and variant information.
    public static Dictionary<VariantKey, Tile> tileDictionaryList = new Dictionary<VariantKey, Tile>();

    public static void Add(VariantKey key, Tile value) 
    {
        tileDictionaryList.Add(key, value);
    }

    public struct VariantKey
    {
        private readonly TileType tileType;
        private readonly string variant;

        public VariantKey(TileType tileType, string variant) 
        {
            this.tileType = tileType;
            this.variant = variant;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
