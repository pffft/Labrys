using Labrys.Editor.FeatureEditor.Commands;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Tools
{
	public class TilePaintTool : Tool
	{
		private HashSet<Vector2Int> manipPositions;
		private Color previewColor;

		public TilePaintTool(EditorWindow window) : base(window)
		{
			manipPositions = new HashSet<Vector2Int>();
			previewColor = Color.white;

			Name = "Tile Brush";
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
				GUI.color = previewColor;
				GUI.Box(r, "");
			}
		}

		private bool isPrimaryControl(Event e)
		{
			return (e.isMouse && e.button == 0);
		}

		private bool isSecondaryControl(Event e)
		{
			return (e.isMouse && e.button == 1);
		}

		public override bool HandleEvent(Event e)
		{
			switch(e.type)
			{
			case EventType.MouseDown:
			case EventType.MouseDrag:
				if (isPrimaryControl(e) || isSecondaryControl(e))
				{
					Vector2Int position = EditorGrid.GetInstance().ScreenToGridPos(e.mousePosition, true);
					bool posHasTile = EditorGrid.GetInstance().Feature.HasSectionAt(position);
					if ((isPrimaryControl(e) && !posHasTile) || (isSecondaryControl(e) && posHasTile))
					{
						manipPositions.Add(position);
						if (isPrimaryControl(e))
						{
							previewColor = Color.green;
						}
						else if (isSecondaryControl(e))
						{
							previewColor = Color.red;
						}
						return true;
					}
				}
				break;
			case EventType.MouseUp:
				if(isPrimaryControl(e) || isSecondaryControl(e))
				{
					//attempt to place a tile in accumpulated positions
					Vector2Int[] finalPositions = new Vector2Int[manipPositions.Count];
					manipPositions.CopyTo(finalPositions);
					Command c;
					if (isPrimaryControl(e))
					{
						c = new AddTileCommand(finalPositions);
					}
					else if (isSecondaryControl(e))
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
