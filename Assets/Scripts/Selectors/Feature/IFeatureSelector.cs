using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Selectors
{
    public interface IFeatureSelector
    {
        /// <summary>
        /// Initialize this FeatureSelector.
        /// </summary>
        /// <param name="features">A list of all known Features.</param>
        void Initialize(Feature[] features);

        /// <summary>
        /// Choose the next Feature to be placed into the dungeon.
        /// </summary>
        /// <returns>The chosen Feature.</returns>
        /// <param name="features">A list of all known Features.</param>
        Feature Select(Feature[] features);
    }
}
