using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.FeatureEditor
{
	[Serializable]
	public class Section : IEnumerable<SectionField>
	{
		public static bool IsValidPosition(Vector2Int gridPosition)
		{
			return gridPosition.x % FeatureAsset.GRID_DENSITY == 0 && gridPosition.y % FeatureAsset.GRID_DENSITY == 0;
		}

		public string variant;

		[SerializeField]
		private List<SectionField> fields;

		public Section()
		{
			fields = new List<SectionField>();
		}

		public bool AddField(string name)
		{
			if (name == null)
				return false;

			if (!fields.Exists((SectionField f) => { return f.Name == name; }))
			{
				fields.Add(new SectionField() { Name = name, Value = null });
				return true;
			}
			return false;
		}

		public bool RemoveField(string name)
		{
			if (name == null)
				return false;

			return fields.Remove(fields.Find((SectionField f) => { return f.Name == name; }));
		}

		private void Set<T>(string name, T value)
		{
			SectionField f = fields.Find((SectionField q) => { return q.Name == name; });
			if (!f.Equals(default(SectionField)))
			{
				f.Value = value.ToString();
			}
		}

		public void SetInt(string name, int value)
		{
			Set(name, value);
		}

		public void SetFloat(string name, float value)
		{
			Set(name, value);
		}

		public void SetBool(string name, bool value)
		{
			Set(name, value);
		}

		public void SetString(string name, string value)
		{
			Set(name, value);
		}

		public int GetInt(string name)
		{
			try
			{
				return int.Parse(GetString(name));
			}
			catch (Exception e) when (e is ArgumentException || e is NullReferenceException)
			{
				throw new ArgumentException($"{name} is not an int", e);
			}
		}

		public float GetFloat(string name)
		{
			try
			{
				return float.Parse(GetString(name));
			}
			catch (Exception e) when (e is ArgumentException || e is NullReferenceException)
			{
				throw new ArgumentException($"{name} is not a float", e);
			}
		}

		public bool GetBool(string name)
		{
			try
			{
				return bool.Parse(GetString(name));
			}
			catch(Exception e) when (e is ArgumentException || e is NullReferenceException)
			{
				throw new ArgumentException($"{name} is not a boolean", e);
			}
		}

		public string GetString(string name)
		{
			SectionField f = fields.Find((SectionField q) => { return q.Name == name; });
			return f.Value;
		}

		public IEnumerator<SectionField> GetEnumerator()
		{
			return fields.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return fields.GetEnumerator();
		}
	}
}
