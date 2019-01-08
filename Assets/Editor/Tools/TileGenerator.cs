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

        if (GUILayout.Button("Regenerate TileSet"))
        {
            char sep = Path.DirectorySeparatorChar;
            DirectoryInfo modelFolder = new DirectoryInfo(Application.dataPath + sep + "Models");

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

                    // Otherwise extract the raw name
                    string tileModelName = tileFiles[j].Name;
                    tileModelName = tileModelName.Substring(0, tileModelName.IndexOf(".", System.StringComparison.Ordinal));

                    string tileModelNameLower = tileModelName.ToLowerInvariant();

                    // Figure out which TileType it maps to
                    TileType type = NameToTileType(tileModelNameLower);

                    // Create the actual Tile object
                    Tile newTile = ScriptableObject.CreateInstance(typeof(Tile)) as Tile;

                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Models" + sep + tileModelName);
                    newTile.prefab = prefab;
                    newTile.prefabString = "Models" + sep + tileModelName;
                    newTile.type = type;
                    newTile.variant = variantName;

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


            //Debug.Log("Hello!");
            //GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Default/DeadEnd.obj");



            //GameObject.Instantiate(obj);
            //Debug.Log("Is object null?: " + (obj == null));
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
