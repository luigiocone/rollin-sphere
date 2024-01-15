using UnityEngine;

public interface ISphereState : IState
{
    public enum Label { DEFAULT, CLIMBING, GROUNDED, ON_STEEP, IN_AIR };
    public Label StateLabel { get; }

    public float MaxAcceleration { get; }
    public float MaxSpeed { get; }
    public Vector3 xAxis { get; }
    public Vector3 zAxis { get; }

    public Vector3 GetGravityEffect();
    public bool UpdateGroundedState();
    public bool UpdateJumpState(out Vector3 jumpDirection, ref int jumpPhase);
}

