using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Labrys.FeatureEditor
{
    public class FeatureLibrary : ScriptableObject, IEnumerable<FeatureAsset>
    {
        private Dictionary<string, ushort> nameToFAID;
        private FeatureAsset[] features;

        public int Count => features.Length;

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
            private set => Add(name, value);
        }

#if UNITY_EDITOR
        [MenuItem("Assets/Create/Labrys/Feature Library")]
        private static void FromDirectory()
        {
            string loadPath = $"Assets/{EditorUtility.OpenFolderPanel("Select target folder", "Assets", "").Replace(Application.dataPath, "")}";
            string savePath = $"{AssetDatabase.GetAssetPath(Selection.activeInstanceID)}/NewFeatureLibrary.asset";

            Debug.Log($"Load path: {loadPath}");
            Debug.Log($"Save path: {savePath}");
            string[] assets = AssetDatabase.FindAssets("t:FeatureAsset", new string[] { loadPath });

            FeatureLibrary library = CreateInstance<FeatureLibrary>();
            foreach (string asset in assets)
            {
                library.Add(asset, AssetDatabase.LoadAssetAtPath<FeatureAsset>(asset));
            }

            ProjectWindowUtil.CreateAsset(library, savePath);
        }
#endif

        private FeatureLibrary(int capacity)
        {
            nameToFAID = new Dictionary<string, ushort>();
            features = new FeatureAsset[capacity];
        }

        private bool Add(string name, FeatureAsset feature)
        {
            if (name == null || feature == null)
                return false;

            nameToFAID.Add(name, (ushort)nameToFAID.Count);
            features[nameToFAID.Count] = feature;

            return true;
        }

        public bool Contains(string name) => nameToFAID.ContainsKey(name);

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
    }
}