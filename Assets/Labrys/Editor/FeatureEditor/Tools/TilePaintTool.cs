using Labrys.Editor.FeatureEditor.Commands;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Tools
{
	public class TilePaintTool : Tool
	{
		private HashSet<Vector2Int> manipPositions;

		public TilePaintTool()
		{
			manipPositions = new HashSet<Vector2Int>();
		}

		public override void Draw()
		{
			//draw tile previews
			base.Draw();
			foreach(Vector2Int position in manipPositions)
			{
				Vector2 screenPos = EditorGrid.GetInstance().GridToScreenPos(position);
				Rect r = new Rect(screenPos, EditorGrid.GetInstance().GetScaledTileSize());
				r.center = screenPos;
				GUI.color = Color.green;
				GUI.Box(r, "");
			}
		}

		public override bool HandleEvent(Event e)
		{
			switch(e.type)
			{
			case EventType.MouseDown:
			case EventType.MouseDrag:
				if (e.button == 0 || e.button == 1)
				{
					Vector2Int position = EditorGrid.GetInstance().ScreenToGridPos(e.mousePosition, true);
					bool posHasTile = EditorGrid.GetInstance().HasTileAt(position);
					if ((e.button == 0 && !posHasTile) || (e.button == 1 && posHasTile))
					{
						manipPositions.Add(position);
						return true;
					}
				}
				break;
			case EventType.MouseUp:
				if(e.button == 0 || e.button == 1)
				{
					//attempt to place a tile in accumpulated positions
					Vector2Int[] finalPositions = new Vector2Int[manipPositions.Count];
					manipPositions.CopyTo(finalPositions);
					Command c;
					if (e.button == 0)
					{
						c = new AddTileCommand(finalPositions);
					}
					else if (e.button == 1)
					{
						c = new RemoveTileCommand(finalPositions);
					}
					else
					{
						return false;
					}
					c.Do();
					History.RecordCommand(c);
					e.Use();
					manipPositions.Clear();
					return true;
				}
				break;
			}
			return false;
		}
	}
}
