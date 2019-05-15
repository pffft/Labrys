using Labrys.FeatureEditor;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Panels
{
	public class TileEditorPanel : InternalPanel
	{
		private const string MIXED_VARIANTS = "-";

		private string variant;
		private string lastSelectedFolder;
		private Vector2 kvpFieldScrollPos;

		public TileEditorPanel(EditorWindow window, DockPosition alignment, float scale) : base(window, alignment, scale)
		{
			variant = "";
			lastSelectedFolder = "";
			kvpFieldScrollPos = Vector2.zero;
		}

		public override bool HandleEvent(Event e)
		{
			const float padding = 5;
			const float defControlHeight = 20;
			float maxWidth = bounds.width - (padding * 2);
			float currY = bounds.yMin + padding;
			float minX = bounds.xMin + padding;

			FeatureAsset feature = FeatureEditorWindow.GetInstance().Feature;

			//selected section number
			Rect selSecCountRect = new Rect(minX, currY, maxWidth, defControlHeight);
			int selectionCount = feature.GetSelectedCount();
			GUI.Box(selSecCountRect, selectionCount + (selectionCount == 1 ? " tile selected" : " tiles selected"));

			currY += selSecCountRect.height + padding;

			//variant field label
			Rect variantFieldLabelRect = new Rect(minX, currY, maxWidth / 4, defControlHeight);
			GUI.Box(variantFieldLabelRect, "Variant");

			//variant field select button
			Rect variantSelButRect = new Rect(bounds.xMax - padding - (maxWidth / 4), currY, maxWidth / 4, defControlHeight);
			if (GUI.Button(variantSelButRect, "Select"))
			{
				string fullPath = EditorUtility.OpenFolderPanel("Select Variant Folder", lastSelectedFolder != "" ? lastSelectedFolder : Application.dataPath, "");
				string[] splitFullPath = fullPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				variant = splitFullPath[splitFullPath.Length - 1];
				SetVariant(variant);

				//save parent of selected folder
				lastSelectedFolder = fullPath.Substring(0,
					Mathf.Max(
						fullPath.LastIndexOf(Path.AltDirectorySeparatorChar),
						fullPath.LastIndexOf(Path.DirectorySeparatorChar)));
			}

			//variant field text box
			Rect variantTextRect = new Rect(minX + (maxWidth / 4), currY, maxWidth / 2, defControlHeight);
			string compoundV = GetVariant();
			if (compoundV == null)
			{
				variant = MIXED_VARIANTS;
			}
			else
			{
				variant = compoundV;
			}
			string newVariant = EditorGUI.TextField(variantTextRect, variant);
			if (newVariant != MIXED_VARIANTS && newVariant != variant)
			{
				Undo.RegisterCompleteObjectUndo(feature, "Change variant");
				SetVariant(variant = newVariant);
				EditorUtility.SetDirty(feature);
			}

			currY += defControlHeight + padding;

			//kvp scroll view
			Rect scrollViewRect = new Rect(minX, currY, maxWidth, bounds.yMax - currY);
			Rect scrollBoundsRect = new Rect(scrollViewRect);
			scrollBoundsRect.height = 250;
			scrollBoundsRect.width -= 20;

			kvpFieldScrollPos = GUI.BeginScrollView(scrollViewRect, kvpFieldScrollPos, scrollBoundsRect, false, true);

			Rect r = new Rect(scrollBoundsRect);
			float startY = r.y;
			r.height = 20;
			for (int i = 0; i < 10; i++)
			{
				r.y = startY + (20 + padding) * i;
				GUI.Button(r, "test " + i);
			}

			GUI.EndScrollView(handleScrollWheel: true);

			return bounds.Contains(e.mousePosition);
		}

		private string GetVariant()
		{
			string compoundVariant = null;
			FeatureAsset feature = FeatureEditorWindow.GetInstance().Feature;
			Vector2Int[] selectedPositions = feature.GetSelectedSections();
			foreach (Vector2Int position in selectedPositions)
			{
				if (feature.TryGetSection(position, out FeatureAsset.Section section))
				{
					if (compoundVariant == null)
					{
						//first variant found
						compoundVariant = section.variant;
					}
					else if (compoundVariant != section.variant)
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
					section.variant = variant;
				}
			}
		}
	}
}
