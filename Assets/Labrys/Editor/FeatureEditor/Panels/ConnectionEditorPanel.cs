using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Panels
{
	public class ConnectionEditorPanel : InternalPanel
	{
		private static string[] modes = new string[] { "Open/Close", "Mark External" };

		public int CurrentUseMode { get; private set; }

		public ConnectionEditorPanel([NotNull] EditorWindow window, DockPosition postion, float width) : base(window, postion, width)
		{

		}

		public override void Draw()
		{
			base.Draw();
			Rect r = new Rect(bounds);
			r.height /= 2;
			CurrentUseMode = GUI.Toolbar(r, CurrentUseMode, modes);
		}

		public override bool HandleEvent(Event e)
		{
			return false;
		}
	}
}
