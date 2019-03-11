using Labrys.Editor.FeatureEditor.Tools;
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

		private SelectionTool selectTool = new SelectionTool();
		private TilePaintTool tilePaintTool = new TilePaintTool();
		private ConnectionEditorTool connectionEditorTool = new ConnectionEditorTool();

		private Tool activeTool;

		public MainWindow()
		{
			activeTool = tilePaintTool;
		}

		private void OnEnable()
		{
			EditorGrid.GetInstance().viewport = position;
			EditorGrid.GetInstance().Recenter();
		}

		private void OnGUI()
		{
			EditorGrid.GetInstance().viewport = position;
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
				bool hasUndo = History.NextUndo != null;
				editMenu.AddItem(new GUIContent("Undo " + (hasUndo ? History.NextUndo.Description : "")), hasUndo, ()=> { History.Undo(); });
				editMenu.AddItem(new GUIContent("Redo ???"), History.CanRedo(), () => { History.Redo(); });
				editMenu.AddDisabledItem(new GUIContent("TODO"));

				editMenu.ShowAsContext();
			}

			if(GUI.Button(new Rect(140f, 0f, 64f, 20f), "View"))
			{
				GenericMenu viewMenu = new GenericMenu();
				viewMenu.AddDisabledItem(new GUIContent("TODO"));

				viewMenu.ShowAsContext();
			}

			activeTool.Draw();

			HandleEvent(Event.current);

			if (GUI.changed)
				Repaint ();
		}

		private void HandleEvent(Event e)
		{
			if (EditorGrid.GetInstance().HandleEvent(e))
				GUI.changed = true;

			if (activeTool.HandleEvent(e))
				GUI.changed = true;

			switch (e.type)
			{
			case EventType.KeyDown:
				if (e.control)
				{
					/* Currently an issue with getting undos to work with Unity's existing system.
					 * TODO: figure that shit out
					if (e.keyCode == KeyCode.Z)
					{
						History.Undo();
						e.Use();
					}
					else if (e.keyCode == KeyCode.Y)
					{
						History.Redo();
						e.Use();
					}
					*/
				}
				else
				{
					if(e.keyCode == KeyCode.S)
					{
						activeTool = selectTool;
					}
					else if(e.keyCode == KeyCode.A)
					{
						activeTool = tilePaintTool;
					}
					else if (e.keyCode == KeyCode.C)
					{
						activeTool = connectionEditorTool;
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
	}
}
