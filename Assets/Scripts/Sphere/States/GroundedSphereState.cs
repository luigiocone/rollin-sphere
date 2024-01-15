using UnityEngine;

public class GroundedSphereState : DefaultSphereState
{
    public GroundedSphereState(SpherePhysics status, SphereStateMachine fsm) 
        : base(status, fsm)
    { }

    public override ISphereState.Label StateLabel => 
	    ISphereState.Label.GROUNDED;

    public override Vector3 GetGravityEffect()
    {
        // Standing still on a cliff (e.g. a wall). Delete the gravity component that causes 
        // the sphere to slides down the wall, but still pull the sphere to the surface
        if (status.Velocity.sqrMagnitude < 0.01f) 
	    {
            float magnitude = Vector3.Dot(status.Gravity, status.ContactNormal) 
		        * Time.fixedDeltaTime;
            return status.ContactNormal * magnitude;
        }
        
        // Climb input slows down input
        if (status.WantsToClimb) {
            float acceleration = status.MaxClimbAcceleration * status.GripStrengthFactor;
            Vector3 direction = status.ContactNormal * acceleration;
            return (status.Gravity - direction) * Time.fixedDeltaTime;
        }

        return base.GetGravityEffect();
    }

    public override bool UpdateJumpState(out Vector3 jumpDirection, ref int jumpPhase)
    {
        jumpDirection = status.ContactNormal;
        return true;
    }

    public override bool UpdateGroundedState() => true;
}
