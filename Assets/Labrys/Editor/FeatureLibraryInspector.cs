using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Labrys.Editor
{
    [CustomEditor(typeof(FeatureLibrary))]
    public class FeatureLibraryInspector : UnityEditor.Editor
    {
        private AnimBool fadeGroupVal = new AnimBool(false);

        public void OnEnable()
        {
            fadeGroupVal.valueChanged.AddListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            FeatureLibrary library = (FeatureLibrary)serializedObject.targetObject;

            EditorGUILayout.SelectableLabel(library.TargetDirectory);

            if (GUILayout.Button("Refresh"))
            {
                Undo.RegisterCompleteObjectUndo(library, "Feature Library Refresh");
                library.Refresh();
                EditorUtility.SetDirty(library);
            }

            GUILayout.Space(25);
            if (GUILayout.Button(fadeGroupVal.value ? "Hide contents" : "Show contents"))
                fadeGroupVal.target = !fadeGroupVal.value;
            if (EditorGUILayout.BeginFadeGroup(fadeGroupVal.faded))
            {
                foreach (FeatureLibrary.Entry entry in library)
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
            EditorGUILayout.EndFadeGroup();
        }
    }
}