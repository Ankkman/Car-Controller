using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Gear
{
    public float ratio;
    public float shiftUpRPM;
    public float shiftDownRPM;
     public float maxSpeed;  
}

public class Engine : MonoBehaviour
{
    [Header("Engine Curve")]
    public AnimationCurve torqueCurve;       // X = RPM, Y = Torque (Nm)

    [Header("Drivetrain")]
    public List<WheelCollider> driveWheels;  // Rear wheels for RWD
    public float finalDriveRatio = 3.5f;
    public float reverseGearRatio = -2.8f;

    [Header("Gearbox")]
    public Gear[] gears;
    public bool automatic = true;
    public float shiftCooldown = 0.3f;       // Seconds between shifts (prevents flicker)

    [Header("Debug / Input")]
    public float throttleInput;              // -1..1 range to support reverse

    [SerializeField] private float engineRPM;
    [SerializeField] private float wheelRPM;
    [SerializeField] private int currentGear; // Used exclusively for Auto Mode

    [HideInInspector] public int manualGearIndex = -1; // -1 = Neutral/Invalid. 0-5 = 1st to 6th Gear.

    private float gearRatio;
    private float lastShiftTime = -1f;
    private CarController carController;

    public float EngineRPM => engineRPM;
    public int CurrentGear => currentGear;

    void Start()
    {
        carController = GetComponent<CarController>();

        if (gears == null || gears.Length == 0)
        {
            // Changed from 1 gear to 6 gears so it works out of the box!
            gears = new Gear[] {
                new Gear { ratio = 3.5f, shiftUpRPM = 6000, shiftDownRPM = 3500 },
                new Gear { ratio = 2.2f, shiftUpRPM = 6000, shiftDownRPM = 3500 },
                new Gear { ratio = 1.5f, shiftUpRPM = 6000, shiftDownRPM = 3500 },
                new Gear { ratio = 1.1f, shiftUpRPM = 6000, shiftDownRPM = 3500 },
                new Gear { ratio = 0.85f, shiftUpRPM = 6000, shiftDownRPM = 3500 },
                new Gear { ratio = 0.65f, shiftUpRPM = 6000, shiftDownRPM = 3500 }
            };
        }
        currentGear = 0;
        gearRatio = gears[currentGear].ratio;
        engineRPM = 1000f; // idle
    }

    public void SetManualGear(int index)
    {
        manualGearIndex = index;
    }

    void Update()
    {
        // Automatic shifting with cooldown (Only runs if automatic is true)
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

        // --- DYNAMIC GEAR RATIO SELECTOR ---
        float activeRatio = gearRatio; // Default to the Auto ratio

        if (carController != null)
        {
            // If we are in MANUAL mode and currently driving forward
            if (!automatic && carController.currentMode == CarController.TransmissionMode.Drive)
            {
                // If we have a valid manual gear index, use its ratio
                if (manualGearIndex >= 0 && manualGearIndex < gears.Length)
                {
                    activeRatio = gears[manualGearIndex].ratio;
                }
            }
            // If we are in Reverse
            else if (carController.currentMode == CarController.TransmissionMode.Reverse)
            {
                activeRatio = reverseGearRatio;
            }
        }
        // ----------------------------------------

        // Engine RPM is exactly the wheel RPM multiplied by the active gear ratios
        engineRPM = Mathf.Abs(wheelRPM) * activeRatio * finalDriveRatio;

        // Prevent engine RPM from dropping to zero when stationary (set a small idle)
        if (engineRPM < 900f && throttleInput > 0.01f)
            engineRPM = 1000f; // idle under throttle
        else if (engineRPM < 800f)
            engineRPM = 800f;  // absolute idle

        // Compute torque from curve, multiplied by throttle
        float engineTorque = torqueCurve.Evaluate(engineRPM) * throttleInput;
        
        // Apply the active ratio to the torque output
        float torquePerWheel = engineTorque * activeRatio * finalDriveRatio / driveWheels.Count;

        // --- NEW SPEED LIMITER FOR MANUAL MODE ---

                if (!automatic && carController != null && carController.currentMode == CarController.TransmissionMode.Drive)
        {
            if (manualGearIndex >= 0 && manualGearIndex < gears.Length)
            {
                float currentSpeedKmh = carController.ForwardSpeed * 3.6f; 
                float gearMaxSpeed = gears[manualGearIndex].maxSpeed;
                
                // Ensure the Max Speed is set (above 0) so we don't accidentally limit reverse!
                if (gearMaxSpeed > 0.5f)
                {
                    // If we are AT or ABOVE the max speed, cut the torque completely.
                    if (currentSpeedKmh >= gearMaxSpeed)
                    {
                        torquePerWheel = 0f; 
                    }
                }
            }
        }
        
        // // --- TEMP DIAGNOSTIC LIMITER ---
        // if (!automatic && carController != null)
        // {
        //     if (carController.currentMode != CarController.TransmissionMode.Drive)
        //     {
        //         Debug.Log($"Limiter failed: Transmission mode is {carController.currentMode}, expected Drive.");
        //     }
        //     else if (manualGearIndex < 0 || manualGearIndex >= gears.Length)
        //     {
        //         Debug.Log($"Limiter failed: Invalid manualGearIndex ({manualGearIndex}). Max gears is {gears.Length}.");
        //     }
        //     else
        //     {
        //         float currentSpeedKmh = carController.ForwardSpeed * 3.6f; 
        //         float gearMaxSpeed = gears[manualGearIndex].maxSpeed;
                
        //         if (gearMaxSpeed <= 0.5f)
        //         {
        //             Debug.Log($"Limiter failed: maxSpeed for gear {manualGearIndex} is set to {gearMaxSpeed} (Needs to be > 0.5).");
        //         }
        //         else if (currentSpeedKmh >= gearMaxSpeed)
        //         {
        //             torquePerWheel = 0f; 
        //             Debug.Log($"SUCCESS: Speed limit hit! {currentSpeedKmh:F1} >= {gearMaxSpeed}. Torque cut.");
        //         }
        //     }
        // }
        // else if (automatic)
        // {
        //     // If you are testing in automatic mode, the limiter block is completely skipped.
        //     Debug.Log("Limiter failed: The engine is still set to AUTOMATIC mode.");
        // }

       
        
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