using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.FeatureEditor
{
	[Serializable]
	public class Section : IEnumerable<Section.Field>
	{
		[Serializable]
		public class Field
		{
			public static bool operator==(Field left, Field right)
			{
				if (ReferenceEquals(left, right))
					return true;
				if (left == null)
					return false;
				return left.Equals(right);
			}

			public static bool operator !=(Field left, Field right)
			{
				return !(left == right);
			}

			public string name;
			public string value;

			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;

				Field other = (Field)obj;

				return other.name == name;
			}

			public override int GetHashCode()
			{
				return name.GetHashCode();
			}
		}

		public static bool IsValidPosition(Vector2Int gridPosition)
		{
			return gridPosition.x % FeatureAsset.GRID_DENSITY == 0 && gridPosition.y % FeatureAsset.GRID_DENSITY == 0;
		}

		public string variant;

		[SerializeField]
		private List<Field> fields;

		public bool AddField(string name)
		{
			if (name == null)
				return false;

			if (!fields.Exists((Field f) => { return f.name == name; }))
			{
				fields.Add(new Field() { name = name, value = null });
				return true;
			}
			return false;
		}

		public bool RemoveField(string name)
		{
			if (name == null)
				return false;

			return fields.Remove(fields.Find((Field f) => { return f.name == name; }));
		}

		private void Set<T>(string name, T value)
		{
			Field f = fields.Find((Field q) => { return q.name == name; });
			if (!f.Equals(default(Field)))
			{
				f.value = value.ToString();
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
			Field f = fields.Find((Field q) => { return q.name == name; });
			return f.value;
		}

		public IEnumerator<Field> GetEnumerator()
		{
			return ((IEnumerable<Field>)fields).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Field>)fields).GetEnumerator();
		}
	}
}
