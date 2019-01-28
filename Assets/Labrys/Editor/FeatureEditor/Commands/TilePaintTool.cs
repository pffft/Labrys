using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Commands
{
	public class TilePaintTool : Tool
	{
		private List<Tile> placedTiles;

		public override bool HandleEvent(Event e)
		{
			switch(e.type)
			{
			case EventType.MouseDown:
				if(e.button == 0)
				{
					//attempt to place a tile in brush position
				}
				break;
			case EventType.MouseDrag:
				if(e.button == 0)
				{
					//continue attempting to place
				}
				break;
			case EventType.MouseUp:
				if(e.button == 0)
				{
					//round up newly placed tiles for command to store in case of undo
				}
				break;
			}
			return false;
		}

		public override Command Use()
		{
			throw new System.NotImplementedException();
		}
	}
}
