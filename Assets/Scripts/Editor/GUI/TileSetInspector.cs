using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TileSet))]
public class TileSetInspector : Editor
{

    SerializedProperty tileSetList;

    bool showPosition = true;
    List<bool> variantVisibility;
    private int variantCount = -1;

    private void OnEnable()
    {
        tileSetList = serializedObject.FindProperty("AllTiles");
    }

    public override void OnInspectorGUI()
    {
        TileSet tileSet = (TileSet)target; 
        serializedObject.Update();

        // Needed to plan out the whole UI
        string[] allVariants = tileSet.GetVariants();

        // Ensure that the list of booleans matches the number of variants appropriately.
        if (variantCount != allVariants.Length) 
        {
            // Create new if we haven't already. 
            if (variantCount == -1 || variantVisibility == null) {
                variantVisibility = new List<bool>();
            }

            // Too small, so add some new entries (closed by default) 
            if (variantCount < allVariants.Length)
            {
                for (int i = variantCount; i < allVariants.Length; i++)
                {
                    variantVisibility.Add(false);
                }
            } 
            // Too big, so cut off the last elements without modifying previous
            else
            {
                for (int i = variantCount; i > allVariants.Length; i--) 
                {
                    variantVisibility.RemoveAt(i);
                }
            }

            // Update the size so we can resize next time
            variantCount = allVariants.Length;
        }

        showPosition = EditorGUILayout.Foldout(showPosition, "Tiles in dictionary");
        if (showPosition)
        {
            EditorGUI.indentLevel++;

            // For each variant, create an entry
            for (int i = 0; i < allVariants.Length; i++)
            {

                // Grab all Tile objects
                List<Tile> tiles = tileSet.Get(new TileSet.VariantKey(TileType.ANY, allVariants[i]));
                Debug.Log($"For variant {allVariants[i]}, found {tiles.Count} tiles.");

                variantVisibility[i] = EditorGUILayout.Foldout(variantVisibility[i], $"Variant \"{allVariants[i]}\"");
                if (variantVisibility[i])
                {
                    EditorGUI.indentLevel++;

                    for (int j = 0; j < TileType.TypeList.Length; j++)
                    {
                        Tile foundTile = null;
                        foreach (Tile tile in tiles)
                        {
                            if (tile.type.Name.Equals(TileType.TypeList[j].Name))
                            {
                                foundTile = tile;
                                break;
                            }
                        }

                        if (foundTile != null)
                        {
                            //SerializedObject serObj = new SerializedObject(foundTile);
                            //EditorGUILayout.PropertyField(serObj.FindProperty("prefab"), new GUIContent(tiles[j].name));
                            Tile blankTile = null;
                            EditorGUILayout.ObjectField(blankTile, typeof(Tile), false);
                        }
                        else 
                        {
                            Tile blankTile = null;
                            EditorGUILayout.ObjectField(blankTile, typeof(Tile));
                            //EditorGUILayout.PropertyField(new SerializedProperty());
                        }
                    }

                    EditorGUI.indentLevel--;
                }
            }

            EditorGUILayout.LabelField("Test");
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
    