using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Commands
{
	public class RemoveTileCommand : AddTileCommand
	{
		public RemoveTileCommand(Vector2Int[] positions) : base(positions) { }

		public override void Do()
		{
			base.Undo();
		}

		public override void Undo()
		{
			base.Do();
		}
	}
}
