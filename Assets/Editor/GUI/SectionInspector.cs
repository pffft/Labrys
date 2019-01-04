using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(Section))]
public class SectionInspector : Editor
{
    public override void OnInspectorGUI()
    {
        //Section room = (Section)target;

        //EditorGUILayout.LabelField("normal");
        //EditorGUILayout.LabelField(System.Convert.ToString(room.getAdjMask(), 2).PadLeft(16, '0'));
            
        // TODO think about the design of this inspector a bit more.
        // At a basic level, expose the static TileType values in a list (like how enums look)
        // On a more complex level, have a more visual way to choose the TileType.
    }
}