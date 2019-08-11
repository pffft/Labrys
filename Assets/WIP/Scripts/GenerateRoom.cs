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
        for (int i = 0; i < diameter; i+=4) 
        {
            for (int j = 0; j < diameter; j+=4) 
            {
                GameObject floorTemplate = floorTiles[Random.Range(0, floorTiles.Length)];

                // Random rotation
                Quaternion randomRotation = Quaternion.AngleAxis(Random.Range(0, 3) * 90, Vector3.up);

                GameObject instantiated = GameObject.Instantiate(floorTemplate, offset + scale * new Vector3(1.5f + i, 0, 1.5f + j), randomRotation);
                instantiated.transform.parent = floor.transform;

                // Random flip on both x and z axes (todo this breaks colliders)
                //instantiated.transform.localScale = new Vector3(Random.value < 0.5f ? 1 : -1, 1, Random.value < 0.5f ? 1 : -1);
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
