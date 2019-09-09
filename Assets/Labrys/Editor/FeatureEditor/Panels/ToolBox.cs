using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Panels
{
    [System.Serializable]
    public class ToolBox : InternalPanel
    {
        [SerializeField]
        private List<Tool> tools;
        [SerializeField]
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
            Rect r = new Rect(bounds);
            r.height = tools.Count * 30;
            r.width -= 10;
            r.x += 5;
            r.y += 5;
            selectedTool = GUI.SelectionGrid(r, selectedTool, GetToolNames(), 1);
        }

        public override bool HandleEvent(Event e)
        {
            switch(e.type)
            {
            case EventType.KeyDown:
                if(e.isKey)
                {
                    if(e.keyCode == KeyCode.Alpha1)
                    {
                        selectedTool = 0;
                    }
                    else if (e.keyCode == KeyCode.Alpha2)
                    {
                        selectedTool = 1;
                    }
                    else if (e.keyCode == KeyCode.Alpha3)
                    {
                        selectedTool = 2;
                    }
                }
                break;
            }

            return bounds.Contains(e.mousePosition);
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
