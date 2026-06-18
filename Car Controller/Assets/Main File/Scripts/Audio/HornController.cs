using UnityEngine;

public class HornController : MonoBehaviour
{
    public AudioSource hornAudio;

    void Update()
    {
        if (hornAudio == null) return;

        // Keyboard PC Inputs
        if (Input.GetKeyDown(KeyCode.H)) PlayHorn();
        if (Input.GetKeyUp(KeyCode.H)) StopHorn();
    }

    // --- PUBLIC METHODS FOR MOBILE & PC USE ---
    public void PlayHorn()
    {
        if (hornAudio != null && !hornAudio.isPlaying)
        {
            hornAudio.Play();
        }
    }

    public void StopHorn()
    {
        if (hornAudio != null)
        {
            hornAudio.Stop();
        }
    }
}
