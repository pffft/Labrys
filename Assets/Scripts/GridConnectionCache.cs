using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys
{
    public class GridConnectionCache
    {
        private Dictionary<Vector2Int, Connection> hashCache; 
        private CacheElement[] cache;
        private int currentPosition;
        private int size;
        private int count;

        public struct CacheElement
        {
            public Vector2Int position;
            public Connection connection;
        }

        public GridConnectionCache(int size)
        {
            cache = new CacheElement[size];
            for (int i = 0; i < size; i++)
            {
                cache[i] = new CacheElement();
            }
            hashCache = new Dictionary<Vector2Int, Connection>(size);
            this.size = size;
        }

        public Connection? Get(Vector2Int position)
        {
            if (hashCache.TryGetValue(position, out Connection connection))
            {
                return connection;
            }
            else
            {
                return null;
            }
        }

        public void Add(Vector2Int position, Connection connection)
        {
            if (count >= size)
            {
                hashCache.Remove(cache[currentPosition].position);
            }
            hashCache.Add(position, connection);

            cache[currentPosition].position = position;
            cache[currentPosition].connection = connection;

            currentPosition = (currentPosition + 1) % size;
            count++;
        }
    }
}
