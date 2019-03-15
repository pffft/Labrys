using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Generation.Selectors
{
    public class RandomConfigurationSelector : IConfigurationSelector
    {
        public void Initialize()
        {
        }

        public Feature.PlacementConfiguration Select(Feature.PlacementConfiguration[] configurations)
        {
            Debug.Log(configurations.Length);
            int chosen = Random.Range(0, configurations.Length);
            Debug.Log("Chose: " + chosen);
            return configurations[chosen];
        }
    }
}
