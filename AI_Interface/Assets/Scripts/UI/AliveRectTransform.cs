using UnityEngine;

public class AliveRectTransform : MonoBehaviour
{
    public float noiseScale = 1f;
    public float noiseSpeed = 1f;

    private RectTransform rectTransform;
    private Vector3 initialScale;
    private float timeOffset;

    public float heartbeatSpeed = 1f;
    public float heartbeatScale = 0.1f;
    public float heartbeatOffset = 0.5f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        initialScale = rectTransform.localScale;
        timeOffset = Random.Range(0f, 100f); // Randomize the start time offset
    }

    void FixedUpdate()
    {
        Vector3 scale = Vector3.zero;

        // Calculate the scale based on a sine wave
        float scaleFactor = Mathf.Sin(Time.time * heartbeatSpeed + heartbeatOffset) * heartbeatScale + 1f;
       scale = initialScale * scaleFactor;

        // Apply the scale to the RectTransform
        rectTransform.localScale = scale;

        // Calculate noise values based on time and scale factors
        float noiseX = Mathf.PerlinNoise(Time.time * noiseSpeed + timeOffset, 0.1f) * 2f - 1f;
        float noiseY = Mathf.PerlinNoise(0.1f, Time.time * noiseSpeed + timeOffset) * 2f - 1f;

        // Apply noise to the scale of the RectTransform
        float scaleChange = Mathf.PerlinNoise(Time.time * noiseSpeed + timeOffset, Time.time * noiseSpeed + timeOffset) * noiseScale;
        scale = initialScale + new Vector3(noiseX * scaleChange, noiseY * scaleChange, 0f);

        rectTransform.localScale = scale;
    }
}