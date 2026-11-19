using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Targets")]
    public Transform carTarget; // Drag your car here

    [Header("Offsets")]
    public Vector3 moveOffset = new Vector3(0f, 3f, -6f); // Height and distance behind car

    [Header("Smoothness")]
    public float moveSmoothness = 5f;
    public float rotSmoothness = 5f;

    void LateUpdate()
    {
        if (carTarget == null) return;

        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        // TransformPoint converts local offset into world space relative to the car's orientation
        Vector3 targetPos = carTarget.TransformPoint(moveOffset);
        
        // Linearly interpolate position
        transform.position = Vector3.Lerp(transform.position, targetPos, moveSmoothness * Time.deltaTime);
    }

    void HandleRotation()
    {
        // Look at the car while smoothly interpolating rotation angles
        Vector3 direction = carTarget.position - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotSmoothness * Time.deltaTime);
    }
}
