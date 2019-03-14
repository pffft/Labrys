using System;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.FeatureEditor
{
	[CreateAssetMenu(menuName = "Labrys/Feature", fileName = "New Feature")]
	public class FeatureAsset : ScriptableObject, ISerializationCallbackReceiver
	{
		public const float GRID_DENSITY = 2f;

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
		}

		public void AddSection(Vector2Int gridPosition)
		{
			if(Section.IsValidPosition(gridPosition)
				&& !sections.ContainsKey(gridPosition))
			{
				sections.Add(gridPosition, new Section() { Variant = "Default" });
				UpdateLinks(gridPosition);
			}
		}

		public bool RemoveSection(Vector2Int gridPosition)
		{
			if(sections.Remove(gridPosition))
			{
				UpdateLinks(gridPosition);
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

			foreach(Vector2Int adjPos in GetAdjPositions())
			{
				Vector2Int linkPos = adjPos + gridPosition;
				if (!links.TryGetValue(linkPos, out Link existingLink))
				{
					Link newLink = new Link() { Open = true, External = true };
					if(CheckLinkValid(linkPos))
					{
						links.Add(linkPos, newLink);
					}
				}
				else
				{
					if(!CheckLinkValid(linkPos))
					{
						links.Remove(linkPos);
					}
				}
			}

			// Local function to check given position has the required amount of section neighbors for a link
			bool CheckLinkValid(Vector2Int gridPos)
			{
				int neighborCount = 0;
				foreach (Vector2Int tilePos in Link.GetSubjectTileGridPositions(gridPos))
				{
					if (sections.ContainsKey(tilePos))
					{
						neighborCount++;
					}
				}
				Link.GetMinMaxSubjectTileCount(gridPos, out int nMin, out int nMax);
				return nMax == 2 ? neighborCount >= nMin : neighborCount == nMax;
			}
		}

		private Vector2Int[] GetAdjPositions()
		{
			Vector2Int[] positions = new Vector2Int[8];
			positions[0] = Vector2Int.right;
			positions[1] = Vector2Int.up + Vector2Int.right;
			positions[2] = Vector2Int.up;
			positions[3] = Vector2Int.up + Vector2Int.left;
			positions[4] = Vector2Int.left;
			positions[5] = Vector2Int.down + Vector2Int.left;
			positions[6] = Vector2Int.down;
			positions[7] = Vector2Int.down + Vector2Int.right;
			return positions;
		}
	}
}
