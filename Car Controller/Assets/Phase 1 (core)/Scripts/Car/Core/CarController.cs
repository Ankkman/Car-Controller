using UnityEngine;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    public enum TransmissionMode
    {
        Park,
        Reverse,
        Neutral,
        Drive
    }

    [Header("Wheel Colliders")]
    public List<WheelCollider> frontWheels;
    public List<WheelCollider> rearWheels;

    [Header("Performance")]
    public float motorTorque = 1500f;
    public float maxSteerAngle = 30f;
    public float brakeTorque = 3000f;

    [Header("Center of Mass")]
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0);

    [Header("Transmission")]
    public bool isAutomaticMode = true; 
    public TransmissionMode currentMode = TransmissionMode.Park;
    public float transmissionSwitchSpeed = 1f;

    [Header("Engine State")]
    public bool engineOn = false; // Starts OFF
    public AudioSource engineStartSound; // Drag an AudioSource for the start sound here
    public AudioSource engineStopSound;  // Drag an AudioSource for the stop sound here

    [Header("Mobile Settings")]
    public bool useMobileInputs = false; 

    [HideInInspector] public float mobileVerticalInput = 0f;
    [HideInInspector] public float mobileSteerInput = 0f;

    private Rigidbody rb;
    private float throttleInput;
    private float steerInput;
    private float brakeInput;

    public float CurrentSteerInput => steerInput; // Allows other scripts to read it safely


    public BrakeSystem brakeSystem;
    public Engine engine;

    private float initializationTimer = 0f; // Fixes sound spike at start

    public float ForwardSpeed => rb != null ? Vector3.Dot(transform.forward, rb.linearVelocity) : 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass += centerOfMassOffset;

        WheelFrictionCurve forwardFriction = new WheelFrictionCurve();
        forwardFriction.extremumSlip = 0.4f;
        forwardFriction.extremumValue = 1.0f;
        forwardFriction.asymptoteSlip = 0.8f;
        forwardFriction.asymptoteValue = 0.75f;
        forwardFriction.stiffness = 1.2f;

        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve();
        sidewaysFriction.extremumSlip = 0.2f;
        sidewaysFriction.extremumValue = 1.0f;
        sidewaysFriction.asymptoteSlip = 0.5f;
        sidewaysFriction.asymptoteValue = 0.75f;
        sidewaysFriction.stiffness = 2.0f;

        foreach (var w in frontWheels)
        {
            w.forwardFriction = forwardFriction;
            w.sidewaysFriction = sidewaysFriction;
        }

        foreach (var w in rearWheels)
        {
            w.forwardFriction = forwardFriction;
            w.sidewaysFriction = sidewaysFriction;
        }

        // FIX: Set brake to 0 on frame zero so suspension can settle into the ground!
        throttleInput = 0f;
        brakeInput = 0f; 
        if (brakeSystem != null)
            brakeSystem.SetBrakeInput(brakeInput);
            
        if (engine != null)
            engine.throttleInput = 0f;
    }

    public void ToggleEngineState()
    {
        engineOn = !engineOn;

        if (engineOn)
        {
            // ENGINE ON: Start in Neutral, release brakes
            currentMode = TransmissionMode.Neutral;
            throttleInput = 0f;
            brakeInput = 0f;
            if (brakeSystem != null) brakeSystem.SetBrakeInput(0f);
            
            // Play Start Sound
            if (engineStartSound != null) engineStartSound.Play();
        }
        else
        {
            // ENGINE OFF: Apply physical brakes immediately, cut everything
            currentMode = TransmissionMode.Park;
            throttleInput = 0f;
            brakeInput = 1f;
            if (brakeSystem != null) brakeSystem.SetBrakeInput(brakeInput);
            
            // Play Stop Sound
            if (engineStopSound != null) engineStopSound.Play();
        }
    }

    void Update()
    {
        // --- PC IGNITION INPUT PROTECTION & SPAWN SETTLE ---
        if (initializationTimer < 0.1f) 
        {
            initializationTimer += Time.deltaTime;
            throttleInput = 0f;
            brakeInput = 0f; 
            
            if (brakeSystem != null) brakeSystem.SetBrakeInput(brakeInput);
            
            // --- FIX: Forcefully put Rigidbody to sleep so suspension doesn't jitter/sink on frame zero ---
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            return; 
        }

        // --- MASTER SWITCH: If engine is OFF, do nothing! ---
        if (!engineOn)
        {
            throttleInput = 0f;
            brakeInput = 1f; // Safely lock brakes after the rigid body settles
            if (brakeSystem != null) brakeSystem.SetBrakeInput(brakeInput); 
            return; 
        }

        // --- INPUT PROJECTION SYSTEM --- 
        float verticalInput = 0f; 

        if (useMobileInputs) { 
            verticalInput = mobileVerticalInput; 
            steerInput = mobileSteerInput; 
        } else { 
            verticalInput = Input.GetAxis("Vertical"); 
            steerInput = Input.GetAxis("Horizontal"); 
        } 

        //-------------------------------------------------
        // DRIVE MODE
        //-------------------------------------------------
        if (currentMode == TransmissionMode.Drive)
        {
            throttleInput = Mathf.Max(0f, verticalInput);
            brakeInput = verticalInput < -0.1f ? 1f : 0f;
            if (Input.GetKey(KeyCode.Space)) brakeInput = 1f;
        }
        //-------------------------------------------------
        // REVERSE MODE
        //-------------------------------------------------
        else if (currentMode == TransmissionMode.Reverse)
        {
            if (isAutomaticMode)
            {
                if (verticalInput < -0.1f) 
                {
                    throttleInput = 1f; 
                    brakeInput = 0f;    
                }
                else
                {
                    throttleInput = 0f; 
                    brakeInput = verticalInput > 0.1f ? 1f : 0f; 
                }
            }
            else
            {
                throttleInput = Mathf.Max(0f, verticalInput); 
                brakeInput = verticalInput < -0.1f ? 1f : 0f;
            }

            if (Input.GetKey(KeyCode.Space)) brakeInput = 1f;
        }
        //-------------------------------------------------
        // NEUTRAL
        //-------------------------------------------------
        else
        {
            throttleInput = 0f;
            brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        }

        // --- CRITICAL BRAKE OUTPUT PIPELINE ---
        if (brakeSystem != null)
        {
            brakeSystem.SetBrakeInput(brakeInput);
        }

        if (engine != null)
        {
            engine.throttleInput = throttleInput;
        }

        HandleSteering();
    }

    private void HandleSteering()
    {
        float targetSteerAngle = steerInput * maxSteerAngle;
        foreach (var wheel in frontWheels)
        {
            wheel.steerAngle = targetSteerAngle;
        }
    }
}
