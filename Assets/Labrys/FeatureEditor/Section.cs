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

        public void SetField(string name, string value)
        {
            Set(name, value);
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
