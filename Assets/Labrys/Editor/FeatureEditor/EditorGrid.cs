using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class EditorGrid
	{
		private const float SQRT_2 = 1.41421f;

		public float scale;
		public float lineSpacing;
		public Color lineColor;

		private Vector2 offset;
		private Vector2 drag;

		private Dictionary<Vector2Int, Tile> tiles;
		private Queue<MapUpdateOperation> staleTiles;

		private MainWindow hostWindow;

		public EditorGrid(MainWindow host, float lineSpacing, Color lineColor)
		{
			hostWindow = host;
			this.lineSpacing = lineSpacing;
			this.lineColor = lineColor;
			scale = 1f;

			offset = Vector2.zero;

			tiles = new Dictionary<Vector2Int, Tile> ();
			staleTiles = new Queue<MapUpdateOperation> ();
		}

		public void Draw()
		{
			Color lightLineColor = new Color (lineColor.r, lineColor.g, lineColor.b, lineColor.a * 0.5f);

			float scaledSpacing = lineSpacing * scale;

			int xLineCount = Mathf.CeilToInt(hostWindow.position.width / scaledSpacing);
			int yLineCount = Mathf.CeilToInt (hostWindow.position.height / scaledSpacing);

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
					new Vector2 (scaledSpacing * i, hostWindow.position.height + scaledSpacing) + wrappedOffset);
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
					new Vector2 (hostWindow.position.width + scaledSpacing, scaledSpacing * i) + wrappedOffset);
			}
			Handles.EndGUI ();

			DrawTiles (wrappedOffset);
		}

		private void DrawTiles(Vector2 offset)
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

		public bool HandleEvent(Event e)
		{
			drag = Vector2.zero;

			bool guiChanged = false;
			if (tiles != null)
			{
				
				foreach (Tile t in tiles.Values)
				{
					if (t.HandleEvent (e) && !guiChanged)
						guiChanged = true;
				}
			}

			switch (e.type)
			{
			case EventType.MouseDrag:
				//drag the grid and the tiles
				if (e.button == 0)
				{
					Shift (e.delta);
					guiChanged = true;
				}
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
					t.Drag (dPos);
				}
			}
		}

		public void Recenter()
		{
			foreach (Tile t in tiles.Values)
				t.bounds.position -= offset;
			offset = Vector2.zero;

			Shift(hostWindow.position.size / 2f);
		}

		public void Resize(float scale)
		{
			this.scale = scale;
			foreach (Tile t in tiles.Values)
			{
				t.Resize(scale);
				RealignTile(t);
			}
		}

		public void CreateTile(Vector2 mousePos)
		{
			Tile t = new Tile (mousePos, new Vector2 (lineSpacing, lineSpacing), hostWindow.defaultTileStyle, hostWindow.selectedTileStyle);
			t.removed += RemoveTile;
			t.dragFinished += AlignTile;
			t.position = ScreenToGridPos (mousePos);

			//if align is successful, then tile will be added
			TryAlignTile (t);
			t.Resize(scale);
		}

		public void RemoveTile(Tile t)
		{
			t.removed -= RemoveTile;
			t.dragFinished -= AlignTile;
			staleTiles.Enqueue (new MapUpdateOperation ()
			{
				Action = (Tile sub) => {
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
				Vector2Int oldGridPos = t.position;
				t.position = newGridPos;
				t.bounds.position = GridToScreenPos (newGridPos);

				//TryAlignTile gets called in the tile update loop, where we can't make modifications to the contents of the dictionary.
				//queue up an operation for after the loop is done to re-orient the tile  in the dictionary.
				staleTiles.Enqueue (new MapUpdateOperation ()
				{
					Action = (Tile sub) => {
						tiles.Remove (oldGridPos);
						tiles.Add (sub.position, sub);
					},
					Subject = t
				});

				return true;
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

		private Vector2Int ScreenToGridPos(Vector2 screenPos)
		{
			return new Vector2Int (
				Mathf.RoundToInt ((screenPos.x - offset.x) / (lineSpacing * scale)),
				Mathf.RoundToInt ((screenPos.y - offset.y) / (lineSpacing * scale)));
		}

		private Vector2 GridToScreenPos(Vector2Int gridPos)
		{
			return new Vector2 (
				(gridPos.x * lineSpacing * scale) + offset.x,
				(gridPos.y * lineSpacing * scale) + offset.y);
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
