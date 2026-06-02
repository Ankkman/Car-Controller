using UnityEngine;

public class HeadlightController : MonoBehaviour
{
    public Light leftHeadlight;
    public Light rightHeadlight;

    private bool headlightsOn;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            headlightsOn = !headlightsOn;

            if (leftHeadlight != null)
                leftHeadlight.enabled = headlightsOn;

            if (rightHeadlight != null)
                rightHeadlight.enabled = headlightsOn;
        }
    }
}