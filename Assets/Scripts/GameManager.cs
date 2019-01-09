using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System.IO;

public class GameManager : MonoBehaviour
{
    public TileSet TileSet = new TileSet();

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) {
            Instance = this;
        } else {
            Debug.LogError("Duplicate GameManager exists in scene!");
            UnityEditor.EditorGUIUtility.PingObject(gameObject);
        }
        GenerateTileSet();
    }

    private void GenerateTileSet()
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
