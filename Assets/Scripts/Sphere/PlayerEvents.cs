using UnityEngine;
using UnityEngine.Events;

public class PlayerEvents : MonoBehaviour
{
    [SerializeField, Tooltip("Player die if its transform.y is lower than this value")]
    float SelfDestructionY = -20f;

    public UnityAction onDie;
    public UnityAction onDamaged;

    HealthManager m_HealthManager;

    void Start()
    {
        var stats = GetComponentInChildren<StatsCollectionManager>();
        m_HealthManager = stats.GetStatManager<HealthManager>();
        m_HealthManager.DamageManager.OnDamage += OnDamaged;
        m_HealthManager.Health.OnZero += (StatValueChangeEvent evt) => OnDie();
    }

    void Update()
    {
        if (this.transform.position.y < SelfDestructionY)
            m_HealthManager.DamageManager.Kill();
    }

    void OnDamaged(Damage d) => 
	    onDamaged?.Invoke();

    void OnDie()
    {
        onDie?.Invoke();
        EventManager.Broadcast(Events.PlayerDeathEvent);
    }
}

