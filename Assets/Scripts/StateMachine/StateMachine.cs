using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine<T> : MonoBehaviour where T : IState
{
    protected abstract T InitialState { get; }
    public T CurrState { get; protected set; }

    protected void SetInitialState() => CurrState = InitialState;

    public virtual T GetNextState() => (T) CurrState.CheckTransitions();

    public T UpdateStateMachine()
    {
        // Change only if curr and next states are different
        T next = GetNextState();
        bool equals = EqualityComparer<T>.Default.Equals(next, CurrState);
        if (!equals)
            ChangeState(next);
        return CurrState;
    }

    public void ChangeState(T next)
    {
        CurrState.OnExit();
        CurrState = next;
        CurrState.OnEnter();
    }
}

