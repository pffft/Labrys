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
		[Serializable]
		public class Section
		{
			public static bool IsValidPosition(Vector2Int gridPosition)
			{
				return gridPosition.x % GRID_DENSITY == 0 && gridPosition.y % GRID_DENSITY == 0;
			}

			public string Variant { get; set; }
		}

		[Serializable]
		public class Link
		{
			public static void GetMinMaxSubjectTileCount(Vector2Int gridPos, out int min, out int max)
			{
				if (gridPos.x % 2 != 0)
				{
					if (gridPos.y % 2 != 0)
					{
						min = max = 4;
					}
					else
					{
						min = 1;
						max = 2;
					}
				}
				else if (gridPos.y % 2 != 0)
				{
					min = 1;
					max = 2;
				}
				else
				{
					min = max = -1;
				}
			}

			public static IEnumerable<Vector2Int> GetSubjectTileGridPositions(Vector2Int gridPos)
			{
				HashSet<Vector2Int> uniquePositions = new HashSet<Vector2Int>();

				int lowerX = Mathf.FloorToInt(gridPos.x / GRID_DENSITY) * (int)GRID_DENSITY;
				int upperX = Mathf.FloorToInt((gridPos.x / GRID_DENSITY) + 0.5f) * (int)GRID_DENSITY;
				int lowerY = Mathf.FloorToInt(gridPos.y / GRID_DENSITY) * (int)GRID_DENSITY;
				int upperY = Mathf.FloorToInt((gridPos.y / GRID_DENSITY) + 0.5f) * (int)GRID_DENSITY;

				uniquePositions.Add(new Vector2Int(lowerX, lowerY));
				uniquePositions.Add(new Vector2Int(lowerX, upperY));
				uniquePositions.Add(new Vector2Int(upperX, lowerY));
				uniquePositions.Add(new Vector2Int(upperX, upperY));

				Vector2Int[] finalPositions = new Vector2Int[uniquePositions.Count];
				uniquePositions.CopyTo(finalPositions);
				return finalPositions;
			}

			public bool Open { get; set; } 
			public bool External { get; set; }
		}

		public const float GRID_DENSITY = 2f;

		private static Vector2Int[] dirVectors = new Vector2Int[] {
				Vector2Int.right,
				Vector2Int.up + Vector2Int.right,
				Vector2Int.up,
				Vector2Int.up + Vector2Int.left,
				Vector2Int.left,
				Vector2Int.down + Vector2Int.left,
				Vector2Int.down,
				Vector2Int.down + Vector2Int.right
		};

		private static Connection[] dirConnections = new Connection[] {
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
		public static FeatureAsset FromFeature(Feature f)
		{
			FeatureAsset asset = CreateInstance<FeatureAsset>();

			//make sections and assign variants
			foreach (KeyValuePair<Vector2Int, Generation.Section> section in f.Elements)
			{
				asset.AddSection(section.Key * (int)GRID_DENSITY);
				if(asset.TryGetSection(section.Key * (int)GRID_DENSITY, out Section s))
				{
					s.Variant = section.Value.GetVariant();
				}
			}

			//set links open/closed and internal/external
			foreach (KeyValuePair<Vector2Int, Generation.Section> section in f.Elements)
			{
				for(int i = 0; i < dirVectors.Length; i++)
				{
                    if(asset.TryGetLink((section.Key + dirVectors[i]) * (int)GRID_DENSITY, out Link link))
					{
						link.Open = (section.Value.internalConnections | dirConnections[i]) == dirConnections[i];
						link.External = (section.Value.externalConnections | dirConnections[i]) == dirConnections[i];
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

			for(int i = 0; i < sectionPositions.Count; i++)
			{
				sections.Add(sectionPositions[i], sectionData[i]);
			}

			for (int i = 0; i < linkPositions.Count; i++)
			{
				links.Add(linkPositions[i], linkData[i]);
			}

			for(int i = 0; i < selectionPositions.Count; i++)
			{
				selected.Add(selectionPositions[i]);
			}
		}

		public void OnBeforeSerialize()
		{
			sectionPositions.Clear();
			sectionData.Clear();
			foreach(KeyValuePair<Vector2Int, Section> kvp in sections)
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
			foreach(Vector2Int position in selected)
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
			if(Section.IsValidPosition(gridPosition)
				&& !sections.ContainsKey(gridPosition))
			{
				sections.Add(gridPosition, new Section() { Variant = "default" });
				UpdateLinks(gridPosition);
			}
		}

		public bool RemoveSection(Vector2Int gridPosition)
		{
			if(sections.Remove(gridPosition))
			{
				UpdateLinks(gridPosition);
				DeselectSection(gridPosition);
				return true;
			}
			return false;
		}

		public bool SelectSection(Vector2Int gridPosition)
		{
			if(Section.IsValidPosition(gridPosition))
			{
				return selected.Add(gridPosition);
			}
			return false;
		}

		public bool IsSelected(Vector2Int gridPosition)
		{
			if (Section.IsValidPosition(gridPosition))
			{
				return selected.Contains(gridPosition);
			}
			return false;
		}

		public bool DeselectSection(Vector2Int gridPosition)
		{
			return selected.Remove(gridPosition);
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
			section = default;
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
			link = default;
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
				return;

			foreach(Vector2Int dir in dirVectors)
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
					Link newLink = new Link() { Open = true, External = canBeExternal };
					if(isValid)
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

					if (!canBeExternal)
						existingLink.External = false;
				}
			}
		}

		public Feature ToFeature()
		{
			Feature feature = new Feature();
			foreach(KeyValuePair<Vector2Int, Section> section in sections)
			{
				Connection internalConnections = Connection.None;
				Connection externalConnections = Connection.None;
				for(int i = 0; i < dirVectors.Length; i++)
				{
					Vector2Int adjPos = section.Key + dirVectors[i];
					if(TryGetLink(adjPos, out Link link))
					{
						if(link.Open)
						{
							internalConnections |= dirConnections[i];
						}

						if(link.External)
						{
							externalConnections |= dirConnections[i];
						}
					}
				}

				Vector2Int position = new Vector2Int(section.Key.x / (int)GRID_DENSITY, section.Key.y / (int)GRID_DENSITY);
				feature.Add(position, internalConnections, section.Value.Variant, externalConnections);
			}
			return feature;
		}
	}
}
