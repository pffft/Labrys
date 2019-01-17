using System.Collections.Generic;
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

		
		public GUIStyle defaultTileStyle;
		public GUIStyle selectedTileStyle;

		private Vector2 drag;
		private EditorGrid grid;

		private void OnEnable()
		{
			defaultTileStyle = new GUIStyle ();
			defaultTileStyle.normal.background = (Texture2D)EditorGUIUtility.Load ("builtin skins/darkskin/images/node1.png");
			defaultTileStyle.border = new RectOffset (1, 1, 1, 1);

			selectedTileStyle = new GUIStyle ();
			selectedTileStyle.normal.background = (Texture2D)EditorGUIUtility.Load ("builtin skins/darkskin/images/node1 on.png");
			selectedTileStyle.border = new RectOffset (1, 1, 1, 1);

			grid = new EditorGrid (this, 64, new Color(0.4f, 0.4f, 0.4f));
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
			case EventType.ScrollWheel:
				if (Mathf.Sign (e.delta.y) > 0)
					grid.Resize (grid.scale / 1.1f);
				else
					grid.Resize (grid.scale * 1.1f);
				GUI.changed = true;
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
