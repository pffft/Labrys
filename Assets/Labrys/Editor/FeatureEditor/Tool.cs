using UnityEditor;
using UnityEngine;

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
	}
}
