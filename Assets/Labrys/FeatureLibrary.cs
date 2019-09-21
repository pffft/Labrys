using Labrys.FeatureEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Labrys
{
    public class FeatureLibrary : ScriptableObject, IEnumerable<FeatureLibrary.Entry>, ISerializationCallbackReceiver, ICloneable
    {
        [field: SerializeField]
        public string TargetDirectory { get; private set; }

        private Dictionary<string, ushort> nameToFAID;
        private FeatureAsset[] features;

        public int Count => features.Length;

        public FeatureAsset this[ushort faid] => Contains(faid) ? features[faid] : null;

        public FeatureAsset this[string name]
        {
            get => this[GetId(name)];
            private set => Add(name, value);
        }

#if UNITY_EDITOR
        [MenuItem("Assets/Create/Labrys/Feature Library")]
        private static void Create()
        {
            string saveDir = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            if (!File.GetAttributes(saveDir).HasFlag(FileAttributes.Directory))
                saveDir = saveDir.Replace(Path.GetFileName(saveDir), "");

            string loadPath = $"Assets{EditorUtility.OpenFolderPanel("Select target Feature folder", saveDir, "").Replace(Application.dataPath, "")}";
            string savePath = $"{saveDir}/NewFeatureLibrary.asset";

            FeatureLibrary library = CreateInstance<FeatureLibrary>();
            library.TargetDirectory = loadPath;
            library.Refresh();

            ProjectWindowUtil.CreateAsset(library, savePath);
        }

        public void Refresh()
        {
            string[] assetGUIDs = AssetDatabase.FindAssets($"t:{nameof(FeatureAsset)}", new string[] { TargetDirectory });
            nameToFAID = new Dictionary<string, ushort>();
            features = new FeatureAsset[assetGUIDs.Length];

            foreach (string guid in assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Add(path.Replace(TargetDirectory, "").Replace(".asset", "").TrimStart('/'),
                    AssetDatabase.LoadAssetAtPath<FeatureAsset>(path));
            }
        }
#endif
        private FeatureLibrary()
        {
            nameToFAID = new Dictionary<string, ushort>();
            features = new FeatureAsset[0];
        }

        private bool Add(string name, FeatureAsset feature)
        {
            if (name == null || feature == null)
                return false;

            features[nameToFAID.Count] = feature;
            nameToFAID.Add(name, (ushort)nameToFAID.Count);

            return true;
        }

        public FeatureAsset GetFeature(string name) => this[name];

        public FeatureAsset GetFeature(ushort faid) => this[faid];

        public bool Contains(string name) => nameToFAID.ContainsKey(name);

        public bool Contains(ushort faid) => 0 <= faid && faid < features.Length;

        public ushort GetId(string name)
        {
            if (nameToFAID.TryGetValue(name, out ushort faid))
            {
                return faid;
            }
            throw new ArgumentException($"Unknown name: {name}.");
        }

        public IEnumerator<Entry> GetEnumerator()
        {
            if (nameToFAID == null)
                yield break;

            foreach(KeyValuePair<string, ushort> entry in nameToFAID)
            {
                yield return new Entry(entry.Value, entry.Key, this[entry.Value]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public object Clone()
        {
            FeatureLibrary clone = CreateInstance<FeatureLibrary>();

            clone.TargetDirectory = TargetDirectory;

            clone.features = new FeatureAsset[features.Length];
            Array.Copy(features, clone.features, features.Length);

            clone.nameToFAID = new Dictionary<string, ushort>();
            foreach(KeyValuePair<string, ushort> pair in nameToFAID)
            {
                clone.nameToFAID.Add(pair.Key, pair.Value);
            }

            return clone;
        }

        public override string ToString()
        {
            string str = "";

            foreach(Entry e in this)
            {
                str += $"{{faid: {e.FaID}, name: {e.Name}, feature: {e.Feature.ToString()}}}\n";
            }

            return $"TargetDirectory: {TargetDirectory}, Contents: [\n{str}\n]";
        }

        public struct Entry
        {
            public ushort FaID { get; }
            public string Name { get; }
            public FeatureAsset Feature { get; }

            public Entry(ushort faid, string name, FeatureAsset feature)
            {
                FaID = faid;
                Name = name;
                Feature = feature;
            }
        }

        #region SERIALIZATION
        [SerializeField]
        private List<string> serializedNames;
        [SerializeField]
        private List<FeatureAsset> serializedFeatures;

        public void OnBeforeSerialize()
        {
            if (nameToFAID == null || features == null)
                return;

            serializedNames = new List<string>();
            serializedFeatures = new List<FeatureAsset>();
            foreach(KeyValuePair<string, ushort> pair in nameToFAID)
            {
                serializedNames.Add(pair.Key);
                serializedFeatures.Add(features[pair.Value]);
            }
        }

        public void OnAfterDeserialize()
        {
            if (serializedNames == null
                || serializedFeatures == null
                || serializedNames.Count != serializedFeatures.Count)
                return;

            nameToFAID = new Dictionary<string, ushort>();
            features = new FeatureAsset[serializedFeatures.Count];
            for (int i = 0; i < serializedNames.Count; i++)
            {
                Add(serializedNames[i], serializedFeatures[i]);
            }
            serializedNames = null;
            serializedFeatures = null;
        }
        #endregion
    }
}