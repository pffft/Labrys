using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Panels
{
	public class ToolBox : InternalPanel
	{
		private List<Tool> tools;
		private int selectedTool;

		public Tool ActiveTool { get { return tools[selectedTool]; } }

		public ToolBox(EditorWindow window, DockPosition alignment, float scale) : base(window, alignment, scale)
		{
			tools = new List<Tool>();
			selectedTool = 0;
		}

		public override void Draw()
		{
			base.Draw();
			selectedTool = GUI.SelectionGrid(bounds, selectedTool, GetToolNames(), 1);
		}

		public void AddTool(Tool t)
		{
			tools.Add(t);
		}

		public bool RemoveTool(Tool t)
		{
			return tools.Remove(t);
		}

		private string[] GetToolNames()
		{
			string[] names = new string[tools.Count];
			for(int i = 0; i < tools.Count; i++)
			{
				names[i] = tools[i].Name;
			}

			return names;
		}
	}
}
