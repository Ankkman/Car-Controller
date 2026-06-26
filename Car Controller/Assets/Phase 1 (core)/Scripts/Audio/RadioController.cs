using UnityEngine;

public class RadioController : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource radioSource;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.5f; // Starts at 50% volume

    void Start()
    {
        if (radioSource != null)
        {
            radioSource.volume = musicVolume;
            
            // Start playing automatically if it hasn't already
            if (!radioSource.isPlaying)
            {
                radioSource.Play();
            }
        }
    }

    void Update()
    {
        if (radioSource == null) return;

        // Keep the actual volume slider linked to our inspector slider
        radioSource.volume = musicVolume;

        // Press M to Toggle Pause / Resume
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (radioSource.isPlaying)
            {
                radioSource.Pause();
                Debug.Log("Radio Paused");
            }
            else
            {
                radioSource.UnPause();
                Debug.Log("Radio Resumed");
            }
        }
    }
}
