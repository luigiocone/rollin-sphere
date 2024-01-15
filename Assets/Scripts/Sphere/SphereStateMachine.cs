using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpherePhysics))]
public class SphereStateMachine : StateMachine<ISphereState>
{
    protected override ISphereState InitialState => m_States[ISphereState.Label.DEFAULT];

    Dictionary<ISphereState.Label, ISphereState> m_States;
    SpherePhysics m_Status;

    void Awake()
    {
        m_Status = GetComponent<SpherePhysics>();
        m_States = new Dictionary<ISphereState.Label, ISphereState> {
            { ISphereState.Label.DEFAULT,  new DefaultSphereState  (m_Status, this) },
            { ISphereState.Label.CLIMBING, new ClimbingSphereState (m_Status, this) },
            { ISphereState.Label.GROUNDED, new GroundedSphereState (m_Status, this) },
            { ISphereState.Label.ON_STEEP, new OnSteepSphereState  (m_Status, this) },
            { ISphereState.Label.IN_AIR,   new InAirSphereState    (m_Status, this) }
        };
        this.SetInitialState();
    }

    public override ISphereState GetNextState()
    {
        // Checks are ordered because states are not mutually exclusive
        // E.g. a climbing state is also a grounded state (wall as a walkable surface)
        if (m_Status.IsClimbing && m_Status.Stamina.CurrValue > 0f)
            return m_States[ISphereState.Label.CLIMBING];

        if (m_Status.WalkableSurfaceDetected)
            return m_States[ISphereState.Label.GROUNDED];

        if (m_Status.SteepDetected)
            return m_States[ISphereState.Label.ON_STEEP];

        return m_States[ISphereState.Label.IN_AIR];
    }
}

