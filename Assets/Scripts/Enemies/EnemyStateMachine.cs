using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyController))]
public class EnemyStateMachine : StateMachine<IEnemyState>
{
    protected override IEnemyState InitialState => States[IEnemyState.Label.PATROL];

    public Dictionary<IEnemyState.Label, IEnemyState> States;
    EnemyController m_Controller;

    void Start()
    {
        // Start patrolling
        m_Controller = GetComponent<EnemyController>();
        m_Controller.RestartPatrolling();

        States = new Dictionary<IEnemyState.Label, IEnemyState> {
            { IEnemyState.Label.PATROL, new EnemyPatrolState(m_Controller, this) },
            { IEnemyState.Label.FOLLOW, new EnemyFollowState(m_Controller, this) },
            { IEnemyState.Label.ATTACK, new EnemyAttackState(m_Controller, this) },
        };
        this.SetInitialState();

        var detector = m_Controller.DetectionModule;
        detector.OnDetectedTarget += OnDetectedTarget;
        detector.OnLostTarget += OnLostTarget;
    }

    void Update()
    {
        this.UpdateStateMachine();
        CurrState.OnStay();
    }

    void OnDetectedTarget()
    {
        if (CurrState.StateLabel == IEnemyState.Label.PATROL)
            ChangeState(States[IEnemyState.Label.FOLLOW]);
    }

    void OnLostTarget()
    {
        bool targetWasEngaged = CurrState.StateLabel == IEnemyState.Label.FOLLOW
            || CurrState.StateLabel == IEnemyState.Label.ATTACK;
        if (targetWasEngaged)
            ChangeState(States[IEnemyState.Label.PATROL]);
    }
}

