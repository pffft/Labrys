
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Generation
{
    /// <summary>
    /// A basic GameObject loader. Will try to instantiate the GameObject as soon
    /// as it is received, and block until it is done.
    /// </summary>
    public class BasicLoader : IGameObjectLoader
    {
        public void Load(GameObject toLoad, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            GameObject go = GameObject.Instantiate(toLoad, position, rotation);
            go.transform.localScale = scale;
        }

        public void LastGameObjectSent() { }
    }
}
