using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public new Light light;

    [Range(0.5f, 5f)]
    public float averageIntensity = 1.25f;

    [Range(0f, 2.5f)]
    public float flickerIntensity = 0.25f;

    [Range(1f, 100f)]
    public float flickerSpeed = 1.0f / 0.2f;

    ////[Range(0.01f, 10f)]
    //private float intensityScale = 1f;

    ////[Range(0.001f, 1000f)]
    //private float perlinScale = 0.2f;

    private float noiseSeed;

    private Queue<float> noiseBuffer;
    private float lastSum;

    // Start is called before the first frame update
    void Start()
    {
        noiseBuffer = new Queue<float>(5);
        lastSum = 0;
        if (light == null) {
            light = GetComponent<Light>();
        }
        if (light == null) {
            Debug.LogError("Light Flicker component is unassigned to a light.");
        } else {
            this.averageIntensity = light.intensity;
        }
        noiseSeed = Random.value * 100f;
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

        float perlinCoord = Time.time * flickerSpeed;
        float noise = Mathf.PerlinNoise(noiseSeed, perlinCoord);
        noiseBuffer.Enqueue(noise);
        lastSum += noise;

        //this.light.intensity = intensityScale * lastSum / 5f;
        this.light.intensity = averageIntensity + (2 * flickerIntensity * lastSum / 5f);
    }
}
