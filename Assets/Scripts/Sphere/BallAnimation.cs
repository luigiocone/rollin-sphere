using UnityEngine;

public class BallAnimation : MonoBehaviour
{
    [SerializeField]
    Material
        normalMaterial = default,
        climbingMaterial = default;

    [SerializeField, Min(0f)]
    float 
        ballAlignSpeed = 45f, 
        ballAirRotation = 0.5f;   
    
    float ballRadius;
    SpherePhysics status;
    Rigidbody body;
    MeshRenderer meshRenderer;

    void Awake()
    {
        status = GetComponentInParent<SpherePhysics>();
        body = GetComponentInParent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        ballRadius = GetComponentInParent<SphereCollider>().radius;
    }

    void LateUpdate()
    {
        CheckSphereStatus(out Vector3 rotationPlaneNormal, out float rotationFactor);   

        // Get movement from velocity taking into account also the connected body velocity
        Vector3 lastConnectionVelocity = status.LastConnectionVelocity;
        Vector3 movement = (body.velocity - lastConnectionVelocity) * Time.deltaTime;

        // Ignore the relative vertical motion
        movement -= rotationPlaneNormal * Vector3.Dot(movement, rotationPlaneNormal);

        // Get the covered distance by the ball
        float distance = movement.magnitude;
        bool isMoving = distance >= 0.001f;

        Quaternion rotation = transform.localRotation;
        Rigidbody connectedBody = status.ConnectedBody;
        Rigidbody previousConnectedBody = status.PreviousConnectedBody;

        // If sphere is connected to a rotating body, rotate sphere with it 
        if (connectedBody && connectedBody == previousConnectedBody) {
            rotation = Quaternion.Euler(
                connectedBody.angularVelocity * (Mathf.Rad2Deg * Time.deltaTime)
            ) * rotation;

            if (!isMoving) {
                // If ball is standing still apply only the connected body rotation
                transform.localRotation = rotation;
                return;
            }
        }
        else if (!isMoving){
            // Abort if ball is standing still
            return;
        }

        // Compute the rotation angle using the covered distance and the ball radius
        float angle = distance * rotationFactor * (180f / Mathf.PI) / ballRadius;

        Vector3 rotationAxis = Vector3.Cross(rotationPlaneNormal, movement).normalized;
        rotation = Quaternion.Euler(rotationAxis * angle) * rotation;
        if (ballAlignSpeed > 0f) {
            rotation = AlignBallRotation(rotationAxis, rotation, distance);
        }
        transform.localRotation = rotation;
    }

    void CheckSphereStatus(out Vector3 rotationPlaneNormal, out float rotationFactor)
    {
        // Set material and 'out' variables by checking the parent sphere status
        Material ballMaterial = normalMaterial;
        rotationPlaneNormal = status.LastContactNormal;
        rotationFactor = 1f;

        var label = status.StateMachine.CurrState.StateLabel;
        if (label == ISphereState.Label.CLIMBING) {
            ballMaterial = climbingMaterial;
        }
        else if (!status.WalkableSurfaceDetected) {
            if (label == ISphereState.Label.ON_STEEP)
                rotationPlaneNormal = status.LastSteepNormal;
            else 
                rotationFactor = ballAirRotation;
        }
        meshRenderer.material = ballMaterial;
    }

    Quaternion AlignBallRotation(Vector3 rotationAxis, Quaternion rotation, float traveledDistance) {
        // Similar to the gravity alignement update in OrbitCamera
        Vector3 ballAxis = transform.up;
        float dot = Mathf.Clamp(Vector3.Dot(ballAxis, rotationAxis), -1f, 1f);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        float maxAngle = ballAlignSpeed * traveledDistance;

        Quaternion newAlignment = Quaternion.FromToRotation(ballAxis, rotationAxis) * rotation;
        if (angle <= maxAngle) {
            return newAlignment;
        }
        // else
        return Quaternion.SlerpUnclamped(
            rotation, newAlignment, maxAngle / angle
        );
    }
}
