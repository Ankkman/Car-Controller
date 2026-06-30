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
        // --- FIXED INITIALIZATION FOR RUNTIME DYNAMIC COUPLING ---
        if (carController == null) carController = GetComponent<CarController>();
        if (carController == null) carController = GetComponentInParent<CarController>();

        if (inputHandler == null) inputHandler = GetComponent<VehicleInputHandler>();
        if (inputHandler == null) inputHandler = GetComponentInParent<VehicleInputHandler>();
        
        // Final fallback if the audio controller sits on a deeply nested child object
        if (inputHandler == null)
        {
            Transform searchObj = transform;
            while (searchObj != null && inputHandler == null)
            {
                inputHandler = searchObj.GetComponent<VehicleInputHandler>();
                searchObj = searchObj.parent;
            }
        }

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
        // Try to relink if our handler was missed during awake instantiation frames
        if (inputHandler == null)
        {
            GameObject playerVehicle = GameObject.FindGameObjectWithTag("Player");
            if (playerVehicle != null)
            {
                inputHandler = playerVehicle.GetComponent<VehicleInputHandler>();
            }
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

        // --- ENGINE MIS-SHIFT HIJACK LOGIC ---
        if (isMisShifting)
        {
            misShiftTimer -= Time.deltaTime;
            
            float wobble = Mathf.Sin(Time.time * 60f) * 0.15f;
            engineSource.pitch = misShiftPitchTarget + wobble;
            engineSource.volume = maxVolume; 

            if (misShiftTimer <= 0f)
            {
                isMisShifting = false; 
            }
        }
        else
        {
            float rpmNormalized = Mathf.Clamp01(engine.EngineRPM / maxRPM);
            engineSource.pitch = Mathf.Lerp(minPitch, maxPitch, rpmNormalized);
            engineSource.volume = Mathf.Lerp(minVolume, maxVolume, rpmNormalized);
        }

        // Track gear changes to play the click sound
        int activeGear = engine.automatic ? engine.CurrentGear : (inputHandler != null ? inputHandler.CurrentManualGear : 0);

        if (activeGear != lastMonitoredGear)
        {
            // Only play the click sound if manual mode is running!
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
            transmissionEffectsSource.PlayOneShot(gearShiftClickClip, 0.8f);
        }
    }

    public void TriggerEngineMisShiftScream()
    {
        isMisShifting = true;
        misShiftTimer = 0.45f; 
        misShiftPitchTarget = maxPitch * 1.25f; 
    }
}
