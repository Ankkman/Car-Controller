using UnityEngine;
using System.Collections.Generic;

public class TireSkidMarkController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    public List<WheelCollider> wheels;

    [Header("Trail Renderers")]
    public List<TrailRenderer> skidTrails;

    [Header("Settings")]
    public float slipThreshold = 0.7f;
    public float speedThreshold = 20f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        foreach (var trail in skidTrails)
        {
            trail.emitting = false;
        }
    }

    void Update()
    {
        float speedKmh = rb.linearVelocity.magnitude * 3.6f;

        for (int i = 0; i < wheels.Count; i++)
        {
            bool shouldEmit = false;

            if (speedKmh > speedThreshold)
            {
                WheelHit hit;

                if (wheels[i].GetGroundHit(out hit))
                {
                    float slip =
                        Mathf.Max(
                            Mathf.Abs(hit.forwardSlip),
                            Mathf.Abs(hit.sidewaysSlip)
                        );

                    if (slip > slipThreshold)
                    {
                        shouldEmit = true;
                    }
                }
            }

            if (i < skidTrails.Count)
            {
                skidTrails[i].emitting = shouldEmit;
            }
        }
    }
}