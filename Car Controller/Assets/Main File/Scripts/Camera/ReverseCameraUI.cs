using UnityEngine;

public class ReverseCameraUI : MonoBehaviour
{
    public GameObject reverseUI;

    public Rigidbody rb;

    void Update()
    {
        float speed =
            Vector3.Dot(
                rb.transform.forward,
                rb.linearVelocity
            );

        reverseUI.SetActive(speed < -0.5f);
    }
}