using UnityEngine;

public class DefaultSphereState : ISphereState
{
    readonly protected SpherePhysics status;
    readonly protected SphereStateMachine fsm;

    public virtual ISphereState.Label StateLabel => 
	    ISphereState.Label.DEFAULT;

    public virtual float MaxAcceleration {
        get => (status.WalkableSurfaceDetected)
	        ? status.MaxAcceleration 
	        : status.MaxAirAcceleration;
    }

    public virtual float MaxSpeed {
        get => (status.WalkableSurfaceDetected && status.WantsToClimb)
	        ? status.MaxClimbSpeed 
	        : status.MaxSpeed;
    }

    public virtual Vector3 xAxis => status.RightAxis;
    public virtual Vector3 zAxis => status.ForwardAxis;

    public DefaultSphereState(SpherePhysics status, SphereStateMachine fsm)
    {
        this.status = status;
        this.fsm = fsm;
    }

    public virtual Vector3 GetGravityEffect() => status.Gravity * Time.fixedDeltaTime;

    public virtual bool UpdateGroundedState() => false;

    public virtual bool UpdateJumpState(out Vector3 jumpDirection, ref int jumpPhase) 
    {
        // Return false when jump is not allowed
        jumpDirection = Vector3.zero;
        return false;
    }

    public virtual void OnEnter() { }
    public virtual void OnStay()  { }
    public virtual void OnExit()  { }
    public virtual IState CheckTransitions() => fsm.GetNextState();
}
