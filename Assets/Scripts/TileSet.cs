using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Profiling;

namespace Labrys
{
    /// <summary>
    /// A database for looking up specific individual tiles.
    /// 
    /// TODO this seems to have serialization problems. It looks like forcing
    /// serialization + deserialization fixes it somewhat, but it'll be good
    /// to ensure the problem doesn't repeat.
    /// </summary>
    [CreateAssetMenu(fileName = "NewTileSet", menuName = "Labrys/TileSet")]
    [System.Serializable]
    public class TileSet : ScriptableObject, ISerializationCallbackReceiver
    {
        // Literally every tile, without an index. Used for fast returns of "all" queries.
        // This gets serialized, and the index is rebuilt from this on load.
        [SerializeField]
        private List<Tile> allTiles;

        [SerializeField]
        public int status = 0;

        // All tiles, sorted on their variants. Rebuilt on serialization.
        private Dictionary<string, List<Tile>> tilesByVariant;

        // All tiles, sorted on their TileTypes. Rebuilt on serialization.
        private Dictionary<TileType, List<Tile>> tilesByTileType;

        public TileSet()
        {
            //tilesByVariant = new Dictionary<string, List<Tile>>();
            //tilesByTileType = new Dictionary<TileType, List<Tile>>();

            switch (status)
            {
                // Case 0: uninitialized. This is a brand new TileSet.
                case 0:
                    if (allTiles != null)
                    {
                        //Debug.LogError($"Invalid internal state for TileSet. Status 0, but allTiles is not null.");
                    }
                    allTiles = new List<Tile>();
                    status = 2;
                    break;
                // Case 1: initialized, but not deserialized.
                case 1:
                    if (allTiles == null || allTiles.Count <= 0)
                    {
                        //Debug.LogError($"Invalid internal state for TileSet. Status 1, but allTiles is null/empty.");
                    }
                    OnAfterDeserialize();
                    status = 2;
                    break;
                case 2:
                    if (allTiles == null || allTiles.Count <= 0)
                    {
                        //Debug.LogError($"Invalid internal state for TileSet. Status 2, but allTiles is null/empty.");
                    }
                    if (tilesByVariant == null || tilesByTileType == null)
                    {
                        //Debug.LogError($"Invalid internal state for TileSet. Status 2, but indexing dictionaries are null.");
                    }
                    break;
                default:
                    //Debug.LogError($"Invalid TileSet serialization status: {status}");
                    break;
            }

        }

        public void Initialize()
        {
            if (status == 1)
            {
                OnAfterDeserialize();
                status = 2;
            }
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
            else
            {
                //set.OnAfterDeserialize();
                set.Initialize();
            }
            return set;
        }

        // Adds the value to the overall storage.
        public void Add(in Tile value)
        {
            if (value == null)
            {
                //throw new System.Exception("Tried to add null Tile to TileSet- this is invalid");
            }
            allTiles.Add(value);
            AddToIndex(value);
        }

        // Used for serialization- indexes the value for faster lookup.
        private void AddToIndex(in Tile value)
        {
            //Debug.Log($"Adding Tile with type {value.type.Name} and variant {value.variant} to index.");
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

        public bool Get(VariantKey key, ref List<Tile> tiles)
        {
            // Initial checks to the ref parameter. We want an empty list.
            if (tiles == null)
            {
                tiles = new List<Tile>();
            }
            else if (tiles.Count > 0)
            {
                tiles.Clear();
            }

            Profiler.BeginSample("TileSet get");
            // If we don't care about the variant, search by TileType.
            if (key.variant == null) 
            {
                // If we don't care about either, return all Tiles.
                if (key.tileType == TileType.ANY)
                {
                    //List<Tile> allTilesCopy = new List<Tile>();
                    for (int i = 0; i < allTiles.Count; i++)
                    {
                        tiles.Add(allTiles[i]);
                    }
                    Profiler.EndSample();
                    return true;
                    //return allTiles;
                }

                // Else we care about TileType, but not variant. Return all matching.
                for (int i = 0; i < tilesByTileType[key.tileType].Count; i++) 
                {
                    tiles.Add(tilesByTileType[key.tileType][i]);
                }
                Profiler.EndSample();
                return true;
            }

            // We care about variant, but not TileType. Return all matching.
            if (key.tileType == TileType.ANY)
            {
                for (int i = 0; i < tilesByVariant[key.variant].Count; i++) 
                {
                    tiles.Add(tilesByVariant[key.variant][i]);
                }
                Profiler.EndSample();
                return true;
            }

            // Make sure we have the variant first.
            if (tilesByVariant.ContainsKey(key.variant))
            {
                // If we do- in this case, we care about both. Search by variant first, since that leaves <=16 to iterate over.
                // TODO if we store a HashSet, we can reduce linear factor from 16 to 1. But conversions from Set->List
                // are expensive, if we need to return all tiles of a variant.
                List<Tile> tempTiles = tilesByVariant[key.variant];
                for (int i = 0; i < tempTiles.Count; i++)
                {
                    // When we find a matching TileType, return it.
                    if (tempTiles[i].type == key.tileType)
                    {
                        tiles.Add(tempTiles[i]);
                        Profiler.EndSample();
                        return true;
                        //return new List<Tile> { tempTiles[i] };
                    }
                }
            }

            // Still haven't found it. Try to get default, if we haven't already.
            if (!key.variant.Equals("default"))
            {
                Debug.Log($"Failed to find requested key: {key}. Looking up fallback using default variant.");

                List<Tile> tempTiles = tilesByVariant["default"];
                for (int i = 0; i < tempTiles.Count; i++)
                {
                    if (tempTiles[i].type == key.tileType)
                    {
                        tiles.Add(tempTiles[i]);
                        Profiler.EndSample();
                        return true;
                    }
                }
            }

            // Failed to find it.
            Debug.Log($"Failed to find default variant for tiletype: {key.tileType.Name}.");
            Profiler.EndSample();
            //return new List<Tile>();
            return false;
        }

        // Returns an array of all the valid variants in this TileSet.
        public string[] GetVariants()
        {
            switch(status)
            {
                case 0:
                    //Debug.LogError("Tried to GetVariants on status 0 TileSet.");
                    return new string[0];
                case 1:
                    // ok, initialized but not deserialized. Fix it and try again
                    //Debug.LogWarning("GetVariants called with status 1. Deserializing and trying again.");
                    OnAfterDeserialize();
                    return GetVariants();
                case 2:
                    if (tilesByVariant == null || (tilesByVariant.Count == 0 && allTiles.Count > 0))
                    {
                        //Debug.LogError("Invalid state during GetVariants. Status 2, but indexing dictionary is null.");
                        return new string[0];
                    }

                    // Slow search over allTiles
                    //HashSet<string> strings = new HashSet<string>();
                    //for (int index = 0; index < allTiles.Count; index++)
                    //{
                    //    strings.Add(allTiles[index].variant);
                    //}
                    //string[] longToReturn = new string[strings.Count];
                    //strings.CopyTo(longToReturn);

                    //return longToReturn;

                    // Fast search over valid indexing dictionaries
                    string[] toReturn = new string[tilesByVariant.Keys.Count];
                    int i = 0;
                    foreach (string s in tilesByVariant.Keys)
                    {
                        toReturn[i++] = s;
                    }
                    return toReturn;
                default:
                    Debug.LogError($"Invalid status during GetVariants: {status}");
                    return new string[0];
            }
        }

        // Rebuild the index (dictionaries)
        public void OnAfterDeserialize()
        {
            switch(status)
            {
                case 0:
                    //Debug.LogWarning("Tried to deserialize uninitialized object. Ignoring.");
                    return;
                case 1:
                    status = 2;
                    for (int i = 0; i < allTiles.Count; i++)
                    {
                        if (allTiles[i] == null || allTiles[i].variant == null)
                        {
                            //Debug.LogError($"Invalid state during deserialization. Status 1, and found null index in allTiles: {i}.");
                            continue;
                        }
                        //Debug.Log($"Deserializing tile from allTiles: {allTiles[i].type.Name} and variant {allTiles[i].variant}.");
                        AddToIndex(allTiles[i]);
                    }
                    return;
                case 2:
                    //Debug.LogError("Invalid state after serialization. Status 2, expected 0 or 1.");
                    return;
            }
        }

        // Ensure the dictionaries are cleared first- they're not serialized
        public void OnBeforeSerialize()
        {

            for (int i = 0; i < allTiles.Count; i++)
            {
                if (allTiles[i] == null)
                {
                    //Debug.LogError($"allTiles index {i} is null before serializing.");
                }
            }

            // If 0, then unchanged. If 2, change back to 1 until re-serialized.
            switch (status)
            {
                case 0:
                    //Debug.LogWarning("Tried to serialize uninitialized object. Ignoring.");
                    return;
                case 1:
                    //Debug.LogError("Invalid state before serialization. Status 1, expected 0 or 2.");
                    return;
                case 2:
                    status = 1;
                    if (tilesByVariant == null)
                    {
                        //Debug.LogError("Failed to clear variant index dictionary.");
                    }
                    else
                    {
                        tilesByVariant.Clear();
                    }

                    if (tilesByVariant == null)
                    {
                        //Debug.LogError("Failed to clear tiletype index dictionary.");
                    }
                    else
                    {
                        tilesByTileType.Clear();
                    }
                    return;

            }
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
