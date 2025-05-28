using UnityEngine;

public class TransformVisualiser : MonoBehaviour
{
    [Tooltip("Length of the drawn axes.")]
    public float axisLength = 1.0f;

    [Tooltip("Size of the sphere marking the position.")]
    public float positionMarkerSize = 0.1f;

    [Tooltip("Color for the position marker.")]
    public Color positionColor = Color.yellow;

    [Tooltip("Color for the Forward (Z) axis.")]
    public Color forwardColor = Color.blue;

    [Tooltip("Color for the Up (Y) axis.")]
    public Color upColor = Color.green;

    [Tooltip("Color for the Right (X) axis.")]
    public Color rightColor = Color.red;


    // If you want the gizmos to be drawn even when the object is NOT selected,
    // uncomment the following method and comment out OnDrawGizmosSelected.
    // Be warned, this can clutter your scene if used on many objects.
    void OnDrawGizmos()
    {
        DrawVisuals();
    }

    private void DrawVisuals()
    {
        Transform t = this.transform;

        // Draw a sphere at the transform's position
        Gizmos.color = positionColor;
        Gizmos.DrawSphere(t.position, positionMarkerSize);

        // Draw lines representing the local axes
        // Forward (Local Z - typically Blue)
        Gizmos.color = forwardColor;
        Gizmos.DrawRay(t.position, t.forward * axisLength);

        // Up (Local Y - typically Green)
        Gizmos.color = upColor;
        Gizmos.DrawRay(t.position, t.up * axisLength);

        // Right (Local X - typically Red)
        Gizmos.color = rightColor;
        Gizmos.DrawRay(t.position, t.right * axisLength * -1);
    }
}
