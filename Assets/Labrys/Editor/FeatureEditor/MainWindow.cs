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

		private void OnEnable()
		{
			EditorGrid.GetInstance().viewport = position;
			EditorGrid.GetInstance().Recenter();
		}

		private void OnGUI()
		{
			EditorGrid.GetInstance().viewport = position;
			HandleEvent (Event.current);

			EditorGrid.GetInstance().Draw ();

			EditorGUI.DrawRect(new Rect(0f, 0f, position.width, 22f), GUI.color);
			if(GUI.Button(new Rect(0f, 0f, 64f, 20f), "File"))
			{
				GenericMenu fileMenu = new GenericMenu();
				fileMenu.AddItem(new GUIContent("New Feature..."), false, CreateNewFeature);
				fileMenu.AddItem(new GUIContent("Open Feature..."), false, OpenFeature);
				fileMenu.AddItem(new GUIContent("Save"), false, SaveFeature);
				fileMenu.AddItem(new GUIContent("Save As..."), false, SaveAsFeature);

				fileMenu.ShowAsContext();
			}

			if (GUI.Button(new Rect(70f, 0f, 64f, 20f), "Edit"))
			{
				GenericMenu editMenu = new GenericMenu();
				editMenu.AddDisabledItem(new GUIContent("TODO"));

				editMenu.ShowAsContext();
			}

			if(GUI.Button(new Rect(140f, 0f, 64f, 20f), "View"))
			{
				GenericMenu viewMenu = new GenericMenu();
				viewMenu.AddDisabledItem(new GUIContent("TODO"));

				viewMenu.ShowAsContext();
			}

			if (GUI.changed)
				Repaint ();
		}

		private void HandleEvent(Event e)
		{
			GUI.changed = EditorGrid.GetInstance().HandleEvent (e);

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

		private void CreateNewFeature()
		{
			Debug.Log("TODO");
		}

		private void OpenFeature()
		{
			Debug.Log("TODO");
		}

		private void SaveFeature()
		{
			Debug.Log("TODO");
		}

		private void SaveAsFeature()
		{
			Debug.Log("TODO");
		}

		private void HandleContextMenu(Vector2 mousePos)
		{
			GenericMenu menu = new GenericMenu ();
			menu.AddItem(new GUIContent("Recenter view"), false, () => { EditorGrid.GetInstance().Recenter (); });
			menu.AddSeparator ("");
			menu.AddItem(new GUIContent("Add tile"), false, () => EditorGrid.GetInstance().CreateTile(mousePos));
			menu.ShowAsContext ();
		}
	}
}
