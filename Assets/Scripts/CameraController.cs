using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Transform rotationalCenter;
    public float smoothSpeed = 0.125f;
    public Vector3 offset = new Vector3(0, 5, -10);

    // for scroll wheel zoom
    public float zoomSpeed = 100f;
    public float maxZoomDistance = 7f; // How far out the camera can zoom (more negative Z)
    public float minZoomDistance = 1f;  // How close the camera can zoom (less negative Z)

    private void Awake()
    {
        if (target == null)
        {
            if (Player.Instance != null)
            {
                target = Player.Instance.transform;
            }
            else
            {
                Debug.LogError("CameraController: Player.Instance is null and no target was assigned.");
            }
        }

        if (rotationalCenter == null)
        {
            Debug.LogWarning("CameraController: RotationalCenter is not assigned.");
        }
    }

    void Update()
    {

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0f)
        {

            offset.z += (scrollInput*-1f) * zoomSpeed * Time.deltaTime;

            offset.z = Mathf.Clamp(offset.z,minZoomDistance, maxZoomDistance);
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Transform currentRotationalCenter = rotationalCenter != null ? rotationalCenter : target;

        Vector3 directionFromCenterToTarget = target.position - currentRotationalCenter.position;

        Vector3 lookDirection;
        if (directionFromCenterToTarget.sqrMagnitude < 0.0001f)
        {
            lookDirection = target.forward;

            if (lookDirection.sqrMagnitude < 0.0001f)
            {
                lookDirection = Vector3.forward; // Default to world forward
            }
        }
        else
        {
            lookDirection = directionFromCenterToTarget.normalized;
        }


        Vector3 desiredPosition = target.position + Quaternion.LookRotation(lookDirection) * offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}
