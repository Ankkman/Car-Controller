using UnityEngine;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public List<WheelCollider> frontWheels;   // 2 wheels
    public List<WheelCollider> rearWheels;    // 2 wheels

    [Header("Performance")]
    public float motorTorque = 1500f;         // Peak torque at the wheels (Nm)
    public float maxSteerAngle = 30f;
    public float brakeTorque = 3000f;

    [Header("Center of Mass")]
    public Vector3 centerOfMassOffset = new Vector3(0, -0.5f, 0);

    private Rigidbody rb;
    private float throttleInput;
    private float steerInput;
    private float brakeInput;

    public BrakeSystem brakeSystem;
    public Engine engine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass += centerOfMassOffset;

        // Setup friction curves
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
    }

    void Update()
    {
        // Get input – you can replace with your own input system
        throttleInput = Input.GetAxis("Vertical");

        // Disable throttle while braking
        if (Input.GetKey(KeyCode.Space))
        {
            throttleInput = 0f;
        }

        steerInput = Input.GetAxis("Horizontal");    // A/D or Left/Right

        // Existing brake
        brakeInput = Input.GetKey(KeyCode.Space) ? 1f : 0f;
        if (brakeSystem != null)
            brakeSystem.SetBrakeInput(brakeInput);

        // Handbrake – full locking of rear wheels for sliding
        float handbrake = Input.GetKey(KeyCode.LeftShift) ? 1f : 0f;
        if (brakeSystem != null)
            brakeSystem.SetHandbrakeInput(handbrake);

    }

    void FixedUpdate()
    {
        // Steering (front wheels)
        foreach (var w in frontWheels)
            w.steerAngle = steerInput * maxSteerAngle;

        // Pass throttle to the engine simulation system
        if (engine != null)
        {
            engine.throttleInput = throttleInput; 
        }


    }
}