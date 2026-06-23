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

        if (engineSource != null)
        {
            engineSource.playOnAwake = false;
            engineSource.volume = 0f;
            engineSource.Stop();
        }
    }

    void Update()
    {
        if (engine == null || engineSource == null || carController == null)
            return;

        // UPDATED CHECK: Mute engine if car is OFF
        if (!carController.engineOn)
        {
            engineSource.volume = 0f;
            if (engineSource.isPlaying) engineSource.Stop();
            return; 
        }

        // Make sure it starts playing once the engine is ON
        if (!engineSource.isPlaying)
        {
            engineSource.Play();
        }

        // Get a 0 to 1 value based on current RPM
        float rpmNormalized = Mathf.Clamp01(engine.EngineRPM / maxRPM);

        engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, rpmNormalized);
        engineSource.volume = Mathf.Lerp(minVolume, maxVolume, rpmNormalized);
    }
}