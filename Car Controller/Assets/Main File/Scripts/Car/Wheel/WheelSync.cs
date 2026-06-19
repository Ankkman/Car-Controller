using UnityEngine;

public class WheelSync : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform visualWheel;   // the mesh or an empty that contains the mesh

    void Update()
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        visualWheel.position = pos;
        visualWheel.rotation = rot;
    }
}