using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A database for looking up specific individual tiles.
/// </summary>
[CreateAssetMenu(fileName = "NewTileSet", menuName = "Labrys/TileSet")]
[System.Serializable]
public class TileSet : ScriptableObject
{
    // Roughly a 2D array; TileType is the x axis, variant is the y axis.
    // TODO: figure out how to index this. Also, Tiles will know their TileType and variant information.
    [SerializeField]
    private List<Tile> AllTiles = new List<Tile>();

    public TileSet()
    {
        AllTiles = new List<Tile>();
    }

    /// <summary>
    /// Loads the default tile set from disk. The location is hard-coded.
    /// </summary>
    /// <returns>The default tile set.</returns>
    public static TileSet LoadDefaultTileSet()
    {
        TileSet set = UnityEditor.AssetDatabase.LoadAssetAtPath<TileSet>("Assets/TileSets/DefaultTileSet.asset");
        if (set == null)
        {
            Debug.LogWarning("Failed to load default TileSet.");
        }
        return set;
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

    public string[] GetVariants()
    {
        HashSet<string> variants = new HashSet<string>();

        for (int i = 0; i < AllTiles.Count; i++) 
        {
            variants.Add(AllTiles[i].variant);
        }

        string[] toReturn = new string[variants.Count];
        variants.CopyTo(toReturn);

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
            return MatchTileType(other) || MatchVariant(other);
        }

        private bool MatchTileType(VariantKey other)
        {
            // The "ANY" TileType acts as a wildcard.
            if (this.tileType.Equals(TileType.ANY) || other.tileType.Equals(TileType.ANY))
            {
                return true;
            }

            if (this.tileType.Name.Equals(other.tileType.Name))
            {
                return true;
            }

            return false;
        }

        private bool MatchVariant(VariantKey other)
        {
            // null is a wildcard, so is always true
            if (this.variant == null || other.variant == null)
            {
                return true;
            }

            if (this.variant.Equals(other.variant))
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
