using UnityEngine; 

public class ReverseCameraUI : MonoBehaviour  
{  
    public GameObject reverseUI;  
    public Rigidbody rb; 

    // Core controller reference to read active gear states  
    private CarController carController; 

    void Start()  
    {  
        if (rb == null)  
        {  
            rb = GetComponent<Rigidbody>();  
        } 

        // Fetch the car controller attached to this prefab  
        carController = GetComponent<CarController>(); 

        // DYNAMIC LINK: Find the panel in the scene if it's missing from the prefab  
        if (reverseUI == null)  
        {  
            GameObject inGameUI = GameObject.FindGameObjectWithTag("InGameUI");  
            if (inGameUI != null)  
            {  
                // Ensure your UI panel child inside the canvas is named exactly "ReverseCameraPanel"  
                Transform foundPanel = inGameUI.transform.Find("ReverseCameraPanel");  
                if (foundPanel != null)  
                {  
                    reverseUI = foundPanel.gameObject;  
                }  
            }  
        }  
    } 

    void Update()  
    {  
        // Safety exit if references aren't fully resolved yet  
        if (reverseUI == null || rb == null) return; 

        // Calculate physical direction vector speed (Negative = moving backward)  
        float speed = Vector3.Dot(rb.transform.forward, rb.linearVelocity); 

        if (carController != null)  
        {  
            // SMART GEAR CHECK: Is the transmission explicitly shifted into Reverse?  
            bool isInReverseGear = (carController.currentMode == CarController.TransmissionMode.Reverse); 

            // PHYSICS CHECK: Is the vehicle actively rolling backward down a hill?  
            bool isRollingBackward = (speed < -0.2f); 

            // Pop up the overlay if either condition is met  
            reverseUI.SetActive(isInReverseGear || isRollingBackward);  
        }  
        else  
        {  
            // Pure physics fallback calculation if carController isn't ready  
            reverseUI.SetActive(speed < -0.5f);  
        }  
    }  
}
