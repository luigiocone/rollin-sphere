using UnityEngine;

public class GravitySource : MonoBehaviour {

    public virtual Vector3 GetGravity (Vector3 position) {
        return Physics.gravity;
    }

    void OnEnable () {
        // Called when object is created, activated, or enabled
        CustomGravity.Register(this);
    }

    void OnDisable () {
        // Called when object is destroyed, deactivated, or disabled
        CustomGravity.Unregister(this);
    }
}
