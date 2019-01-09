using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A database for looking up specific individual tiles.
/// </summary>
public class TileSet
{
    // Roughly a 2D array; TileType is the x axis, variant is the y axis.
    // TODO: figure out how to index this. Also, Tiles will know their TileType and variant information.
    private List<Tile> AllTiles = new List<Tile>();

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
        //Debug.Log($"TileSet was requested TileType {key.tileType.Name} and variant \"{key.variant}\".");
        List<Tile> toReturn = new List<Tile>();
        for (int i = 0; i < AllTiles.Count; i++) 
        {
            VariantKey tileKey = new VariantKey(AllTiles[i].type, AllTiles[i].variant);
            //Debug.Log($"Checking tilekey with TileType {tileKey.tileType.Name} and variant \"{tileKey.variant}\".");
            if (key.Matches(tileKey))
            {
                toReturn.Add(AllTiles[i]);
            }
        }
        return toReturn;
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
            if (this.tileType.Name.Equals(other.tileType.Name))
            {
                if (this.variant.Equals(other.variant))
                {
                    return true;
                }
                //Debug.Log("No match; variants differ");
            }
            //Debug.Log("No match; tile types differ");

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
