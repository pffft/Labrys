using JetBrains.Annotations;
using UnityEditor;

namespace Labrys.Editor.FeatureEditor.Panels
{
	public class ToolBox : InternalPanel
	{
		public ToolBox([NotNull] EditorWindow window, DockPosition alignment, float scale) : base(window, alignment, scale)
		{

		}
	}
}
