using UnityEngine;

namespace Labrys.FeatureEditor
{
    [System.Serializable]
    public class SectionField : IField
    {
        public static bool operator ==(SectionField left, SectionField right)
        {
            if (ReferenceEquals(left, right))
                return true;
            if (left == null)
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(SectionField left, SectionField right)
        {
            return !(left == right);
        }

        [field: SerializeField]
        public string Name { get; set; }

        [field: SerializeField]
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            SectionField other = (SectionField)obj;

            return other.Name == Name;
        }

        public override int GetHashCode()
        {
            return Name != null ? Name.GetHashCode() : 0;
        }
    }
}
