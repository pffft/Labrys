using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class EditorGrid
	{
		public const float GRID_DENSITY = 2f;

		private static EditorGrid instance;

		public Rect viewport;

		public float scale = 1f;
		public float lineSpacing = 64f;
		public Color lineColor = new Color(0.4f, 0.4f, 0.4f);

		private Vector2 offset;
		private Vector2 drag;

		private Dictionary<Vector2Int, Tile> tiles;
		private List<Tile> selectedTiles;
		private Dictionary<Vector2Int, Connection> connections;

		public static EditorGrid GetInstance()
		{
			if(instance == null)
			{
				instance = new EditorGrid();
			}
			return instance;
		}

		public EditorGrid()
		{
			viewport = new Rect();

			offset = Vector2.zero;

			tiles = new Dictionary<Vector2Int, Tile>();
			connections = new Dictionary<Vector2Int, Connection>();
		}

		public void Draw()
		{
			Color lightLineColor = new Color (lineColor.r, lineColor.g, lineColor.b, lineColor.a * 0.5f);

			float scaledSpacing = lineSpacing * scale;

			int xLineCount = Mathf.CeilToInt(viewport.width / scaledSpacing);
			int yLineCount = Mathf.CeilToInt (viewport.height / scaledSpacing);

			Vector2 wrappedOffset = new Vector2 (
				Mathf.Abs (offset.x) % scaledSpacing * Mathf.Sign (offset.x), 
				Mathf.Abs (offset.y) % scaledSpacing * Mathf.Sign (offset.y));
			Vector2Int linesOffset = ScreenToGridPos (offset);

			Handles.BeginGUI ();
			for (int i = 0; i < xLineCount + 1; i++)
			{
				int xGridLine = ScreenToGridPos(new Vector2(scaledSpacing * i + wrappedOffset.x, 0f)).x;
				if (xGridLine % 10 == 0)
					Handles.color = xGridLine == 0 ? Color.black : lineColor;
				else
					Handles.color = lightLineColor;
				Handles.DrawLine (
					new Vector2 (scaledSpacing * i, -scaledSpacing) + wrappedOffset, 
					new Vector2 (scaledSpacing * i, viewport.height + scaledSpacing) + wrappedOffset);
			}

			for (int i = 0; i < yLineCount + 1; i++)
			{
				int yGridLine = ScreenToGridPos(new Vector2(0f, scaledSpacing * i + wrappedOffset.y)).y;
				if (yGridLine % 10 == 0)
					Handles.color = yGridLine == 0 ? Color.black : lineColor;
				else
					Handles.color = lightLineColor;
				Handles.DrawLine (
					new Vector2 (-scaledSpacing, scaledSpacing * i) + wrappedOffset, 
					new Vector2 (viewport.width + scaledSpacing, scaledSpacing * i) + wrappedOffset);
			}
			Handles.EndGUI ();

			DrawObjects ();
		}

		private void DrawObjects()
		{
			if (tiles != null)
			{
				foreach (GridObject t in tiles.Values)
					t.Draw ();
			}

			if (connections != null)
			{
				foreach (GridObject c in connections.Values)
					c.Draw();
			}
		}

		public bool HandleEvent(Event e)
		{
			drag = Vector2.zero;

			bool guiChanged = false;
			switch (e.type)
			{
			case EventType.MouseDrag:
				//drag the grid and the tiles
				if (e.button == 2)
				{
					Shift (e.delta);
					guiChanged = true;
				}
				break;
			case EventType.ScrollWheel:
				if (Mathf.Sign(e.delta.y) > 0)
					Resize(scale / 1.1f);
				else
					Resize(scale * 1.1f);
				guiChanged = true;
				break;
			case EventType.KeyDown:
				//create a new tile
				if (e.keyCode == KeyCode.N)
				{
					CreateTile (e.mousePosition);
					guiChanged = true;
				}
				break;
			}

			return guiChanged;
		}

		private void Shift(Vector2 dPos)
		{
			drag = dPos;
			offset += drag;
		}

		public void Recenter()
		{
			offset = Vector2.zero;

			Shift(viewport.size / 2f);
		}

		public void Resize(float scale)
		{
			float dScale = scale / this.scale;
			offset -= viewport.size / 2f;
			offset *= dScale;
			offset += viewport.size / 2f;

			this.scale = scale;
		}

		public void CreateTile(Vector2 screenPos)
		{
			Tile t = new Tile (screenPos, new Vector2 (lineSpacing, lineSpacing));
			t.GridPosition = ScreenToGridPos (screenPos, evenOnly: true);

			if (!tiles.ContainsKey(t.GridPosition))
			{
				tiles.Add(t.GridPosition, t);
				TryAddConnections(t);
			}
		}

		public Tile GetTile(Vector2 screenPos)
		{
			Vector2Int gridPos = ScreenToGridPos(screenPos, true);
			if(tiles.TryGetValue(gridPos, out Tile t))
			{
				return t;
			}
			return null;
		}

		public bool MoveTile(Vector2 screenPos, Tile t)
		{
			Vector2Int newGridPos = ScreenToGridPos(screenPos, evenOnly: true);
			if (!tiles.ContainsKey(newGridPos))
			{
				tiles.Remove(t.GridPosition);
				TryRemoveConnections(t);

				t.GridPosition = newGridPos;
				tiles.Add(t.GridPosition, t);

				TryAddConnections(t);
				return true;
			}
			else
			{
				t.RevertDrag();
				return false;
			}
		}

		public void RemoveGridObject(GridObject go)
		{
			if (typeof(Tile) == go.GetType())
			{
				Tile t = (Tile)go;
				tiles.Remove(go.GridPosition);
				TryRemoveConnections(t);
			}
			else
			{
				connections.Remove(go.GridPosition);
			}
			GUI.changed = true;
		}

		private void TryAddConnections(Tile t)
		{
			foreach (Vector2Int connectionPos in t.GetAllAdjPosition())
			{
				if (!connections.ContainsKey(connectionPos))
				{
					Connection c = new Connection(connectionPos);
					connections.Add(connectionPos, c);
					int neighborCount = 0;
					foreach (Vector2Int tilePos in c.GetSubjectTileGridPositions())
					{
						if (tiles.ContainsKey(tilePos))
						{
							neighborCount++;
						}
					}
					//TODO also do this to existing connections that neighbor new/moved tiles
					c.Enabled = neighborCount == c.GetMaxSubjectTileCount();
				}
			}
		}

		private void TryRemoveConnections(Tile t)
		{
			//check all connections are still valid
			foreach (Vector2Int connectionPos in t.GetAllAdjPosition())
			{
				if (connections.TryGetValue(connectionPos, out Connection connection))
				{
					int neighborCount = 0;
					foreach (Vector2Int tilePos in connection.GetSubjectTileGridPositions())
					{
						if (tiles.ContainsKey(tilePos))
						{
							neighborCount++;
						}
					}

					//remove connections that have no tile neightbors
					if (neighborCount == 0)
					{
						connections.Remove(connectionPos);
					}
					connection.Enabled = neighborCount == connection.GetMaxSubjectTileCount();
				}
			}
		}

		/// <summary>
		/// Transforms a screen position to a grid position (1:1).
		/// </summary>
		/// <param name="screenPos"></param>
		/// <returns></returns>
		public Vector2 ScreenToGridSpace(Vector2 screenPos, bool evenOnly = false)
		{
			Vector2 gridPos = new Vector2(screenPos.x - offset.x, screenPos.y - offset.y) / (lineSpacing * scale);
			if(evenOnly)
			{
				gridPos = new Vector2(Mathf.Round(gridPos.x), Mathf.Round(gridPos.y));
			}
			return gridPos * GRID_DENSITY;
		}

		/// <summary>
		/// Transforms a screen position to a specific grid position that can be used to identify a tile (n:1).
		/// </summary>
		/// <param name="screenPos"></param>
		/// <returns></returns>
		public Vector2Int ScreenToGridPos(Vector2 screenPos, bool evenOnly = false)
		{
			Vector2 gridSpacePos = ScreenToGridSpace(screenPos, evenOnly);
			return new Vector2Int(Mathf.RoundToInt(gridSpacePos.x), Mathf.RoundToInt(gridSpacePos.y));
		}

		/// <summary>
		/// Transforms a grid position into a corresponding screen position (1:1).
		/// </summary>
		/// <param name="gridPos"></param>
		/// <returns></returns>
		public Vector2 GridToScreenSpace(Vector2 gridPos)
		{
			return new Vector2(
				(gridPos.x * lineSpacing * scale / GRID_DENSITY) + offset.x,
				(gridPos.y * lineSpacing * scale / GRID_DENSITY) + offset.y);
		}

		/// <summary>
		/// Same as GridToScreenSpace(Vector2), but takes a Vector2Int.
		/// </summary>
		/// <param name="gridPos"></param>
		/// <returns></returns>
		public Vector2 GridToScreenPos(Vector2Int gridPos)
		{
			return GridToScreenSpace(gridPos);
		}
	}
}
