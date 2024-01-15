using UnityEngine;

// This class is an Observer of the AutomaticSlider Subject class
// Interpolate is bound to the slider's event 
public class PositionInterpolator : MonoBehaviour {

    [SerializeField]
    Rigidbody body = default;

    [SerializeField, Tooltip("Starting and ending points of interpolation")]
    Vector3 from = default, to = default;

    [SerializeField, Tooltip("'from' and 'to' vectors are relative to this Transform")]
    Transform relativeTo = default;

    public void Interpolate (float t) {
        Vector3 p;
        if (relativeTo) {
            p = Vector3.LerpUnclamped(
                relativeTo.TransformPoint(from), relativeTo.TransformPoint(to), t
            );
        }
        else {
            p = Vector3.LerpUnclamped(from, to, t);
        }
        body.MovePosition(p);
    }
}
