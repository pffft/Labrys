﻿using Labrys.FeatureEditor;
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

		public TileEditorPanel(EditorWindow window, DockPosition alignment, float scale) : base(window, alignment, scale)
		{
			variant = "";
			lastSelectedFolder = "";
		}

		public override bool HandleEvent(Event e)
		{
			const float padding = 5;
			FeatureAsset feature = FeatureEditorWindow.GetInstance().Feature;

			//selected section number
			Rect internalBounds = new Rect(bounds);
			internalBounds.width -= 10;
			internalBounds.x += padding;
			internalBounds.y += padding;
			internalBounds.height = 20;
			int selectionCount = feature.GetSelectedCount();
			GUI.Box(internalBounds, selectionCount + (selectionCount == 1 ? " tile selected" : " tiles selected"));

			internalBounds.y += 25;

			//variant field label
			internalBounds.width /= 4;
			internalBounds.x = bounds.xMin + padding;
			GUI.Box(internalBounds, "Variant");

			//variant field select button
			internalBounds.x = bounds.xMax - padding - internalBounds.width;
			if (GUI.Button(internalBounds, "Select"))
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
			internalBounds.width *= 2;
			internalBounds.x = bounds.xMax - padding - (internalBounds.width * 3/2);
			string compoundV = GetVariant();
			if (compoundV == null)
			{
				variant = MIXED_VARIANTS;
			}
			else
			{
				variant = compoundV;
			}
			string newVariant = EditorGUI.TextField(internalBounds, variant);
			if (newVariant != MIXED_VARIANTS && newVariant != variant)
			{
				Undo.RegisterCompleteObjectUndo(feature, "Change variant");
				SetVariant(variant = newVariant);
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