using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Transform rotationalCenter;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 5, -10);

    void LateUpdate()
    {
        // Get the vector between target and rotational center
        Vector3 directionVector = target.position - rotationalCenter.position;

        // Calculate desired position by adding the offset to this vector
        Vector3 desiredPosition = target.position + Quaternion.LookRotation(directionVector) * offset;

        // Smooth the camera movement
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // Optional: Make the camera look at the target
        transform.LookAt(target);
    }
}
