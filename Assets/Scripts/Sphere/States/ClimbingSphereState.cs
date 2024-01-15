using UnityEngine;

public class ClimbingSphereState : DefaultSphereState
{
    Modifier m_StaminaModifierTemplate = new();
    Modifier m_StaminaModifierCopy;

    public ClimbingSphereState(SpherePhysics status, SphereStateMachine fsm) 
        : base(status, fsm)
    {
        m_StaminaModifierTemplate.StatId = StatId.STAMINA;
        m_StaminaModifierTemplate.Type = Modifier.ModifierType.ADDER;
        m_StaminaModifierTemplate.Amount = -10f;
        m_StaminaModifierTemplate.Duration = float.PositiveInfinity;
        m_StaminaModifierTemplate.ApplyPeriod = 0.1f;
    }

    public override ISphereState.Label StateLabel => 
	    ISphereState.Label.CLIMBING;

    public override float MaxAcceleration 
    {
        get => status.MaxClimbAcceleration;
    }

    public override float MaxSpeed
    {
        get => status.MaxClimbSpeed; 
    }

    // Ignore camera's orientation and make movement relative to 
    // the wall orientation and gravity directions
    public override Vector3 xAxis 
    {
        get => Vector3.Cross(status.ContactNormal, status.UpAxis);
    }

    public override Vector3 zAxis => status.UpAxis;

    public override Vector3 GetGravityEffect()
    {
        // Instead of gravity, a "climber grip" is applied
	    float magnitude = status.MaxClimbAcceleration * status.GripStrengthFactor 
	        * Time.fixedDeltaTime;
        Vector3 climberGrip = -status.ContactNormal * magnitude;
        return climberGrip;
    }

    public override bool UpdateJumpState(out Vector3 jumpDirection, ref int jumpPhase)
    {
        jumpDirection = status.ContactNormal;
        return true;
    }

    public override bool UpdateGroundedState()
    {
        if (!status.IsClimbing)
            return false;

        // If there are multiple climbing contacts, sphere could be in a crevasse
        if (status.ClimbContactCount > 1) {
            status.ClimbNormal.Normalize();
            float upDot = Vector3.Dot(status.UpAxis, status.ClimbNormal);

            if (upDot >= status.MinGroundDotProduct) {
                // If here, climb normal counts as ground, meaning a crevasse situation
                // Don't use the aggregate climb normal, use the last evaluated  
                status.ClimbNormal = status.LastClimbNormal;
            }
        }

        status.GroundContactCount = 1;
        status.ContactNormal = status.ClimbNormal;
        return true;
    }

    public override void OnEnter() =>
        m_StaminaModifierCopy = status.Stamina.CopyModifier(m_StaminaModifierTemplate);

    public override void OnExit() =>
        status.Stamina.RemoveModifier(m_StaminaModifierCopy);
}

