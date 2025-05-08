using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform opponent;
    [SerializeField] private float minDistance = 8f;    // Closest zoom
    [SerializeField] private float maxDistance = 15f;   // Farthest zoom
    [SerializeField] private float baseHeight = 5f;       // Default height
    [SerializeField] private float smoothTime = 0.3f;     // Time for smoothing position
    [SerializeField] private float rotationSmoothSpeed = 5f; // Slerp speed for rotation
    [SerializeField] private float heightMultiplier = 1.5f;  // Extra height for jumps

    // Used for SmoothDamp
    private Vector3 currentVelocity = Vector3.zero;

    void LateUpdate()
    {
        if (player == null || opponent == null) return;

        Vector3 midpoint = (player.position + opponent.position) / 2f;
        Vector3 delta = player.position - opponent.position;

        float horizontalDistance = new Vector3(delta.x, 0, delta.z).magnitude;
        float verticalDistance = Mathf.Abs(delta.y);

        // Smooth zoom logic
        float zoomFactor = Mathf.InverseLerp(0f, 20f, horizontalDistance);
        float targetDistance = Mathf.Lerp(minDistance, maxDistance, zoomFactor);

        // Smooth height logic
        float averageY = (player.position.y + opponent.position.y) / 2f;
        float targetHeight = Mathf.Max(averageY + verticalDistance * heightMultiplier, baseHeight);
        midpoint.y = targetHeight;

        Vector3 desiredPosition = midpoint - transform.forward * targetDistance;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);

        Quaternion desiredRotation = Quaternion.LookRotation(midpoint - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
    }


    public void SetTargets(Transform p1, Transform p2)
    {
        player = p1;
        opponent = p2;
    }
}
