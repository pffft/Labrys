using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Tools
{
	public class ConnectionEditorTool : Tool
	{
		private HashSet<Vector2Int> manipPositions;
		private Color previewColor;

		public override void Draw()
		{
			base.Draw();
		}

		public override bool HandleEvent(Event e)
		{
			switch (e.type)
			{
			case EventType.MouseDown:
			case EventType.MouseDrag:
				if (e.button == 0 || e.button == 1)
				{
					
					return true;
				}
				break;
			case EventType.MouseUp:
				if (e.button == 0 || e.button == 1)
				{
					
					return true;
				}
				break;
			}
			return false;
		}
	}
}
