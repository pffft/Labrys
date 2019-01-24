using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class MainWindow : EditorWindow
	{
		[MenuItem("Window/Labrys Feature Editor")]
		private static void OpenWindow()
		{
			MainWindow window = GetWindow<MainWindow> ();
			window.titleContent = new GUIContent ("Labrys Feature Editor");
		}

		private Vector2 drag;
		private EditorGrid grid;

		private void OnEnable()
		{
			EditorGrid grid = EditorGrid.GetInstance();
			grid.viewport = position;
			grid.Recenter();
		}

		private void OnGUI()
		{
			HandleEvent (Event.current);

			grid.Draw ();

			if (GUI.changed)
				Repaint ();
		}

		private void HandleEvent(Event e)
		{
			GUI.changed = grid.HandleEvent (e);

			switch (e.type)
			{
			//open context menu
			case EventType.MouseDown:
				if (e.button == 1)
				{
					HandleContextMenu (e.mousePosition);
				}
				break;
			case EventType.KeyDown:
				if (e.control)
				{
					if (e.keyCode == KeyCode.Z)
					{
						//TODO undo
					}
					else if (e.keyCode == KeyCode.Y)
					{
						//TODO redo
					}
				}
				break;
			default:
				return;
			}
		}

		private void HandleContextMenu(Vector2 mousePos)
		{
			GenericMenu menu = new GenericMenu ();
			menu.AddItem(new GUIContent("Recenter view"), false, () => { grid.Recenter (); });
			menu.AddSeparator ("");
			menu.AddItem(new GUIContent("Add tile"), false, () => grid.CreateTile(mousePos));
			menu.ShowAsContext ();
		}
	}
}
