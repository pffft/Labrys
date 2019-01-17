using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class Connection
	{
		public bool Open { get; private set; }
		public bool External { get; private set; }

		public Vector2 Position { get; set; }
		public Vector2 DrawPosition { get; set; }

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

		public Connection(Vector2 position)
		{
			Position = position;
			Open = true;
			External = false;
		}

		public void Draw()
		{
			GUI.Box(new Rect(DrawPosition, new Vector2(1f, 1f)), "");
			Handles.color = Open ? Color.green : Color.red;
			Handles.BeginGUI();
			Handles.DrawWireDisc(new Vector3(DrawPosition.x, DrawPosition.y), Vector3.forward, 1f);
			Handles.EndGUI();
		}

		public bool HandleEvent(Event e)
		{
			switch(e.type)
			{
			case EventType.MouseDown:
				if(e.button == 0)
				{
					//open connection
					Open = true;
					return true;
				}
				else if(e.button == 1)
				{
					//block connection
					Open = false;
					return true;
				}
				break;
			}
			return false;
		}

		public IEnumerable<Vector2Int> GetSubjectGridPositions()
		{
			List<Vector2Int> positions = new List<Vector2Int>();
			if(Mathf.FloorToInt(Position.x) != Mathf.CeilToInt(Position.x))
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
