using Labrys.Editor.FeatureEditor.Commands;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Tools
{
	public class SelectionTool : Tool
	{
		private Rect selectionRect;

		public SelectionTool()
		{
			selectionRect = new Rect();
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
					return true;
				}
				break;
			case EventType.MouseDrag:
				if(e.button == 0)
				{
					//update selection box
					selectionRect.max = e.mousePosition;
					e.Use();
					return true;
				}
				break;
			case EventType.MouseUp:
				if(e.button == 0)
				{
					//select all tiles within selection box

					selectionRect = new Rect();
					return true;
				}
				break;
			}
			return false;
		}

		public override Command Use()
		{
			return new SelectionCommand()
			{
				SelectedTiles = new List<Tile>()
			};
		}
	}
}
