using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Labrys.Generation
{
    /// <summary>
    /// A logical collection of Section objects. A Feature contains a list of
    /// Section objects, their positions, information about the variations they
    /// can take on (either hard-coded, or algorithmically determined), and how 
    /// this feature can connect internally or externally.
    /// </summary>
    public class Feature
    {
        /// <summary>
        /// Dictionary representing the Sections in this Feature. 
        /// </summary>
        /// <value>The elements.</value>
        public Dictionary<Vector2Int, Section> Elements { get; private set; }

        public int MinX { get; private set; }
        public int MinY { get; private set; }
        public int MaxX { get; private set; }
        public int MaxY { get; private set; }

        public Feature() {
            this.Elements = new Dictionary<Vector2Int, Section>();

            // This sets good defaults, so any addition will change these values
            this.MinX = int.MaxValue;
            this.MinY = int.MaxValue;
            this.MaxX = int.MinValue;
            this.MaxY = int.MinValue;
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
            MinX = Mathf.Min(position.x, MinX);
            MaxX = Mathf.Max(position.x, MaxX);

            MinY = Mathf.Min(position.y, MinY);
            MaxY = Mathf.Max(position.y, MaxY);

            // Add the section into the feature. If it exists, replace, but warn.
            if (Elements.ContainsKey(position))
            {
                Elements[position] = element;
                Debug.LogWarning($"Overwrote Section at position {position} with new one, with variant \"{element.GetVariant()}\".");
            }
            else
            {
                Elements.Add(position, element);
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

        // Takes a corner of the bounding box, and rotates it around. This will
        // return the new bottom-left corner for a given rotation.
        // 
        // I.e., rot = 0 gives bottom-left corner. Rot = 1 gives bottom-right,
        // but rotated 90 degrees clockwise so it's the bottom-left corner.
        public Vector2Int RotatedMin(int rot)
        {
            switch ((int)Mathf.Repeat(rot, 4))
            {
                case 0: return Rotate(new Vector2Int(MinX, MinY), rot);
                case 1: return Rotate(new Vector2Int(MaxX, MinY), rot);
                case 2: return Rotate(new Vector2Int(MaxX, MaxY), rot);
                case 3: return Rotate(new Vector2Int(MinX, MaxY), rot);
                default: return Rotate(new Vector2Int(MinX, MinY), rot);
            }
        }

        /// <summary>
        /// Returns a list of valid configurations at the provided grid position.
        /// 
        /// A configuration is uniquely defined by the grid position, the local position (which
        /// represents the specific Section we place at the adjacent grid position), and the
        /// rotation used. Additional information is provided to aid in selecting an interesting
        /// configuration in generation (such as the direction we connected in).
        /// 
        /// A configuration is considered valid if every Section within this Feature can be
        /// placed onto the Grid without overlapping any existing Grid Sections.
        /// 
        /// 
        /// 
        /// TODO find some way to optimize this, if it's possible to do so. Maybe a cache of recently checked
        /// positions, placed in the CanConnect method? This would amortize O(n^2) checks into O(n)? unique
        /// lookups, but O(n^2) comparisons still. 
        /// 
        /// Another idea: statically maintain a list of "internal" Sections within each Feature, and ignore those
        /// as seed positions within the CheckPlacement method. The thinking being internal points would never have
        /// any external ability to connect. Theoretically still O(n^2) (case study: space-filling curves have n^2
        /// surface area and take up n^2 space), but in practice, most Features are likely to have a linear exposed
        /// area.
        /// 
        /// Tangent from the above idea. Space filling curves like the spiral or comb have O(n^2) surface area, but
        /// need "thin" or "small" connection points to be satisfied. Because we try to place Features off of a single 
        /// Section, we aren't being smart about how it can connect, so we allow bad cases like this. If we take the
        /// neighboring e.g. 2-3 Sections in the Grid, we can try fitting small polygons within the Feature instead of
        /// individual Sections. If the small polygon is logarithmic in size [width * height = O(log^2(n))], then the 
        /// "gaps" in the space filling curve must be at least logarithmic as well [O(sqrt(log^2(n))) == O(log(n))]. This 
        /// might mean that the worst-case behavior is avoided, because the large gaps might force the number of Sections 
        /// to be O(n log n) instead of O(n^2).
        /// 
        /// Another idea: Change the iteration order of the CanPlace method to check nearby Sections first, to
        /// increase the likelihood of breaking out early when checking for overlaps. Or check Grid sections near
        /// the bounding box? Some heuristics might be faster than others. 
        /// 
        /// Another idea: Do a bounding box check first, if there's a way to do a cheap check in the global grid.
        /// 
        /// Another idea: Keep track if this Feature has rotational symmetry (either 2 way or 4 way). If so, we can
        /// memoize the different rotation checks for up to 4x speedup in 4-way rotationally symmetrical cases.
        /// 
        /// </summary>
        /// <returns>A List containing all valid configurations found.</returns>
        /// <param name="placedSections">The grid, containing the positions of Sections already placed.</param>
        /// <param name="gridPosition">The position in the grid we want to place the Feature at.</param>
        public List<Configuration> CanConnect(Grid placedSections, Vector2Int gridPosition) 
        {
            Section? maybeGridSection = placedSections[gridPosition];
            if (maybeGridSection == null)
            {
                // Empty Section is invalid, we always need to be passed in a full one.
                return new List<Configuration>();
            }
            Section gridSection = maybeGridSection.Value;

            // A cache of the configurations we've seen while trying to place this Feature.
            //
            // Two configurations are considered equal if their gridPosition+localPosition and rotation are equal.
            // 
            // This provides up to 4x speedup. Can be achieved with dense Features (approaching filled square
            // or a checkerboard pattern). 
            // 
            // Motivating 2x2 Feature example: checking if the top-right corner can be placed North of a given
            // Section is equivalent to checking if the bottom-left corner can be placed West of the same Section.
            // With larger dense Features, Sections away from the edges will produce up to 4 duplicate placement checks.
            Dictionary<Configuration, bool> cache = new Dictionary<Configuration, bool>();

            // Build up all possible valid configurations we'll return
            List<Configuration> toReturn = new List<Configuration>();

            // Try to place the Feature at all adjacent grid positions
            CheckPlacements(gridPosition + Vector2Int.up);
            CheckPlacements(gridPosition + Vector2Int.right);
            CheckPlacements(gridPosition + Vector2Int.down);
            CheckPlacements(gridPosition + Vector2Int.left);

            return toReturn;

            // Checks all possible configurations of placing this Feature down at
            // a given grid position. Add them to the returning list
            void CheckPlacements(Vector2Int pos)
            {
                // If the provided input position already has a Section, then trivially
                // any Section we try to place here will conflict. So we return early.
                if (placedSections[pos] != null) 
                {
                    return;
                }

                // Else try every possible local position.
                foreach (KeyValuePair<Vector2Int, Section> element in Elements)
                {
                    // And every possible rotational variant for each.
                    for (int rot = 0; rot < 4; rot++)
                    {
                        Configuration configuration = new Configuration(pos, element.Key, rot);

                        // Have we already tried this configuration? Check the cache.
                        if (!cache.TryGetValue(configuration, out bool canPlace))
                        {
                            // If it's not in there, compute it manually and add it.
                            canPlace = CanPlace(placedSections, configuration);
                            cache[configuration] = canPlace;
                        } else {
                            Debug.Log("Cache hit!");
                        }

                        // If the configuration is valid (no overlaps w/ grid), add it
                        if (canPlace) 
                        {
                            toReturn.Add(configuration);
                        }
                    }
                }
            }
        }

        public bool CanPlace(Grid placedSections, Configuration configuration)
        {
            return CanPlace(placedSections, configuration.gridPosition, configuration.localPosition, configuration.rotation);
        }

        /// <summary>
        /// Can we place this Feature at the provided position?
        /// 
        /// </summary>
        /// <returns><c>true</c>, if the Feature could be placed, <c>false</c> otherwise.</returns>
        /// <param name="placedSections">The grid, containing all the Sections we wish to avoid.</param>
        /// <param name="gridPosition">The grid position we want to place the Feature at.</param>
        /// <param name="localPosition">The location of the Section we want to place at "gridPosition".</param>
        /// <param name="rotation">Rotation.</param>
        public bool CanPlace(Grid placedSections, Vector2Int gridPosition, Vector2Int localPosition, int rotation)
        {
            foreach (Vector2Int sectionPosition in Elements.Keys)
            {
                // Shift this Section's position over by the offset, "localPosition". 
                // This places "localPosition" at (0, 0). 
                // Then rotate the point by the provided rotation.
                //
                // Because "localPosition" is provided in the same coordinates as "sectionPosition",
                // the resulting position is already normalized. 
                Vector2Int rotatedPoint = Rotate(sectionPosition - localPosition, rotation);

                if (placedSections[gridPosition + rotatedPoint] != null)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a copy of this Feature's Sections, but placed in a given Configuration.
        /// 
        /// This will translate and rotate the positions of each Section in the same way that
        /// CanPlace uses to check the validity of a Configuration.
        /// 
        /// TODO use this method in CanPlace, and just check against the Grid. 
        /// TODO Also find a way to optimize this so we don't duplicate every entry and transform every time-
        /// maybe a duplicate dictionary used for temporary transforms? 
        /// </summary>
        /// <returns>The configuration.</returns>
        /// <param name="configuration">Configuration.</param>
        public Dictionary<Vector2Int, Section> GetConfiguration(Configuration configuration) 
        {
            Dictionary<Vector2Int, Section> toReturn = new Dictionary<Vector2Int, Section>();
            foreach (KeyValuePair<Vector2Int, Section> keyValuePair in Elements) 
            {
                toReturn.Add(
                    configuration.gridPosition + Rotate(keyValuePair.Key - configuration.localPosition, configuration.rotation),
                    keyValuePair.Value
                );
            }
            return toReturn;
        }


        public struct Configuration
        {
            // These 3 variables are needed to uniquely identify this configuration
            public readonly Vector2Int gridPosition;
            public readonly Vector2Int localPosition;
            public readonly int rotation;

            // Any other variables are useful, but not necessary.

            public Configuration(Vector2Int gridPosition, Vector2Int localPosition, int rotation) 
            {
                this.gridPosition = gridPosition;
                this.localPosition = localPosition;
                this.rotation = rotation;
            }

            // Hashcode to determine unique configurations
            // (gridPosition + localPosition) uniquely determines the world position,
            // so they're treated as one variable in the hashcode.
            public override int GetHashCode()
            {
                return ((gridPosition + localPosition).GetHashCode() << 2) | rotation;
            }

            // Two configurations are equal if they require identical checks-
            // specifically, if gridPosition + localPosition and rotation are equal,
            // then CanPlace will check exactly the same tiles.
            public override bool Equals(object obj)
            {
                if (!(obj is Configuration)) 
                {
                    return false;
                }
                Configuration other = (Configuration)obj;

                return (gridPosition + localPosition == other.gridPosition + other.localPosition) && 
                    rotation == other.rotation;
            }
        }
    }
}
