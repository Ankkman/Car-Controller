using UnityEngine;
using System.Collections.Generic;

public class TireScreechController : MonoBehaviour
{
    [Header("Wheels")]
    public List<WheelCollider> wheels;

    [Header("Audio")]
    public AudioSource screechAudio;

    [Header("Settings")]
    public float slipThreshold = 0.4f;
    public float speedThreshold = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Safely verify the audio state without forcing abrupt state changes
        if (screechAudio != null)
        {
            screechAudio.loop = true;
            if (!screechAudio.isPlaying)
            {
                screechAudio.Play();
            }
        }
    }

    void Update()
    {
        if (screechAudio == null || rb == null)
            return;

        bool screeching = false;
        float speedKmh = rb.linearVelocity.magnitude * 3.6f;

        if (speedKmh > speedThreshold)
        {
            foreach (var wheel in wheels)
            {
                WheelHit hit;
                if (wheel.GetGroundHit(out hit))
                {
                    float slip = Mathf.Max(Mathf.Abs(hit.forwardSlip), Mathf.Abs(hit.sidewaysSlip));

                    if (slip > slipThreshold)
                    {
                        screeching = true;
                        break;
                    }
                }
            }
        }

        // Smoothly scale volume up to 1f when sliding, down to 0f when gripping
        float targetVolume = screeching ? 1f : 0f;
        
        screechAudio.volume = Mathf.MoveTowards(
            screechAudio.volume,
            targetVolume,
            Time.deltaTime * 4f // Slightly faster response speed for instant reaction
        );
    }
}
