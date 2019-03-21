using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Labrys.Tiles;

namespace Labrys.Generation
{
    [CustomEditor(typeof(TileSet))]
    public class TileSetInspector : Editor
    {

        SerializedProperty tileSetList;

        bool showPosition = true;
        List<bool> variantVisibility;
        private int variantCount = -1;

        bool addingVariant = false;

        private void OnEnable()
        {
            tileSetList = serializedObject.FindProperty("allTiles");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            TileSet tileSet = (TileSet)target;

            EditorGUILayout.LabelField($"Serialization status: {tileSet.status}");

            // Needed to plan out the whole UI
            string[] allVariants = tileSet.GetVariants();

            // Ensure that the list of booleans matches the number of variants appropriately.
            if (variantCount != allVariants.Length)
            {
                // Create new if we haven't already. 
                if (variantCount == -1 || variantVisibility == null)
                {
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

                Debug.Log($"Found {variantCount} variants.");

                // For each variant, create an entry
                for (int i = 0; i < allVariants.Length; i++)
                {

                    // Grab all Tile objects
                    List<Tile> tiles = null;
                    bool found = tileSet.Get(new TileSet.VariantKey(TileType.ANY, allVariants[i]), ref tiles);
                    if (!found)
                    {
                        Debug.LogError($"Failed to find any tiles for variant {allVariants[i]}");
                    }
                    else
                    {
                        Debug.Log($"For variant {allVariants[i]}, found {tiles.Count} tiles.");
                    }

                    variantVisibility[i] = EditorGUILayout.Foldout(variantVisibility[i], $"Variant \"{allVariants[i]}\"");
                    if (variantVisibility[i])
                    {
                        EditorGUI.indentLevel++;


                        for (int j = 0; j < TileType.TypeList.Length; j++)
                        {
                            Tile foundTile = null;
                            foreach (Tile tile in tiles)
                            {
                                if (tile.type == TileType.TypeList[j])
                                {
                                    foundTile = tile;
                                    break;
                                }
                            }

                            if (foundTile != null)
                            {
                                //SerializedObject serObj = new SerializedObject(foundTile);
                                //EditorGUILayout.PropertyField(serObj.FindProperty("gameObject"), new GUIContent(TileType.TypeList[j].Name));

                                GameObject gameObject = EditorGUILayout.ObjectField(new GUIContent(TileType.TypeList[j].Name), foundTile, typeof(Tile), false) as GameObject;

                            }
                            else
                            {
                                //Debug.Log($"Couldn't find a Tile for variant {allVariants[i]} and type {TileType.TypeList[j].Name}. Filling with blank space.");
                                //Tile blankTile = null;
                                //EditorGUILayout.ObjectField(blankTile, typeof(Tile));
                                //EditorGUILayout.PropertyField(new SerializedProperty());
                                Tile blankTile = null;
                                blankTile = EditorGUILayout.ObjectField(new GUIContent(TileType.TypeList[j].Name), blankTile, typeof(Tile), false) as Tile;

                                if (blankTile != null)
                                {
                                    tileSet.Add(blankTile);
                                }
                            }
                        }

                        EditorGUI.indentLevel--;
                    }
                }

                // New variant field
                if (addingVariant)
                {
                    string newName = EditorGUILayout.TextField("VariantName");
                    if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                    {
                        addingVariant = false;

                        Tile dummyTile = new Tile()
                        {
                            type = TileType.ANY,
                            variant = newName
                        };

                        tileSet.Add(dummyTile);
                    }
                }

                // Button to toggle above field visibility
                if (GUILayout.Button("Add new variant"))
                {
                    addingVariant = true;
                }

                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}