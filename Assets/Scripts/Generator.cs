﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using Labrys.Tiles;
using Labrys.Selectors;

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

        private IFeatureSelector featureSelector = new RandomFeatureSelector();
        private IPositionSelector positionSelector = new RandomPositionSelector();
        private IConfigurationSelector configurationSelector = new RandomConfigurationSelector();

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
            //for (int i = 0; i < 5000; i++)
            //{
            //    Vector2Int position = Vector2Int.RoundToInt(25 * Random.insideUnitCircle);

            //    grid[position] = Section.Default();
            //}

            // Roughly 100k sections

            //int sideLength = (int)Mathf.Sqrt(500000) + 1;

            //for (int i = 0; i < sideLength; i++)
            //{
            //    for (int j = 0; j < sideLength; j++) 
            //    {
            //        //grid[new Vector2Int(i, j)] = new Section(allowedConnections: Connection.North | Connection.South);
            //        grid[new Vector2Int(i, j)] = Section.Default();
            //    }
            //}

            // Feature testing
            grid[new Vector2Int(0, 0)] = Section.Default();
            grid[new Vector2Int(1, 0)] = Section.Default();
            grid[new Vector2Int(0, 1)] = Section.Default();
            grid[new Vector2Int(1, 1)] = Section.Default();

            // Basic 2x2
            Feature feature = new Feature();
            feature.Add(new Vector2Int(0, 0));
            feature.Add(new Vector2Int(1, 0));
            feature.Add(new Vector2Int(0, 1));
            feature.Add(new Vector2Int(1, 1));

            //feature.Add(new Vector2Int(0, 2)); // These next 5 make it a 3x3
            //feature.Add(new Vector2Int(1, 2));
            //feature.Add(new Vector2Int(2, 2));
            //feature.Add(new Vector2Int(2, 1));
            //feature.Add(new Vector2Int(2, 0));

            // 3x1 feature
            //feature.Add(0, 0);
            //feature.Add(1, 0);
            //feature.Add(2, 0);
            //feature.Add(3, 0); // Add these two for
            //feature.Add(3, 1); // a long L feature


            // Code for testing Feature configuration
            //List<Feature.PlacementConfiguration> configs = new List<Feature.PlacementConfiguration>();

            //configs.AddRange(feature.CanConnect(grid, Vector2Int.zero));
            //configs.AddRange(feature.CanConnect(grid, new Vector2Int(1, 0)));
            //configs.AddRange(feature.CanConnect(grid, new Vector2Int(0, 1)));
            //configs.AddRange(feature.CanConnect(grid, new Vector2Int(1, 1)));

            //StartCoroutine(CoolPlaceTiles(feature, configs));


            //Assert.False(feature.CanPlace(grid, new Vector2Int(0, 0), new Vector2Int(1, 0), 1));
            //feature.GetTransformedSections(new Vector2Int(1, 0), 1).ForEach(pos => grid[pos] = new Section());

            //Debug.Log($"Can place feature at center?: " + feature.CanPlace(grid, Vector2Int.zero, Vector2Int.zero, 0));
            //Debug.Log($"Can place feature to right?: " + feature.CanPlace(grid, Vector2Int.right, Vector2Int.zero, 0));

            Generate();


            Profiler.EndSample();

            Profiler.BeginSample("Resolving tiles");
            PlaceTiles();
            Profiler.EndSample();
        }

        // Visually appealing tile placement strategy.
        private IEnumerator CoolPlaceTiles(Feature feature, List<Feature.PlacementConfiguration> configs)
        {
            yield return new WaitForSeconds(0.5f);

            int count = 0;
            foreach (Feature.PlacementConfiguration configuration in configs)
            {
                Debug.Log($"Trying configuration {count++}.");
                // Clear all gameobjects (inefficiently, but just for the demo)
                Tile[] tiles = GameObject.FindObjectsOfType<Tile>();
                for (int i = 0; i < tiles.Length; i++) 
                {
                    GameObject.DestroyImmediate(tiles[i].gameObject);
                }

                // Make a new empty grid
                grid = new Grid();
                grid[new Vector2Int(0, 0)] = Section.Default();
                grid[new Vector2Int(1, 0)] = Section.Default();
                grid[new Vector2Int(0, 1)] = Section.Default();
                grid[new Vector2Int(1, 1)] = Section.Default();

                // Place the configuration
                foreach (KeyValuePair<Vector2Int, Section> keyValuePair in feature.GetConfiguration(configuration))
                {
                    grid[keyValuePair.Key] = keyValuePair.Value;
                }

                // Resolve to physical
                PlaceTiles();

                // Wait
                yield return new WaitForSeconds(Mathf.Max(0.1f, (10 - count) / 10f));
            }
        }

        private void Generate() 
        {
            // Temporary: feature list
            Feature basicFeature = new Feature();
            basicFeature.Add(new Vector2Int(0, 0), Connection.Cardinal);
            basicFeature.Add(new Vector2Int(1, 0), Connection.West | Connection.East);
            basicFeature.Add(new Vector2Int(-1, 0), Connection.West | Connection.East);
            basicFeature.Add(new Vector2Int(0, 1), Connection.North | Connection.South);
            basicFeature.Add(new Vector2Int(0, -1), Connection.North | Connection.South);

            Feature[] featureList = { basicFeature };

            // Pick some ending condition
            int count = 0;
            while (count++ < 100) 
            {
                Feature nextFeature = featureSelector.Select(featureList);
                if (nextFeature == null) 
                {
                    Debug.Log("Failed to get valid Feature.");
                    continue;
                }

                Vector2Int nextPosition = positionSelector.Select(grid);

                // Get all valid placements
                List<Feature.PlacementConfiguration> configurations = nextFeature.CanConnect(grid, nextPosition);

                if (configurations.Count == 0) 
                {
                    Debug.Log("Failed to find valid configuration at position: " + nextPosition);
                    continue;
                }

                // Choose one of those placements
                Feature.PlacementConfiguration configuration = configurationSelector.Select(configurations.ToArray());

                // Add that Feature in
                Dictionary<Vector2Int, Section> toAdd = nextFeature.GetConfiguration(configuration);
                foreach (KeyValuePair<Vector2Int, Section> pair in toAdd) 
                {
                    grid[pair.Key] = pair.Value;
                }
            }

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
