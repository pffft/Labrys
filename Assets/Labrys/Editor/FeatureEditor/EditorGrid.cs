using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class EditorGrid
	{
		private static EditorGrid instance;

		public float scale = 1f;
		public float lineSpacing = 64f;
		public Color lineColor = new Color(0.4f, 0.4f, 0.4f);

		private Vector2 offset;
		private Vector2 drag;

		private Dictionary<Vector2Int, Tile> tiles;
		private Queue<MapUpdateOperation> staleTiles;

		private Dictionary<Vector2, Connection> connections;

		public Rect viewport;

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

			tiles = new Dictionary<Vector2Int, Tile> ();
			staleTiles = new Queue<MapUpdateOperation> ();

			connections = new Dictionary<Vector2, Connection>();
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

			DrawTiles ();
			DrawConnections();
		}

		private void DrawTiles()
		{
			if (tiles != null)
			{
				foreach (Tile t in tiles.Values)
					t.Draw ();
			}

			while (staleTiles.Count > 0)
			{
				MapUpdateOperation operation = staleTiles.Dequeue ();
				operation.Action.Invoke (operation.Subject);
			}
		}

		private void DrawConnections()
		{
			if (connections != null)
			{
				foreach (Connection c in connections.Values)
					c.Draw();
			}
		}

		public bool HandleEvent(Event e)
		{
			drag = Vector2.zero;

			bool guiChanged = false;
			if (tiles != null)
			{
				foreach (Tile t in tiles.Values)
				{
					if (t.HandleEvent (e))
						guiChanged = true;
				}
			}

			if(connections != null)
			{
				foreach(Connection c in connections.Values)
				{
					if (c.HandleEvent(e))
						guiChanged = true;
				}
			}

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
				GUI.changed = true;
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

			if (tiles != null)
			{
				foreach (Tile t in tiles.Values)
				{
					t.Shift (dPos);
				}
			}

			if (connections != null)
			{
				foreach (Connection c in connections.Values)
				{
					c.Shift(dPos);
				}
			}
		}

		public void Recenter()
		{
			foreach (Tile t in tiles.Values)
				t.bounds.position -= offset;
			foreach (Connection c in connections.Values)
				c.DrawPosition -= offset;
			offset = Vector2.zero;

			Shift(viewport.size / 2f);
		}

		public void Resize(float scale)
		{
			this.scale = scale;
			foreach (Tile t in tiles.Values)
			{
				t.Resize(scale);
				RealignTile(t);
			}
			foreach (Connection c in connections.Values)
			{
				c.Resize(scale);
				RealignConnection(c);
			}
		}

		public void CreateTile(Vector2 mousePos)
		{
			Tile t = new Tile (mousePos, new Vector2 (lineSpacing, lineSpacing));
			t.removed += RemoveTile;
			t.dragFinished += AlignTile;
			t.position = ScreenToGridPos (mousePos);
			t.Resize(scale);

			//if align is successful, then tile will be added
			if(TryAlignTile (t))
			{
				
			}
			
		}

		public void RemoveTile(Tile t)
		{
			t.removed -= RemoveTile;
			t.dragFinished -= AlignTile;
			staleTiles.Enqueue (new MapUpdateOperation ()
			{
				Action = (Tile sub) => {
					TryRemoveConnections(sub);
					tiles.Remove (sub.position);
				},
				Subject = t
			});
			GUI.changed = true;
		}

		public Tile TryGetTile(Vector2Int position)
		{
			if(tiles.TryGetValue(position, out Tile t))
			{
				return t;
			}
			return null;
		}

		private void AlignTile(Tile t)
		{
			TryAlignTile (t);
		}

		/// <summary>
		/// Attempt to place a tile in a grid coordinate based on its current visual position.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		private bool TryAlignTile(Tile t)
		{
			GUI.changed = true;
			Vector2Int newGridPos = ScreenToGridPos (t.bounds.position);
			if (tiles.ContainsKey (newGridPos))
			{
				//space is occupied, return to last valid position
				t.bounds.position = GridToScreenPos (t.position);
				return false;
			}
			else
			{
				//space is valid, move tile
				//TryAlignTile gets called in the tile update loop, where we can't make modifications to the contents of the dictionary.
				//queue up an operation for after the loop is done to re-orient the tile  in the dictionary.
				staleTiles.Enqueue (new MapUpdateOperation ()
				{
					Action = (Tile sub) => {
						TryRemoveConnections(sub);

						//move tile
						tiles.Remove(sub.position);
						sub.position = newGridPos;
						sub.bounds.position = GridToScreenPos(newGridPos);
						tiles.Add (sub.position, sub);

						//add new connections in new position
						foreach (Vector2Int neighborPos in Tile.GetAllNeighborDirections())
						{
							Vector2 connectionPos = new Vector2(neighborPos.x / 2f, neighborPos.y / 2f) + sub.position;
							if (!connections.ContainsKey(connectionPos))
							{
								Debug.Log(connectionPos);
								connections.Add(connectionPos, new Connection(connectionPos, GridToScreenSpace(connectionPos + new Vector2(0.5f, 0.5f))));
							}
						}
					},
					Subject = t
				});

				return true;
			}
		}

		private void TryRemoveConnections(Tile t)
		{
			if (tiles.ContainsKey(t.position))
			{
				//check all connections are still valid
				foreach (Vector2Int neighborPos in Tile.GetAllNeighborDirections())
				{
					Vector2 connectionPos = new Vector2(neighborPos.x / 2f, neighborPos.y / 2f) + t.position;
					if (connections.TryGetValue(connectionPos, out Connection connection))
					{
						bool hasNeighbor = false;
						foreach (Vector2Int tilePos in connection.GetSubjectGridPositions())
						{
							if (tiles.ContainsKey(tilePos))
							{
								hasNeighbor = true;
								break;
							}
						}

						//remove connections that have no tile neightbors
						if (!hasNeighbor)
						{
							connections.Remove(connectionPos);
						}
					}
				}
			}
		}

		/// <summary>
		/// Snap tile visual position to corresponding grid position.
		/// </summary>
		/// <param name="t"></param>
		private void RealignTile(Tile t)
		{
			GUI.changed = true;
			if(tiles.ContainsKey(t.position))
			{
				t.bounds.position = GridToScreenPos(t.position);
			}
		}

		private void RealignConnection(Connection c)
		{
			GUI.changed = true;
			if (connections.ContainsKey(c.Position))
			{
				c.DrawPosition = GridToScreenSpace(c.Position + new Vector2(0.5f, 0.5f));
			}
		}

		/// <summary>
		/// Transforms a screen position to a grid position (1:1).
		/// </summary>
		/// <param name="screenPos"></param>
		/// <returns></returns>
		public Vector2 ScreenToGridSpace(Vector2 screenPos)
		{
			return new Vector2(
				(screenPos.x - offset.x) / (lineSpacing * scale),
				(screenPos.y - offset.y) / (lineSpacing * scale));
		}

		/// <summary>
		/// Transforms a screen position to a specific grid position that can be used to identify a tile (n:1).
		/// </summary>
		/// <param name="screenPos"></param>
		/// <returns></returns>
		public Vector2Int ScreenToGridPos(Vector2 screenPos)
		{
			Vector2 gridSpacePos = ScreenToGridSpace(screenPos);
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
				(gridPos.x * lineSpacing * scale) + offset.x,
				(gridPos.y * lineSpacing * scale) + offset.y);
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

		/// <summary>
		/// An action to make on a tile after the tile update loop has completed
		/// </summary>
		private struct MapUpdateOperation
		{
			public Tile.TileAction Action { get; set; }
			public Tile Subject { get; set; }
		}
	}
}
