using Labrys.FeatureEditor;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Panels
{
	public class TileEditorPanel : InternalPanel
	{
		private string variant;

		public TileEditorPanel(EditorWindow window, DockPosition alignment, float scale) : base(window, alignment, scale)
		{
			variant = "";
		}

		public override bool HandleEvent(Event e)
		{
			const float padding = 5;
			FeatureAsset feature = FeatureEditorWindow.GetInstance().Feature;

			Rect internalBounds = new Rect(bounds);
			internalBounds.width -= 10;
			internalBounds.x += padding;
			internalBounds.y += padding;
			internalBounds.height = 20;
			int selectionCount = feature.GetSelectedCount();
			GUI.Box(internalBounds, selectionCount + (selectionCount == 1 ? " tile selected" : " tiles selected"));

			internalBounds.y += 25;

			internalBounds.width /= 4;
			internalBounds.x = bounds.xMin + padding;
			GUI.Box(internalBounds, "Variant");

			internalBounds.width *= 3;
			internalBounds.x = bounds.xMax - padding - internalBounds.width;
			string compoundV = GetVariant();
			if (compoundV == null)
			{
				variant = "-";
			}
			else
			{
				variant = compoundV;
			}
			variant = EditorGUI.TextField(internalBounds, variant);
			if (variant != "-")
			{
				Undo.RegisterCompleteObjectUndo(feature, "Change variant");
				SetVariant(variant);
				EditorUtility.SetDirty(feature);
			}

			return bounds.Contains(e.mousePosition);
		}

		private string GetVariant()
		{
			string compoundVariant = null;
			FeatureAsset feature = FeatureEditorWindow.GetInstance().Feature;
			Vector2Int[] selectedPositions = feature.GetSelectedSections();
			foreach(Vector2Int position in selectedPositions)
			{
				if (feature.TryGetSection(position, out FeatureAsset.Section section))
				{
					if (compoundVariant == null)
					{
						//first variant found
						compoundVariant = section.Variant;
					}
					else if (compoundVariant != section.Variant)
					{
						//variant names are not uniform
						return null;
					}
					//variant names are uniform so far, continue
				}
			}
			return compoundVariant;
		}

		private void SetVariant(string variant)
		{
			FeatureAsset feature = FeatureEditorWindow.GetInstance().Feature;
			Vector2Int[] selectedPositions = feature.GetSelectedSections();
			foreach (Vector2Int position in selectedPositions)
			{
				if (feature.TryGetSection(position, out FeatureAsset.Section section))
				{
					section.Variant = variant;
				}
			}
		}
	}
}
