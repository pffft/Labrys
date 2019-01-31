using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys
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
        void Load(GameObject toLoad, Vector3 position, Quaternion rotation, Vector3 scale);

        void LastGameObjectSent();
    }
}
