using UnityEngine;

public class RotateUI : MonoBehaviour
{
    // Speed of rotation
    public float rotationSpeed = 50f;

    void FixedUpdate()
    {
        // Rotate the UI element continuously around the z-axis
        transform.Rotate(Vector3.forward * (rotationSpeed * Time.deltaTime));
    }
}
