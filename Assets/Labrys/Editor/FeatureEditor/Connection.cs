using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class Connection
	{
		public bool Open { get; private set; }
		public bool External { get; private set; }

		private Tile tile1, tile2;
		public Vector2 DrawPosition => new Vector2(
				(tile1.bounds.position.x + tile2.bounds.position.x) / 2f,
				(tile1.bounds.position.y + tile2.bounds.position.y) / 2f);

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

		public Connection(Tile tile1, Tile tile2)
		{
			this.tile1 = tile1;
			this.tile2 = tile2;
			Open = true;
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

		public override bool Equals(object obj)
		{
			Connection other = (Connection)obj;
			return DrawPosition == other.DrawPosition;;
		}

		public override int GetHashCode()
		{
			return DrawPosition.GetHashCode();
		}
	}
}
