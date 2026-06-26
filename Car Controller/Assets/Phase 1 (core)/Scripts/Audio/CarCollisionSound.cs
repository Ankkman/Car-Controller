using UnityEngine;

public class CarCollisionSound : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource collisionAudio;

    [Header("Settings")]
    public float minCollisionForce = 2f; // Minimum impact speed to trigger sound
    public float volumeMultiplier = 0.1f; // Adjusts how loud it gets based on impact speed

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionAudio == null) return;

        // Calculate how hard the truck hit the object
        float impactForce = collision.relativeVelocity.magnitude;

        // Only play if the hit was hard enough (prevents tiny scrapes from blasting sound)
        if (impactForce > minCollisionForce)
        {
            // Dynamically scale volume: harder hits = louder crashes!
            float calculatedVolume = Mathf.Clamp01(impactForce * volumeMultiplier);
            
            // Play the sound once safely over any current audio buffer
            collisionAudio.PlayOneShot(collisionAudio.clip, calculatedVolume);
        }
    }
}
