using Labrys.Editor.FeatureEditor.Panels;
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

		private ToolBox toolBox;

		public MainWindow()
		{
			toolBox = new ToolBox(this, InternalPanel.DockPosition.left, 150f);
			toolBox.AddTool(new SelectionTool(this));
			toolBox.AddTool(new TilePaintTool(this));
			toolBox.AddTool(new ConnectionEditorTool(this));
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
			toolBox.ActiveTool.Draw();

			toolBox.Draw();

			HandleEvent(Event.current);

			if (GUI.changed)
				Repaint ();
		}

		private void HandleEvent(Event e)
		{
			if (toolBox.HandleEvent(e))
			{
				GUI.changed = true;
				return;
			}

			if (toolBox.ActiveTool.HandleEvent(e))
			{
				GUI.changed = true;
				return;
			}

			if (EditorGrid.GetInstance().HandleEvent(e))
			{
				GUI.changed = true;
				return;
			}

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

				}
				break;
			default:
				return;
			}
		}
	}
}
