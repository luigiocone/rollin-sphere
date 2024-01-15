public interface IState
{
    public void OnEnter();
    public void OnStay();
    public void OnExit();

    public IState CheckTransitions();
}

