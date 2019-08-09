using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRoom : MonoBehaviour
{
    public GameObject[] floorTiles;

    public GameObject[] wallTiles;

    public GameObject[] torches;

    private const int diameter = 16;
    private const int height = 3;

    private const float scale = 1f;

    public void Generate() 
    {
        GameObject floor = new GameObject("Floor");

        // Generate floors
        for (int i = 0; i < diameter; i++) 
        {
            for (int j = 0; j < diameter; j++) 
            {
                GameObject floorTemplate = floorTiles[Random.Range(0, floorTiles.Length)];

                // Random rotation
                Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.up);

                GameObject instantiated = GameObject.Instantiate(floorTemplate, scale * new Vector3(i, -0.5f, j), randomRotation);
                instantiated.transform.parent = floor.transform;

                // Random flip on both x and z axes (todo this breaks colliders)
                //instantiated.transform.localScale = new Vector3(Random.value < 0.5f ? 1 : -1, 1, Random.value < 0.5f ? 1 : -1);
            }
        }

        GameObject walls = new GameObject("Walls");

        // Generate walls
        for (int i = 0; i < diameter; i++) 
        {
            for (int j = 0; j < height; j++)
            {
                GameObject wallTemplate;
                Quaternion randomRotation;
                GameObject instantiated;

                // Cut out holes for the doors
                if (i == (diameter / 2) - 1 || i == diameter / 2) 
                {
                    continue;
                }

                // -X wall
                wallTemplate = wallTiles[Random.Range(0, wallTiles.Length)];
                randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
                instantiated = GameObject.Instantiate(wallTemplate, scale * new Vector3(-0.5f, j, i), randomRotation);
                instantiated.transform.parent = walls.transform;

                // -Z wall
                wallTemplate = wallTiles[Random.Range(0, wallTiles.Length)];
                randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
                instantiated = GameObject.Instantiate(wallTemplate, scale * new Vector3(i, j, -0.5f), Quaternion.AngleAxis(90, Vector3.up) * randomRotation);
                instantiated.transform.parent = walls.transform;

                // +X wall
                wallTemplate = wallTiles[Random.Range(0, wallTiles.Length)];
                randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
                instantiated = GameObject.Instantiate(wallTemplate, scale * new Vector3(diameter - 0.5f, j, i), Quaternion.AngleAxis(180, Vector3.up) * randomRotation);
                instantiated.transform.parent = walls.transform;

                // +Z wall
                wallTemplate = wallTiles[Random.Range(0, wallTiles.Length)];
                randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
                instantiated = GameObject.Instantiate(wallTemplate, scale * new Vector3(i, j, diameter - 0.5f), Quaternion.AngleAxis(270, Vector3.up) * randomRotation);
                instantiated.transform.parent = walls.transform;
            }
        }

        // Middle walls
        for (int i = 0; i < (2f / 3f * diameter) + 1; i++) 
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
        }

        // Torches
        for (int i = 0; i <= diameter / 8; i++)
        {
            GameObject torchTemplate;

            // -X
            torchTemplate = torches[Random.Range(0, torches.Length)];
            GameObject.Instantiate(torchTemplate, scale * new Vector3(-0.5f, 1, (i + 1) * 3 + 1f), Quaternion.AngleAxis(180, Vector3.up));

            // +X
            torchTemplate = torches[Random.Range(0, torches.Length)];
            GameObject.Instantiate(torchTemplate, scale * new Vector3(diameter - 0.5f, 1, (i + 1) * 3 + 1f), Quaternion.AngleAxis(0, Vector3.up));

            // -Z
            torchTemplate = torches[Random.Range(0, torches.Length)];
            GameObject.Instantiate(torchTemplate, scale * new Vector3((i + 1) * 3 + 1f,  1, -1), Quaternion.AngleAxis(90, Vector3.up));

            // +Z
            torchTemplate = torches[Random.Range(0, torches.Length)];
            GameObject.Instantiate(torchTemplate, scale * new Vector3((i + 1) * 3 + 1f, 1, (2 * diameter) - 1), Quaternion.AngleAxis(270, Vector3.up));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Generate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
