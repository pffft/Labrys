using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Labrys.Generation;
using Labrys.FeatureEditor;

namespace Tests
{
    public class FeatureAssetTest
    {
        [Test]
        public void CanConvertToFromFeature()
        {
            Feature feature1 = new Feature();
            feature1.Add(0, 0);
            feature1.Add(1, 0);
            feature1.Add(2, 0);

            FeatureAsset featureAsset1 = FeatureAsset.FromFeature(feature1);
            Feature testFeature1 = featureAsset1.ToFeature();

            // Basic tests- all input should be present in output
            //Assert.Contains(new Vector2Int(0, 0), testFeature1.Elements.Keys);
            //Assert.Contains(new Vector2Int(1, 0), testFeature1.Elements.Keys);
            //Assert.Contains(new Vector2Int(2, 0), testFeature1.Elements.Keys);

            // Harder test- collections must be equivalent to satisfy identity
            Debug.Log("Input");
            foreach (KeyValuePair<Vector2Int, Section> kvp in feature1.Elements) 
            {
                Debug.Log(kvp.Key + ", Section: " + kvp.Value.ToString());
            }
            Debug.Log("Output");
            foreach (KeyValuePair<Vector2Int, Section> kvp in testFeature1.Elements)
            {
                Debug.Log(kvp.Key + ", Section: " + kvp.Value.ToString());
            }

            CollectionAssert.AreEqual(feature1.Elements, testFeature1.Elements);


            // Ensure bounds are unchanged
            Assert.AreEqual(feature1.MaxX, testFeature1.MaxX);
            Assert.AreEqual(feature1.MaxY, testFeature1.MaxY);
            Assert.AreEqual(feature1.MinX, testFeature1.MinX);
            Assert.AreEqual(feature1.MinY, testFeature1.MinY);
        }
    }
}