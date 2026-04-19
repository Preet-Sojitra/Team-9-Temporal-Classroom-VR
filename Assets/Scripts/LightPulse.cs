using UnityEngine;

public class LightPulse : MonoBehaviour
{
    public float minIntensity = 1f;
    public float maxIntensity = 5f;
    public float pulseSpeed = 1.5f;

    private Light spotLight;

    void Start()
    {
        spotLight = GetComponent<Light>();
    }

    void Update()
    {
        // Sine wave between min and max intensity
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        spotLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
    }
}