﻿using Labrys.Editor.FeatureEditor.Commands;
using Labrys.Editor.FeatureEditor.Panels;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Tools
{
	public class ConnectionEditorTool : Tool
	{
		private HashSet<Vector2Int> manipPositions;
		private Color previewColor;

		private ConnectionEditorPanel panel;

		public ConnectionEditorTool(EditorWindow window) : base(window)
		{
			manipPositions = new HashSet<Vector2Int>();
			previewColor = Color.white;
			panel = new ConnectionEditorPanel(window, InternalPanel.DockPosition.right, 150f);
		}

		public override void Draw()
		{
			Handles.color = previewColor;
			Handles.BeginGUI();
			foreach (Vector2Int position in manipPositions)
			{
				Vector2 screenPos = EditorGrid.GetInstance().GridToScreenPos(position);
				Handles.DrawSolidDisc(new Vector3(screenPos.x, screenPos.y), Vector3.forward, EditorGrid.GetInstance().scale * Connection.SIZE);
			}
			Handles.EndGUI();

			panel.Draw();
		}

		public override bool HandleEvent(Event e)
		{
			if (panel.HandleEvent(e))
				return true;

			switch (e.type)
			{
			case EventType.MouseDown:
			case EventType.MouseDrag:
				if (e.button == 0 || e.button == 1)
				{
					Vector2Int position = EditorGrid.GetInstance().ScreenToGridPos(e.mousePosition);
					if (EditorGrid.GetInstance().HasConnectionAt(position))
					{
						manipPositions.Add(position);
						if (e.button == 0)
						{
							previewColor = Color.green;
						}
						else if (e.button == 1)
						{
							previewColor = Color.red;
						}
						return true;
					}
				}
				break;
			case EventType.MouseUp:
				if (e.button == 0 || e.button == 1)
				{
					Vector2Int[] finalPositions = new Vector2Int[manipPositions.Count];
					manipPositions.CopyTo(finalPositions);
					Command c = new ConnectionToggleCommand(finalPositions) { TargetState = e.button == 0 ? true : e.button == 1 ? false : false };
					c.Do();
					History.RecordCommand(c);
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
