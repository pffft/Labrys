using Labrys.Generation;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.FeatureEditor
{
    /// <summary>
    /// A file-backed representation of a Feature for use with the Feature Editor
    /// </summary>
    [CreateAssetMenu(menuName = "Labrys/Feature", fileName = "New Feature")]
    public class FeatureAsset : ScriptableObject, ISerializationCallbackReceiver
    {
        public const int GRID_DENSITY = 2;

        /// <summary>
        /// A set of direction vectors in unit circle order used in Feature/FeatureAsset conversions.
        /// </summary>
        private static readonly Vector2Int[] dirVectors =
        {
                Vector2Int.right,
                Vector2Int.up + Vector2Int.right,
                Vector2Int.up,
                Vector2Int.up + Vector2Int.left,
                Vector2Int.left,
                Vector2Int.down + Vector2Int.left,
                Vector2Int.down,
                Vector2Int.down + Vector2Int.right
        };

        /// <summary>
        /// An array containing all the connections, in unit circle order. Used in Feature/FeatureAsset conversions.
        /// </summary>
        private static readonly Connection[] dirConnections =
        {
                Connection.East,
                Connection.Northeast,
                Connection.North,
                Connection.Northwest,
                Connection.West,
                Connection.Southwest,
                Connection.South,
                Connection.Southeast
        };

#if UNITY_EDITOR
        /// <summary>
        /// Converts a Generation Feature into an Editor FeatureAsset.
        /// </summary>
        /// <returns>A feature asset.</returns>
        /// <param name="f">The feature to convert.</param>
        public static FeatureAsset FromFeature(Feature f)
        {
            FeatureAsset asset = CreateInstance<FeatureAsset>();

            //make sections and assign variants
            foreach (KeyValuePair<Vector2Int, Generation.Section> gSection in f.Elements)
            {
                asset.AddSection(gSection.Key * GRID_DENSITY);
                if (asset.TryGetSection(gSection.Key * GRID_DENSITY, out Section feSection))
                {
                    feSection.variant = gSection.Value.GetVariant();
                }
            }

            //set links open/closed and internal/external
            foreach (KeyValuePair<Vector2Int, Generation.Section> section in f.Elements)
            {
                for (int i = 0; i < dirVectors.Length; i++)
                {
                    if (asset.TryGetLink((section.Key + dirVectors[i]) * GRID_DENSITY, out Link link))
                    {
                        link.open = (section.Value.internalConnections | dirConnections[i]) == dirConnections[i];
                        link.external = (section.Value.externalConnections | dirConnections[i]) == dirConnections[i];
                    }
                }
            }

            return asset;
        }
#endif

        private Dictionary<Vector2Int, Section> sections;
        private Dictionary<Vector2Int, Link> links;
        private HashSet<Vector2Int> selected;

        #region SERIALIZATION
        [SerializeField, HideInInspector]
        private List<Vector2Int> sectionPositions;
        [SerializeField, HideInInspector]
        private List<Section> sectionData;

        [SerializeField, HideInInspector]
        private List<Vector2Int> linkPositions;
        [SerializeField, HideInInspector]
        private List<Link> linkData;

        [SerializeField, HideInInspector]
        private List<Vector2Int> selectionPositions;

        public void OnAfterDeserialize()
        {
            if (sectionPositions.Count != sectionData.Count
                || linkPositions.Count != linkData.Count)
                throw new ArgumentException("Malformed feature data");

            sections.Clear();
            for (int i = 0; i < sectionPositions.Count; i++)
            {
                sections.Add(sectionPositions[i], sectionData[i]);
            }
            sectionPositions.Clear();
            sectionData.Clear();

            links.Clear();
            for (int i = 0; i < linkPositions.Count; i++)
            {
                links.Add(linkPositions[i], linkData[i]);
            }
            linkPositions.Clear();
            linkData.Clear();

            selected.Clear();
            for (int i = 0; i < selectionPositions.Count; i++)
            {
                selected.Add(selectionPositions[i]);
            }
            selectionPositions.Clear();
        }

        public void OnBeforeSerialize()
        {
            sectionPositions.Clear();
            sectionData.Clear();
            foreach (KeyValuePair<Vector2Int, Section> kvp in sections)
            {
                sectionPositions.Add(kvp.Key);
                sectionData.Add(kvp.Value);
            }

            linkPositions.Clear();
            linkData.Clear();
            foreach (KeyValuePair<Vector2Int, Link> kvp in links)
            {
                linkPositions.Add(kvp.Key);
                linkData.Add(kvp.Value);
            }

            selectionPositions.Clear();
            foreach (Vector2Int position in selected)
            {
                selectionPositions.Add(position);
            }
        }
        #endregion

        public FeatureAsset()
        {
            sections = new Dictionary<Vector2Int, Section>();
            links = new Dictionary<Vector2Int, Link>();
            selected = new HashSet<Vector2Int>();

            sectionPositions = new List<Vector2Int>();
            sectionData = new List<Section>();
            linkPositions = new List<Vector2Int>();
            linkData = new List<Link>();
            selectionPositions = new List<Vector2Int>();
        }

        public void AddSection(Vector2Int gridPosition)
        {
            if (Section.IsValidPosition(gridPosition)
                && !sections.ContainsKey(gridPosition))
            {
                sections.Add(gridPosition, new Section() { variant = "default" });
                UpdateLinks(gridPosition);
            }
        }

        public bool RemoveSection(Vector2Int gridPosition)
        {
            if (sections.Remove(gridPosition))
            {
                UpdateLinks(gridPosition);
                DeselectSection(gridPosition);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true iff the position is a valid Section, and it was successfully added.
        /// </summary>
        /// <param name="gridPosition"></param>
        /// <returns></returns>
        public bool SelectSection(Vector2Int gridPosition)
        {
            return Section.IsValidPosition(gridPosition)
                    && sections.ContainsKey(gridPosition)
                    && selected.Add(gridPosition);
        }

        public void SelectAllSections()
        {
            foreach (Vector2Int position in sections.Keys)
            {
                selected.Add(position);
            }
        }

        public bool IsSelected(Vector2Int gridPosition)
        {
            return Section.IsValidPosition(gridPosition)
                && selected.Contains(gridPosition);
        }

        public bool DeselectSection(Vector2Int gridPosition)
        {
            return selected.Remove(gridPosition);
        }

        public void DeselectAllSections()
        {
            selected.Clear();
        }

        public int GetSelectedCount()
        {
            return selected.Count;
        }

        public Vector2Int[] GetSelectedSections()
        {
            Vector2Int[] sel = new Vector2Int[selected.Count];
            selected.CopyTo(sel);
            return sel;
        }

        public delegate void SectionOperation(Section s);
        public void ForAllSelectedSections(SectionOperation operation)
        {
            foreach (Vector2Int pos in GetSelectedSections())
            {
                if (TryGetSection(pos, out Section s))
                {
                    operation.Invoke(s);
                }
            }
        }

        public bool HasSectionAt(Vector2Int gridPosition)
        {
            return Section.IsValidPosition(gridPosition)
                && sections.ContainsKey(gridPosition);
        }

        public bool HasLinkAt(Vector2Int gridPosition)
        {
            return !Section.IsValidPosition(gridPosition)
                && links.ContainsKey(gridPosition);
        }

        public bool TryGetSection(Vector2Int gridPosition, out Section section)
        {
            section = null;
            if (Section.IsValidPosition(gridPosition)
                && sections.TryGetValue(gridPosition, out Section secRef))
            {
                section = secRef;
                return true;
            }
            return false;
        }

        public bool TryGetLink(Vector2Int gridPosition, out Link link)
        {
            link = null;
            if (!Section.IsValidPosition(gridPosition)
                && links.TryGetValue(gridPosition, out Link linkRef))
            {
                link = linkRef;
                return true;
            }
            return false;
        }

        public IReadOnlyDictionary<Vector2Int, Section> GetSections()
        {
            return sections;
        }

        public IReadOnlyDictionary<Vector2Int, Link> GetLinks()
        {
            return links;
        }

        private void UpdateLinks(Vector2Int gridPosition)
        {
            if (!Section.IsValidPosition(gridPosition))
            {
                return;
            }

            foreach (Vector2Int dir in dirVectors)
            {
                Vector2Int linkPos = dir + gridPosition;
                int neighborCount = 0;
                foreach (Vector2Int tilePos in Link.GetSubjectTileGridPositions(linkPos))
                {
                    if (sections.ContainsKey(tilePos))
                    {
                        neighborCount++;
                    }
                }
                Link.GetMinMaxSubjectTileCount(linkPos, out int nMin, out int nMax);

                bool canBeExternal = neighborCount < nMax;
                bool isValid = nMax == 2 ? neighborCount >= nMin : neighborCount == nMax;

                if (!links.TryGetValue(linkPos, out Link existingLink))
                {
                    Link newLink = new Link() { open = true, external = canBeExternal };
                    if (isValid)
                    {
                        links.Add(linkPos, newLink);
                    }
                }
                else
                {
                    if (!isValid)
                    {
                        links.Remove(linkPos);
                    }

                    existingLink.external &= canBeExternal;
                }
            }
        }

        /// <summary>
        /// Converts this Editor FeatureAsset into a Generation Feature.
        /// 
        /// There may be fewer connections in the output of this compared to a
        /// FeatureAsset made from an input Feature. This is because FeatureAsset
        /// aggressively prunes connections that do not exist in the structure.
        /// </summary>
        /// <returns>A new feature.</returns>
        public Feature ToFeature()
        {
            Feature feature = new Feature();
            foreach (KeyValuePair<Vector2Int, Section> feSection in sections)
            {
                Connection internalConnections = Connection.None;
                Connection externalConnections = Connection.None;
                for (int i = 0; i < dirVectors.Length; i++)
                {
                    Vector2Int adjPos = feSection.Key + dirVectors[i];
                    if (TryGetLink(adjPos, out Link link))
                    {
                        if (link.open)
                        {
                            internalConnections |= dirConnections[i];
                        }

                        if (link.external)
                        {
                            externalConnections |= dirConnections[i];
                        }
                    }
                }

                Vector2Int position = new Vector2Int(feSection.Key.x / GRID_DENSITY, feSection.Key.y / GRID_DENSITY);
                feature.Add(position, internalConnections, feSection.Value.variant, externalConnections);
            }
            return feature;
        }
    }
}
