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
    public bool isAutomaticMode = true; // <--- ADD THIS LINE
    // --- FIXED: Formally default variables to start locked in Park Mode ---
    public TransmissionMode currentMode = TransmissionMode.Park;
    public bool isParked = true;
    public float transmissionSwitchSpeed = 1f;

    [Header("Mobile Settings")]
    public bool useMobileInputs = false; 

    [HideInInspector] public float mobileVerticalInput = 0f;
    [HideInInspector] public float mobileSteerInput = 0f;


    private Rigidbody rb;
    private float throttleInput;
    private float steerInput;
    private float brakeInput;

    public BrakeSystem brakeSystem;
    public Engine engine;

    private float initializationTimer = 0f; // fix sound at start


    public float ForwardSpeed =>
        Vector3.Dot(transform.forward, rb.linearVelocity);

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

        // --- CRITICAL SPIDER-FIX FOR PC STARTUP SPIKE ---
        // Forcefully clamp physical inputs to 0 on frame zero before FixedUpdate runs
        throttleInput = 0f;
        brakeInput = 1f;
        if (brakeSystem != null)
            brakeSystem.SetBrakeInput(brakeInput);
            
        if (engine != null)
            engine.throttleInput = 0f;
    }


    void Update()
    {
        // --- PC IGNITION INPUT PROTECTION ---
        if (initializationTimer < 0.05f)
        {
            initializationTimer += Time.deltaTime;
            throttleInput = 0f;
            brakeInput = 1f;
            if (brakeSystem != null) brakeSystem.SetBrakeInput(brakeInput);
            return; 
        }

        //-------------------------------------------------
        // PARK MODE INPUT CHECK
        //-------------------------------------------------
        if (Input.GetKeyDown(KeyCode.P))
        {
            isParked = !isParked;

            if (isParked)
            {
                currentMode = TransmissionMode.Park;
                throttleInput = 0f;
                brakeInput = 1f;
                if (brakeSystem != null) brakeSystem.SetBrakeInput(brakeInput);
                return; 
            }
            else
            {
                currentMode = TransmissionMode.Neutral;
            }
        }

        //------------------------------------------------- 
        // PARK MODE EXECUTION 
        //------------------------------------------------- 
        if (isParked) { 
            throttleInput = 0f; 
            brakeInput = 1f; 
            if (brakeSystem != null) brakeSystem.SetBrakeInput(brakeInput); 
            return; 
        } 

        // --- CHANGE STARTS HERE --- 
        float verticalInput = 0f; 
        float steerInputTemp = 0f; 

        if (useMobileInputs) { 
            verticalInput = mobileVerticalInput; 
            steerInputTemp = mobileSteerInput; 
            steerInput = steerInputTemp; 
        } else { 
            verticalInput = Input.GetAxis("Vertical"); 
            steerInput = Input.GetAxis("Horizontal"); 
        } 
        // --- CHANGE ENDS HERE --- 

        float speed = Mathf.Abs(ForwardSpeed); 


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
            // SPLIT LOGIC FOR AUTO VS MANUAL
            if (isAutomaticMode)
            {
                // AUTO MODE: 
                // Brake (-1) = Give torque to go backwards.
                // Gas (+1) = Cut torque and apply physical brakes to stop reverse momentum.
                if (verticalInput < -0.1f) 
                {
                    throttleInput = 1f; // Send full throttle to move backwards
                    brakeInput = 0f;    // Do not lock wheels
                }
                else
                {
                    throttleInput = 0f; // Cut all reverse power
                    brakeInput = verticalInput > 0.1f ? 1f : 0f; // If Gas (+1) is pressed, apply brakes!
                }
            }
            else
            {
                // MANUAL MODE: Gas pedal triggers throttle, Brake pedal triggers physical brake
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
            
            float handbrake = Input.GetKey(KeyCode.LeftShift) ? 1f : 0f;
            brakeSystem.SetHandbrakeInput(handbrake);
        }
    }

    void FixedUpdate()
    {
        foreach (var w in frontWheels)
        {
            w.steerAngle = steerInput * maxSteerAngle;
        }

        if (engine != null)
        {
            engine.throttleInput = throttleInput;
        }
    }
}
