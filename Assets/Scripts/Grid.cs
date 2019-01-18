using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Labrys
{
    public class Grid
    {
        private List<Feature> features;
        private Dictionary<Vector2Int, Section> globalGrid;

        public Grid()
        {
            features = new List<Feature>();
            globalGrid = new Dictionary<Vector2Int, Section>();
        }

        public Section this[Vector2Int pos]
        {
            get
            {
                if (globalGrid.TryGetValue(pos, out Section val))
                {
                    return val;
                }
                return null;
            }

            set
            {
                if (value == null)
                {
                    return;
                }

                if (globalGrid.ContainsKey(pos))
                {
                    //Debug.LogWarning($"Overriding existing Section at {pos}.");
                    globalGrid[pos] = value;
                }
                else
                {
                    globalGrid.Add(pos, value);
                }
            }
        }

        public Vector2Int[] GetFullCells()
        {
            Vector2Int[] toReturn = new Vector2Int[globalGrid.Count];
            globalGrid.Keys.CopyTo(toReturn, 0);
            return toReturn;
        }

        /// <summary>
        /// If a Section has a neighbor, sets a Connection flag. Returns the combined
        /// set of Connection flags.
        /// 
        /// TODO: optimize this method slightly. TryGetValue is relatively slow,
        /// and is called 16 times when it can be called as few as 8 times.
        /// CanConnect is already quite fast, so can be called more often.
        /// 
        /// </summary>
        /// <returns>The physical adjacencies.</returns>
        /// <param name="position">Position.</param>
        public Connection GetPhysicalAdjacencies(Vector2Int position)
        {
            Connection physicalAdjacent = Connection.None;

            Section thisSection = globalGrid[position];

            /*
             * Cardinal directions are connected iff (using North as example):
             * 
             * thisSection is allowed to connect North
             * there is a Section to the North
             * the North Section is allowed to connect South
             */

            /*
             * Diagonal connections are connected iff (using NE as example):
             * 
             * thisSection is allowed to connect North, East, and Northeast
             * there is a Section to the North, East, and Northeast
             * the North Section is allowed to connect South, East, and Southeast
             * the East Section is allowed to connect West, North, and Northwest
             * the Northeast Section is allowed to connect West, South, and Southwest
             * 
             * Since connections and adjacencies are 1:1 if allowed, we're good.
             */

            // North
            if (thisSection.CanConnect(Connection.North))
            {
                if (globalGrid.TryGetValue(position + Vector2Int.up, out Section up))
                {
                    if (up.CanConnect(Connection.South))
                    {
                        physicalAdjacent |= Connection.North;
                    }
                }
            }

            // East
            if (thisSection.CanConnect(Connection.East))
            {
                if (globalGrid.TryGetValue(position + Vector2Int.right, out Section right))
                {
                    if (right.CanConnect(Connection.West))
                    {
                        physicalAdjacent |= Connection.East;
                    }
                }
            }

            // South
            if (thisSection.CanConnect(Connection.South))
            {
                if (globalGrid.TryGetValue(position + Vector2Int.down, out Section down))
                {
                    if (down.CanConnect(Connection.North))
                    {
                        physicalAdjacent |= Connection.South;
                    }
                }
            }

            // West
            if (thisSection.CanConnect(Connection.West))
            {
                if (globalGrid.TryGetValue(position + Vector2Int.left, out Section left))
                {
                    if (left.CanConnect(Connection.East))
                    {
                        physicalAdjacent |= Connection.West;
                    }
                }
            }

            // NW
            if (thisSection.CanConnect(Connection.West | Connection.North | Connection.Northwest))
            {
                bool okay = true;

                if (globalGrid.TryGetValue(position + Vector2Int.up, out Section up))
                {
                    okay &= up.CanConnect(Connection.West | Connection.South | Connection.Southwest);
                }
                else
                {
                    okay = false;
                }

                if (globalGrid.TryGetValue(position + Vector2Int.left, out Section left))
                {
                    okay &= left.CanConnect(Connection.North | Connection.East | Connection.Northeast);
                }
                else
                {
                    okay = false;
                }

                if (globalGrid.TryGetValue(position + Vector2Int.up + Vector2Int.left, out Section upLeft))
                {
                    okay &= upLeft.CanConnect(Connection.East | Connection.South | Connection.Southeast);
                }
                else
                {
                    okay = false;
                }

                physicalAdjacent |= okay ? Connection.Northwest : Connection.None;
            }

            // NE
            if (thisSection.CanConnect(Connection.North | Connection.East | Connection.Northeast))
            {
                bool okay = true;

                if (globalGrid.TryGetValue(position + Vector2Int.up, out Section up))
                {
                    okay &= up.CanConnect(Connection.East | Connection.South | Connection.Southeast);
                }
                else
                {
                    okay = false;
                }

                if (globalGrid.TryGetValue(position + Vector2Int.right, out Section right))
                {
                    okay &= right.CanConnect(Connection.North | Connection.West | Connection.Northwest);
                }
                else
                {
                    okay = false;
                }

                if (globalGrid.TryGetValue(position + Vector2Int.up + Vector2Int.right, out Section upRight))
                {
                    okay &= upRight.CanConnect(Connection.West | Connection.South | Connection.Southwest);
                }
                else
                {
                    okay = false;
                }

                physicalAdjacent |= okay ? Connection.Northeast : Connection.None;
            }

            // SW
            if (thisSection.CanConnect(Connection.West | Connection.South | Connection.Southwest))
            {
                bool okay = true;

                if (globalGrid.TryGetValue(position + Vector2Int.down, out Section down))
                {
                    okay &= down.CanConnect(Connection.West | Connection.North | Connection.Northwest);
                }
                else
                {
                    okay = false;
                }

                if (globalGrid.TryGetValue(position + Vector2Int.left, out Section left))
                {
                    okay &= left.CanConnect(Connection.South | Connection.East | Connection.Southeast);
                }
                else
                {
                    okay = false;
                }

                if (globalGrid.TryGetValue(position + Vector2Int.down + Vector2Int.left, out Section downLeft))
                {
                    okay &= downLeft.CanConnect(Connection.East | Connection.North | Connection.Northeast);
                }
                else
                {
                    okay = false;
                }

                physicalAdjacent |= okay ? Connection.Southwest : Connection.None;
            }

            // SE
            if (thisSection.CanConnect(Connection.South | Connection.East | Connection.Southeast))
            {
                bool okay = true;

                if (globalGrid.TryGetValue(position + Vector2Int.down, out Section down))
                {
                    okay &= down.CanConnect(Connection.East | Connection.North | Connection.Northeast);
                }
                else
                {
                    okay = false;
                }

                if (globalGrid.TryGetValue(position + Vector2Int.right, out Section right))
                {
                    okay &= right.CanConnect(Connection.South | Connection.West | Connection.Southwest);
                }
                else
                {
                    okay = false;
                }

                if (globalGrid.TryGetValue(position + Vector2Int.down + Vector2Int.right, out Section downRight))
                {
                    okay &= downRight.CanConnect(Connection.West | Connection.North | Connection.Northwest);
                }
                else
                {
                    okay = false;
                }

                physicalAdjacent |= okay ? Connection.Southeast : Connection.None;
            }

            return physicalAdjacent;
        }

        // The adjacent positions, in order
        private static Vector2Int[] adjacentOffsets =
        {
            Vector2Int.right,
            Vector2Int.right + Vector2Int.up,
            Vector2Int.up,
            Vector2Int.left + Vector2Int.up,
            Vector2Int.left,
            Vector2Int.left + Vector2Int.down,
            Vector2Int.down,
            Vector2Int.right + Vector2Int.down
        };

        // All the connections, in counterclockwise order.
        // More are added to prevent the need for modulo operators.
        private static Connection[] connectionsOrdered =
        {
            Connection.East,
            Connection.Northeast,
            Connection.North,
            Connection.Northwest,
            Connection.West,
            Connection.Southwest,
            Connection.South,
            Connection.Southeast,

            Connection.East, // 8
            Connection.Northeast,
            Connection.North,
            Connection.Northwest // 11
        };
        private static Section[] adjacentSections = new Section[8];
        //private Dictionary<Vector2Int, Section> sectionCache = new Dictionary<Vector2Int, Section>(10);
        //private static LinkedList<System.Tuple<Vector2Int, Section>> sectionCache = new LinkedList<System.Tuple<Vector2Int, Section>>();
        private GridSectionCache sectionCache = new GridSectionCache(500);

        private static Vector2Int positionPlusOffset;
        private static Section[] TwoByTwoArea = new Section[4];

        public Connection GetPhysicalAdjacencies2(Vector2Int position)
        {
            Connection physicalAdjacent2 = Connection.None;

            Section thisSection = globalGrid[position];

            Profiler.BeginSample("Basic checks");
            // Build up a cache, do basic checks (sufficient for cardinal directions)
            for (int i = 0; i < 8; i++)
            {
                if (thisSection.CanConnect(connectionsOrdered[i]))
                {
                    positionPlusOffset = position + adjacentOffsets[i];

                    // Try to find the Section in the cache, first.
                    //foreach ((Vector2Int cachePosition, Section cacheSection) in sectionCache)
                    //foreach (System.Tuple<Vector2Int, Section> tuple in sectionCache)
                    //{
                    //    if (tuple.Item1 == positionPlusOffset)
                    //    {
                    //        adjacentSections[i] = tuple.Item2;
                    //        goto found;
                    //    }
                    //}

                    /*
                    Section cacheSection = sectionCache.Get(positionPlusOffset);
                    if (cacheSection != null)
                    {
                        Profiler.BeginSample("Cache hit");
                        Profiler.EndSample();
                        adjacentSections[i] = cacheSection;
                    }
                    // If we couldn't find it in the cache, look it up in the big dictionary.
                    // adjacentSections[i] is set to the Section, or null if it couldn't be found.
                    else if (globalGrid.TryGetValue(positionPlusOffset, out adjacentSections[i]))
                    {
                        // Update the cache (if we found something)
                        //if (sectionCache.Count == 10)
                        //{
                        //    sectionCache.RemoveLast();
                        //    sectionCache.AddFirst((positionPlusOffset, adjacentSections[i]));
                        //}
                        //else
                        //{
                        //    sectionCache.AddFirst((positionPlusOffset, adjacentSections[i]));
                        //}
                        sectionCache.Add(positionPlusOffset, adjacentSections[i]);
                    }
                    // Didn't find it in either cache or dictionary- continue.
                    else
                    {
                        continue;
                    }
                    */

                    Section foundSection = null;

                    Profiler.BeginSample("Cache lookup");
                    foundSection = sectionCache.Get(positionPlusOffset);
                    Profiler.EndSample();

                    if (foundSection != null)
                    {
                        Profiler.BeginSample("Cache hit");
                        Profiler.EndSample();
                    }
                    else
                    {
                        Profiler.BeginSample("Cache miss - TryGetValue");
                        if (globalGrid.TryGetValue(positionPlusOffset, out foundSection))
                        {
                            sectionCache.Add(positionPlusOffset, foundSection);
                        }
                        Profiler.EndSample();
                    }

                    if (foundSection == null)
                    {
                        Profiler.BeginSample("Section not found skip");
                        Profiler.EndSample();
                        continue;
                    }

                    adjacentSections[i] = foundSection;

                    //continue;

                    // If found, then do additional checks.
                    //found: {
                    //}

                    Profiler.BeginSample("Section found postprocessing");
                    // i + 4 is the opposite direction connection; e.g., N => S.
                    if (adjacentSections[i].CanConnect(connectionsOrdered[i + 4]))
                    {
                        physicalAdjacent2 |= connectionsOrdered[i];
                    }
                    Profiler.EndSample();


                }
            }
            Profiler.EndSample();
            Profiler.BeginSample("Diagonal checks");

            /*
             * Check the diagonal connections. If they're false, then don't compute further-
             * if they're true, then we need to do additional checks. Specifically, we only
             * checked if there is something in the diagonal direction that we can connect
             * to, and that it can connect back. We also need to see if the adjacent cardinal
             * connections exist and can connect.
             * 
             * Check2x2 expects sections in this orientation:
             * 
             * 3-2
             * 0-1
             * 
             * And returns true they are fully connected (i.e., following the diagonal rules).
             */

            if ((physicalAdjacent2 & Connection.Northeast) == Connection.Northeast)
            {
                TwoByTwoArea[0] = thisSection;
                TwoByTwoArea[1] = adjacentSections[0];
                TwoByTwoArea[2] = adjacentSections[1];
                TwoByTwoArea[3] = adjacentSections[2];

                if (!Check2x2(TwoByTwoArea))
                {
                    physicalAdjacent2 &= ~Connection.Northeast;
                }
            }

            if ((physicalAdjacent2 & Connection.Northwest) == Connection.Northwest)
            {
                TwoByTwoArea[0] = adjacentSections[4];
                TwoByTwoArea[1] = thisSection;
                TwoByTwoArea[2] = adjacentSections[2];
                TwoByTwoArea[3] = adjacentSections[3];

                if (!Check2x2(TwoByTwoArea))
                {
                    physicalAdjacent2 &= ~Connection.Northwest;
                }
            }

            if ((physicalAdjacent2 & Connection.Southwest) == Connection.Southwest)
            {
                TwoByTwoArea[0] = adjacentSections[5];
                TwoByTwoArea[1] = adjacentSections[6];
                TwoByTwoArea[2] = thisSection;
                TwoByTwoArea[3] = adjacentSections[4];

                if (!Check2x2(TwoByTwoArea))
                {
                    physicalAdjacent2 &= ~Connection.Southwest;
                }
            }

            if ((physicalAdjacent2 & Connection.Southeast) == Connection.Southeast)
            {
                TwoByTwoArea[0] = adjacentSections[6];
                TwoByTwoArea[1] = adjacentSections[7];
                TwoByTwoArea[2] = adjacentSections[0];
                TwoByTwoArea[3] = thisSection;

                if (!Check2x2(TwoByTwoArea))
                {
                    physicalAdjacent2 &= ~Connection.Southeast;
                }
            }
            Profiler.EndSample();

            return physicalAdjacent2;
        }
        
        private static readonly Connection[] connectionsToCheck =
        {
            Connection.East,
            Connection.Northeast,
            Connection.North,
            Connection.North,
            Connection.Northwest,
            Connection.West,
            Connection.West,
            Connection.Southwest,
            Connection.South,
            Connection.South,
            Connection.Southeast,
            Connection.East
        };

        private bool Check2x2(Section[] sections) 
        {
            // Check none are null
            for (int i = 0; i < 4; i++)
            {
                if (sections[i] == null)
                {
                    return false;
                }
            }

            // We do 3 checks per each element of the 2x2 in the following pattern:
            // 0, 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 0

            for (int i = 0; i < 4; i++)
            {
                if (!sections[i].CanConnect(connectionsToCheck[(3 * i) + 0]))
                {
                    return false;
                }
                if (!sections[i].CanConnect(connectionsToCheck[(3 * i) + 1]))
                {
                    return false;
                }
                if (!sections[i].CanConnect(connectionsToCheck[(3 * i) + 2]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
