using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Labrys.Editor.FeatureEditor
{
	public abstract class Tool
	{
		public string Name { get; protected set; }

		protected EditorWindow window;

		public Tool(EditorWindow window)
		{
			this.window = window;
		}

		public virtual void Draw() { }
		public abstract bool HandleEvent(Event e);

		protected void ChangeAsset(Object asset, string actionDesc, UnityAction action)
		{
			Undo.RegisterCompleteObjectUndo(asset, actionDesc);
			action.Invoke();
			EditorUtility.SetDirty(asset);
		}
	}
}
