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
    //[CreateAssetMenu(fileName = "NewFeature", menuName = "Labrys/RoomFeature")]
    //[System.Serializable]
    public class Feature// : ScriptableObject
    {
        private Dictionary<Vector2Int, Section> elements;

        public int minX, minY, maxX, maxY;
        private int offsetX, offsetY;
        private int rotation;

        public Feature() {
            this.elements = new Dictionary<Vector2Int, Section>();

            // Set good defaults, so every addition will change these values
            this.minX = int.MaxValue;
            this.minY = int.MaxValue;
            this.maxX = int.MinValue;
            this.maxY = int.MinValue;
        }

        // Seperate x,y add
        public void Add(int x, int y, Connection allowedConnections = Connection.All, string variant = "default", Connection externalConnections = Connection.All)
        {
            Add(new Vector2Int(x, y), new Section(allowedConnections, externalConnections, variant));
        }

        // Vector2Int add
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
        //public Feature Rotate(int rotation)
        //{

        //    Feature rotatedFeature = new Feature();
        //    // Transform every position (x, y) -> (y, -x) to rotate clockwise 90 degrees.
        //    foreach (KeyValuePair<Vector2Int, Section> pair in elements)
        //    {
        //        Vector2Int pos = pair.Key;
        //        rotatedFeature.Add(new Vector2Int(pos.y, -pos.x), pair.Value);
        //    }

        //    return rotatedFeature;
        //}

        /// <summary>
        /// Rotates this Feature clockwise 90 degrees "rotation" amount of times.
        /// For example, to flip this Feature upside-down, call Rotate(2).
        /// </summary>
        /// <param name="rotation">Rotation.</param>
        private void Rotate(int rot) 
        {
            this.rotation = (int)Mathf.Repeat(this.rotation + rot, 4);
        }

        private void Offset(Vector2Int position) 
        {
            Offset(position.x, position.y);
        }

        private void Offset(int x, int y) 
        {
            this.offsetX += x;
            this.offsetY += y;

            this.minX += x;
            this.minY += y;

            this.maxX += x;
            this.maxY += y;
        }

        private Vector2Int RotatedMin()
        {
            switch (this.rotation)
            {
                case 0: return Rotate(new Vector2Int(minX, minY), rotation);
                case 1: return Rotate(new Vector2Int(maxX, minY), rotation);
                case 2: return Rotate(new Vector2Int(maxX, maxY), rotation);
                case 3: return Rotate(new Vector2Int(minX, maxY), rotation);
                default: return Rotate(new Vector2Int(minX, minY), rotation);
            }
        }

        // Rotates the given position 90 degrees clockwise "rot" number of times.
        public static Vector2Int Rotate(Vector2Int pos, int rot)
        {
            switch ((int)Mathf.Repeat(rot, 4))
            {
                case 0: return pos;
                case 1: return new Vector2Int(pos.y, -pos.x);
                case 2: return new Vector2Int(-pos.x, -pos.y);
                case 3: return new Vector2Int(-pos.y, pos.x);
                default: return pos;
            }
        }

        // Takes a corner of the bounding box, and rotates it around.
        // I.e., rot = 0 gives bottom-left corner. Rot = 1 gives bottom-right,
        // but rotated 90 degrees clockwise so it's the bottom-left corner.
        public Vector2Int RotatedMin(int rot)
        {
            switch ((int)Mathf.Repeat(rot, 4))
            {
                case 0: return Rotate(new Vector2Int(minX, minY), rot);
                case 1: return Rotate(new Vector2Int(maxX, minY), rot);
                case 2: return Rotate(new Vector2Int(maxX, maxY), rot);
                case 3: return Rotate(new Vector2Int(minX, maxY), rot);
                default: return Rotate(new Vector2Int(minX, minY), rot);
            }
        }

        public List<Vector2Int> GetTransformedSections(Vector2Int offset, int rotation) 
        {
            List<Vector2Int> toReturn = new List<Vector2Int>();

            foreach (Vector2Int section in elements.Keys)
            {
                toReturn.Add(Rotate(section + offset, rotation));
            }

            return toReturn;
        }

        public bool CanConnect(Grid placedSections, Vector2Int position, Connection connection) 
        {
            return false;
        }

        /// <summary>
        /// Can we place this Feature at the provided position?
        /// </summary>
        /// <returns><c>true</c>, if place was caned, <c>false</c> otherwise.</returns>
        /// <param name="placedSections">The grid, containing all the Sections we wish to avoid.</param>
        /// <param name="gridPosition">The grid position we want to place the Feature at.</param>
        /// <param name="localPosition">The location of the Section we want to place at "gridPosition".</param>
        /// <param name="rot">Rotation.</param>
        public bool CanPlace(Grid placedSections, Vector2Int gridPosition, Vector2Int localPosition, int rot)
        {
            //Offset(localPosition);
            //Offset(new Vector2Int(-localPosition.x, -localPosition.y));
            Rotate(rot);

            foreach (Vector2Int sectionPosition in elements.Keys)
            {
                Vector2Int transformedOffset = Rotate(localPosition, rotation);

                Vector2Int rotatedPoint = Rotate(sectionPosition, rotation);

                //Vector2Int transformedPoint = rotatedPoint;
                //Vector2Int rotatedMin = new Vector2Int(minX, minY);
                Vector2Int rotatedMin = RotatedMin();

                //Vector2Int normalized = rotatedPoint - rotatedMin - transformedOffset;
                Vector2Int normalized = rotatedPoint - transformedOffset;

                if (placedSections[gridPosition + normalized] != null)
                {
                    // Undo transforms
                    //Offset(localPosition);
                    //Offset(new Vector2Int(-localPosition.x, -localPosition.y));
                    Rotate(-rot);
                    return false;
                }
            }

            // Undo transforms
            //Offset(new Vector2Int(-localPosition.x, -localPosition.y));
            //Offset(localPosition);
            Rotate(-rot);

            return true;

            //foreach (Vector2Int sectionPosition in elements.Keys) 
            //{
            //    /*
            //     * Grid position is where we want to place the Feature at.
            //     * Local position is the offset from the (0, 0) point of the Feature.
            //     * Section position is the section in the Feature we try to connect with.
            //     */
            //    //Vector2Int absolutePosition = gridPosition + sectionPosition - localPosition;

            //    // TODO this is a bit buggy. Check if local position is being applied properly under rotation.

            //    // Rotate the position in our dictionary
            //    Vector2Int rotatedPos = Rotate(sectionPosition - localPosition, rotation);
            //    Vector2Int rotatedMin = RotatedMin(rotation);

            //    // Normalize the position so (0, 0) is the minimum positioned element
            //    Vector2Int normalized = rotatedPos - rotatedMin;

            //    //Vector2Int rotatedLocalPos = Rotate(localPosition, rotation);
            //    //Vector2Int normalizedWithOffset = normalized - rotatedLocalPos;

            //    Vector2Int absolutePosition = gridPosition + normalized;

            //    if (placedSections[absolutePosition] != null) 
            //    {
            //        return false;
            //    }
            //}

            return true;
        }
    }
}
