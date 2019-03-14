using Labrys.FeatureEditor;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class EditorGrid
	{
		public static Color lineColor = new Color(0.4f, 0.4f, 0.4f);

		private static Color selectedColor = new Color(0.5f, 0.5f, 0.8f);

		public const float LINK_SIZE = 10f;
		private static Color openColor = new Color(0f, 0.8f, 0f);
		private static Color closedColor = new Color(0.9f, 0f, 0f);
		private static Color externalColor = new Color(0.8f, 0.8f, 0f);

		private static EditorGrid instance;

		public Rect viewport;

		public float scale = 1f;
		public float lineSpacing = 64f;

		private Vector2 offset;
		private Vector2 drag;

		public FeatureAsset Feature { get; set; }

		public static EditorGrid GetInstance()
		{
			if(instance == null)
			{
				instance = new EditorGrid();
			}
			return instance;
		}

		public EditorGrid()
		{
			viewport = new Rect();

			offset = Vector2.zero;
		}

		public void Draw()
		{
			Color lightLineColor = new Color (lineColor.r, lineColor.g, lineColor.b, lineColor.a * 0.5f);

			float scaledSpacing = lineSpacing * scale;

			int xLineCount = Mathf.CeilToInt(viewport.width / scaledSpacing);
			int yLineCount = Mathf.CeilToInt (viewport.height / scaledSpacing);

			Vector2 wrappedOffset = new Vector2 (
				Mathf.Abs (offset.x) % scaledSpacing * Mathf.Sign (offset.x), 
				Mathf.Abs (offset.y) % scaledSpacing * Mathf.Sign (offset.y));
			Vector2Int linesOffset = ScreenToGridPos (offset);

			Handles.BeginGUI ();
			for (int i = 0; i < xLineCount + 1; i++)
			{
				int xGridLine = ScreenToGridPos(new Vector2(scaledSpacing * i + wrappedOffset.x, 0f)).x;
				if (xGridLine % 10 == 0)
					Handles.color = xGridLine == 0 ? Color.black : lineColor;
				else
					Handles.color = lightLineColor;
				Handles.DrawLine (
					new Vector2 (scaledSpacing * i, -scaledSpacing) + wrappedOffset, 
					new Vector2 (scaledSpacing * i, viewport.height + scaledSpacing) + wrappedOffset);
			}

			for (int i = 0; i < yLineCount + 1; i++)
			{
				int yGridLine = ScreenToGridPos(new Vector2(0f, scaledSpacing * i + wrappedOffset.y)).y;
				if (yGridLine % 10 == 0)
					Handles.color = yGridLine == 0 ? Color.black : lineColor;
				else
					Handles.color = lightLineColor;
				Handles.DrawLine (
					new Vector2 (-scaledSpacing, scaledSpacing * i) + wrappedOffset, 
					new Vector2 (viewport.width + scaledSpacing, scaledSpacing * i) + wrappedOffset);
			}
			Handles.EndGUI ();

			DrawObjects ();
		}

		private void DrawObjects()
		{
			if (Feature == null)
				return;

			Vector2 screenPosition = Vector2.zero;
			Rect bounds = new Rect(screenPosition, GetScaledTileSize());

			// Draw Sections
			foreach(KeyValuePair<Vector2Int, FeatureAsset.Section> section in Feature.GetSections())
			{
				screenPosition = GridToScreenPos(section.Key);				bounds.position = screenPosition;
				bounds.center = screenPosition;
				Color temp = GUI.color;
				GUI.color = Feature.IsSelected(section.Key) ? selectedColor : Color.white;
				GUI.Box(bounds, "");
				GUI.color = temp;
			}

			// Draw Links
			foreach(KeyValuePair<Vector2Int, FeatureAsset.Link> link in Feature.GetLinks())
			{
				screenPosition = GridToScreenPos(link.Key);
				Handles.color = link.Value.Open ? openColor : closedColor;
				Handles.BeginGUI();
				Handles.DrawSolidDisc(new Vector3(screenPosition.x, screenPosition.y), Vector3.forward, scale * LINK_SIZE);
				if (link.Value.External)
				{
					Handles.color = externalColor;
					Handles.DrawSolidArc(new Vector3(screenPosition.x, screenPosition.y), Vector3.forward, Vector3.left, 180f, scale * LINK_SIZE);
				}
				Handles.EndGUI();
			}
		}

		public bool HandleEvent(Event e)
		{
			drag = Vector2.zero;

			bool guiChanged = false;
			switch (e.type)
			{
			case EventType.MouseDrag:
				//drag the grid and the tiles
				if (e.button == 2)
				{
					Shift(e.delta);
					guiChanged = true;
				}
				break;
			case EventType.ScrollWheel:
				if (Mathf.Sign(e.delta.y) > 0)
					Resize(scale / 1.1f);
				else
					Resize(scale * 1.1f);
				guiChanged = true;
				break;
			}

			return guiChanged;
		}

		private void Shift(Vector2 dPos)
		{
			drag = dPos;
			offset += drag;
		}

		public void Recenter()
		{
			offset = Vector2.zero;

			Shift(viewport.size / 2f);
		}

		public void Resize(float scale)
		{
			float dScale = scale / this.scale;
			offset -= viewport.size / 2f;
			offset *= dScale;
			offset += viewport.size / 2f;

			this.scale = scale;
		}

		public Vector2 GetBaseTileSize()
		{
			return new Vector2(lineSpacing, lineSpacing);
		}

		public Vector2 GetScaledTileSize()
		{
			return new Vector2(lineSpacing, lineSpacing) * scale;
		}

		/// <summary>
		/// Transforms a screen position to a grid position (1:1).
		/// </summary>
		/// <param name="screenPos"></param>
		/// <returns></returns>
		public Vector2 ScreenToGridSpace(Vector2 screenPos, bool evenOnly = false)
		{
			Vector2 gridPos = new Vector2(screenPos.x - offset.x, screenPos.y - offset.y) / (lineSpacing * scale);
			if(evenOnly)
			{
				gridPos = new Vector2(Mathf.Round(gridPos.x), Mathf.Round(gridPos.y));
			}
			return gridPos * FeatureAsset.GRID_DENSITY;
		}

		/// <summary>
		/// Transforms a screen position to a specific grid position that can be used to identify a tile (n:1).
		/// </summary>
		/// <param name="screenPos"></param>
		/// <returns></returns>
		public Vector2Int ScreenToGridPos(Vector2 screenPos, bool evenOnly = false)
		{
			Vector2 gridSpacePos = ScreenToGridSpace(screenPos, evenOnly);
			return new Vector2Int(Mathf.RoundToInt(gridSpacePos.x), Mathf.RoundToInt(gridSpacePos.y));
		}

		/// <summary>
		/// Transforms a grid position into a corresponding screen position (1:1).
		/// </summary>
		/// <param name="gridPos"></param>
		/// <returns></returns>
		public Vector2 GridToScreenSpace(Vector2 gridPos)
		{
			return new Vector2(
				(gridPos.x * lineSpacing * scale / FeatureAsset.GRID_DENSITY) + offset.x,
				(gridPos.y * lineSpacing * scale / FeatureAsset.GRID_DENSITY) + offset.y);
		}

		/// <summary>
		/// Same as GridToScreenSpace(Vector2), but takes a Vector2Int.
		/// </summary>
		/// <param name="gridPos"></param>
		/// <returns></returns>
		public Vector2 GridToScreenPos(Vector2Int gridPos)
		{
			return GridToScreenSpace(gridPos);
		}

		public Vector2Int[] RectToGridPositions(Rect r, bool evenOnly = false)
		{
			Vector2Int startingGridPos = ScreenToGridPos(r.min, evenOnly);
			Vector2Int endingGridPos = ScreenToGridPos(r.max, evenOnly);

			Vector2Int selectionAreaDimen = endingGridPos - startingGridPos;
			int xIncr = (evenOnly ? (int)FeatureAsset.GRID_DENSITY : 1) * (int)Mathf.Sign(selectionAreaDimen.x);
			int yIncr = (evenOnly ? (int)FeatureAsset.GRID_DENSITY : 1) * (int)Mathf.Sign(selectionAreaDimen.y);

			List<Vector2Int> gridPositons = new List<Vector2Int>();
			for (int x = startingGridPos.x; x != endingGridPos.x + xIncr; x += xIncr)
			{
				for (int y = startingGridPos.y; y != endingGridPos.y + yIncr; y += yIncr)
				{
					gridPositons.Add(new Vector2Int(x, y));
				}
			}
			return gridPositons.ToArray();
		}
	}
}
