using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys
{
    /// <summary>
    /// A logical collection of Section objects. A Feature contains a list of
    /// Section objects, their positions, information about the variations they
    /// can take on (either hard-coded, or algorithmically determined), how this
    /// feature can connect externally.
    /// 
    /// Position -> Section, variation, external connections
    /// Boundary information (lowest/highest X/Y for normalization)
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "NewFeature", menuName = "Labrys/RoomFeature")]
    [System.Serializable]
    public class Feature : ScriptableObject
    {
        private Dictionary<Vector2Int, Section> elements;

        private int minX, minY, maxX, maxY;

        public void Add(Vector2Int position, Connection allowedConnections = Connection.All, string variant = "default", Connection externalConnections = Connection.All)
        {
            Add(position, new Section(allowedConnections, externalConnections, variant));
        }

        private void Add(Vector2Int position, Section element)
        {
            // Update the boundaries
            minX = Mathf.Min(position.x, minX);
            maxX = Mathf.Max(position.x, maxX);

            minY = Mathf.Min(position.y, minY);
            maxY = Mathf.Max(position.y, maxY);

            // Add the section into the feature. If it exists, replace, but warn.
            if (elements.ContainsKey(position))
            {
                elements[position] = element;
                Debug.LogWarning($"Overwrote Section at position {position} with new one, with variant \"{element.GetVariant()}\".");
            }
            else
            {
                elements.Add(position, element);
            }
        }

        /// <summary>
        /// Returns this Feature, but rotated (rotation * 90) degrees clockwise.
        /// Used for generator trying to place Features into the world.
        /// 
        /// TODO: can make this implicit by just changing the input coordinates
        /// </summary>
        /// <returns>The rotate.</returns>
        /// <param name="rotation">Rotation.</param>
        public Feature Rotate(int rotation)
        {

            Feature rotatedFeature = new Feature();
            // Transform every position (x, y) -> (y, -x) to rotate clockwise 90 degrees.
            foreach (KeyValuePair<Vector2Int, Section> pair in elements)
            {
                Vector2Int pos = pair.Key;
                rotatedFeature.Add(new Vector2Int(pos.y, -pos.x), pair.Value);
            }

            return rotatedFeature;
        }
    }
}
