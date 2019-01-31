using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys
{
    public class GridSectionCache
    {
        private Dictionary<Vector2Int, Section> hashCache;
        private CacheElement[] cache;
        private int currentPosition;
        private int size;
        private int count;

        public struct CacheElement
        {
            public Vector2Int position;
            public Section section;
        }

        public GridSectionCache(int size)
        {
            cache = new CacheElement[size];
            for (int i = 0; i < size; i++)
            {
                cache[i] = new CacheElement();
            }
            hashCache = new Dictionary<Vector2Int, Section>(size);
            this.size = size;
        }

        public Section? Get(Vector2Int position)
        {

            return hashCache.TryGetValue(position, out Section section) ? section : (Section?)null;

            /*
            for (int i = 0; i < Mathf.Min(size, count); i++)
            {
                if (position == cache[i].position)
                {
                    return cache[i];
                }
            }
            return null;
            */
        }

        public void Add(Vector2Int position, Section section)
        {
            if (count >= size)
            {
                hashCache.Remove(cache[currentPosition].position);
            }
            hashCache.Add(position, section);

            cache[currentPosition].position = position;
            cache[currentPosition].section = section;

            currentPosition = (currentPosition + 1) % size;
            count++;
        }
    }
}
