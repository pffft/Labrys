using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Generation.Selectors
{
    /// <summary>
    /// Given a known Feature and position, there can be multiple valid Configurations
    /// in which we can validly place this Feature. This represents the logic to
    /// choose one of these Configurations.
    /// </summary>
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
        Feature.Configuration Select(Feature.Configuration[] configurations);
    }
}
