using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Panels
{
	public class ConnectionEditorPanel : InternalPanel
	{
		public ConnectionEditorPanel([NotNull] EditorWindow window, DockPosition postion, float width) : base(window, postion, width)
		{

		}

		public override void Draw()
		{
			base.Draw();
		}

		public override bool HandleEvent(Event e)
		{
			return false;
		}
	}
}
