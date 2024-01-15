using UnityEngine;

public class InAirSphereState : DefaultSphereState
{
    public InAirSphereState(SpherePhysics status, SphereStateMachine fsm) 
        : base(status, fsm)
    { }

    public override ISphereState.Label StateLabel => 
	    ISphereState.Label.IN_AIR;

    public override float MaxAcceleration 
    {
        get => status.MaxAirAcceleration;
    }

    public override bool UpdateJumpState(out Vector3 jumpDirection, ref int jumpPhase)
    {
        if (status.MaxAirJumps > 0 && jumpPhase <= status.MaxAirJumps) {
            // If sphere is falling off a surface and air jump is allowed, consider one jump as already made
            if (jumpPhase == 0) jumpPhase = 1;

            jumpDirection = status.ContactNormal;
            return true;
        }
        return base.UpdateJumpState(out jumpDirection, ref jumpPhase);
    }

    public override bool UpdateGroundedState()
    {
        // In this state sphere is not grounded, but keep the sphere stuck to the ground if needed
        if(SnapToGround())
            return true;
 
        // Here if the sphere is not touching the ground
        // Use the upAxis as contactNormal, eventual air jumps still go straight up
        status.ContactNormal = status.UpAxis;
        return false;
    }

    bool SnapToGround () {
        // Snap only if sphere jumped a long ago or if previously was grounded
        if (status.StepsSinceLastGrounded > 1 || status.StepsSinceLastJump <= 2)
            return false;

        // Don't snap at high speeds
        Vector3 velocity = status.Velocity;
        float speed = velocity.magnitude;
        if (speed > status.MaxSnapSpeed) 
            return false;
        
        // Check if there's ground below the sphere
        Vector3 upAxis = status.UpAxis;
        bool groundBelowSphere = Physics.Raycast(
            status.Body.position, -upAxis, out RaycastHit hit,
            status.ProbeDistance, status.WhatIsGround, QueryTriggerInteraction.Ignore
            );

        if (!groundBelowSphere)
            return false;

        // Check if the surface hit by the raycast counts as ground
        float upDot = Vector3.Dot(upAxis, hit.normal);
        bool isSteep = upDot < status.GetMinDot(hit.collider.gameObject.layer);
        if (isSteep)
            return false;

        // If here, the sphere must be snapped to the ground
        status.GroundContactCount = 1;
        status.ContactNormal = hit.normal;

        // Adjust velocity to align with the ground
        // If dot <= 0 don't adjust velocity because it already points down (e.g. because of gravity) 
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f) {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        status.Velocity = velocity;
        status.ConnectedBody = hit.rigidbody;

        // If this method succeds, then the sphere is considered grounded (return true)
        return true;
    }
}


