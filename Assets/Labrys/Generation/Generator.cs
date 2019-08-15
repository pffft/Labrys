using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

using Labrys.Tiles;
using Labrys.Generation.Selectors;

namespace Labrys.Generation
{
    public class Generator : MonoBehaviour
    {
        /// <summary>
        /// A map of positions : Sections used to represent the current structure
        /// of the dungeon at this generation step.
        /// </summary>
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

        /// <summary>
        /// The game object loader implementation. After generation is complete, this loader will
        /// be called to create the physical GameObjects (with different events passed in).
        /// </summary>
        /// <remarks>
        /// BasicLoader will try to load every GameObject as soon as it is requested, which acts as
        /// a blocking operation.
        /// 
        /// LazyLoader will wait until every GameObject that will be generated is requested, at
        /// which point it will load up to "amountPerFrame" objects per frame. The order in which
        /// they are loaded can also be specified. These parameters can be tweaked to make the
        /// loading block minimally or maximally, or ensure a certain framerate.
        /// 
        /// TODO: add the loader and selectors to editor using a custom inspector.
        /// </remarks>
        //private IGameObjectLoader gameObjectLoader = new LazyLoader(LazyLoader.Priority.Distance, 100);
        private IGameObjectLoader gameObjectLoader = new BasicLoader();

        /// <summary>
        /// Logic for deciding which Feature will get placed in the next generation step.
        /// </summary>
        private IFeatureSelector featureSelector = new RandomFeatureSelector();

        /// <summary>
        /// Logic for deciding at which position we will try to place a Feature 
        /// in the next generation step.
        /// </summary>
        private IPositionSelector positionSelector = new RandomPositionSelector();

        /// <summary>
        /// Given a position and Feature, this holds the logic for which specific 
        /// valid Configuration we should place down. 
        /// </summary>
        private IConfigurationSelector configurationSelector = new RandomConfigurationSelector();

        /// <summary>
        /// Singleton instance of the Generator object.
        /// </summary>
        public static Generator Instance;

        private void Awake()
        {
            // Singleton setup code
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

            // This apparently makes TileSet validiate itself. Close enough.
            //tileSet.Initialize();
            tileSet.OnBeforeSerialize();
            tileSet.OnAfterDeserialize();

            grid = new Grid();

            Profiler.BeginSample("Adding tiles to grid");

            // Primary generation algorithm. Repeatedly adds features to the grid.
            Generate();

            Profiler.EndSample();

            Profiler.BeginSample("Resolving tiles");

            // A method that resolves Section structures into physical Tiles.
            PlaceTiles();

            Profiler.EndSample();
        }

        // Visually appealing tile placement strategy.
        // TODO this will probably be removed.
        private IEnumerator CoolPlaceTiles(Feature feature, List<Feature.Configuration> configs)
        {
            yield return new WaitForSeconds(0.5f);

            int count = 0;
            foreach (Feature.Configuration configuration in configs)
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

        /// <summary>
        /// Primary generation method. Summary:
        /// 
        /// 1. Gather feature list (stubbed for now)
        /// 2. Generate starting room
        /// 3. If ending condition is met, goto 10. Else continue.
        /// 4. Select next Feature.
        /// 5. Select next position.
        /// 6. Compute all valid configurations.
        /// 7. Select next configuration.
        /// 8. Add all the Sections of the configured Feature.
        /// 9. Goto 3.
        /// 10. Done.
        /// </summary>
        private void Generate() 
        {
            // Temporary: feature list
            Feature basicFeature = new Feature();
            // TODO I don't think this works properly with connections specified.
            basicFeature.Add(new Vector2Int(0, 0), Connection.All, externalConnections: Connection.All);

            Feature[] featureList = { basicFeature };

            // Initialize with a single starting room
            // Later, this can be e.g. a specific starting chamber.
            //grid[0, 0] = Section.Default();
            grid[0, 0] = new Section(variant: "concrete");

            // Pick some ending condition. For now it's just "try 100 times".
            int count = 0;
            while (count++ < 100) 
            {
                Feature nextFeature = featureSelector.Select(featureList);
                if (nextFeature == null) 
                {
                    Debug.Log("Failed to get valid Feature.");
                    continue;
                }

                Vector2Int nextPosition = positionSelector.Select(new ReadOnlyGrid(grid));

                // Get all valid placements
                List<Feature.Configuration> configurations = nextFeature.CanConnect(grid, nextPosition);

                if (configurations.Count == 0) 
                {
                    Debug.Log("Failed to find valid configuration at position: " + nextPosition);
                    continue;
                }

                // Choose one of those placements
                Feature.Configuration configuration = configurationSelector.Select(configurations.ToArray());

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
            TileSet.VariantKey searchKey = new TileSet.VariantKey();
            searchKey.variant = "concrete";

            List<Tile> searchResult = new List<Tile>();

            foreach (Vector2Int position in grid.GetOccupiedCells())
            {
                Profiler.BeginSample("Calculating tile information");

                Profiler.BeginSample("Extracting section");
                // Search by keys; guaranteed not null, so we cast from Section? -> Section
                Section section = (Section)grid[position]; 
                Profiler.EndSample();

                Profiler.BeginSample("Getting physical adjacencies");
                // Find all the Sections that neighbor this one ("physical adjacency").
                // Cached + optimized algorithm
                Connection physicalAdjacencies = grid.GetPhysicalAdjacencies(position); 
                Profiler.EndSample();

                Profiler.BeginSample("Resolving TileType");
                // Knowing physical adjacencies, get the TileType and rotation
                (TileType type, int rotation) = TileType.GetTileType(physicalAdjacencies, section.internalConnections);
                Profiler.EndSample();

                Profiler.BeginSample("Searching for Tiles");

                // Hardcoded to have "default" variant for efficieny. 
                // TODO replace searchKey.variant with Section.getVariant(), and logic
                // to fallback to a search for "default".
                searchKey.tileType = type;
                if (!tileSet.Get(searchKey, ref searchResult))
                {
                    Debug.LogError("Couldn't resolve Section into a Tile.");
                }
                Profiler.EndSample();

                // Arbitrarily choose first one; TODO add smarter logic to choose which
                // variant to take (and add parameters for it)
                Tile chosenTile = searchResult[0];
                chosenTile.section = section;

                Profiler.EndSample();
                Profiler.BeginSample("Placing physical GO");

                // Compute the GameObject values from the known parameters
                Vector3 worldPosition = new Vector3(distanceScale * position.x, 0, distanceScale * position.y);
                Quaternion worldRotation = Quaternion.AngleAxis(90f * rotation, Vector3.up);

                // Create a new instance of the Tile's prefab and place it in the world
                gameObjectLoader.Load(chosenTile.gameObject, worldPosition, worldRotation, tileScale * Vector3.one);

                Profiler.EndSample();
            }

            gameObjectLoader.LastGameObjectSent();
        }
    }
}
