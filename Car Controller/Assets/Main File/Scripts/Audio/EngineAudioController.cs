using UnityEngine;

public class EngineAudioController : MonoBehaviour
{
    public Engine engine; 
    public CarController carController; 
    public AudioSource engineSource;

    [Header("Pitch Settings")]
    public float minPitch = 0.8f;
    public float maxPitch = 2.2f;

    [Header("Volume Settings")]
    public float minVolume = 0.3f;  
    public float maxVolume = 1.0f;  

    [Header("RPM Settings")]
    public float maxRPM = 7000f;

    void Start()
    {
        if (carController == null) 
            carController = GetComponentInParent<CarController>();

        // --- CRITICAL FIX: FORCE ABSOLUTE SILENCE ON FRAME ZERO ---
        if (engineSource != null)
        {
            engineSource.playOnAwake = false; // Disable awake playback via code
            engineSource.volume = 0f;         // Kill volume instantly at boot
            engineSource.Stop();              // Force stop any native caching loops
        }
    }

    void Update()
    {
        if (engine == null || engineSource == null)
            return;

        // Mute engine sound completely when parked
        if (carController != null && carController.isParked)
        {
            engineSource.volume = 0f;
            
            // --- OPTIMIZATION: Stop the source channel so it cannot leak sound ---
            if (engineSource.isPlaying)
            {
                engineSource.Stop();
            }
            return; 
        }

        // Make sure it starts playing once we unpark
        if (!engineSource.isPlaying)
        {
            engineSource.Play();
        }

        // Get a 0 to 1 value based on current RPM
        float rpmNormalized = Mathf.Clamp01(engine.EngineRPM / maxRPM);

        // 1. Smoothly adjust Pitch
        engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, rpmNormalized);

        // 2. Smoothly adjust Volume based on engine load
        engineSource.volume = Mathf.Lerp(minVolume, maxVolume, rpmNormalized);
    }
}
