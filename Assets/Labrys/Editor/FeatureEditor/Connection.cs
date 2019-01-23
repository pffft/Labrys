using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class Connection
	{
		private const float SIZE = 10f;

		public bool Open { get; private set; }
		public bool External { get; private set; }

		public Vector2 Position { get; set; }
		public Vector2 DrawPosition { get; set; }

		private float scale;

		public delegate void ConnectionAction(Connection t);
		public event ConnectionAction removed;

		public static bool operator ==(Connection left, Connection right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Connection left, Connection right)
		{
			return !left.Equals(right);
		}

		public Connection(Vector2 position, Vector2 drawPosition)
		{
			Position = position;
			DrawPosition = drawPosition;
			Open = true;
			External = false;
		}

		public void Shift(Vector2 dPos)
		{
			DrawPosition += dPos;
		}

		public void Resize(float scale)
		{
			this.scale = scale;
		}

		public void Draw()
		{
			Handles.color = Open ? Color.green : Color.red;
			Handles.BeginGUI();
			Handles.DrawSolidDisc(new Vector3(DrawPosition.x, DrawPosition.y), Vector3.forward, scale * SIZE);
			Handles.EndGUI();
		}

		public bool HandleEvent(Event e)
		{
			if (Vector2.Distance(e.mousePosition, DrawPosition) < scale * SIZE)
			{
				switch (e.type)
				{
				case EventType.MouseDown:
					if (e.button == 1)
					{
						//toggle connection
						Open = !Open;
						e.Use();
						return true;
					}
					break;
				}
			}
			return false;
		}

		public IEnumerable<Vector2Int> GetSubjectGridPositions()
		{
			HashSet<Vector2Int> uniquePositions = new HashSet<Vector2Int>();

			int lowerX = Mathf.FloorToInt(Position.x), upperX = Mathf.FloorToInt(Position.x + 0.5f);
			int lowerY = Mathf.FloorToInt(Position.y), upperY = Mathf.FloorToInt(Position.y + 0.5f);

			uniquePositions.Add(new Vector2Int(lowerX, lowerY));
			uniquePositions.Add(new Vector2Int(lowerY, upperY));
			uniquePositions.Add(new Vector2Int(upperX, lowerY));
			uniquePositions.Add(new Vector2Int(upperX, upperY));

			Vector2Int[] finalPositions = new Vector2Int[uniquePositions.Count];
			uniquePositions.CopyTo(finalPositions);
			return finalPositions;
		}

		public override bool Equals(object obj)
		{
			Connection other = (Connection)obj;
			return Position == other.Position;
		}

		public override int GetHashCode()
		{
			return Position.GetHashCode();
		}
	}
}
