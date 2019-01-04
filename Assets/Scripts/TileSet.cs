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
    public static Dictionary<VariantKey, Tile>[] tileDictionaryList = new Dictionary<VariantKey, Tile>[15];

    static TileSet() {
        for (int i = 0; i < tileDictionaryList.Length; i++) 
        {
            tileDictionaryList[i] = new Dictionary<VariantKey, Tile>();
        }
    }

    public struct VariantKey 
    {
        private readonly string variant;

        public VariantKey(string variant) 
        {
            this.variant = variant;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
