public class EnemyPatrolState : IEnemyState
{
    EnemyController m_Controller;
    EnemyStateMachine m_Fsm;

    public EnemyPatrolState(EnemyController controller, EnemyStateMachine fsm)
    {
        m_Controller = controller;
        m_Fsm = fsm;
    }

    public IEnemyState.Label StateLabel => IEnemyState.Label.PATROL;

    // State will be changed by state machine once target is detected
    public IState CheckTransitions() => this;

    public void OnEnter() { }
    public void OnExit() { }
    public void OnStay() =>         
        m_Controller.SetNavDestination(m_Controller.GetPatrolDestination());
}

