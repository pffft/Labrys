using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRoom : MonoBehaviour
{
    public GameObject[] floorTiles;

    public GameObject[] wallTiles;

    public GameObject[] torches;

    private const int diameter = 8;
    private const int height = 2;

    public void Generate() 
    {
        // Generate floors
        for (int i = 0; i < diameter; i++) 
        {
            for (int j = 0; j < diameter; j++) 
            {
                GameObject floorTemplate = floorTiles[Random.Range(0, floorTiles.Length)];

                // Random rotation
                Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.up);

                GameObject instantiated = GameObject.Instantiate(floorTemplate, new Vector3(i * 2, -1, j * 2), randomRotation);

                // Random flip on both x and z axes (todo this breaks colliders)
                //instantiated.transform.localScale = new Vector3(Random.value < 0.5f ? 1 : -1, 1, Random.value < 0.5f ? 1 : -1);
            }
        }

        // Generate walls
        for (int i = 0; i < diameter; i++) 
        {
            for (int j = 0; j < height; j++)
            {
                GameObject wallTemplate;
                Quaternion randomRotation;
                GameObject instantiated;

                // -X wall
                wallTemplate = wallTiles[Random.Range(0, wallTiles.Length)];
                randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
                instantiated = GameObject.Instantiate(wallTemplate, new Vector3(-1, j * 2, i * 2), randomRotation);

                // -Z wall
                wallTemplate = wallTiles[Random.Range(0, wallTiles.Length)];
                randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
                instantiated = GameObject.Instantiate(wallTemplate, new Vector3(i * 2, j * 2, -1), Quaternion.AngleAxis(90, Vector3.up) * randomRotation);

                // +X wall
                wallTemplate = wallTiles[Random.Range(0, wallTiles.Length)];
                randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
                instantiated = GameObject.Instantiate(wallTemplate, new Vector3((2 * diameter) - 1, j * 2, i * 2), Quaternion.AngleAxis(180, Vector3.up) * randomRotation);

                // +Z wall
                wallTemplate = wallTiles[Random.Range(0, wallTiles.Length)];
                randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.left);
                instantiated = GameObject.Instantiate(wallTemplate, new Vector3(i * 2, j * 2, (2 * diameter) - 1), Quaternion.AngleAxis(270, Vector3.up) * randomRotation);
            }
        }

        // Torches
        for (int i = 0; i <= diameter / 8; i++)
        {
            GameObject torchTemplate;

            // -X
            torchTemplate = torches[Random.Range(0, torches.Length)];
            GameObject.Instantiate(torchTemplate, new Vector3(-1, 2, (i + 1) * 3 + 1f), Quaternion.AngleAxis(180, Vector3.up));

            // +X
            torchTemplate = torches[Random.Range(0, torches.Length)];
            GameObject.Instantiate(torchTemplate, new Vector3((2 * diameter) - 1, 2, (i + 1) * 3 + 1f), Quaternion.AngleAxis(0, Vector3.up));

            // -Z
            torchTemplate = torches[Random.Range(0, torches.Length)];
            GameObject.Instantiate(torchTemplate, new Vector3((i + 1) * 3 + 1f,  2, -1), Quaternion.AngleAxis(90, Vector3.up));

            // +Z
            torchTemplate = torches[Random.Range(0, torches.Length)];
            GameObject.Instantiate(torchTemplate, new Vector3((i + 1) * 3 + 1f, 2, (2 * diameter) - 1), Quaternion.AngleAxis(270, Vector3.up));
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
