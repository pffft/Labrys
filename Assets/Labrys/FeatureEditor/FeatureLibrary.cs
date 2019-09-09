using System;
using System.Collections;
using System.Collections.Generic;

namespace Labrys.FeatureEditor
{
    public class FeatureLibrary : IEnumerable<FeatureAsset>
    {
        private const int INITIAL_ARR_SIZE = 4;

        private Dictionary<string, ushort> nameToFAID;
        private FeatureAsset[] features;
        private int head;

        public int Count { get; private set; }

        public bool IsReadOnly => false;

        /// <summary>
        /// With this enanabled, all Feature Asset IDs are persistent for the life of this library object.
        /// WARNING: this may cause the space to run out sooner under high churn conditions.
        /// </summary>
        public bool PreserveFAIDs => false;

        public FeatureAsset this[ushort faid]
        {
            get
            {
                if (faid >= 0 && faid < features.Length)
                {
                    return features[faid];
                }
                return null;
            }
        }

        public FeatureAsset this[string name]
        {
            get => this[GetId(name)];
            set => Add(name, value);
        }

        public FeatureLibrary()
        {
            nameToFAID = new Dictionary<string, ushort>();
            features = new FeatureAsset[INITIAL_ARR_SIZE];
            Count = 0;
        }

        public bool Add(string name, FeatureAsset feature)
        {
            if (name == null || feature == null)
                return false;

            if (Count >= features.Length && !TryResize())
            {
                throw new ArgumentOutOfRangeException($"This {nameof(FeatureLibrary)} is out of space!");
            }

            nameToFAID.Add(name, (ushort)head);
            features[head] = feature;

            head++;
            Count++;

            return true;
        }

        public bool Remove(string name)
        {
            if (name == null)
                return false;

            features[GetId(name)] = null;
            return nameToFAID.Remove(name);
        }

        public bool Remove(short faid)
        {
            features[faid] = null;

            List<string> names = new List<string>();
            foreach(KeyValuePair<string, ushort> pair in nameToFAID)
            {
                if (pair.Value == faid)
                    names.Add(pair.Key);
            }

            if (names.Count > 0)
            {
                foreach (string name in names)
                {
                    nameToFAID.Remove(name);
                }
                Count--;
                return true;
            }
            return false;
        }

        public bool Contains(string name) => nameToFAID.ContainsKey(name);

        public void Clear()
        {
            nameToFAID.Clear();
            features = new FeatureAsset[INITIAL_ARR_SIZE];
        }

        public ushort GetId(string name)
        {
            if (nameToFAID.TryGetValue(name, out ushort faid))
            {
                return faid;
            }
            throw new ArgumentException($"Unknown name: {name}.");
        }

        /// <summary>
        /// If PreserveFAIDs is set to false, then the array will be compacted 
        /// </summary>
        /// <returns></returns>
        private bool TryResize()
        {
            if (Count >= short.MaxValue)
                return false;

            FeatureAsset[] newArr = new FeatureAsset[features.Length * 2];
            if (PreserveFAIDs)
            {
                for (int i = 0; i < features.Length; i++)
                {
                    if (features[i] != null)
                    {
                        newArr[i] = features[i];
                    }
                }
                features = newArr;
                return true;
            }
            else
            {
                //TODO: rebuild nameToFAID dictionary after compression
                //NOTE: maybe use FeatureAsset#name instead of allowing user to set name
                Dictionary<string, ushort> newDict = new Dictionary<string, ushort>();
                int j = 0;
                for (int i = 0; i < features.Length; i++)
                {
                    if (features[i] != null)
                    {
                        newArr[j] = features[i];
                        newDict.Add
                        j++;
                    }
                }

                head = j;
                features = newArr;
                return true;
            }
        }

        public IEnumerator<FeatureAsset> GetEnumerator()
        {
            return ((IEnumerable<FeatureAsset>)features).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<FeatureAsset>)features).GetEnumerator();
        }
    }
}