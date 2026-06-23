using UnityEngine;

public class HeadlightController : MonoBehaviour
{
    public Light leftHeadlight;
    public Light rightHeadlight;

    [Header("Audio Settings")]
    public AudioSource headlightSwitchSound; // Drag an AudioSource here for the click sound

    private bool headlightsOn;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleHeadlights();
        }
    }

    // --- PUBLIC METHOD FOR MOBILE ACCESS ---
    public void ToggleHeadlights()
    {
        headlightsOn = !headlightsOn;

        if (leftHeadlight != null)
            leftHeadlight.enabled = headlightsOn;

        if (rightHeadlight != null)
            rightHeadlight.enabled = headlightsOn;

        // --- PLAY HEADLIGHT CLICK SOUND ---
        if (headlightSwitchSound != null)
        {
            headlightSwitchSound.Play();
        }
    }
}
