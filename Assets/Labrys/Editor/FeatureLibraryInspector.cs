using UnityEngine;
using UnityEditor;

namespace Labrys.Editor
{
    [CustomEditor(typeof(FeatureLibrary))]
    public class FeatureLibraryInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            FeatureLibrary library = (FeatureLibrary)serializedObject.targetObject;
            library.OnAfterDeserialize();

            EditorGUILayout.SelectableLabel(library.TargetDirectory);

            if (GUILayout.Button("Refresh"))
            {
                Undo.RegisterCompleteObjectUndo(library, "Feature Library Refresh");
                library.Refresh();
                EditorUtility.SetDirty(library);
                Debug.Log($"[Labrys:FL] Refreshed {library.name}: \n{library.ToString()}");
            }

            GUILayout.Space(25);
            GUILayout.Label("Contains:", EditorStyles.boldLabel);
            foreach(FeatureLibrary.Entry entry in library)
            {
                EditorGUILayout.BeginHorizontal();
                Debug.Log($"{entry.FaID}: {entry.Name}");
                EditorGUILayout.SelectableLabel($"{entry.FaID}: {entry.Name}", GUILayout.Height(20));
                if (GUILayout.Button("-->"))
                {
                    ProjectWindowUtil.ShowCreatedAsset(entry.Feature);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}