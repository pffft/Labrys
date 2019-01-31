﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Labrys
{
    public class Generator : MonoBehaviour
    {

        private Grid grid;

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

        // The loader implementation.
        //private IGameObjectLoader gameObjectLoader = new LazyLoader(LazyLoader.Priority.Distance, 100);
        private IGameObjectLoader gameObjectLoader = new BasicLoader();

        public static Generator Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("Multiple generators exist in one scene!");
                UnityEditor.EditorGUIUtility.PingObject(gameObject);
            }
        }

        private void Start()
        {
            if (tileSet == null)
            {
                Debug.LogWarning("TileSet instance not set on Generator. Use the inspector to do so.");
                tileSet = TileSet.LoadDefaultTileSet();
            }

            //Debug.Log($"Generator started. TileSet status: {tileSet.status}");

            // This apparently makes TileSet validiate itself. Close enough.
            //tileSet.Initialize();
            tileSet.OnBeforeSerialize();
            tileSet.OnAfterDeserialize();

            //sectionGrid = new Dictionary<Vector2Int, Section>();
            grid = new Grid();

            // Dead-end
            //grid[new Vector2Int(-1, 0)] = new Section();

            //// Corner that can't connect to dead-end
            //grid[new Vector2Int(-1, 1)] = new Section(Connection.All & ~Connection.South);
            //grid[new Vector2Int(-1, 2)] = new Section();

            // 2x2 room (corner rooms)
            //grid[new Vector2Int(0, 0)] = new Section();
            //grid[new Vector2Int(1, 0)] = new Section();
            //grid[new Vector2Int(0, 1)] = new Section();
            //grid[new Vector2Int(1, 1)] = new Section();


            // 2 wide + shape
            //grid[new Vector2Int(0, 0)] = new Section();
            //grid[new Vector2Int(1, 0)] = new Section();
            //grid[new Vector2Int(0, 1)] = new Section();
            //grid[new Vector2Int(1, 1)] = new Section();

            //grid[new Vector2Int(-1, 0)] = new Section();
            //grid[new Vector2Int(-1, 1)] = new Section();

            //grid[new Vector2Int(2, 0)] = new Section();
            //grid[new Vector2Int(2, 1)] = new Section();

            //grid[new Vector2Int(0, -1)] = new Section();
            //grid[new Vector2Int(1, -1)] = new Section();

            //grid[new Vector2Int(0, 2)] = new Section();
            //grid[new Vector2Int(1, 2)] = new Section();

            // Single + shape
            //grid[new Vector2Int(-1, 0)] = new Section();
            //grid[new Vector2Int(1, 0)] = new Section();
            //grid[new Vector2Int(0, 0)] = new Section();
            //grid[new Vector2Int(0, 1)] = new Section();
            //grid[new Vector2Int(0, -1)] = new Section();

            Profiler.BeginSample("Adding tiles to grid");
            //for (int i = 0; i < 25000; i++)
            //{
            //    Vector2Int position = Vector2Int.RoundToInt(75 * Random.insideUnitCircle);

            //    grid[position] = Section.Default();
            //}

            // Roughly 100k sections
            for (int i = 0; i < 317; i++)
            {
                for (int j = 0; j < 317; j++) 
                {
                    //grid[new Vector2Int(i, j)] = new Section(allowedConnections: Connection.North | Connection.South);
                    grid[new Vector2Int(i, j)] = Section.Default();
                }
            }
            Profiler.EndSample();

            Profiler.BeginSample("Resolving tiles");
            PlaceTiles();
            Profiler.EndSample();
        }


        /// <summary>
        /// Finds a Tile for every placed Section in the world. 
        /// </summary>
        private void PlaceTiles()
        {
            //Debug.Log("Placing tiles!");
            TileSet.VariantKey searchKey = new TileSet.VariantKey();
            searchKey.variant = "default";
            List<Tile> searchResult = new List<Tile>();

            foreach (Vector2Int position in grid.GetFullCells())
            {
                Profiler.BeginSample("Calculating tile information");

                Profiler.BeginSample("Extracting section");
                Section section = (Section)grid[position]; // Search by keys; guaranteed not null
                Profiler.EndSample();

                Profiler.BeginSample("Getting physical adjacencies");
                Connection physicalAdjacencies = grid.GetPhysicalAdjacencies2(position);
                Profiler.EndSample();

                //Debug.Log($"For position {position}, have connections: {physicalAdjacencies}.");

                Profiler.BeginSample("Resolving TileType");
                (TileType type, int rotation) = TileType.GetTileType(physicalAdjacencies, section.internalConnections);
                Profiler.EndSample();
                Profiler.BeginSample("Searching for Tiles");

                searchKey.tileType = type;
                //searchKey.variant = "default";

                //List<Tile> tileList = tileSet.Get(searchKey);
                if (!tileSet.Get(searchKey, ref searchResult))
                {
                    Debug.LogError("Couldn't resolve Section into a Tile.");
                }
                Profiler.EndSample();
                //Debug.Log("")

                // Arbitrarily choose first one; TODO add smarter logic to choose which
                // variant to take (and add parameters for it)
                Tile chosenTile = searchResult[0];

                Profiler.EndSample();
                Profiler.BeginSample("Placing physical GO");

                Vector3 worldPosition = new Vector3(distanceScale * position.x, 0, distanceScale * position.y);
                Quaternion worldRotation = Quaternion.AngleAxis(90f * rotation, Vector3.up);

                // Create a new instance of the Tile's prefab and place it in the world
                // Scale it based on the tileScale value.
                //GameObject instantiatedObject = GameObject.Instantiate(chosenTile.gameObject, worldPosition, worldRotation);
                //instantiatedObject.transform.localScale = tileScale * Vector3.one;
                gameObjectLoader.Load(chosenTile.gameObject, worldPosition, worldRotation, tileScale * Vector3.one);

                Profiler.EndSample();

                //Debug.Log("Placed object at: " + worldPosition);
            }

            gameObjectLoader.LastGameObjectSent();
        }
    }
}
