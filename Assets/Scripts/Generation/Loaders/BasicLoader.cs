
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Generation
{
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
