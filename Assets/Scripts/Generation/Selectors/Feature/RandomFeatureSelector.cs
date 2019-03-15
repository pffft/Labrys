using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Generation.Selectors
{
    public class RandomFeatureSelector : IFeatureSelector
    {
        public void Initialize(Feature[] features)
        {
        }

        public Feature Select(Feature[] features)
        {
            //if (Random.Range(0, 1f) < 0.9f) {
            //    return features[0];
            //}
            //return features[1];
            return features[Random.Range(0, features.Length)];
        }
    }
}
