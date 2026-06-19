using UnityEngine;

public class ManualTransmission : MonoBehaviour
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

        // In Manual Mode, the car NEVER shifts on its own. 
        // It solely depends on the mobile UI buttons calling these methods:
    }

    // --- PUBLIC METHODS FOR YOUR UI BUTTONS ---

    public void ShiftUp()
    {
        if (carController == null || carController.isParked) return;

        if (carController.currentMode == CarController.TransmissionMode.Reverse)
            carController.currentMode = CarController.TransmissionMode.Neutral;
        else if (carController.currentMode == CarController.TransmissionMode.Neutral)
            carController.currentMode = CarController.TransmissionMode.Drive;
    }

    public void ShiftDown()
    {
        if (carController == null || carController.isParked) return;

        if (carController.currentMode == CarController.TransmissionMode.Drive)
            carController.currentMode = CarController.TransmissionMode.Neutral;
        else if (carController.currentMode == CarController.TransmissionMode.Neutral)
            carController.currentMode = CarController.TransmissionMode.Reverse;
    }
}