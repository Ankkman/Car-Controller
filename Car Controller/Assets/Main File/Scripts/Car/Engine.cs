using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Gear
{
    public float ratio;
    public float shiftUpRPM;
    public float shiftDownRPM;
}

public class Engine : MonoBehaviour
{
    [Header("Engine Curve")]
    public AnimationCurve torqueCurve;       // X = RPM, Y = Torque (Nm)

    [Header("Drivetrain")]
    public List<WheelCollider> driveWheels;  // Rear wheels for RWD
    public float finalDriveRatio = 3.5f;

    [Header("Gearbox")]
    public Gear[] gears;
    public bool automatic = true;
    public float shiftCooldown = 0.3f;       // Seconds between shifts (prevents flicker)

    [Header("Debug / Input")]
    public float throttleInput;              // 0..1 for forward (negative not used here)

    [SerializeField] private float engineRPM;
    [SerializeField] private float wheelRPM;
    [SerializeField] private int currentGear;

    private float gearRatio;
    private float lastShiftTime = -1f;

    public float EngineRPM => engineRPM;
    public int CurrentGear => currentGear;

    void Start()
    {
        if (gears == null || gears.Length == 0)
        {
            gears = new Gear[] { new Gear { ratio = 3.0f, shiftUpRPM = 6000, shiftDownRPM = 3500 } };
        }
        currentGear = 0;
        gearRatio = gears[currentGear].ratio;
        engineRPM = 1000f; // idle
    }

    void Update()
    {
        // Automatic shifting with cooldown
        if (automatic && gears.Length > 1 && Time.time - lastShiftTime > shiftCooldown)
        {
            if (engineRPM > gears[currentGear].shiftUpRPM && currentGear < gears.Length - 1)
                ShiftUp();
            else if (engineRPM < gears[currentGear].shiftDownRPM && currentGear > 0)
                ShiftDown();
        }
    }

    void FixedUpdate()
    {
        // Average RPM of grounded drive wheels
        wheelRPM = 0f;
        int grounded = 0;
        foreach (var wheel in driveWheels)
        {
            if (wheel.isGrounded)
            {
                wheelRPM += wheel.rpm;
                grounded++;
            }
        }
        if (grounded > 0)
            wheelRPM /= grounded;

        // Engine RPM is exactly the wheel RPM multiplied by the gear ratios
        // No smoothing – the physical connection is rigid
        engineRPM = Mathf.Abs(wheelRPM) * gearRatio * finalDriveRatio;

        // Prevent engine RPM from dropping to zero when stationary (set a small idle)
        if (engineRPM < 900f && throttleInput > 0.01f)
            engineRPM = 1000f; // idle under throttle
        else if (engineRPM < 800f)
            engineRPM = 800f;  // absolute idle

        // Compute torque from curve, multiplied by throttle
        float engineTorque = torqueCurve.Evaluate(engineRPM) * throttleInput;
        float torquePerWheel = engineTorque * gearRatio * finalDriveRatio / driveWheels.Count;

        foreach (var wheel in driveWheels)
            wheel.motorTorque = torquePerWheel;
    }

    void ShiftUp()
    {
        if (currentGear < gears.Length - 1)
        {
            currentGear++;
            gearRatio = gears[currentGear].ratio;
            lastShiftTime = Time.time;
        }
    }

    void ShiftDown()
    {
        if (currentGear > 0)
        {
            currentGear--;
            gearRatio = gears[currentGear].ratio;
            lastShiftTime = Time.time;
        }
    }

    public void SetGear(int index)
    {
        if (index >= 0 && index < gears.Length)
        {
            currentGear = index;
            gearRatio = gears[index].ratio;
            lastShiftTime = Time.time;
        }
    }
}