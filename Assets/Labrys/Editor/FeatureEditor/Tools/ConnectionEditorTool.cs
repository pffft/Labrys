using Labrys.Editor.FeatureEditor.Panels;
using Labrys.FeatureEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Labrys.Editor.FeatureEditor.Tools
{
    public class ConnectionEditorTool : Tool
    {
        private HashSet<Vector2Int> manipPositions;
        private Color previewColor;

        private bool settingExternal = false;

        public ConnectionEditorTool(EditorWindow window) : base(window)
        {
            manipPositions = new HashSet<Vector2Int>();
            previewColor = Color.white;

            Name = "Connection Editor";
        }

        public override void Draw()
        {
            Handles.color = previewColor;
            Vector3 rotStartDir = settingExternal ? Vector3.right : Vector3.down;
            Handles.BeginGUI();
            foreach (Vector2Int position in manipPositions)
            {
                Vector2 screenPos = EditorGrid.GetInstance().GridToScreenPos(position);
                Handles.DrawSolidArc(new Vector3(screenPos.x, screenPos.y), Vector3.forward, rotStartDir, 180f, EditorGrid.GetInstance().scale * EditorGrid.LINK_SIZE);
            }
            Handles.EndGUI();
        }

        public override bool HandleEvent(Event e)
        {
            switch (e.type)
            {
            case EventType.MouseDown:
                settingExternal = (IsPrimaryControl(e) || IsSecondaryControl(e)) && e.control;
                goto case EventType.MouseDrag;
            case EventType.MouseDrag:
                if (IsPrimaryControl(e) || IsSecondaryControl(e))
                {
                    Vector2Int position = EditorGrid.GetInstance().ScreenToGridPos(e.mousePosition);
                    if (FeatureEditorWindow.GetInstance().Feature.HasLinkAt(position))
                    {
                        manipPositions.Add(position);
                        if(settingExternal)
                        {
                            previewColor = Color.yellow;
                        }
                        else if (IsPrimaryControl(e))
                        {
                            previewColor = Color.green;
                        }
                        else if (IsSecondaryControl(e))
                        {
                            previewColor = Color.red;
                        }
                        return true;
                    }
                }
                break;
            case EventType.MouseUp:
                if (IsPrimaryControl(e) || IsSecondaryControl(e))
                {
                    FeatureAsset feature = FeatureEditorWindow.GetInstance().Feature;
                    string description;
                    UnityAction action;

                    bool targetState = IsPrimaryControl(e) ? true : IsSecondaryControl(e) ? false : false;
                    if (settingExternal)
                    {
                        description = targetState ? "Set connection(s) to external" : "Set connections(s) to internal";
                        action = () => {
                            foreach (Vector2Int position in manipPositions)
                            {
                                if (feature.TryGetLink(position, out Link link))
                                {
                                    link.external = targetState;
                                }
                            }
                        };
                    }
                    else
                    {
                        description = targetState ? "Set connection(s) to open" : "Set connections(s) to closed";
                        action = () => {
                            foreach (Vector2Int position in manipPositions)
                            {
                                if (feature.TryGetLink(position, out Link link))
                                {
                                    link.open = targetState;
                                }
                            }
                        };
                    }

                    ChangeAsset(feature, description, action);

                    e.Use();
                    manipPositions.Clear();
                    return true;
                }
                break;
            }
            return false;
        }
    }
}
