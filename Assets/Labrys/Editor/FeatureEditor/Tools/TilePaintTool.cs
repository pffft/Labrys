using Labrys.FeatureEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Labrys.Editor.FeatureEditor.Tools
{
    public class TilePaintTool : Tool
    {
        private HashSet<Vector2Int> manipPositions;
        private Color previewColor;

        public TilePaintTool(EditorWindow window) : base(window)
        {
            manipPositions = new HashSet<Vector2Int>();
            previewColor = Color.white;

            Name = "Tile Brush";
        }

        public override void Draw()
        {
            //draw tile previews
            base.Draw();
            foreach (Vector2Int position in manipPositions)
            {
                Vector2 screenPos = EditorGrid.GetInstance().GridToScreenPos(position);
                Rect r = new Rect(screenPos, EditorGrid.GetInstance().GetScaledTileSize())
                {
                    center = screenPos
                };
                GUI.color = previewColor;
                GUI.Box(r, "");
            }
        }

        public override bool HandleEvent(Event e)
        {
            switch(e.type)
            {
            case EventType.MouseDown:
            case EventType.MouseDrag:
                if (IsPrimaryControl(e) || IsSecondaryControl(e))
                {
                    Vector2Int position = EditorGrid.GetInstance().ScreenToGridPos(e.mousePosition, true);
                    bool posHasTile = FeatureEditorWindow.GetInstance().Feature.HasSectionAt(position);
                    if ((IsPrimaryControl(e) && !posHasTile) || (IsSecondaryControl(e) && posHasTile))
                    {
                        manipPositions.Add(position);
                        if (IsPrimaryControl(e))
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
                if(IsPrimaryControl(e) || IsSecondaryControl(e))
                {
                    //attempt to place a tile in accumpulated positions
                    FeatureAsset feature = FeatureEditorWindow.GetInstance().Feature;
                    string description;
                    UnityAction action;

                    //add sections
                    if (IsPrimaryControl(e))
                    {
                        description = "Add section(s)";
                        action = () => {
                            foreach (Vector2Int position in manipPositions)
                            {
                                FeatureEditorWindow.GetInstance().Feature.AddSection(position);
                            }
                        };
                    }
                    //remove sections
                    else if (IsSecondaryControl(e))
                    {
                        description = "Remove section(s)";
                        action = () => {
                            foreach (Vector2Int position in manipPositions)
                            {
                                FeatureEditorWindow.GetInstance().Feature.RemoveSection(position);
                            }
                        };
                    }
                    else
                    {
                        return false;
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
