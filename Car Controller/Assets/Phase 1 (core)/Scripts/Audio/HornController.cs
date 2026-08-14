using UnityEngine;

public class HornController : MonoBehaviour
{
    public AudioSource hornAudio;
    private CarController carController; // Reference to track ignition

    void Start()
    {
        carController = GetComponentInParent<CarController>();
    }

    void Update()
    {
        if (hornAudio == null) return;

        // --- MASTER IGNITION SAFETY CHECK ---
        // If the horn is actively playing but the engine is shut down, kill the horn sound instantly
        if (carController != null && !carController.engineOn && hornAudio.isPlaying)
        {
            StopHorn();
            return;
        }

        // Keyboard PC Inputs
        if (Input.GetKeyDown(KeyCode.H)) PlayHorn();
        if (Input.GetKeyUp(KeyCode.H)) StopHorn();
    }

    public void PlayHorn()
    {
        // --- BLOCKED IF ENGINE IS OFF ---
        if (carController != null && !carController.engineOn) return;

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
