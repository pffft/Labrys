using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{

    private Dictionary<Vector2Int, Section> sectionGrid;

    [SerializeField]
    [Tooltip("The palatte of Tiles to use for generation purposes.")]
    private TileSet tileSet = null;


    /// <summary>
    /// By how much we scale every single Tile when we are finished.
    /// </summary>
    [SerializeField]
    [Tooltip("Scale factor for physical Tiles")]
    private float tileScale = 1f;

    /// <summary>
    /// How far apart the grid spacing is. Typically same as tileScale, but if
    /// you want gaps between every single Tile you can make this larger.
    /// 
    /// Tiles will be placed every "distanceScale" units; i.e., if it is 1.1,
    /// then Tiles will be placed at (0, 0), (1.1, 0), (2.2, 0), etc.
    /// </summary>
    [SerializeField]
    [Tooltip("Scale factor for grid size. By default same as Tile Scale")]
    private float distanceScale = 1f;

    private static Generator _instance;
    public Generator Instance 
    {
        get {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Debug.LogError("Multiple generators exist in one scene!");
            }
            return _instance;
        }
    }

    private void Start()
    {
        if (tileSet == null) {
            Debug.LogWarning("TileSet instance not set on Generator. Use the inspector to do so.");
            tileSet = TileSet.LoadDefaultTileSet();
        }

        sectionGrid = new Dictionary<Vector2Int, Section>();

        //// Dead-end
        //sectionGrid.Add(new Vector2Int(-1, 0), new Section());

        //// Corner that can't connect to dead-end
        //sectionGrid.Add(new Vector2Int(-1, 1), new Section(Connection.All & ~Connection.South));
        //sectionGrid.Add(new Vector2Int(-1, 2), new Section());

        //// 2x2 room (corner rooms)
        //sectionGrid.Add(new Vector2Int(0, 0), new Section()); 
        //sectionGrid.Add(new Vector2Int(1, 0), new Section());
        //sectionGrid.Add(new Vector2Int(0, 1), new Section());
        //sectionGrid.Add(new Vector2Int(1, 1), new Section());

        // 2 wide + shape
        //sectionGrid.Add(new Vector2Int(0, 0), new Section());
        //sectionGrid.Add(new Vector2Int(1, 0), new Section());
        //sectionGrid.Add(new Vector2Int(0, 1), new Section());
        //sectionGrid.Add(new Vector2Int(1, 1), new Section());

        //sectionGrid.Add(new Vector2Int(-1, 0), new Section());
        //sectionGrid.Add(new Vector2Int(-1, 1), new Section());

        //sectionGrid.Add(new Vector2Int(2, 0), new Section());
        //sectionGrid.Add(new Vector2Int(2, 1), new Section());

        //sectionGrid.Add(new Vector2Int(0, -1), new Section());
        //sectionGrid.Add(new Vector2Int(1, -1), new Section());

        //sectionGrid.Add(new Vector2Int(0, 2), new Section());
        //sectionGrid.Add(new Vector2Int(1, 2), new Section());

        // Single + shape
        sectionGrid.Add(new Vector2Int(-1, 0), new Section());
        sectionGrid.Add(new Vector2Int(1, 0), new Section());
        sectionGrid.Add(new Vector2Int(0, -1), new Section());
        sectionGrid.Add(new Vector2Int(0, 1), new Section());
        sectionGrid.Add(new Vector2Int(0, 0), new Section());

        //for (int i = 0; i < 2500; i++) 
        //{
        //    Vector2Int position = Vector2Int.RoundToInt(25 * Random.insideUnitCircle);
        //    if (sectionGrid.ContainsKey(position)) 
        //    {
        //        continue;
        //    }

        //    sectionGrid.Add(position, new Section());
        //}


        PlaceTiles();
    }

    /// <summary>
    /// Finds a Tile for every placed Section in the world. 
    /// </summary>
    private void PlaceTiles()
    {
        //Debug.Log("Placing tiles!");
        foreach (Vector2Int position in sectionGrid.Keys)
        {
            Section section = sectionGrid[position];
            Connection physicalAdjacencies = GetPhysicalAdjacencies(position);

            //Debug.Log($"For position {position}, have connections: {physicalAdjacencies}.");

            (TileType type, int rotation) = TileType.GetTileType(physicalAdjacencies, section.allowedConnections);
            List<Tile> tileList = tileSet.Get(new TileSet.VariantKey(type, "frozen"));
            //Debug.Log("")

            if (tileList.Count == 0) 
            {
                Debug.LogError("Couldn't resolve Section into a Tile.");
            }

            // Arbitrarily choose first one; TODO add smarter logic to choose which
            // variant to take (and add parameters for it)
            Tile chosenTile = tileList[0];

            Vector3 worldPosition = new Vector3(distanceScale * position.x, 0, distanceScale * position.y);
            Quaternion worldRotation = Quaternion.AngleAxis(90f * rotation, Vector3.up);

            // Create a new instance of the Tile's prefab and place it in the world
            // Scale it based on the tileScale value.
            GameObject instantiatedObject = GameObject.Instantiate(chosenTile.prefab, worldPosition, worldRotation);
            instantiatedObject.transform.localScale = tileScale * Vector3.one;

            //Debug.Log("Placed object at: " + worldPosition);
        }
    }

    /// <summary>
    /// If a Section has a neighbor, sets a Connection flag. Returns the combined
    /// set of Connection flags.
    /// </summary>
    /// <returns>The physical adjacencies.</returns>
    /// <param name="position">Position.</param>
    private Connection GetPhysicalAdjacencies(Vector2Int position) 
    {
        Connection physicalAdjacent = Connection.None;

        Section thisSection = sectionGrid[position];

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
            if (sectionGrid.TryGetValue(position + Vector2Int.up, out Section up))
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
            if (sectionGrid.TryGetValue(position + Vector2Int.right, out Section right)) 
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
            if (sectionGrid.TryGetValue(position + Vector2Int.down, out Section down)) 
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
            if (sectionGrid.TryGetValue(position + Vector2Int.left, out Section left))
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

            if (sectionGrid.TryGetValue(position + Vector2Int.up, out Section up)) 
            {
                okay &= up.CanConnect(Connection.West | Connection.South | Connection.Southwest);
            } 
            else 
            {
                okay = false;
            }

            if (sectionGrid.TryGetValue(position + Vector2Int.left, out Section left)) 
            {
                okay &= left.CanConnect(Connection.North | Connection.East | Connection.Northeast);
            }
            else
            {
                okay = false;
            }

            if (sectionGrid.TryGetValue(position + Vector2Int.up + Vector2Int.left, out Section upLeft))
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

            if (sectionGrid.TryGetValue(position + Vector2Int.up, out Section up))
            {
                okay &= up.CanConnect(Connection.East | Connection.South | Connection.Southeast);
            }
            else
            {
                okay = false;
            }

            if (sectionGrid.TryGetValue(position + Vector2Int.right, out Section right))
            {
                okay &= right.CanConnect(Connection.North | Connection.West | Connection.Northwest);
            }
            else
            {
                okay = false;
            }

            if (sectionGrid.TryGetValue(position + Vector2Int.up + Vector2Int.right, out Section upRight))
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

            if (sectionGrid.TryGetValue(position + Vector2Int.down, out Section down))
            {
                okay &= down.CanConnect(Connection.West | Connection.North | Connection.Northwest);
            }
            else
            {
                okay = false;
            }

            if (sectionGrid.TryGetValue(position + Vector2Int.left, out Section left))
            {
                okay &= left.CanConnect(Connection.South | Connection.East | Connection.Southeast);
            }
            else
            {
                okay = false;
            }

            if (sectionGrid.TryGetValue(position + Vector2Int.down + Vector2Int.left, out Section downLeft))
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

            if (sectionGrid.TryGetValue(position + Vector2Int.down, out Section down))
            {
                okay &= down.CanConnect(Connection.East | Connection.North | Connection.Northeast);
            }
            else
            {
                okay = false;
            }

            if (sectionGrid.TryGetValue(position + Vector2Int.right, out Section right))
            {
                okay &= right.CanConnect(Connection.South | Connection.West | Connection.Southwest);
            }
            else
            {
                okay = false;
            }

            if (sectionGrid.TryGetValue(position + Vector2Int.down + Vector2Int.right, out Section downRight))
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
}
