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
            return features[Random.Range(0, features.Length)];
        }
    }
}
