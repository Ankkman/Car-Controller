using UnityEngine;

public class HornController : MonoBehaviour
{
    public AudioSource hornAudio;

    void Update()
    {
        if (hornAudio == null) return;

        // 1. Play the horn when the H key is first pressed down
        if (Input.GetKeyDown(KeyCode.H))
        {
            hornAudio.Play();
        }

        // 2. Stop the horn instantly the exact moment the H key is released
        if (Input.GetKeyUp(KeyCode.H))
        {
            hornAudio.Stop();
        }
    }
}
