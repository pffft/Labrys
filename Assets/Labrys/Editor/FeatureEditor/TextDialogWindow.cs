using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class TextDialogWindow : EditorWindow
	{
		public delegate void DialogAction(string value);

		public static TextDialogWindow Create(DialogAction onClose)
		{
			TextDialogWindow tdw = GetWindow<TextDialogWindow>();
			tdw.onClose = onClose;
			return tdw;
		}

		public string Label { get; set; }
		public string UserText { get; private set; }
		private DialogAction onClose;

		public void OnGUI()
		{
			UserText = EditorGUILayout.TextField(Label, UserText);

			if (GUILayout.Button("Accept"))
			{
				onClose?.Invoke(UserText);
				Close();
			}
		}
	}
}
