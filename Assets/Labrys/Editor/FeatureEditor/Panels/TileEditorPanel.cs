using Labrys.FeatureEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Labrys.Editor.FeatureEditor.Panels
{
    public class TileEditorPanel : InternalPanel
    {
        private const string MIXED_VALUES = "-";

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
            string initialVariant = GetVariant();
            if (initialVariant == null)
            {
                variant = MIXED_VALUES;
            }
            else
            {
                variant = initialVariant;
            }
            string newVariant = EditorGUI.TextField(variantTextRect, variant);
            if (newVariant != MIXED_VALUES && newVariant != variant)
            {
                Undo.RegisterCompleteObjectUndo(feature, "Change variant");
                feature.ForAllSelectedSections((Section s) => { s.variant = variant = newVariant; });
                EditorUtility.SetDirty(feature);
            }

            currY += defControlHeight + padding;

            //kvp add new 
            Rect addNewKvpRect = new Rect(minX, currY, maxWidth, defControlHeight);
            if (GUI.Button(addNewKvpRect, "Add New Field"))
            {
                TextDialogWindow.Create((string text) => {
                    Undo.RegisterCompleteObjectUndo(feature, "Add New Field");
                    feature.ForAllSelectedSections((Section s) => { s.AddField(text); });
                    EditorUtility.SetDirty(feature);
                });
            }

            currY += defControlHeight + padding;

            //kvp scroll view
            IField[] fields = GetFields();
            const float fieldRectHeight = 20;

            Rect scrollViewRect = new Rect(minX, currY, maxWidth, bounds.yMax - currY);
            Rect scrollBoundsRect = new Rect(scrollViewRect);
            scrollBoundsRect.height = (fieldRectHeight + padding) * fields.Length;
            scrollBoundsRect.width -= 20;

            kvpFieldScrollPos = GUI.BeginScrollView(scrollViewRect, kvpFieldScrollPos, scrollBoundsRect, false, true);

            Rect fieldRect = new Rect(scrollBoundsRect);
            float startY = fieldRect.y;
            fieldRect.height = fieldRectHeight;
            for (int i = 0; i < fields.Length; i++)
            {
                fieldRect.y = startY + (20 + padding) * i;

                Rect fieldRectPartial = new Rect(fieldRect);
                fieldRectPartial.width = fieldRect.width / 2;
                GUI.Box(fieldRectPartial, fields[i].Name);

                fieldRectPartial.x += fieldRect.width / 2;
                fieldRectPartial.width /= 2;
                string initalValue = fields[i].Value;
                string modifiValue = EditorGUI.TextField(fieldRectPartial, initalValue);
                if (modifiValue != initalValue)
                {
                    Undo.RegisterCompleteObjectUndo(feature, $"Modified {fields[i].Name}");
                    feature.ForAllSelectedSections((Section s) => { s.SetField(fields[i].Name, modifiValue); });
                    EditorUtility.SetDirty(feature);
                }

                fieldRectPartial.x += fieldRect.width / 4;
                if (GUI.Button(fieldRectPartial, "Remove"))
                {
                    feature.ForAllSelectedSections((Section s) => { s.RemoveField(fields[i].Name); });
                }
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
                if (feature.TryGetSection(position, out Section section))
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

        }

        private IField[] GetFields()
        {
            Dictionary<string, AggSectionField> aggregatedFields = new Dictionary<string, AggSectionField>();
            FeatureAsset feature = FeatureEditorWindow.GetInstance().Feature;

            int count = 0;

            // Create aggregate field objects containing all fields in the enumerated sections, common or not
            feature.ForAllSelectedSections((Section s) => {
                count++;
                foreach (SectionField sf in s)
                {
                    if (aggregatedFields.TryGetValue(sf.Name, out AggSectionField aggregate))
                    {
                        aggregate.Fields.Add(sf);
                    }
                    else
                    {
                        aggregatedFields.Add(sf.Name, new AggSectionField() { Fields = { sf } });
                    }
                }
            });

            // If an aggregate contains a field for every enumerated section, then add it to the return list;
            // this can be done because fields are guaranteed to be unique per section
            List<AggSectionField> commonAggregates = new List<AggSectionField>();
            foreach (AggSectionField aggregate in aggregatedFields.Values)
            {
                if (aggregate.Fields.Count == count)
                {
                    commonAggregates.Add(aggregate);
                }
            }

            return commonAggregates.ToArray();
        }
    }
}
