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
		public string variant;
		public Tile[] internalLinks;
		public byte externalLinks;

		public static Vector2 LinkDirectionToVector2(LinkDirection dir)
		{
			switch (dir)
			{
			case LinkDirection.Right:
				return Vector2.right;
			case LinkDirection.Up:
				return Vector2.up;
			case LinkDirection.Left:
				return Vector2.left;
			case LinkDirection.Down:
				return Vector2.down;
			default:
				return Vector2.zero;
			}
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
			internalLinks = new Tile[4];
			externalLinks = 0x0;
		}

		public void Drag(Vector2 dPos)
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
			Rect connectorBox = new Rect (Vector2.zero, bounds.size / 8f);

			//right
			connectorBox.center = new Vector2(bounds.xMax - connectorBox.width * 2, bounds.y + bounds.height/2);
			GUI.Box (connectorBox, "");

			//up
			connectorBox.center = new Vector2 (bounds.x, bounds.yMin);
			GUI.Box (connectorBox, "");

			//left
			connectorBox.center = new Vector2 (bounds.xMin, bounds.y);
			GUI.Box (connectorBox, "");

			//down
			connectorBox.center = new Vector2 (bounds.x, bounds.yMax);
			GUI.Box (connectorBox, "");
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
					Drag (e.delta);
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
						ClearLinks ();
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
				ClearLinks ();
				if (removed != null)
					removed.Invoke (this);
			});
			menu.ShowAsContext ();
		}

		public bool SetLink(LinkDirection dir, Tile other)
		{
			if (other != null
				&& (other.bounds.position - bounds.position).magnitude > bounds.width)
				return false;

			if (HasLink (dir) || other.HasLink ((LinkDirection)(((int)dir + 2) % 4)))
				return false;

			internalLinks[(int)dir] = other;
			if (other != null)
				other.internalLinks[((int)dir + 2) % 4] = this;

			return true;
		}

		public bool HasLink(LinkDirection dir)
		{
			return internalLinks[(int)dir] == null || ((externalLinks & (1 << (int)dir)) != 0);
		}

		public bool RemoveLink(LinkDirection dir)
		{
			return SetLink (dir, null);
		}

		public void ClearLinks()
		{
			SetLink (LinkDirection.Right, null);
			SetLink (LinkDirection.Up, null);
			SetLink (LinkDirection.Left, null);
			SetLink (LinkDirection.Down, null);
		}
	}
}
