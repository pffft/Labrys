using Labrys.FeatureEditor;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Labrys.Editor
{
    [CustomEditor(typeof(FeatureLibrary))]
    public class FeatureLibraryInspector : UnityEditor.Editor
    {
        private FeatureLibrary library;
        private bool autoRefresh;
        private Vector2 scrollPosition;

        private void OnEnable()
        {
            library = (FeatureLibrary)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            library.OnAfterDeserialize();

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Refresh"))
            {
                library.Refresh();
            }

            bool newAutoRefresh = GUILayout.Toggle(autoRefresh, "Auto");
            if (newAutoRefresh != autoRefresh)
            {
                autoRefresh = newAutoRefresh;
                if (autoRefresh)
                {
                    //TODO ???
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(25);
            GUILayout.Label("Contains:", EditorStyles.boldLabel);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach(KeyValuePair<string, FeatureAsset> entry in library)
            {
                if (GUILayout.Button(entry.Key))
                {
                    ProjectWindowUtil.ShowCreatedAsset(entry.Value);
                }
            }
            GUILayout.EndScrollView();
        }

        private class AssetDatabaseListener : UnityEditor.AssetModificationProcessor
        {
            private static void OnWillCreateAsset(string assetName)
            {

            }
        }
    }
}