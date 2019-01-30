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

		private Dictionary<Vector2Int, GridObject> tiles;
		private Dictionary<Vector2Int, GridObject> connections;
		private Queue<GridUpdateOperation> staleObjects;

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

			tiles = new Dictionary<Vector2Int, GridObject>();
			connections = new Dictionary<Vector2Int, GridObject>();
			staleObjects = new Queue<GridUpdateOperation>();
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

			while (staleObjects.Count > 0)
			{
				GridUpdateOperation operation = staleObjects.Dequeue ();
				operation.Action.Invoke (operation.Subject);
			}
		}

		public bool HandleEvent(Event e)
		{
			drag = Vector2.zero;

			bool guiChanged = false;
			if (tiles != null)
			{
				foreach (GridObject t in tiles.Values)
				{
					if (t.HandleEvent (e))
						guiChanged = true;
				}
			}

			if (connections != null)
			{
				foreach (GridObject c in connections.Values)
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

		public void CreateTile(Vector2 mousePos)
		{
			Tile t = new Tile (mousePos, new Vector2 (lineSpacing, lineSpacing));
			t.removed += RemoveGridObject;
			t.dragFinished += AlignTile;
			t.GridPosition = ScreenToGridPos (mousePos, evenOnly: true);

			//if align is successful, then tile will be added
			if(TryAlignTile (t))
			{
				
			}
			
		}

		public void RemoveGridObject(GridObject go)
		{
			if (typeof(Tile) == go.GetType())
			{
				Tile t = (Tile)go;
				t.removed -= RemoveGridObject;
				t.dragFinished -= AlignTile;
				staleObjects.Enqueue(new GridUpdateOperation()
				{
					Action = (GridObject sub) => {
						tiles.Remove(sub.GridPosition);
						TryRemoveConnections(t);
					},
					Subject = t
				});
			}
			else
			{
				staleObjects.Enqueue(new GridUpdateOperation()
				{
					Action = (GridObject sub) => {
						connections.Remove(sub.GridPosition);
					},
					Subject = go
				});
			}
			GUI.changed = true;
		}

		private void AlignTile(GridObject t)
		{
			if(typeof(Tile) == t.GetType())
				TryAlignTile ((Tile)t);
		}

		/// <summary>
		/// Attempt to place a tile in a grid coordinate based on its current visual position.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		private bool TryAlignTile(Tile t)
		{
			GUI.changed = true;
			Vector2Int newGridPos = ScreenToGridPos (t.ScreenPosition, evenOnly: true);
			if (tiles.ContainsKey (newGridPos))
			{
				//space is occupied, return to last valid position
				t.RevertDrag();
				return false;
			}
			else
			{
				//space is valid, move tile
				//TryAlignTile gets called in the tile update loop, where we can't make modifications to the contents of the dictionary.
				//queue up an operation for after the loop is done to re-orient the tile in the dictionary.
				staleObjects.Enqueue (new GridUpdateOperation ()
				{
					Action = (GridObject sub) => {
						Tile subTile = (Tile)sub;
						
						//move tile
						tiles.Remove(subTile.GridPosition);
						TryRemoveConnections(subTile);
						subTile.GridPosition = newGridPos;
						tiles.Add (subTile.GridPosition, subTile);

						//add new connections in new position
						foreach (Vector2Int connectionPos in subTile.GetAllAdjPosition())
						{
							if (!connections.ContainsKey(connectionPos))
							{
								connections.Add(connectionPos, new Connection(connectionPos));
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
			//check all connections are still valid
			foreach (Vector2Int connectionPos in t.GetAllAdjPosition())
			{
				if (connections.TryGetValue(connectionPos, out GridObject go))
				{
					if (typeof(Connection) == go.GetType())
					{
						Connection connection = (Connection)go;
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

		/// <summary>
		/// An action to make on a tile after the tile update loop has completed
		/// </summary>
		private struct GridUpdateOperation
		{
			public GridObject.Action Action { get; set; }
			public GridObject Subject { get; set; }
		}
	}
}
