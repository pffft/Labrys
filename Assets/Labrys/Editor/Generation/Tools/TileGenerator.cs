using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

using Labrys.Tiles;

namespace Labrys.Generation
{
    //[CustomEditor(typeof(GameManager))]
    public class TileGenerator : EditorWindow
    {
        private static char sep = Path.DirectorySeparatorChar;

        private string inputPath = null;
        private string tilePath = null;
        private string tileSetPath = null;
        private string tileSetName = null;

        [MenuItem("Window/Labrys/TileGenerator")]
        private static void Init()
        {
            TileGenerator generator = GetWindow<TileGenerator>();
            generator.Show();
        }

        private void OnFocus()
        {
            Debug.Log("Tile generator focused");
            Debug.Log($"TileSetName: {tileSetName}");
            inputPath = EditorPrefs.GetString("Labrys.InputPath", "Assets/Models");
            tilePath = EditorPrefs.GetString("Labrys.TilePath", "Assets/Tiles");
            tileSetPath = EditorPrefs.GetString("Labrus.TileSetPath", "Assets/TileSets");
            tileSetName = EditorPrefs.GetString("Labrus.TileSetName", "DefaultTileSet");
        }

        private void OnLostFocus()
        {
            Debug.Log("Tile generator lost focus");
            Debug.Log($"TileSetName: {tileSetName}");
            EditorPrefs.SetString("Labrys.InputPath", inputPath);
            EditorPrefs.SetString("Labrys.TilePath", tilePath);
            EditorPrefs.SetString("Labrys.TileSetPath", tileSetPath);
            EditorPrefs.SetString("Labrys.TileSetName", tileSetName);
        }

        private void OnDestroy()
        {
            Debug.Log("Tile generator destroyed");
            Debug.Log($"TileSetName: {tileSetName}");
            EditorPrefs.SetString("Labrys.InputPath", inputPath);
            EditorPrefs.SetString("Labrys.TilePath", tilePath);
            EditorPrefs.SetString("Labrys.TileSetPath", tileSetPath);
            EditorPrefs.SetString("Labrys.TileSetName", tileSetName);
        }

        // The main GUI generator.
        private void OnGUI()
        {

            // Try to load default input path. Should be "Assets/Models".
            // Maybe later we can try other defaults, like "Assets/Prefabs" or "Assets/Sprites".
            if (inputPath == null)
            {
                DirectoryInfo info = new DirectoryInfo(Application.dataPath + sep + "Models");
                if (info.Exists)
                {
                    inputPath = TrimFolderName(info.FullName);
                }
            }

            // Try to load default Tile asset path. Should be "Assets/Tiles".
            if (tilePath == null)
            {
                DirectoryInfo info = new DirectoryInfo(Application.dataPath + sep + "Tiles");
                if (info.Exists)
                {
                    tilePath = TrimFolderName(info.FullName);
                }
            }

            // Try to load default TileSet path. Should be "Assets/TileSets".
            if (tileSetPath == null)
            {
                DirectoryInfo info = new DirectoryInfo(Application.dataPath + sep + "TileSets");
                if (info.Exists)
                {
                    tileSetPath = TrimFolderName(info.FullName);
                }
            }

            #region Input directory
            inputPath = EditorGUILayout.TextField("Input for Tile assets", inputPath ?? "None");
            if (GUILayout.Button("Select input directory"))
            {
                string path = EditorUtility.OpenFolderPanel("Select input directory (models, gameobjects, sprites, etc)", "Assets/", "Test");

                if (IsValidAssetFolder(path))
                {
                    path = path.Substring(path.LastIndexOf("Assets", System.StringComparison.Ordinal));
                }
                else
                {
                    Debug.LogError("Invalid directory selected. Must be within current Unity project.");
                    path = "Assets/Models";
                }

                path = TrimFolderName(path);

                inputPath = path;
            }

            GUILayout.Label("", GUI.skin.horizontalSlider);
            #endregion

            #region Tile directory
            tilePath = EditorGUILayout.TextField("Tile asset directory", tilePath ?? "None");
            if (GUILayout.Button("Select Tile directory"))
            {
                string path = EditorUtility.OpenFolderPanel("Select directory containing Tile assets", "Assets/", "Test");

                if (IsValidAssetFolder(path))
                {
                    path = path.Substring(path.LastIndexOf("Assets", System.StringComparison.Ordinal));
                }
                else
                {
                    Debug.LogError("Invalid directory selected. Must be within current Unity project.");
                    path = "Assets/Tiles";
                }

                path = TrimFolderName(path);

                tilePath = path;
            }

            GUILayout.Label("", GUI.skin.horizontalSlider);
            #endregion

            #region TileSet output directory and name
            tileSetPath = EditorGUILayout.TextField("TileSet output directory", tileSetPath ?? "None");
            if (GUILayout.Button("Select TileSet output directory"))
            {
                string path = EditorUtility.OpenFolderPanel("Select TileSet output directory", "Assets/", "Test");

                if (IsValidAssetFolder(path))
                {
                    path = path.Substring(path.LastIndexOf("Assets", System.StringComparison.Ordinal));
                }
                else
                {
                    Debug.LogError("Invalid directory selected. Must be within current Unity project.");
                    path = "Assets/TileSets";
                }

                path = TrimFolderName(path);

                tileSetPath = path;
            }

            tileSetName = EditorGUILayout.TextField("Generated TileSet name", tileSetName ?? "DefaultTileSet");

            GUILayout.Label("", GUI.skin.horizontalSlider);
            #endregion

            // Compiles tile files. (turns models -> Tile assets)
            if (GUILayout.Button("Generate Tile assets from input"))
            {
                GenerateTileAssets();
            }

            // Compiles tile files into a pile. (turns Tile assets -> TileSet asset)
            // TODO: add customization for the location of the generated output file,
            // and add smarter checks to create folders if they don't already exist.
            if (GUILayout.Button("Generate TileSet from Tile assets"))
            {
                GenerateTileSet();
            }
        }

        // When the user selects an input/output folder, we make sure it's valid.
        // It needs to be within the current Unity project's "Assets" folder.
        private bool IsValidAssetFolder(string folderName)
        {
            return folderName.StartsWith(Application.dataPath, System.StringComparison.Ordinal) && folderName.Contains("Assets");
        }

        // Trims a folder of the form "[/]?[Assets/]?<name>[/]?" into just "<name>".
        private string TrimAssetFolder(string assetFolderName)
        {
            if (assetFolderName.StartsWith("/", System.StringComparison.Ordinal) && assetFolderName.Length > 0)
            {
                assetFolderName = assetFolderName.Substring(1);
            }

            if (assetFolderName.StartsWith("Assets", System.StringComparison.OrdinalIgnoreCase))
            {
                assetFolderName = assetFolderName.Substring("Assets".Length);
            }

            if (assetFolderName.StartsWith("/", System.StringComparison.Ordinal))
            {
                assetFolderName = assetFolderName.Substring(1);
            }
            return assetFolderName;
        }

        private string TrimFolderName(string folderName)
        {
            // If this is within a Unity project (it should be!), show a relative path name
            if (folderName.Contains("Assets/"))
            {
                string assetsString = "Assets/";
                folderName = folderName.Substring(folderName.IndexOf(assetsString, System.StringComparison.Ordinal));
            }
            else if (folderName.Contains("Assets"))
            {
                string assetsString = "Assets";
                folderName = folderName.Substring(folderName.IndexOf(assetsString, System.StringComparison.Ordinal));
            }
            else
            {
                Debug.LogError("Invalid path. Must be inside the current Unity project.");
                folderName = null;
            }
            return folderName;
        }

        /// <summary>
        /// Loads in "Assets/Models/[variantName]/*" into Tiles under "Assets/Tiles/[variantName]/*".
        /// </summary>
        public void GenerateTileAssets()
        {
            // Clean up the input folders so we can reference files within them properly.
            string relativeInputPath = TrimAssetFolder(inputPath);
            string relativeTilePath = TrimAssetFolder(tilePath);

            // Ensure the output Tile folder exists.
            DirectoryInfo baseOutputFolder = new DirectoryInfo(Application.dataPath + sep + relativeTilePath);
            if (!baseOutputFolder.Exists)
            {
                baseOutputFolder.Create();
            }

            //DirectoryInfo modelFolder = new DirectoryInfo(Application.dataPath + sep + "Models");
            DirectoryInfo modelFolder = new DirectoryInfo(Application.dataPath + sep + relativeInputPath);

            // The directories here represent different variants.
            DirectoryInfo[] variantFolders = modelFolder.GetDirectories();
            for (int i = 0; i < variantFolders.Length; i++)
            {
                string variantName = variantFolders[i].Name;

                // Ensure the variant Tile folder location exists
                string outputFolderName = Application.dataPath + sep + relativeTilePath + sep + variantName;
                DirectoryInfo outputFolder = new DirectoryInfo(outputFolderName);
                if (!outputFolder.Exists)
                {
                    outputFolder.Create();
                }

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
                    //Tile newTile = ScriptableObject.CreateInstance(typeof(Tile)) as Tile;

                    //string prefabLocation = $"Assets{sep}Models{sep}{variantName}{sep}{tileModelName}{tileFiles[j].Extension}";
                    string prefabLocation = "Assets" + sep + relativeInputPath + sep + variantName + sep + tileModelName + tileFiles[j].Extension;
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabLocation);
                    if (prefab == null)
                    {
                        //Debug.LogWarning("Failed to load in prefab at location: \"" + prefabLocation + "\".");
                        Debug.LogWarning($"Failed to load in prefab at location: \"{prefabLocation}\".");
                        continue;
                    }

                    // Clone the GameObject so we can modify it without using the AssetPostProcessor
                    GameObject clone = GameObject.Instantiate(prefab);

                    // Get the Tile component of the prefab (or add it if it doesn't exist)
                    Tile tileComponent = clone.GetComponent<Tile>();
                    if (tileComponent == null)
                    {
                        tileComponent = clone.AddComponent<Tile>();
                    }

                    tileComponent.type = type;
                    tileComponent.variant = variantName.ToLowerInvariant();

                    // Serialize the asset into the Tile folder
                    //AssetDatabase.CreateAsset(newTile, "Assets" + sep + relativeTilePath + sep + variantName + sep + tileModelName + ".asset");
                    //AssetDatabase.CreateAsset(tileComponent, "Assets" + sep + relativeTilePath + sep + variantName + sep + tileModelName + ".prefab");
                    string outputLocation = "Assets" + sep + relativeTilePath + sep + variantName + sep + tileModelName + ".prefab";
                    PrefabUtility.SaveAsPrefabAsset(tileComponent.gameObject, outputLocation, out bool success);

                    if (!success)
                    {
                        Debug.LogWarning($"Failed to save prefab at location: \"{outputLocation}\".");
                    }

                    // Destroy the GameObject so it's not in the scene anymore
                    GameObject.DestroyImmediate(clone);
                }
            }
        }

        /// <summary>
        /// Resolves a model's file name into a TileType. I.e., if a file is called "DeadEnd",
        /// ignoring case, it will map to the TileType "DEAD_END".
        /// </summary>
        /// <returns>The tile type.</returns>
        /// <param name="filename">Filename.</param>
        private static TileType NameToTileType(string filename)
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

        /// <summary>
        /// Compiles all Tiles found under "Assets/Tiles/" into a TileSet, accounting
        /// for variant folders. Creates a default TileSet asset for use in GameManager.
        /// </summary>
        public void GenerateTileSet()
        {
            // Clean up the input folders so we can reference files within them properly.
            string relativeTilePath = TrimAssetFolder(tilePath);
            string relativeTileSetPath = TrimAssetFolder(tileSetPath);

            // Ensure TileSet output path exists.
            DirectoryInfo outputFolder = new DirectoryInfo(Application.dataPath + sep + relativeTileSetPath);
            if (!outputFolder.Exists)
            {
                outputFolder.Create();
            }

            // Try to load the Tile assets from the Tile directory
            //DirectoryInfo modelFolder = new DirectoryInfo(Application.dataPath + sep + "Tiles");
            DirectoryInfo tileFolder = new DirectoryInfo(Application.dataPath + sep + relativeTilePath);

            // The directories here represent different variants.
            DirectoryInfo[] variantFolders = tileFolder.GetDirectories();

            TileSet newTileSet = ScriptableObject.CreateInstance<TileSet>();

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

                    //string tileFilePath = "Assets" + sep + "Tiles" + sep + variantName + sep + tileFiles[j].Name;
                    string tileFilePath = "Assets" + sep + relativeTilePath + sep + variantName + sep + tileFiles[j].Name;
                    Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tileFilePath);
                    if (tile == null)
                    {
                        Debug.LogWarning($"Failed to load in file \"{tileFilePath}\".");
                        continue;
                    }

                    // Add the Tile file to the Tile pile.
                    //if (GameManager.Instance != null)
                    //{
                    //    GameManager.Instance.TileSet.Add(tile);
                    //}
                    //else
                    //{
                    //    Debug.LogWarning("GameManager instance is null- can't yet add assets outside of play time");
                    //}
                    newTileSet.Add(tile);
                }
            }

            // Create the TileSet into a new asset.
            //AssetDatabase.CreateAsset(newTileSet, "Assets/TileSets/default.asset");

            //string outputPath = Path.Combine(Application.streamingAssetsPath, relativeTileSetPath, tileSetName + ".asset");
            //BuildPipeline.BuildAssetBundle(newTileSet, null, outputPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

            // TODO use asset bundles?
            AssetDatabase.CreateAsset(newTileSet, "Assets" + sep + relativeTileSetPath + sep + tileSetName + ".asset");
        }


    }
}
