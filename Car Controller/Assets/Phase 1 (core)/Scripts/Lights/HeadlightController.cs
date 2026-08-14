using UnityEngine;

public class HeadlightController : MonoBehaviour
{
    public Light leftHeadlight;
    public Light rightHeadlight;

    [Header("Audio Settings")]
    public AudioSource headlightSwitchSound; 

    private bool headlightsOn;
    private CarController carController; // Reference to track ignition

    void Start()
    {
        carController = GetComponentInParent<CarController>();
    }

    void Update()
    {
        // PC Input
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleHeadlights();
        }

        // --- MASTER IGNITION SAFETY CHECK ---
        // If the headlights are on, but the engine gets turned off, shut them down!
        if (carController != null && !carController.engineOn && headlightsOn)
        {
            ForceTurnOffHeadlights();
        }
    }

    public void ToggleHeadlights()
    {
        // --- BLOCKED IF ENGINE IS OFF ---
        if (carController != null && !carController.engineOn) return;

        headlightsOn = !headlightsOn;

        if (leftHeadlight != null) leftHeadlight.enabled = headlightsOn;
        if (rightHeadlight != null) rightHeadlight.enabled = headlightsOn;

        if (headlightSwitchSound != null)
        {
            headlightSwitchSound.Play();
        }
    }

    private void ForceTurnOffHeadlights()
    {
        headlightsOn = false;
        if (leftHeadlight != null) leftHeadlight.enabled = false;
        if (rightHeadlight != null) rightHeadlight.enabled = false;
    }
}
