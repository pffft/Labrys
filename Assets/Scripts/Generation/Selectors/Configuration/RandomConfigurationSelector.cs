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

        public Feature.Configuration Select(Feature.Configuration[] configurations)
        {
            return configurations[Random.Range(0, configurations.Length)];
        }
    }
}
