using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys
{
    /// <summary>
    /// A database for looking up specific individual tiles.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTileSet", menuName = "Labrys/TileSet")]
    [System.Serializable]
    public class TileSet : ScriptableObject, ISerializationCallbackReceiver
    {
        // Literally every tile, without an index. Used for fast returns of "all" queries.
        // This gets serialized, and the index is rebuilt from this on load.
        [SerializeField]
        private List<Tile> allTiles;

        // All tiles, sorted on their variants. Rebuilt on serialization.
        private Dictionary<string, List<Tile>> tilesByVariant;

        // All tiles, sorted on their TileTypes. Rebuilt on serialization.
        private Dictionary<TileType, List<Tile>> tilesByTileType;

        public TileSet()
        {
            allTiles = new List<Tile>();
            tilesByVariant = new Dictionary<string, List<Tile>>();
            tilesByTileType = new Dictionary<TileType, List<Tile>>();
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

        // Adds the value to the overall storage.
        public void Add(Tile value)
        {
            allTiles.Add(value);
            AddToIndex(value);
        }

        // Used for serialization- indexes the value for faster lookup.
        private void AddToIndex(Tile value)
        {
            Debug.Log($"Adding Tile with type {value.type.Name} and variant {value.variant} to index.");
            if (tilesByVariant == null) 
            {
                tilesByVariant = new Dictionary<string, List<Tile>>();
            }

            if (tilesByTileType == null)
            {
                tilesByTileType = new Dictionary<TileType, List<Tile>>();
            }

            if (!tilesByVariant.ContainsKey(value.variant))
            {
                tilesByVariant.Add(value.variant, new List<Tile>());
            }

            if (!tilesByTileType.ContainsKey(value.type))
            {
                tilesByTileType.Add(value.type, new List<Tile>());
            }

            tilesByVariant[value.variant].Add(value);
            tilesByTileType[value.type].Add(value);
        }

        public List<Tile> Get(VariantKey key)
        {
            // If we don't care about the variant, search by TileType.
            if (key.variant == null) 
            {
                // If we don't care about either, return all Tiles.
                if (key.tileType == TileType.ANY)
                {
                    return allTiles;
                }

                // Else we care about TileType, but not variant. Return all matching.
                return tilesByTileType[key.tileType];
            }

            // We care about variant, but not TileType. Return all matching.
            if (key.tileType == TileType.ANY)
            {
                return tilesByVariant[key.variant];
            }

            // Make sure we have the variant first.
            if (tilesByVariant.ContainsKey(key.variant))
            {
                // If we do- in this case, we care about both. Search by variant first, since that leaves <=16 to iterate over.
                // TODO if we store a HashSet, we can reduce linear factor from 16 to 1. But conversions from Set->List
                // are expensive, if we need to return all tiles of a variant.
                List<Tile> tiles = tilesByVariant[key.variant];
                for (int i = 0; i < tiles.Count; i++)
                {
                    // When we find a matching TileType, return it.
                    if (tiles[i].type == key.tileType)
                    {
                        return new List<Tile> { tiles[i] };
                    }
                }
            }

            // Still haven't found it. Try to get default, if we haven't already.
            if (!key.variant.Equals("default"))
            {
                Debug.Log($"Failed to find requested key: {key}. Looking up fallback using default variant.");

                List<Tile> tiles = tilesByVariant["default"];
                for (int i = 0; i < tiles.Count; i++)
                {
                    if (tiles[i].type == key.tileType)
                    {
                        return new List<Tile> { tiles[i] };
                    }
                }
            }

            // Failed to find it.
            Debug.Log($"Failed to find default variant for tiletype: {key.tileType.Name}.");
            return new List<Tile>();
        }

        // Returns an array of all the valid variants in this TileSet.
        public string[] GetVariants()
        {
            if (tilesByVariant == null || (tilesByVariant.Count == 0 && allTiles.Count > 0))
            {
                //Debug.Log("Doing long search for variants.");
                //HashSet<string> strings = new HashSet<string>();
                //for (int index = 0; index < allTiles.Count; index++)
                //{
                //    strings.Add(allTiles[index].variant);
                //}
                //string[] longToReturn = new string[strings.Count];
                //strings.CopyTo(longToReturn);
                //return longToReturn;
                Debug.Log("Reindexing TileSet.");
                OnAfterDeserialize();
            }

            string[] toReturn = new string[tilesByVariant.Keys.Count];
            int i = 0;
            foreach (string s in tilesByVariant.Keys)
            {
                toReturn[i++] = s;
            }

            return toReturn;
        }

        // Rebuild the index (dictionaries)
        public void OnAfterDeserialize()
        {
            for (int i = 0; i < allTiles.Count; i++)
            {
                Debug.Log($"Deserializing tile from allTiles: {allTiles[i].type.Name} and variant {allTiles[i].variant}.");
                AddToIndex(allTiles[i]);
            }
        }

        // Ensure the dictionaries are cleared first- they're not serialized
        public void OnBeforeSerialize()
        {
            tilesByVariant.Clear();
            tilesByTileType.Clear();
        }

        public struct VariantKey
        {
            public TileType tileType;
            public string variant;

            public VariantKey(TileType tileType, string variant)
            {
                this.tileType = tileType;
                this.variant = variant;
            }
        }
    }
}
