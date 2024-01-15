using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    EnemyController m_Controller;
    EnemyStateMachine m_Fsm;

    public EnemyAttackState(EnemyController controller, EnemyStateMachine fsm)
    {
        m_Controller = controller;
        m_Fsm = fsm;
    }

    public IEnemyState.Label StateLabel => IEnemyState.Label.ATTACK;

    public IState CheckTransitions()
    {
        // Transition to follow when no longer a target in attack range
        if (!m_Controller.IsSeeingTarget || !m_Controller.IsTargetInAttackRange)
            return m_Fsm.States[IEnemyState.Label.FOLLOW];
        return this;
    }

    public void OnEnter() { }
    public void OnExit() { }
    public void OnStay() => UpdateAttackState();

    public void UpdateAttackState()
    {
        var detector = m_Controller.DetectionModule;
        Vector3 targetPos = detector.KnownDetectedTarget.transform.position;
        Vector3 enemyPos = detector.DetectionSourcePoint.position;
        float distance = Vector3.Distance(targetPos, enemyPos);

        Vector3 destination;
        if (distance <= detector.FleeRange)
            destination = m_Controller.GetFleeDestination();
        else if (distance >= detector.AttackRange)
            destination = detector.KnownDetectedTarget.transform.position;
        else
            destination = m_Controller.transform.position;

        m_Controller.SetNavDestination(destination);
        m_Controller.OrientTowards(detector.KnownDetectedTarget.transform.position);
        m_Controller.TryAttack(detector.KnownDetectedTarget.transform.position);
    }
}
