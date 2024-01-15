public class EnemyFollowState : IEnemyState
{
    EnemyController m_Controller;
    EnemyStateMachine m_Fsm;

    public EnemyFollowState(EnemyController controller, EnemyStateMachine fsm)
    {
        m_Controller = controller;
        m_Fsm = fsm;
    }

    public IEnemyState.Label StateLabel => IEnemyState.Label.FOLLOW;

    public IState CheckTransitions() 
    { 
        // Transition to attack when there is a line of sight to the target
        if (m_Controller.IsSeeingTarget && m_Controller.IsTargetInAttackRange)
            return m_Fsm.States[IEnemyState.Label.ATTACK];
        return this;
    }

    public void OnEnter() =>
        m_Controller.NavMeshAgent.updateRotation = false;

    public void OnExit() =>
        m_Controller.NavMeshAgent.updateRotation = true;

    public void OnStay()
    {
        m_Controller.SetNavDestination(m_Controller.KnownDetectedTarget.transform.position);
        m_Controller.OrientTowards(m_Controller.KnownDetectedTarget.transform.position);
        m_Controller.OrientWeaponTowards(m_Controller.KnownDetectedTarget.transform.position);
    }
}
