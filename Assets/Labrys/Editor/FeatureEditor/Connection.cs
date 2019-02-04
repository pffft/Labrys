using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class Connection : GridObject
	{
		private const float SIZE = 10f;
		private static Color openColor = new Color(0f, 0.8f, 0f);
		private static Color closedColor = new Color(0.9f, 0f, 0f);

		public bool Open { get; private set; }
		public bool External { get; private set; }

		public Connection(Vector2Int position)
		{
			GridPosition = position;
			Open = true;
			External = false;
		}

		public override void Draw()
		{
			Handles.color = Open ? openColor : closedColor;
			Handles.BeginGUI();
			Handles.DrawSolidDisc(new Vector3(ScreenPosition.x, ScreenPosition.y), Vector3.forward, Scale * SIZE);
			Handles.EndGUI();
		}

		[System.Obsolete]
		public override bool HandleEvent(Event e)
		{
			if (Vector2.Distance(e.mousePosition, ScreenPosition) < Scale * SIZE)
			{
				switch (e.type)
				{
				case EventType.MouseDown:
					if (e.button == 1)
					{
						//toggle connection
						Open = !Open;
						e.Use();
						return true;
					}
					break;
				}
			}
			return false;
		}

		public void GetMinMaxSubjectTileCount(out int min, out int max)
		{
			if (GridPosition.x % 2 != 0)
			{
				if (GridPosition.y % 2 != 0)
				{
					min = max = 4;
				}
				else
				{
					min = 1;
					max = 2;
				}
			}
			else if (GridPosition.y % 2 != 0)
			{
				min = 1;
				max = 2;
			}
			else
			{
				min = max = -1;
			}
		}

		public IEnumerable<Vector2Int> GetSubjectTileGridPositions()
		{
			HashSet<Vector2Int> uniquePositions = new HashSet<Vector2Int>();

			int lowerX = Mathf.FloorToInt((GridPosition.x / EditorGrid.GRID_DENSITY)) * (int)EditorGrid.GRID_DENSITY;
			int upperX = Mathf.FloorToInt((GridPosition.x / EditorGrid.GRID_DENSITY) + 0.5f) * (int)EditorGrid.GRID_DENSITY;
			int lowerY = Mathf.FloorToInt((GridPosition.y / EditorGrid.GRID_DENSITY)) * (int)EditorGrid.GRID_DENSITY;
			int upperY = Mathf.FloorToInt((GridPosition.y / EditorGrid.GRID_DENSITY) + 0.5f) * (int)EditorGrid.GRID_DENSITY;

			uniquePositions.Add(new Vector2Int(lowerX, lowerY));
			uniquePositions.Add(new Vector2Int(lowerX, upperY));
			uniquePositions.Add(new Vector2Int(upperX, lowerY));
			uniquePositions.Add(new Vector2Int(upperX, upperY));

			Vector2Int[] finalPositions = new Vector2Int[uniquePositions.Count];
			uniquePositions.CopyTo(finalPositions);
			return finalPositions;
		}
	}
}
