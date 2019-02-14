using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Commands
{
	public class AddTileCommand : Command
	{
		public readonly Vector2Int[] manipPositions;

		public AddTileCommand(Vector2Int[] positions)
		{
			manipPositions = positions;
		}

		public override void Do()
		{
			foreach(Vector2Int position in manipPositions)
			{
				Tile t = EditorGrid.GetInstance().CreateTile(position);
			}
		}

		public override void Undo()
		{
			foreach (Vector2Int position in manipPositions)
			{
				EditorGrid.GetInstance().RemoveTile(position);
			}
		}
	}
}
