using UnityEngine;

public class EngineAudioController : MonoBehaviour
{
    public Engine engine; 
    public CarController carController; 
    public VehicleInputHandler inputHandler; 
    public AudioSource engineSource;

    [Header("Pitch Settings")]
    public float minPitch = 0.75f; // Slightly lower for a throatier idle
    public float maxPitch = 2.3f;

    [Header("Volume Settings")]
    public float minVolume = 0.5f;   // FIX 1: Raised from 0.3f so it's always audible on mobile
    public float maxVolume = 1.0f;  

    [Header("RPM Settings")]
    public float maxRPM = 7000f;

    [Header("Audio Smoothing (Realism Boost)")]
    [Tooltip("How fast the engine pitch matches physical wheel speed change.")]
    public float pitchSmoothSpeed = 12f; 
    [Tooltip("Adds volume depth when pinning throttle down vs letting go.")]
    public float loadVolumeContribution = 0.25f; 

    [Header("Transmission Audio")]
    public AudioSource transmissionEffectsSource;
    public AudioClip gearShiftClickClip;

    private float misShiftTimer = 0f;
    private bool isMisShifting = false;
    private float misShiftPitchTarget = 0f;

    private int lastMonitoredGear = 0;
    
    // Smooth tracking variables
    private float smoothedRPM = 800f;
    private float smoothedThrottle = 0f;

    void Start()
    {
        if (carController == null) carController = GetComponent<CarController>();
        if (carController == null) carController = GetComponentInParent<CarController>();

        if (inputHandler == null) inputHandler = GetComponent<VehicleInputHandler>();
        if (inputHandler == null) inputHandler = GetComponentInParent<VehicleInputHandler>();
        
        if (engine == null) engine = GetComponent<Engine>();
        if (engine == null) engine = GetComponentInParent<Engine>();

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

        // FIX 1: Heavy boost for mobile hardware speakers
        if (Application.isMobilePlatform)
        {
            minVolume = 0.65f; // Ensures you can hear the car engine clearly even at low idle speeds
            pitchSmoothSpeed = 10f; 
        }
    }

    void Update()
    {
        if (inputHandler == null)
        {
            GameObject playerVehicle = GameObject.FindGameObjectWithTag("Player");
            if (playerVehicle != null) inputHandler = playerVehicle.GetComponent<VehicleInputHandler>();
        }

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

        // --- REALISM ENGINE: SMOOTHING AND LOAD CALCULATIONS ---
        // 1. Smooth out RPM jumps so pitch changes mimic real mechanical inertia
        smoothedRPM = Mathf.MoveTowards(smoothedRPM, engine.EngineRPM, pitchSmoothSpeed * Time.deltaTime * 500f);
        
        // 2. Extract active throttle state to alter audio dynamics based on pedal load
        float currentInputThrottle = (engine.throttleInput);
        smoothedThrottle = Mathf.Lerp(smoothedThrottle, Mathf.Abs(currentInputThrottle), Time.deltaTime * 8f);

        if (isMisShifting)
        {
            misShiftTimer -= Time.deltaTime;
            float wobble = Mathf.Sin(Time.time * 75f) * 0.2f; // Fast aggressive rev-limiter rattle
            engineSource.pitch = misShiftPitchTarget + wobble;
            engineSource.volume = maxVolume; 

            if (misShiftTimer <= 0f) isMisShifting = false; 
        }
        else
        {
            // Normalize our smoothed RPM parameter
            float rpmNormalized = Mathf.Clamp01(smoothedRPM / maxRPM);
            
            // 3. Pitch Engine matching our smooth calculation curve
            engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, rpmNormalized);
            
            // 4. FIX 2: Dynamic Load Volume
            // The car will sound significantly louder when accelerating hard, and quieter when coasting.
            float baseVolume = Mathf.Lerp(minVolume, maxVolume, rpmNormalized);
            float loadBonus = smoothedThrottle * loadVolumeContribution;
            
            engineSource.volume = Mathf.Clamp(baseVolume + loadBonus, minVolume, maxVolume);
        }

        // Track gear changes
        int activeGear = engine.automatic ? engine.CurrentGear : (inputHandler != null ? inputHandler.CurrentManualGear : 0);

        if (activeGear != lastMonitoredGear)
        {
            if (inputHandler != null && !inputHandler.useAutomaticTransmission) 
            {
                PlayGearClick();
            }
            lastMonitoredGear = activeGear;
        }
    }

    public void PlayGearClick()
    {
        if (transmissionEffectsSource != null && gearShiftClickClip != null)
        {
            transmissionEffectsSource.PlayOneShot(gearShiftClickClip, 0.9f);
        }
    }

    public void TriggerEngineMisShiftScream()
    {
        isMisShifting = true;
        misShiftTimer = 0.45f; 
        misShiftPitchTarget = maxPitch * 1.15f; 
    }
}
