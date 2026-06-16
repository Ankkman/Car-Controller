using UnityEngine;
using System.Collections.Generic;

public class TireSmokeController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public List<WheelCollider> wheels;

    [Header("Smoke Particles")]
    public List<ParticleSystem> smokeParticles;

    [Header("Settings")]
    public float slipThreshold = 0.5f;
    public float speedThreshold = 20f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (rb == null) return;

        float speedKmh = rb.linearVelocity.magnitude * 3.6f;

        for (int i = 0; i < wheels.Count; i++)
        {
            bool smokeActive = false;

            WheelHit hit;

            if (
                speedKmh > speedThreshold &&
                wheels[i].GetGroundHit(out hit)
            )
            {
                float slip =
                    Mathf.Max(
                        Mathf.Abs(hit.forwardSlip),
                        Mathf.Abs(hit.sidewaysSlip)
                    );

                if (slip > slipThreshold)
                {
                    Debug.Log("SMOKE TRIGGERED");
                    smokeActive = true;
                }
            }

            if (i < smokeParticles.Count)
            {
                ParticleSystem.EmissionModule emission =
                    smokeParticles[i].emission;

                emission.enabled = smokeActive;
            }
        }
    }
}