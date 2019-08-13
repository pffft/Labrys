using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRoom : MonoBehaviour
{
    public GameObject[] floorTiles;

    public GameObject[] wallTiles;

    public GameObject[] torches;

    private const int diameter = 12;
    private const int height = 3;

    private Vector3 offset = new Vector3(-5.5f, 0, -5.5f);

    private const float scale = 1f;

    public void Generate() 
    {
        GameObject floor = new GameObject("Floor");

        // Generate floors
        for (int i = 0; i < diameter / 4; i++) 
        {
            for (int j = 0; j < diameter / 4; j++) 
            {
                GameObject floorPart = new GameObject("Floor Segment");
                for (int localX = 0; localX < 4; localX++) 
                {
                    for (int localZ = 0; localZ < 4; localZ++) 
                    {
                        GameObject floorTemplate = floorTiles[Random.Range(0, floorTiles.Length)];

                        // Random rotation
                        Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.up);

                        GameObject instantiated = GameObject.Instantiate(floorTemplate, offset + scale * new Vector3(localX + (4 * i), -0.5f, localZ + (4 * j)), randomRotation);
                        instantiated.transform.parent = floorPart.transform;

                        // Random flip on both x and z axes (todo this breaks colliders)
                        //instantiated.transform.localScale = new Vector3(Random.value < 0.5f ? 1 : -1, 1, Random.value < 0.5f ? 1 : -1);
                    }
                }
                GameObject optimizedFloorPart = Optimize(floorPart);
                optimizedFloorPart.transform.parent = floor.transform;
                GameObject.Destroy(floorPart); // Destroy the unoptimized one
            }
        }

        GameObject walls = new GameObject("Walls");

        // Generate walls
        for (int i = 0; i < diameter; i+=4) 
        {
            GameObject wallTemplate;
            Quaternion randomRotation;
            GameObject instantiated;

            // Cut out holes for the doors
            if (i == 4)
            {
                wallTemplate = wallTiles[1];
            } else
            {
                wallTemplate = wallTiles[0];
            }

            // -X wall
            randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
            instantiated = GameObject.Instantiate(wallTemplate, offset + scale * new Vector3(0, 0, i + 1.5f), Quaternion.AngleAxis(0, Vector3.up));
            instantiated.transform.parent = walls.transform;

            // -Z wall
            randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
            instantiated = GameObject.Instantiate(wallTemplate, offset + scale * new Vector3(i + 1.5f, 0, 0), Quaternion.AngleAxis(270, Vector3.up));
            instantiated.transform.parent = walls.transform;

            // +X wall
            randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
            instantiated = GameObject.Instantiate(wallTemplate, offset + scale * new Vector3(diameter - 1f, 0, i + 1.5f), Quaternion.AngleAxis(180, Vector3.up));
            instantiated.transform.parent = walls.transform;

            // +Z wall
            randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
            instantiated = GameObject.Instantiate(wallTemplate, offset + scale * new Vector3(i + 1.5f, 0, diameter - 1f), Quaternion.AngleAxis(90, Vector3.up));
            instantiated.transform.parent = walls.transform;
        }

        // Middle walls
        /*        for (int i = 0; i < (2f / 3f * diameter) + 1; i++) 
                {
                    for (int j = 0; j < height; j++) 
                    {
                        GameObject wallTemplate;
                        Quaternion randomRotation;
                        GameObject instantiated;

                        // North 2/3rds wall
                        wallTemplate = wallTiles[Random.Range(0, wallTiles.Length)];
                        randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
                        instantiated = GameObject.Instantiate(wallTemplate, scale * new Vector3(i, j, (int)(1f / 3f * diameter) - 0.5f), Quaternion.AngleAxis(90, Vector3.up) * randomRotation);
                        instantiated.transform.parent = walls.transform;

                        // South 2/3rds wall
                        wallTemplate = wallTiles[Random.Range(0, wallTiles.Length)];
                        randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
                        instantiated = GameObject.Instantiate(wallTemplate, scale * new Vector3(diameter - i - 1, j, (int)(2f / 3f * diameter) - 0.5f), Quaternion.AngleAxis(90, Vector3.up) * randomRotation);
                        instantiated.transform.parent = walls.transform;
                    }
                }*/

        Debug.Log("Done with main generation");
        //Optimize(floor);
        //Debug.Log("Optimized floor");
        //Optimize(walls);
        //Debug.Log("Optimized walls");
    }

    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    private GameObject Optimize(GameObject rootObject) 
    {
        List<GameObject> allObjects = GetAllChildren(rootObject);

        Dictionary<string, List<MeshFilter>> meshesByMaterial = new Dictionary<string, List<MeshFilter>>();
        Dictionary<string, Material> nameToMaterial = new Dictionary<string, Material>();

        foreach (GameObject obj in allObjects) 
        {
            // Extract the mesh
            MeshFilter meshFilter = obj.GetComponent<MeshFilter>() ?? null;
            MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>() ?? null;

            // Fail if there's no mesh, or multiple materials per mesh
            if (meshFilter == null || meshRenderer == null || meshRenderer.materials.Length > 1) 
            {
                Debug.Log("Found non-optimizable object: " + obj.name);
                continue;
            }

            // Index the mesh by its material
            string materialName = meshRenderer.material.name;
            if (!meshesByMaterial.ContainsKey(materialName)) 
            {
                meshesByMaterial.Add(materialName, new List<MeshFilter> { meshFilter });
            } 
            else 
            {
                List<MeshFilter> meshes = meshesByMaterial[materialName];
                meshes.Add(meshFilter);
            }

            // Index the material name
            if (!nameToMaterial.ContainsKey(materialName)) 
            {
                nameToMaterial.Add(materialName, meshRenderer.material);
            }
        }

        Debug.Log("Found " + meshesByMaterial.Keys.Count + " materials.");

        GameObject newObject = new GameObject(rootObject.name + "_optimized");

        foreach (KeyValuePair<string, List<MeshFilter>> kvp in meshesByMaterial) 
        {
            string materialName = kvp.Key;
            List<MeshFilter> meshes = kvp.Value;
            Debug.Log("For material " + materialName + " found " + meshes.Count + " meshes.");

            CombineInstance[] combines = new CombineInstance[meshes.Count];
            for (int i = 0; i < meshes.Count; i++)
            {
                if (!meshes[i].mesh.isReadable)
                {
                    Debug.Log("Unreadable mesh: " + meshes[i].gameObject.name + ". Making duplicate mesh.");
                    Mesh meshCopy = new Mesh();
                    meshCopy.vertices = meshes[i].mesh.vertices;
                    meshCopy.triangles = meshes[i].mesh.triangles;
                    meshCopy.RecalculateNormals();

                    combines[i].mesh = meshCopy;
                    combines[i].transform = meshes[i].transform.localToWorldMatrix;

                    continue;
                }
                combines[i].mesh = meshes[i].mesh;
                combines[i].transform = meshes[i].transform.localToWorldMatrix;
            }

            // Create a new GameObject with the combined mesh + a copy of the material
            GameObject meshObj = new GameObject(materialName);
            meshObj.isStatic = true;
            meshObj.transform.parent = newObject.transform;

            Mesh combinedMesh = new Mesh();
            combinedMesh.CombineMeshes(combines);

            MeshFilter newMeshFilter = meshObj.AddComponent<MeshFilter>();
            newMeshFilter.mesh = combinedMesh;

            MeshRenderer newMeshRenderer = meshObj.AddComponent<MeshRenderer>();
            newMeshRenderer.material = GameObject.Instantiate<Material>(nameToMaterial[materialName]);
        }

        newObject.isStatic = true;
        return newObject;
    }

    /// <summary>
    /// Recursively finds all children of the given GameObject.
    /// </summary>
    /// <returns>All the children.</returns>
    /// <param name="root">The root GameObject to search.</param>
    private List<GameObject> GetAllChildren(GameObject root) 
    {
        if (root == null) 
        {
            return new List<GameObject>();
        }

        List<GameObject> children = new List<GameObject>();
        if (root.transform.childCount != 0) 
        {
            foreach (Transform child in root.transform) 
            {
                children.AddRange(GetAllChildren(child.gameObject));
            }
        }
        else
        {
            if (root.activeSelf)
            {
                children.Add(root);
            }
        }

        return children;
    }
}
