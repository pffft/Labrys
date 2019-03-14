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
				EditorGrid.GetInstance().Feature.AddSection(position);
			}
		}

		public override void Undo()
		{
			foreach (Vector2Int position in manipPositions)
			{
				EditorGrid.GetInstance().Feature.RemoveSection(position);
			}
		}
	}
}
