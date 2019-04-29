using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Generation
{
    /// <summary>
    /// A lazy loader implementation. Will do nothing until it has received every
    /// GameObject that will be created, and then instantiate a certain amount per
    /// frame. The order in which the objects are loaded can be specified.
    /// </summary>
    public class LazyLoader : IGameObjectLoader
    {
        public enum Priority
        {
            OriginalOrder,
            Distance
        }

        private class GameObjectPayload
        {
            public GameObject obj;
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
        }

        private List<GameObjectPayload> unsortedObjects = new List<GameObjectPayload>();
        private Queue<GameObjectPayload> objectsToLoad = new Queue<GameObjectPayload>();

        private Priority priority;
        private int amountPerFrame;

        public LazyLoader(Priority loadPriority, int amountPerFrame = 250)
        {
            this.priority = loadPriority;
            this.amountPerFrame = amountPerFrame;
        }

        public void Load(GameObject toLoad, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            unsortedObjects.Add(new GameObjectPayload
            {
                obj = toLoad,
                position = position,
                rotation = rotation,
                scale = scale
            });

            //objectsToLoad.Enqueue();
        }

        public void LastGameObjectSent() 
        {
            // Sort the objects by any parameters specified
            if (priority == Priority.Distance)
            {
                unsortedObjects.Sort((a, b) => { return (int)(a.position.sqrMagnitude - b.position.sqrMagnitude); });
            }

            // Add the sorted objects to the queue
            for (int i = 0; i < unsortedObjects.Count; i++)
            {
                objectsToLoad.Enqueue(unsortedObjects[i]);
            }

            Generator.Instance.StartCoroutine(LazyLoad());
        }

        private IEnumerator LazyLoad()
        {
            while (objectsToLoad.Count > 0)
            {
                for (int i = 0; i < Mathf.Min(amountPerFrame, objectsToLoad.Count); i++)
                {
                    GameObjectPayload nextPayload = objectsToLoad.Dequeue();

                    GameObject go = GameObject.Instantiate(nextPayload.obj, nextPayload.position, nextPayload.rotation);
                    go.transform.localScale = nextPayload.scale;
                }
                yield return null;
            }

            yield return null;
        }
    }
}
