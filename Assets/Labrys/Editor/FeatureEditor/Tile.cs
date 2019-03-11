using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class Tile : GridObject
	{
		private static Color selectedColor = new Color(0.5f, 0.5f, 0.8f);

		public event Action removed;
		public event Action dragFinished;

		public bool IsDragging { get; set; }
		public bool IsSelected { get; set; }

		private Rect bounds;
		private Vector2 baseSize;

		[PropertyField(Name = "Variant")]
		public string Variant { get; set; }

		public Vector2Int Right => GridPosition + Vector2Int.right;
		public Vector2Int UpRight => GridPosition + Vector2Int.up + Vector2Int.right;
		public Vector2Int Up => GridPosition + Vector2Int.up;
		public Vector2Int UpLeft => GridPosition + Vector2Int.up + Vector2Int.left;
		public Vector2Int Left => GridPosition + Vector2Int.left;
		public Vector2Int DownLeft => GridPosition + Vector2Int.down + Vector2Int.left;
		public Vector2Int Down => GridPosition + Vector2Int.down;
		public Vector2Int DownRight => GridPosition + Vector2Int.down + Vector2Int.right;

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

		public Tile(Vector2 position, Vector2 size)
		{
			bounds = new Rect (position, size);
			bounds.center = position;
			baseSize = bounds.size;

			Variant = "";
		}

		public override void Draw()
		{
			bounds.position = ScreenPosition;
			bounds.center = ScreenPosition;
			bounds.size = baseSize * Scale * 0.9f;
			Color temp = GUI.color;
			GUI.color = IsSelected ? selectedColor : Color.white;
			GUI.Box (bounds, "");
			GUI.color = temp;
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
				all[i] += GridPosition;
			}
			return all;
		}
	}
}
