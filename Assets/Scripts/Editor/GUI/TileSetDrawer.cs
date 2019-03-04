using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using Labrys.Tiles;

namespace Labrys
{
    //[CustomPropertyDrawer(typeof(TileSet))]
    public class TileSetDrawer : PropertyDrawer
    {

        bool showPosition = true;

        // TODO add a custom drawer for TileSet. First need to make a TileSet asset.
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Debug.Log("Hello TileSet!");
            EditorGUI.BeginProperty(position, label, property);
            //base.OnGUI(position, property, label);

            showPosition = EditorGUILayout.Foldout(showPosition, "Test");
            if (showPosition)
            {
                EditorGUILayout.LabelField("Test");
            }
            EditorGUI.EndProperty();
        }
    }
}
