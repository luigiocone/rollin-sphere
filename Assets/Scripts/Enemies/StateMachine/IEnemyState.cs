using UnityEngine;

public interface IEnemyState : IState
{
    public enum Label {PATROL, ATTACK, FOLLOW };
    public Label StateLabel { get; }
}
