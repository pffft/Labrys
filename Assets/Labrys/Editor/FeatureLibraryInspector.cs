using UnityEngine;
using UnityEditor;

namespace Labrys.Editor
{
    [CustomEditor(typeof(FeatureLibrary))]
    public class FeatureLibraryInspector : UnityEditor.Editor
    {
        private FeatureLibrary library;
        private Vector2 scrollPosition;

        private void OnEnable()
        {
            library = (FeatureLibrary)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            library.OnAfterDeserialize();

            if (GUILayout.Button("Refresh"))
            {
                library.Refresh();
            }

            GUILayout.Space(25);
            GUILayout.Label("Contains:", EditorStyles.boldLabel);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach(FeatureLibrary.Entry entry in library)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.SelectableLabel($"{entry.FaID}: {entry.Name}", GUILayout.Height(15));
                if (GUILayout.Button($"-->"))
                {
                    ProjectWindowUtil.ShowCreatedAsset(entry.Feature);
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }
    }
}