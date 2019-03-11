using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public abstract class Tool
	{
		protected EditorWindow window;

		public Tool(EditorWindow window)
		{
			this.window = window;
		}

		public virtual void Draw() { }
		public abstract bool HandleEvent(Event e);
	}
}
