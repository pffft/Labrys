using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Labrys.FeatureEditor
{
    public class FeatureLibrary : ScriptableObject, IEnumerable<FeatureAsset>, ISerializationCallbackReceiver
    {
        [field: SerializeField]
        //[field: HideInInspector]
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
        private static void FromDirectory()
        {
            string loadPath = $"Assets/{EditorUtility.OpenFolderPanel("Select target Feature folder", "Assets", "").Replace(Application.dataPath, "")}";
            string savePath = $"{AssetDatabase.GetAssetPath(Selection.activeInstanceID)}/NewFeatureLibrary.asset";

            string[] assetGUIDs = AssetDatabase.FindAssets($"t:{nameof(FeatureAsset)}", new string[] { loadPath });

            FeatureLibrary library = CreateInstance<FeatureLibrary>();
            library.TargetDirectory = loadPath;
            library.features = new FeatureAsset[assetGUIDs.Length];
            foreach (string guid in assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string name = path.Replace(library.TargetDirectory, "").Replace(".asset", "");
                library.Add(name, AssetDatabase.LoadAssetAtPath<FeatureAsset>(path));
            }

            ProjectWindowUtil.CreateAsset(library, savePath);
        }
#endif

        private FeatureLibrary()
        {
            nameToFAID = new Dictionary<string, ushort>();
            features = new FeatureAsset[0];
        }

#if UNITY_EDITOR
        public void Refresh()
        {
            string[] assetGUIDs = AssetDatabase.FindAssets($"t:{nameof(FeatureAsset)}", new string[] { TargetDirectory });
            nameToFAID.Clear();
            features = new FeatureAsset[assetGUIDs.Length];

            foreach (string guid in assetGUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Add(path.Replace(TargetDirectory, "").Replace(".asset", ""), AssetDatabase.LoadAssetAtPath<FeatureAsset>(path));
            }
        }
#endif

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

        public IEnumerator<FeatureAsset> GetEnumerator()
        {
            return ((IEnumerable<FeatureAsset>)features).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<FeatureAsset>)features).GetEnumerator();
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
            nameToFAID = null;
            features = null;
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