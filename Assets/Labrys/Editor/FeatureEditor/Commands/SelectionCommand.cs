using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Commands
{
	public class SelectionCommand : Command
	{
		public Vector2Int[] SelectedPositions { get; set; }

		public override void Do()
		{
			foreach(Vector2Int gp in SelectedPositions)
			{
				EditorGrid.GetInstance().SelectTile(gp);
			}
		}

		public override void Undo()
		{
			foreach (Vector2Int gp in SelectedPositions)
			{
				EditorGrid.GetInstance().DeselectTile(gp);
			}
		}
	}
}

