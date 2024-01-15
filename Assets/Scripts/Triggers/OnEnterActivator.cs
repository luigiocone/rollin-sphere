using UnityEngine;
using UnityEngine.Events;

public class OnEnterActivator : MonoBehaviour
{
    [SerializeField] UnityEvent OnEnter = default;

    ITrigger[] m_Triggers;

    void Awake()
    {
        m_Triggers = GetComponents<ITrigger>();
    }

    void OnTriggerEnter(Collider other) {
        foreach (var t in m_Triggers)
            t.Trigger(other);
        OnEnter?.Invoke();
    }
}

