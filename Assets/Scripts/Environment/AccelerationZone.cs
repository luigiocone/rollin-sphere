using UnityEngine;

public class AccelerationZone : MonoBehaviour {

    [SerializeField, Min(0f), Tooltip("If it's set to zero then speed change will be instantenous")]
    float acceleration = 10f; 

    [SerializeField, Min(0f), Tooltip("Target speed")]
    float speed = 10f;

    void OnTriggerEnter (Collider other) {
        Rigidbody body = other.attachedRigidbody;
        if (body) {
            Accelerate(body);
        }
    }

    void OnTriggerStay (Collider other) {
        Rigidbody body = other.attachedRigidbody;
        if (body) {
            Accelerate(body);
        }
    }

    void Accelerate(Rigidbody body) {
        // Convert body velocity to this zone local space
        // It makes possible to accelerate in any direction depending on the zone rotation
        Vector3 velocity = transform.InverseTransformDirection(body.velocity);
        if (velocity.y >= speed) {
            return;
        }

        if (acceleration > 0f) {
            velocity.y = Mathf.MoveTowards(
                velocity.y, speed, acceleration * Time.deltaTime
            );
        }
        else {
            velocity.y = speed;
        }

        // Convert velocity back to world space
        body.velocity = transform.TransformDirection(velocity);
        if (body.TryGetComponent(out SpherePhysics sphere)) {
            sphere.PreventSnapToGround();
        }
    }
}
