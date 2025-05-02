using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    private Rigidbody rb;
    private Camera mainCamera;

    private bool isRolling = false;
    private float stableThreshold = 0.1f; // Velocity threshold to consider "stopped"
    private float stableTime = 0.5f; // Time dice must be stable
    private float timer = 0f;

    [SerializeField] private Transform[] faceCheckers; // Reference to face center points
    [SerializeField] private float minForce = 2f;
    [SerializeField] private float maxForce = 5f;
    [SerializeField] private float torqueMultiplier = 0.5f;

    public System.Action<int> OnDiceRollComplete; void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void Update()
    {

        if (rb.velocity.magnitude < stableThreshold && rb.angularVelocity.magnitude < stableThreshold)
        {
            timer += Time.deltaTime;
            if (timer > stableTime && isRolling)
            {
                isRolling = false;
                DetermineUpFace();
            }
        }
        else
        {
            timer = 0f;
            isRolling = true;
        }
    }

    void DetermineUpFace()
    {
        int upFace = 0;
        float highestDot = -1f;

        for (int i = 0; i < faceCheckers.Length; i++)
        {
            Debug.Log(faceCheckers[i]);
            Vector3 faceNormal = faceCheckers[i].up;
            float dot = Vector3.Dot(faceNormal, Vector3.up);
            if (dot > highestDot)
            {
                highestDot = dot;
                upFace = i + 1; // Assuming faces are 1-6
            }
        }

        Debug.Log("Dice shows: " + upFace);
        OnDiceRollComplete?.Invoke(upFace);
    }

    public void PhysicalRoll()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Random force in random direction
        float forceMagnitude = Random.Range(minForce, maxForce);
        Vector3 forceDirection = new Vector3(
            Random.Range(-0.1f, 0.1f),
            Random.Range(2f, 4f), // Slight upward bias
            Random.Range(-0.1f, 0.1f)
        ).normalized;

        rb.AddForce(forceDirection * forceMagnitude, ForceMode.Impulse);

        // Add random torque for rotation
        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized * forceMagnitude * torqueMultiplier;

        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }
}