using UnityEngine;

public class EngineAudioController : MonoBehaviour
{
    public Engine engine; // Double check this matches your engine script name!
    public AudioSource engineSource;

    [Header("Pitch Settings")]
    public float minPitch = 0.8f;
    public float maxPitch = 2.2f;

    [Header("Volume Settings")]
    public float minVolume = 0.3f;  // Quiet at idle
    public float maxVolume = 1.0f;  // Loud at max RPM

    [Header("RPM Settings")]
    public float maxRPM = 7000f;

    void Update()
    {
        if (engine == null || engineSource == null)
            return;

        // Get a 0 to 1 value based on current RPM
        float rpmNormalized = Mathf.Clamp01(engine.EngineRPM / maxRPM);

        // 1. Smoothly adjust Pitch
        engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, rpmNormalized);

        // 2. Smoothly adjust Volume based on engine load
        engineSource.volume = Mathf.Lerp(minVolume, maxVolume, rpmNormalized);
    }
}
