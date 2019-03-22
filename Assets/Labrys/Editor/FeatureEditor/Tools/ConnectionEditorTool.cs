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

		private ConnectionEditorPanel panel;

		private bool settingExternal = false;

		public ConnectionEditorTool(EditorWindow window) : base(window)
		{
			manipPositions = new HashSet<Vector2Int>();
			previewColor = Color.white;
			panel = new ConnectionEditorPanel(window, InternalPanel.DockPosition.right, 150f);

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

			panel.Draw();
		}

		private bool isPrimaryControl(Event e)
		{
			return (e.isMouse && e.button == 0);
		}

		private bool isSecondaryControl(Event e)
		{
			return (e.isMouse && e.button == 1);
		}

		public override bool HandleEvent(Event e)
		{
			if (panel.HandleEvent(e))
				return true;

			switch (e.type)
			{
			case EventType.MouseDown:
				settingExternal = (isPrimaryControl(e) || isSecondaryControl(e)) && e.control;
				goto case EventType.MouseDrag;
			case EventType.MouseDrag:
				if (isPrimaryControl(e) || isSecondaryControl(e))
				{
					Vector2Int position = EditorGrid.GetInstance().ScreenToGridPos(e.mousePosition);
					if (FeatureEditorWindow.GetInstance().Feature.HasLinkAt(position))
					{
						manipPositions.Add(position);
						if(settingExternal)
						{
							previewColor = Color.yellow;
						}
						else if (isPrimaryControl(e))
						{
							previewColor = Color.green;
						}
						else if (isSecondaryControl(e))
						{
							previewColor = Color.red;
						}
						return true;
					}
				}
				break;
			case EventType.MouseUp:
				if (isPrimaryControl(e) || isSecondaryControl(e))
				{
					FeatureAsset feature = FeatureEditorWindow.GetInstance().Feature;
					string description;
					UnityAction action;

					bool targetState = isPrimaryControl(e) ? true : isSecondaryControl(e) ? false : false;
					if (settingExternal)
					{
						description = targetState ? "Set connection(s) to external" : "Set connections(s) to internal";
						action = () => {
							foreach (Vector2Int position in manipPositions)
							{
								if (feature.TryGetLink(position, out FeatureAsset.Link link))
								{
									link.External = targetState;
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
								if (feature.TryGetLink(position, out FeatureAsset.Link link))
								{
									link.Open = targetState;
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
