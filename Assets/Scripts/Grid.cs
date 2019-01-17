using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

            Connection physicalAdjacent2 = Connection.None;

            // The adjacent positions, in order
            Vector2Int[] offsets =
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

            // Build up a cache
            // TODO This isn't checking every case. Diagonals need more checks.
            Section[] adjacentSections = new Section[8];
            for (int i = 0; i < 8; i++)
            {
                if (thisSection.CanConnect((Connection)(1 << i)))
                {
                    if (globalGrid.TryGetValue(position + offsets[i], out adjacentSections[i]))
                    {
                        int oppositeConnection = (i + 4) % 8;
                        if (adjacentSections[i].CanConnect((Connection)(1 << oppositeConnection)))
                        {
                            physicalAdjacent2 |= (Connection)(1 << i);
                        }
                    }
                }
            }

            // Check diagonal connections. At this point, we know we can connect in that direction,
            // and there is a Section there that can connect back to us.
            // TODO fix this, it's incomplete
            for (int i = 1; i < 8; i += 2)
            {
                Connection diagonal = (Connection)(1 << i);
                Connection ccw = (Connection)(1 << ((i + 1) % 8));
                Connection cw = (Connection)(1 << (i - 1));

                // Check if we can connect to the surrounding cardinal connections and
                // if something's there.
                if (!thisSection.CanConnect(ccw) || !thisSection.CanConnect(cw) || adjacentSections[i - 1] == null || adjacentSections[(i + 1) % 8] == null)
                {
                    physicalAdjacent2 &= ~(Connection)(1 << i);
                    continue;
                }

                Connection ccw2 = (Connection)(1 << ((i + 1) % 8));
                Connection cw2 = (Connection)(1 << ((i - 2) % 8));

                // E.g., i = 1 == NE. (i + 1) is N, and one step CW is E. So, can N connect E?
                // Second statement checks two steps CW, so SE. So, can N connect SE? 
                if (!adjacentSections[(i + 1) % 8].CanConnect(cw) || !adjacentSections[(i + 1) % 8].CanConnect(cw2))
                {
                    physicalAdjacent2 &= ~(Connection)(1 << i);
                    continue;
                }

                // Similarly, this asks can E connect N? And secondly, can E connect NW?
                if (!adjacentSections[i - 1].CanConnect(ccw) || !adjacentSections[i - 1].CanConnect(ccw2))
                {
                    physicalAdjacent2 &= ~(Connection)(1 << i);
                    continue;
                }
            }

            return physicalAdjacent2;

            /*
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
            */
        }
    }
}
