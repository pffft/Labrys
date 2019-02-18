using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Commands
{
	public class SelectionCommand : Command
	{
		public Rect SelectionBox { get; set; }

		public override void Do()
		{
			foreach(Vector2Int gp in EditorGrid.GetInstance().RectToGridPositions(SelectionBox, true))
			{
				EditorGrid.GetInstance().SelectTile(gp);
			}
		}

		public override void Undo()
		{
			foreach (Vector2Int gp in EditorGrid.GetInstance().RectToGridPositions(SelectionBox, true))
			{
				EditorGrid.GetInstance().DeselectTile(gp);
			}
		}
	}
}

