using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Generation
{
    /// <summary>
    /// An interface providing GameObject loading functionality.
    /// 
    /// This is called as the last step in the Generator, after Sections have
    /// been resolved to physical GameObjects containing a Tile component; this
    /// method allows implementations to have lazy loading.
    /// </summary>
    public interface IGameObjectLoader
    {
        /// <summary>
        /// Load the specified toLoad, position, rotation and scale.
        /// </summary>
        /// <param name="toLoad">To load.</param>
        /// <param name="position">Position.</param>
        /// <param name="rotation">Rotation.</param>
        /// <param name="scale">Scale.</param>
        void Load(GameObject toLoad, Vector3 position, Quaternion rotation, Vector3 scale);

        void LastGameObjectSent();
    }
}
