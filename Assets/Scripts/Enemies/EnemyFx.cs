using UnityEngine;
using UnityEngine.AI;

public class EnemyFx : AgentFx 
{
    [Header("Detection FX")]
    [SerializeField] AudioSource OnDetectSfx;
    [SerializeField] ParticleSystem[] OnDetectVfx;

    EnemyController m_EnemyController;
    NavMeshAgent m_NavMeshAgent;

    public override float MaxAgentSpeed => 
	    m_NavMeshAgent ? m_NavMeshAgent.speed : 0f;
    public override float CurrAgentSpeed =>
	    m_NavMeshAgent ? m_NavMeshAgent.velocity.magnitude : 0f;

    protected override void Start()
    {
        m_EnemyController = GetComponentInParent<EnemyController>();
        if(!m_EnemyController)
        {
            this.enabled = false;
            return;
	    }

        m_NavMeshAgent = m_EnemyController.NavMeshAgent;
        base.Start();
    }

    protected override void RegisterToEvents()
    {
        m_EnemyController.onDamage += OnDamaged;
        m_EnemyController.onDie += OnDie;

        var detector = m_EnemyController.DetectionModule;
        if (detector)
        {
            detector.OnDetectedTarget += OnDetectedTarget;
            detector.OnLostTarget += OnLostTarget;
	    }
    }

    void OnDetectedTarget()
    {
        for (int i = 0; i < OnDetectVfx.Length; i++)
            OnDetectVfx[i].Play();

        if (OnDetectSfx)
            OnDetectSfx.Play();
    }

    void OnLostTarget()
    {
        for (int i = 0; i < OnDetectVfx.Length; i++)
            OnDetectVfx[i].Stop();
    }
}
