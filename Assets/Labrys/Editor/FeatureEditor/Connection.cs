using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class Connection
	{
		public Vector2Int Position { get; private set; }
		public Vector2Int Direction { get; private set; }

		public static Vector2Int LinkDirectionToVector2Int(LinkDirection dir)
		{
			switch (dir)
			{
			case LinkDirection.Right:
				return Vector2Int.right;
			case LinkDirection.Up:
				return Vector2Int.up;
			case LinkDirection.Left:
				return Vector2Int.left;
			case LinkDirection.Down:
				return Vector2Int.down;
			default:
				return Vector2Int.zero;
			}
		}

		public static bool operator ==(Connection left, Connection right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Connection left, Connection right)
		{
			return !left.Equals(right);
		}

		public Connection(Vector2Int postion, Vector2Int direction)
		{
			Position = postion;
			Direction = direction;
		}

		public void Draw()
		{

			GUI.Box(new Rect(Position, new Vector2(1f, 1f)), "");
			Handles.BeginGUI();
			Handles.DrawWireDisc(new Vector3(Position.x, Position.y), Vector3.forward, 1f);
			Handles.EndGUI();
		}

		public bool HandleEvent(Event e)
		{
			return false;
		}

		public override bool Equals(object obj)
		{
			Connection other = (Connection)obj;
			return Position == other.Position && Direction == other.Direction;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
