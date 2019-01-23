using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class Tile
	{
		public GUIStyle style, defaultStyle, selectedStyle;
		public bool isDragging;
		public bool isSelected;

		public delegate void TileAction(Tile t);
		public event TileAction removed;
		public event TileAction dragFinished;

		public Rect bounds;
		private Vector2 baseSize;

		public Vector2Int position;
		public string variant;

		public Vector2Int Right => position + Vector2Int.right;
		public Vector2Int UpRight => position + Vector2Int.up + Vector2Int.right;
		public Vector2Int Up => position + Vector2Int.up;
		public Vector2Int UpLeft => position + Vector2Int.up + Vector2Int.left;
		public Vector2Int Left => position + Vector2Int.left;
		public Vector2Int DownLeft => position + Vector2Int.down + Vector2Int.left;
		public Vector2Int Down => position + Vector2Int.down;
		public Vector2Int DownRight => position + Vector2Int.down + Vector2Int.right;

		public static Vector2Int[] GetAllNeighborDirections()
		{
			Vector2Int[] all = new Vector2Int[8];
			all[0] = Vector2Int.right;
			all[1] = Vector2Int.up + Vector2Int.right;
			all[2] = Vector2Int.up;
			all[3] = Vector2Int.up + Vector2Int.left;
			all[4] = Vector2Int.left;
			all[5] = Vector2Int.down + Vector2Int.left;
			all[6] = Vector2Int.down;
			all[7] = Vector2Int.down + Vector2Int.right;
			return all;
		}

		public Tile(Vector2 position, Vector2 size, GUIStyle defaultStyle, GUIStyle selectedStyle)
		{
			bounds = new Rect (position, size);
			bounds.center = position;
			baseSize = bounds.size;
			this.defaultStyle = defaultStyle;
			this.selectedStyle = selectedStyle;
			style = defaultStyle;

			variant = "";
		}

		public void Shift(Vector2 dPos)
		{
			bounds.position += dPos;
		}

		public void Resize(float scale)
		{
			bounds.size = baseSize * scale;
		}

		public void Draw()
		{
			GUI.Box (bounds, variant, style);
		}

		public bool HandleEvent(Event e)
		{
			switch (e.type)
			{
			case EventType.MouseDown:
				//select and drag
				if (e.button == 0)
				{
					if (bounds.Contains (e.mousePosition))
					{
						isDragging = isSelected = true;
						style = selectedStyle;
					}
					else
					{
						isSelected = false;
						style = defaultStyle;
					}
					return true;
				}
				//open tile-specific context menu
				else if (e.button == 1 && isSelected && bounds.Contains (e.mousePosition))
				{
					HandleContextMenu ();
					e.Use ();
				}
				break;
			case EventType.MouseUp:
				//stop dragging
				if (isDragging)
				{
					isDragging = false;
					if (dragFinished != null)
						dragFinished.Invoke (this);
				}
				break;
			case EventType.MouseDrag:
				//perform drag
				if (e.button == 0 && isDragging)
				{
					Shift (e.delta);
					e.Use ();
					return true;
				}
				break;
			case EventType.KeyDown:
				//delete selected tile with Delete or Backspace
				if (e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace)
				{
					if (isSelected)
					{
						e.Use ();
						if (removed != null)
							removed.Invoke (this);
					}
				}
				break;
			}
			return false;
		}

		private void HandleContextMenu()
		{
			GenericMenu menu = new GenericMenu ();
			menu.AddItem (new GUIContent ("Remove tile"), false, () => {
				if (removed != null)
					removed.Invoke (this);
			});
			menu.ShowAsContext ();
		}

		public Vector2Int GetAdjPosition(LinkDirection direction)
		{
			switch(direction)
			{
			case LinkDirection.Right: return Right;
			case LinkDirection.UpRight: return UpRight;
			case LinkDirection.Up: return Up;
			case LinkDirection.UpLeft: return UpLeft;
			case LinkDirection.Left: return Left;
			case LinkDirection.DownLeft: return DownLeft;
			case LinkDirection.Down: return Down;
			case LinkDirection.DownRight: return DownRight;
			default: return Vector2Int.zero;
			}
		}

		public Vector2Int[] GetAllAdjPosition()
		{
			Vector2Int[] all = GetAllNeighborDirections();
			for(int i = 0; i < 8; i++)
			{
				all[i] += position;
			}
			return all;
		}
	}
}
