using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Commands
{
	public class SelectionCommand : Command
	{
		public List<Tile> SelectedTiles { get; set; }

		public override void Do()
		{
			Debug.Log("TODO - select all tiles in a list");
		}

		public override void Undo()
		{
			Debug.Log("TODO - unselect all tiles in a list");
		}
	}
}

