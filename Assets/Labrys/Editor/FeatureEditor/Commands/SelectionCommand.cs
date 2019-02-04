using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Commands
{
	public class SelectionCommand : Command
	{
		public Rect SelectionBox { get; set; }

		public override void Do()
		{
			TraverseGrid((int x, int y) => EditorGrid.GetInstance().SelectTile(new Vector2Int(x, y)));
		}

		public override void Undo()
		{
			TraverseGrid((int x, int y) => EditorGrid.GetInstance().DeselectTile(new Vector2Int(x, y)));
		}

		private void TraverseGrid(PositionOperation operation)
		{
			EditorGrid grid = EditorGrid.GetInstance();
			Vector2Int startingGridPos = grid.ScreenToGridPos(SelectionBox.min, true);
			Vector2Int endingGridPos = grid.ScreenToGridPos(SelectionBox.max, true);
			Vector2Int selectionAreaDimen = endingGridPos - startingGridPos;
			int xSign = (int)Mathf.Sign(selectionAreaDimen.x);
			int ySign = (int)Mathf.Sign(selectionAreaDimen.y);

			for (int x = startingGridPos.x; x <= endingGridPos.x; x += (int)EditorGrid.GRID_DENSITY * xSign)
			{
				for (int y = startingGridPos.y; y <= endingGridPos.y; y += (int)EditorGrid.GRID_DENSITY * ySign)
				{
					operation(x, y);
				}
			}
		}

		private delegate void PositionOperation(int x, int y);
	}
}

