using Labrys.Editor.FeatureEditor.Commands;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Tools
{
	public class SelectionTool : Tool
	{
		private const int SELECT_MODE_NONE = 0;
		private const int SELECT_MODE_SELECT = 1;
		private const int SELECT_MODE_DESELECT = 2;

		private Rect selectionRect;
		private bool isDragging;
		private int selectionMode;

		public SelectionTool(EditorWindow window) : base(window)
		{
			selectionRect = new Rect();

			Name = "Select";
		}

		public override void Draw()
		{
			Color temp = GUI.color;
			GUI.color = new Color(0.5f, 0.5f, 0.8f, 0.5f);
			GUI.Box(selectionRect, "");
			GUI.color = temp;
		}

		public override bool HandleEvent(Event e)
		{
			switch(e.type)
			{
			case EventType.MouseDown:
				if(e.button == 0)
				{
					//start selection box
					selectionRect.position = e.mousePosition;
					e.Use();
					selectionMode = SELECT_MODE_SELECT;
					return true;
				}
				else if (e.button == 1)
				{
					//start selection box
					selectionRect.position = e.mousePosition;
					e.Use();
					selectionMode = SELECT_MODE_DESELECT;
					return true;
				}
				break;
			case EventType.MouseDrag:
				if(e.button == 0 || e.button == 1)
				{
					//update selection box
					selectionRect.max = e.mousePosition;
					e.Use();
					return true;
				}
				break;
			case EventType.MouseUp:
				if(e.button == 0 || e.button == 1)
				{
					//select all tiles within selection box
					Command c;
					if (selectionMode == SELECT_MODE_SELECT)
					{
						c = new SelectionCommand()
						{
							SelectedPositions = EditorGrid.GetInstance().RectToGridPositions(selectionRect, true)
						};
					}
					else if (selectionMode == SELECT_MODE_DESELECT)
					{
						c = new DeselectionCommand()
						{
							SelectedPositions = EditorGrid.GetInstance().RectToGridPositions(selectionRect, true)
						};
					}
					else
					{
						return false;
					}
					c.Do();
					History.RecordCommand(c);
					selectionRect = new Rect();
					selectionMode = SELECT_MODE_NONE;
					e.Use();
					return true;
				}
				break;
			}
			return false;
		}
	}
}
