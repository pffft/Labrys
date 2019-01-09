using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(GameManager))]
public class TileGenerator : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Compile a pile of tile files.
        if (GUILayout.Button("Generate Tile Assets"))
        {
            char sep = Path.DirectorySeparatorChar;
            DirectoryInfo modelFolder = new DirectoryInfo(Application.dataPath + sep + "Models");

            // The directories here represent different variants.
            DirectoryInfo[] variantFolders = modelFolder.GetDirectories();
            for (int i = 0; i < variantFolders.Length; i++)
            {
                string variantName = variantFolders[i].Name;

                // Go through each variant folder and extract all the models we recognize the name of.
                FileInfo[] tileFiles = variantFolders[i].GetFiles();
                for (int j = 0; j < tileFiles.Length; j++)
                {
                    // Ignore .meta files
                    if (tileFiles[j].Extension.Equals(".meta"))
                    {
                        continue;
                    }

                    // Otherwise extract the raw name
                    string tileModelName = tileFiles[j].Name;
                    tileModelName = tileModelName.Substring(0, tileModelName.IndexOf(".", System.StringComparison.Ordinal));

                    string tileModelNameLower = tileModelName.ToLowerInvariant();

                    // Figure out which TileType it maps to
                    TileType type = NameToTileType(tileModelNameLower);

                    // Create the actual Tile object
                    Tile newTile = ScriptableObject.CreateInstance(typeof(Tile)) as Tile;

                    string prefabLocation = "Assets/Models" + sep + variantName + sep + tileModelName + tileFiles[j].Extension;
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabLocation);
                    if (prefab == null) 
                    {
                        //Debug.LogWarning("Failed to load in prefab at location: \"" + prefabLocation + "\".");
                        Debug.LogWarning($"Failed to load in prefab at location: \"{prefabLocation}\".");
                        continue;
                    }
                    newTile.prefab = prefab;
                    newTile.type = type;
                    newTile.variant = variantName.ToLowerInvariant();

                    // Serialize the asset into the Tile folder
                    //if (!AssetDatabase.Contains(newTile))
                    //{
                    AssetDatabase.CreateAsset(newTile, "Assets/Tiles" + sep + variantName + sep + tileModelName + ".asset");
                        //AssetDatabase.AddObjectToAsset(newTile, "Tiles" + sep + tileModelName + ".asset");
                    //}

                    // Add it to the database.
                    //TileSet.Add(new TileSet.VariantKey(type, variantName), newTile);
                }
            }
        }

        if (GUILayout.Button("Generate TileSet")) 
        {
            GenerateTileSet();
        }
    }

    public void GenerateTileSet() 
    {
        // Try to load the Tile assets from the Tile directory
        char sep = Path.DirectorySeparatorChar;
        DirectoryInfo modelFolder = new DirectoryInfo(Application.dataPath + sep + "Tiles");

        // The directories here represent different variants.
        DirectoryInfo[] variantFolders = modelFolder.GetDirectories();

        for (int i = 0; i < variantFolders.Length; i++)
        {
            string variantName = variantFolders[i].Name.ToLowerInvariant();

            // Go through each variant folder and extract all the models we recognize the name of.
            FileInfo[] tileFiles = variantFolders[i].GetFiles();
            for (int j = 0; j < tileFiles.Length; j++)
            {
                // Ignore .meta files
                if (tileFiles[j].Extension.Equals(".meta"))
                {
                    continue;
                }

                string tileFilePath = "Assets" + sep + "Tiles" + sep + variantName + sep + tileFiles[j].Name;
                Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tileFilePath);
                if (tile == null)
                {
                    Debug.LogWarning($"Failed to load in file \"{tileFilePath}\".");
                    continue;
                }

                // Add the Tile file to the Tile pile.
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.TileSet.Add(tile);
                }
                else
                {
                    Debug.LogWarning("GameManager instance is null- can't yet add assets outside of play time");
                }
            }
        }
    }

    private TileType NameToTileType(string filename) 
    {
        filename = filename.ToLowerInvariant();

        foreach (TileType type in TileType.TypeList) 
        {
            string comparisonName = type.Name.Replace("_", "").ToLowerInvariant();
            if (filename.Equals(comparisonName)) 
            {
                return type;
            }
        }

        Debug.LogWarning("Failed to resolve file \"" + filename + "\" to TileType."); 
        return TileType.EMPTY;
    }
}
