using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidbody : MonoBehaviour {

    [SerializeField, Tooltip(
        "If gravity directions will never change, this can be unchecked (more efficient). " + 
        "If this is unchecked and there is a change in gravity directions while the body is " +
        "in equilibrium, the body will not move"
    )]
    bool alwaysApplyGravity = true;

    // Stores how much time the body can float at a very low speeds before put to rest
    float floatDelay;

    Rigidbody body;

    Vector3 gravity;

    void Awake () {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }

    void FixedUpdate () {
        if (!alwaysApplyGravity && IsBodyInEquilibrium()) {
            // Avoid to apply gravity physics at each fixed update for efficiency
            return;
        }

        gravity = CustomGravity.GetGravity(body.position);
        body.AddForce(gravity, ForceMode.Acceleration);
    }

    bool IsBodyInEquilibrium () {
        // PhysX puts bodies to sleep when it can, meaning it's in equilibrium and not floating
        if (body.IsSleeping()) {
            floatDelay = 0f;
            return true;
        }

        // PhysX body can end up making tiny adjustments and never sleep again
        // If body velocity is very low, maybe it has come to rest (virtually sleeping)
        if (body.velocity.sqrMagnitude < 0.0001f) {
            floatDelay += Time.deltaTime;
            if (floatDelay >= 1f) {
                // If the body doesn't really move in a second, then it should've come to rest
                return true;
            }
        }
        
        floatDelay = 0f;
        return false;
    }
}