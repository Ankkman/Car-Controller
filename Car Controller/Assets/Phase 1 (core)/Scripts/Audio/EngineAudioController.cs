using UnityEngine;

public class EngineAudioController : MonoBehaviour
{
    public Engine engine; 
    public CarController carController; 
    public VehicleInputHandler inputHandler; 
    public AudioSource engineSource;

    [Header("Pitch Settings")]
    public float minPitch = 0.8f;
    public float maxPitch = 2.2f;

    [Header("Volume Settings")]
    public float minVolume = 0.3f;  
    public float maxVolume = 1.0f;  

    [Header("RPM Settings")]
    public float maxRPM = 7000f;

    [Header("Transmission Audio")]
    public AudioSource transmissionEffectsSource;
    public AudioClip gearShiftClickClip;

    // --- NEW PROCEDURAL MIS-SHIFT TRACKERS ---
    private float misShiftTimer = 0f;
    private bool isMisShifting = false;
    private float misShiftPitchTarget = 0f;

    private int lastMonitoredGear = 0;

    void Start()
    {
        if (carController == null) 
            carController = GetComponentInParent<CarController>();
            
        if (inputHandler == null)
            inputHandler = GetComponentInParent<VehicleInputHandler>();

        if (engineSource != null)
        {
            engineSource.playOnAwake = false;
            engineSource.volume = 0f;
            engineSource.Stop();
        }

        if (transmissionEffectsSource != null)
        {
            transmissionEffectsSource.playOnAwake = false;
            transmissionEffectsSource.loop = false;
        }

        if (Application.isMobilePlatform)
        {
            minVolume = Mathf.Clamp(minVolume + 0.25f, 0f, 0.9f);
        }
    }

    void Update()
    {
        if (engine == null || engineSource == null || carController == null)
            return;

        if (!carController.engineOn)
        {
            engineSource.volume = 0f;
            if (engineSource.isPlaying) engineSource.Stop();
            return; 
        }

        if (!engineSource.isPlaying)
        {
            engineSource.Play();
        }

        // --- ENGINE MIS-SHIFT HIJACK LOGIC ---
        if (isMisShifting)
        {
            misShiftTimer -= Time.deltaTime;
            
            // Add a vibrating engine wobble effect using a fast math sine wave
            float wobble = Mathf.Sin(Time.time * 60f) * 0.15f;
            engineSource.pitch = misShiftPitchTarget + wobble;
            engineSource.volume = maxVolume; // Max out volume during a bad shift

            if (misShiftTimer <= 0f)
            {
                isMisShifting = false; // Reset back to normal engine simulation
            }
        }
        else
        {
            // Normal Engine Physics Calculation
            float rpmNormalized = Mathf.Clamp01(engine.EngineRPM / maxRPM);
            engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, rpmNormalized);
            engineSource.volume = Mathf.Lerp(minVolume, maxVolume, rpmNormalized);
        }

        // Track gear changes to play the click sound
        int activeGear = engine.automatic ? engine.CurrentGear : (inputHandler != null ? inputHandler.CurrentManualGear : 0);

        if (activeGear != lastMonitoredGear)
        {
            // --- ADD THIS IF CHECK ---
            // Only play the sound in manual mode!
            if (!engine.automatic) 
            {
                PlayGearClick();
            }
            // --------------------------
            
            lastMonitoredGear = activeGear;
        }
    }

    public void PlayGearClick()
    {
        if (transmissionEffectsSource != null && gearShiftClickClip != null)
        {
            transmissionEffectsSource.PlayOneShot(gearShiftClickClip, 0.8f);
        }
    }

    // NEW PUBLIC METHOD: Call this when a bad gear shift is attempted!
    public void TriggerEngineMisShiftScream()
    {
        isMisShifting = true;
        misShiftTimer = 0.45f; // Screen lasts for about half a second
        misShiftPitchTarget = maxPitch * 1.25f; // Boosts pitch past the normal maximum redline
    }
}
