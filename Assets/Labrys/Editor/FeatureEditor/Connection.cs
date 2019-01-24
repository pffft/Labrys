using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public class Connection : GridObject
	{
		private const float SIZE = 10f;

		public bool Open { get; private set; }
		public bool External { get; private set; }

		public event GridObjectAction removed;

		public Connection(Vector2Int position)
		{
			GridPosition = position;
			Open = true;
			External = false;
		}

		public override void Draw()
		{
			Handles.color = Open ? Color.green : Color.red;
			Handles.BeginGUI();
			Handles.DrawSolidDisc(new Vector3(ScreenPosition.x, ScreenPosition.y), Vector3.forward, Scale * SIZE);
			Handles.EndGUI();
		}

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

		public IEnumerable<Vector2Int> GetSubjectGridPositions()
		{
			HashSet<Vector2Int> uniquePositions = new HashSet<Vector2Int>();

			int lowerX = Mathf.FloorToInt(GridPosition.x), upperX = Mathf.FloorToInt(GridPosition.x + 0.5f);
			int lowerY = Mathf.FloorToInt(GridPosition.y), upperY = Mathf.FloorToInt(GridPosition.y + 0.5f);

			uniquePositions.Add(new Vector2Int(lowerX, lowerY));
			uniquePositions.Add(new Vector2Int(lowerY, upperY));
			uniquePositions.Add(new Vector2Int(upperX, lowerY));
			uniquePositions.Add(new Vector2Int(upperX, upperY));

			Vector2Int[] finalPositions = new Vector2Int[uniquePositions.Count];
			uniquePositions.CopyTo(finalPositions);
			return finalPositions;
		}
	}
}
