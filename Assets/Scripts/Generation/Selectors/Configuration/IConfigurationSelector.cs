using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Generation.Selectors
{
    public interface IConfigurationSelector
    {
        /// <summary>
        /// Initialize this ConfigurationSelector.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Select one of the valid configurations to take at this step of generation.
        /// </summary>
        /// <returns>The chosen configuration.</returns>
        Feature.PlacementConfiguration Select(Feature.PlacementConfiguration[] configurations);
    }
}
