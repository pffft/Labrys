using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor
{
	public abstract class GridObject
	{
		public static bool operator ==(GridObject left, GridObject right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(GridObject left, GridObject right)
		{
			return !left.Equals(right);
		}

		/// <summary>
		/// Position in grid space; managed by the EditorGrid.
		/// Not changed by visual movement, scaling etc.
		/// </summary>
		public virtual Vector2Int GridPosition { get; set; }
		public Vector2 ScreenPosition
		{
			get
			{
				return EditorGrid.GetInstance().GridToScreenPos(GridPosition) + drawOffset;
			}
		}

		public float Scale
		{
			get
			{
				return EditorGrid.GetInstance().scale;
			}
		}

		//used for visually representing movement during drag operations
		private Vector2 drawOffset;

		/// <summary>
		/// Draw this object to the viewport.
		/// </summary>
		public abstract void Draw();

		/// <summary>
		/// React to a user-driven event. Returns if the event should change the GUI.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public abstract bool HandleEvent(Event e);

		/// <summary>
		/// Apply an offset to the draw position of this 
		/// </summary>
		/// <param name="dPos"></param>
		public void ApplyShift(Vector2 dPos)
		{
			drawOffset += dPos;
		}

		public void RevertShift()
		{
			drawOffset = Vector2.zero;
		}

		public override bool Equals(object obj)
		{
			GridObject other = (GridObject)obj;
			return GridPosition == other.GridPosition;
		}

		public override int GetHashCode()
		{
			return GridPosition.GetHashCode();
		}

		public delegate void GridObjectAction(GridObject t);
	}
}
