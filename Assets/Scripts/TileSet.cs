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
    public List<Tile> AllTiles;

    public TileSet()
    {
        AllTiles = new List<Tile>();
    }

    public void Add(Tile value) 
    {
        AllTiles.Add(value);
    }

    public List<Tile> Get(VariantKey key)
    {
        List<Tile> possibleTiles = new List<Tile>();
        foreach (Tile tile in AllTiles)
        {
            VariantKey tileKey = new VariantKey(tile.type, tile.variant);
            if (key.Matches(tileKey))
            {
                possibleTiles.Add(tile);
            }
        }
        return possibleTiles;
    }

    public struct VariantKey
    {
        public readonly TileType tileType;
        public readonly string variant;

        public VariantKey(TileType tileType, string variant) 
        {
            this.tileType = tileType;
            this.variant = variant;
        }

        public bool Matches(VariantKey other)
        {
            if (tileType.Equals(other.tileType))
            {
                return true;
            }

            if (variant == null && other.variant == null)
            {
                return true;
            }

            if (variant.Equals(other.variant))
            {
                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
