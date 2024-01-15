using UnityEngine;

public class OnSteepSphereState : DefaultSphereState
{
    public OnSteepSphereState(SpherePhysics status, SphereStateMachine fsm) 
        : base(status, fsm)
    { }

    public override ISphereState.Label StateLabel => 
	    ISphereState.Label.ON_STEEP;

    public override bool UpdateJumpState(out Vector3 jumpDirection, ref int jumpPhase)
    {
        // If here, wall jumping is being performed
        jumpDirection = status.SteepNormal;
        jumpPhase = 0;
        return true;
    }

    public override bool UpdateGroundedState () {
        // If only one steep contact was detected, sphere is surely free to move
        // by the effect of gravity (hence is not grounded)
        if (status.SteepContactCount <= 1)
            return false;

        // Convert multiple steep contacts into one virtual ground (avoid to get stuck in a crevasse)
        status.SteepNormal.Normalize();
        float upDot = Vector3.Dot(status.UpAxis, status.SteepNormal);
        if (upDot < status.MinGroundDotProduct)
            return false;

        // Consider the sphere grounded on computed virtual ground
        status.SteepContactCount = 0;
        status.GroundContactCount = 1;
        status.ContactNormal = status.SteepNormal;
        return true;
    }
}
