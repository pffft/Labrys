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

		private List<Tile> tiles;
		private Queue<Tile> staleTiles;

		private MainWindow hostWindow;

		public EditorGrid(MainWindow host, float lineSpacing, Color lineColor)
		{
			hostWindow = host;
			this.lineSpacing = lineSpacing;
			this.lineColor = lineColor;
			scale = 1f;

			offset = Vector2.zero;

			tiles = new List<Tile> ();
			staleTiles = new Queue<Tile> ();
		}

		public void Draw()
		{
			float scaledSpacing = lineSpacing * scale;

			int xLineCount = Mathf.CeilToInt(hostWindow.position.width / scaledSpacing);
			int yLineCount = Mathf.CeilToInt (hostWindow.position.height / scaledSpacing);

			Handles.BeginGUI ();
			Handles.color = lineColor;

			offset += drag;
			Vector2 wrappedOffset = new Vector2 (Mathf.Abs (offset.x) % scaledSpacing * Mathf.Sign (offset.x), Mathf.Abs (offset.y) % scaledSpacing * Mathf.Sign (offset.y));

			for (int i = 0; i < xLineCount + 1; i++)
			{
				Handles.DrawLine (new Vector2 (scaledSpacing * i, -scaledSpacing) + wrappedOffset, new Vector2 (scaledSpacing * i, hostWindow.position.height + scaledSpacing) + wrappedOffset);
			}

			for (int i = 0; i < yLineCount + 1; i++)
			{
				Handles.DrawLine (new Vector2 (-scaledSpacing, scaledSpacing * i) + wrappedOffset, new Vector2 (hostWindow.position.width + scaledSpacing, scaledSpacing * i) + wrappedOffset);
			}

			Handles.EndGUI ();

			DrawTiles (wrappedOffset);
		}

		private void DrawTiles(Vector2 offset)
		{
			if (tiles != null)
			{
				foreach (Tile t in tiles)
					t.Draw ();
			}

			while (staleTiles.Count > 0)
				tiles.Remove (staleTiles.Dequeue ());
		}

		public bool HandleEvent(Event e)
		{
			drag = Vector2.zero;

			bool guiChanged = false;
			if (tiles != null)
			{
				
				foreach (Tile t in tiles)
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
					OnDrag (e.delta);
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

			return guiChanged; ;
		}

		private void OnDrag(Vector2 dPos)
		{
			drag = dPos;

			if (tiles != null)
			{
				foreach (Tile t in tiles)
				{
					t.Drag (dPos);
				}
			}
		}

		public void Recenter()
		{
			foreach (Tile t in tiles)
				t.bounds.position -= offset;
			offset = Vector2.zero;
		}

		public void Resize(float scale)
		{
			this.scale = scale;
			foreach (Tile t in tiles)
				t.Resize (scale);
		}

		public void CreateTile(Vector2 mousePos)
		{
			Tile t = new Tile (mousePos, new Vector2 (lineSpacing, lineSpacing), hostWindow.defaultTileStyle, hostWindow.selectedTileStyle);
			t.removed += RemoveTile;
			t.dragFinished += AlignTile;
			tiles.Add (t);
			AlignTile (t);
		}

		public void RemoveTile(Tile t)
		{
			t.removed -= RemoveTile;
			t.dragFinished -= AlignTile;
			staleTiles.Enqueue (t);
			GUI.changed = true;
		}

		private void AlignTile(Tile t)
		{
			t.bounds.position = new Vector2 (
				(Mathf.Round ((t.bounds.position.x - offset.x) / lineSpacing) * lineSpacing) + offset.x,
				(Mathf.Round ((t.bounds.position.y - offset.y) / lineSpacing) * lineSpacing) + offset.y
				);
			GUI.changed = true;
		}
	}
}
