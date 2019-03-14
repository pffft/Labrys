using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Commands
{
	class ConnectionToggleCommand : Command
	{
		public bool TargetState { get; set; }

		public readonly Vector2Int[] manipPositions;

		public ConnectionToggleCommand(Vector2Int[] manipPositions)
		{
			this.manipPositions = manipPositions;
		}

		public override void Do()
		{
			foreach (Vector2Int position in manipPositions)
			{
				EditorGrid.GetInstance().SetConnectionState(position, TargetState);
			}
		}

		public override void Undo()
		{
			foreach (Vector2Int position in manipPositions)
			{
				EditorGrid.GetInstance().SetConnectionState(position, !TargetState);
			}
		}
	}
}
