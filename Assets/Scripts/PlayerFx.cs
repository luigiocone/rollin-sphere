using UnityEngine;

public class PlayerFx : AgentFx 
{
    [Header("Jump FX")]
    [SerializeField] AudioSource JumpSfx;

    [Header("Dash FX")]
    [SerializeField] ParticleSystem DashVfx;
    [SerializeField] AudioSource DashSfx;

    [Header("Shield SFX")]
    [SerializeField] AudioSource ReflectionSfx;
    [SerializeField] AudioSource CounterAttackSfx;

    SpherePhysics m_SphereStatus;
    GameObject m_Sphere;
    PlayerEvents m_Events;
    TrailRenderer m_Trail;

    public override float MaxAgentSpeed => m_SphereStatus.MaxSpeed;
    public override float CurrAgentSpeed
    {
        get
        {
            if (m_SphereStatus.WalkableSurfaceDetected || m_SphereStatus.IsClimbing)
                return m_SphereStatus.Velocity.magnitude;
            return 0f;
        }
    }

    protected override void Start()
    {
        m_Events = GetComponentInParent<PlayerEvents>();
        m_Trail = GetComponent<TrailRenderer>();
        base.Start();

        m_SphereStatus = GetComponentInParent<SpherePhysics>();
        if (!m_SphereStatus)
            return;

	    m_SphereStatus.OnJump += OnJump;

        var dasher = m_SphereStatus.gameObject.GetComponentInChildren<Dasher>();
        if (dasher && DashVfx && DashSfx)
        {
            dasher.OnDash += OnDash;
            DashVfx.Stop();
            DashSfx.enabled = true;
        }

        var shield = m_SphereStatus.gameObject.GetComponentInChildren<SphereShield>();
        if (shield && ReflectionSfx && CounterAttackSfx)
        {
            shield.OnReflection += OnReflection;
            shield.OnCounterAttack += OnCounterAttack;
        }

        m_Sphere = m_SphereStatus.gameObject;
    }

    protected override void Update()
    {
        // TODO GameSettings - OnChange event?
        base.Update();
        m_Trail.enabled = GameSettings.SphereTrail;
    }

    protected override void RegisterToEvents()
    {
        m_Events.onDamaged += OnDamaged;
        m_Events.onDie += OnDie;
    }

    protected override void OnDie()
	{
        base.OnDie();
        m_Sphere.SetActive(false);
    }

    void OnJump() => JumpSfx.Play();

    void OnCounterAttack() => CounterAttackSfx.Play();

    void OnReflection() => ReflectionSfx.Play();

    void OnDash(bool started)
    {
        if (started)
        {
            DashVfx.Play();
            DashSfx.Play();
        }
        else 
	        DashVfx.Stop();
    }
}
