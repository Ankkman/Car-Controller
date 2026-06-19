using UnityEngine;

public class AutomaticTransmission : MonoBehaviour
{
    private CarController carController;

    void Start()
    {
        carController = GetComponent<CarController>();
        if (carController == null) return;

        // Start in Neutral
        carController.currentMode = CarController.TransmissionMode.Neutral;
        carController.isParked = false;
    }

    void Update()
    {
        if (carController == null) return;
        if (carController.isParked) return;

        // --- SIMPLE ARCADE AUTO-TRANSMISSION ---

        // 1. If Gas is pressed, always go to Drive (Forward)
        if (carController.mobileVerticalInput > 0.1f)
        {
            carController.currentMode = CarController.TransmissionMode.Drive;
        }
        // 2. If Brake is pressed, always go to Reverse (Backward)
        else if (carController.mobileVerticalInput < -0.1f)
        {
            carController.currentMode = CarController.TransmissionMode.Reverse;
        }
        // 3. If no pedals are pressed, stay in Neutral (Stop moving)
        else
        {
            carController.currentMode = CarController.TransmissionMode.Neutral;
        }
    }
}