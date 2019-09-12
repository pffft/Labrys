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
    public class FeatureLibrary : ScriptableObject, IEnumerable<KeyValuePair<string, FeatureAsset>>, ISerializationCallbackReceiver
    {
        [field: SerializeField]
        public string TargetDirectory { get; private set; }

        private Dictionary<string, ushort> nameToFAID;
        private FeatureAsset[] features;

        public int Count => features.Length;

#if UNITY_EDITOR
        [field: SerializeField]
        public bool AutoRefresh { get; set; } //TODO figure out how to hook into asset creation process for auto refresh; or maybe auto refresh on playmode start
#endif

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
            string saveDir = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
            if (!File.GetAttributes(saveDir).HasFlag(FileAttributes.Directory))
                saveDir = saveDir.Replace(Path.GetFileName(saveDir), "");

            string loadPath = $"Assets{EditorUtility.OpenFolderPanel("Select target Feature folder", saveDir, "").Replace(Application.dataPath, "")}";
            string savePath = $"{saveDir}/NewFeatureLibrary.asset";

            string[] assetGUIDs = AssetDatabase.FindAssets($"t:{nameof(FeatureAsset)}", new string[] { loadPath });

            FeatureLibrary library = CreateInstance<FeatureLibrary>();
            library.TargetDirectory = loadPath;
            library.Refresh();

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
            if (features != null && assetGUIDs.Length != features.Length)
            {
                nameToFAID = new Dictionary<string, ushort>();
                features = new FeatureAsset[assetGUIDs.Length];

                foreach (string guid in assetGUIDs)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Add(path.Replace(TargetDirectory, "").Replace(".asset", "").TrimStart('/'),
                        AssetDatabase.LoadAssetAtPath<FeatureAsset>(path));
                }
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

        public IEnumerator<KeyValuePair<string, FeatureAsset>> GetEnumerator()
        {
            if (nameToFAID == null)
                yield break;

            foreach(KeyValuePair<string, ushort> entry in nameToFAID)
            {
                yield return new KeyValuePair<string, FeatureAsset>(entry.Key, this[entry.Value]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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