using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light light;

    [Range(0.01f, 10f)]
    public float intensityScale = 1f;

    [Range(0.001f, 1000f)]
    public float perlinScale = 1f;

    private Queue<float> noiseBuffer;
    private float lastSum;

    // Start is called before the first frame update
    void Start()
    {
        noiseBuffer = new Queue<float>(5);
        lastSum = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (light == null) {
            return;
        }

        while (noiseBuffer.Count >= 5) {
            lastSum -= noiseBuffer.Dequeue();
        }

        float perlinCoord = Time.time / perlinScale;
        float noise = Mathf.PerlinNoise(0.17f, perlinCoord);
        noiseBuffer.Enqueue(noise);
        lastSum += noise;

        this.light.intensity = intensityScale * lastSum / 5f;
    }
}
