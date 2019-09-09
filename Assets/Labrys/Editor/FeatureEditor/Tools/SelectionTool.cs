using Labrys.FeatureEditor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Labrys.Editor.FeatureEditor.Tools
{
    public class SelectionTool : Tool
    {
        private const int SELECT_MODE_NONE = 0;
        private const int SELECT_MODE_SELECT = 1;
        private const int SELECT_MODE_DESELECT = 2;

        private Rect selectionRect;
        private int selectionMode;

        private Panels.TileEditorPanel tileEditorPanel;

        public SelectionTool(EditorWindow window) : base(window)
        {
            selectionRect = new Rect();
            selectionMode = SELECT_MODE_NONE;

            tileEditorPanel = new Panels.TileEditorPanel(window, InternalPanel.DockPosition.right, 250f);

            Name = "Tile Inspector";
        }

        public override void Draw()
        {
            Color temp = GUI.color;
            GUI.color = new Color(0.5f, 0.5f, 0.8f, 0.5f);
            GUI.Box(selectionRect, "");
            GUI.color = temp;

            tileEditorPanel.Draw();
        }

        public override bool HandleEvent(Event e)
        {
            if (tileEditorPanel.HandleEvent(e))
                return true;

            switch(e.type)
            {
            case EventType.MouseDown:
                if(IsPrimaryControl(e))
                {
                    //start selection box
                    selectionRect.position = e.mousePosition;
                    e.Use();
                    selectionMode = SELECT_MODE_SELECT;
                    return true;
                }
                else if (IsSecondaryControl(e))
                {
                    //start selection box
                    selectionRect.position = e.mousePosition;
                    e.Use();
                    selectionMode = SELECT_MODE_DESELECT;
                    return true;
                }
                break;
            case EventType.MouseDrag:
                if(IsPrimaryControl(e) || IsSecondaryControl(e))
                {
                    //update selection box
                    selectionRect.max = e.mousePosition;
                    e.Use();
                    return true;
                }
                break;
            case EventType.MouseUp:
                if(IsPrimaryControl(e) || IsSecondaryControl(e))
                {
                    //select all tiles within selection box
                    string description;
                    UnityAction action;
                    Vector2Int[] positions = EditorGrid.GetInstance().RectToGridPositions(selectionRect, true);
                    FeatureAsset feature = FeatureEditorWindow.GetInstance().Feature;
                    if (selectionMode == SELECT_MODE_SELECT)
                    {
                        description = "Select section(s)";
                        action = () => {
                            if (!e.shift)
                            {
                                //not additive
                                feature.DeselectAllSections();
                            }
                            foreach (Vector2Int gp in positions)
                            {
                                //additive
                                feature.SelectSection(gp);
                            }
                        };
                    }
                    else if (selectionMode == SELECT_MODE_DESELECT)
                    {
                        description = "Deselect section(s)";
                        action = () => {
                            if (!e.shift)
                            {
                                //not additive
                                feature.DeselectAllSections();
                            }
                            else
                            {
                                //additive
                                foreach (Vector2Int gp in positions)
                                {
                                    feature.DeselectSection(gp);
                                }
                            }
                        };
                    }
                    else
                    {
                        description = "ERROR";
                        action = null;
                    }

                    ChangeAsset(feature, description, action);

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
