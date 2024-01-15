using UnityEngine;

public class GravitySphere : GravitySource {

    [SerializeField]
    float gravity = 9.81f;

    [SerializeField, Min(0f), Tooltip(
        "Distance up to which gravity is applied at full constant strength. " + 
        "Visible as the yellow gizmo."
    )]
    float outerRadius = 10f;

    [SerializeField, Min(0f), Tooltip(
        "Should be '>= outerRadius'. Distance up to which gravity will be linearly reduced " +
        "depending on distance from sphere. Visible as the cyan gizmo."
    )]
    float outerFalloffRadius = 15f;

    float outerFalloffFactor;

    public override Vector3 GetGravity (Vector3 position) {
        Vector3 vector = transform.position - position;
        float distance = vector.magnitude;
        if (distance > outerFalloffRadius) {
            return Vector3.zero;
        }
        
        // Dividing now by distance to avoid 'vector.normalized' later 
        float g = gravity / distance;
        if (distance > outerRadius) {
            g *= 1f - (distance - outerRadius) * outerFalloffFactor;
        }

        // If previous condition were not satisfied, gravity will be applied at full strength
        return g * vector;
    }

    void Awake () {
        OnValidate();
    }

    void OnValidate () {
        outerFalloffRadius = Mathf.Max(outerFalloffRadius, outerRadius);
        outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);
    }

    void OnDrawGizmos () {
        Vector3 p = transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(p, outerRadius);
        if (outerFalloffRadius > outerRadius) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, outerFalloffRadius);
        }
    }
}