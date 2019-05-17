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
				if (left == null)
					return false;
				return left.Equals(right);
			}

			public static bool operator !=(Field left, Field right)
			{
				if (left == null)
				{
					if (right == null)
						return true;
					return false;
				}
				return !left.Equals(right);
			}

			public string name;
			public Type type;
			public string value;

			public override bool Equals(object obj)
			{
				if (obj == null)
					return false;

				Field other = (Field)obj;

				if (other.type != type)
					return false;

				return other.value == value;
			}

			public override int GetHashCode()
			{
				return (type.ToString() + value).GetHashCode();
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
				fields.Add(new Field() { name = name, type = null, value = null });
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
				f.type = typeof(T);
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
			Field f = fields.Find((Field q) => { return q.name == name; });
			if (f.type == typeof(int))
			{
				return int.Parse(f.value);
			}
			throw new InvalidOperationException(name + " is not a integer value");
		}

		public float GetFloat(string name)
		{
			Field f = fields.Find((Field q) => { return q.name == name; });
			if (f.type == typeof(float))
			{
				return float.Parse(f.value);
			}
			throw new InvalidOperationException(name + " is not a floating point value");
		}

		public bool GetBool(string name)
		{
			Field f = fields.Find((Field q) => { return q.name == name; });
			if (f.type == typeof(bool))
			{
				return bool.Parse(f.value);
			}
			throw new InvalidOperationException(name + " is not a boolean value");
		}

		public string GetString(string name)
		{
			Field f = fields.Find((Field q) => { return q.name == name; });
			if (f.type == typeof(string))
			{
				return f.value;
			}
			throw new InvalidOperationException(name + " is not a string value");
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
