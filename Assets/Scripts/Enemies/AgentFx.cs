using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AgentFx : MonoBehaviour
{
    [Header("Flash on hit")]
    [SerializeField, Tooltip("The material used for the body of the hoverbot")]
    protected Renderer[] BodyRenderers;

    [SerializeField]
    [GradientUsageAttribute(true)]
    protected Gradient OnDamageBodyGradient;

    [SerializeField, Range(0f, 1f), Tooltip("The duration of the flash on hit")]
    protected float FlashOnHitDuration = 0.2f;

    [Header("SFX")]
    [SerializeField] protected AudioSource OnMovementSfx;
    [SerializeField] protected AudioSource OnDamageSfx;
    [SerializeField] protected GameObject OnDeathSfx;

    [Header("VFX")]
    [SerializeField] protected ParticleSystem[] OnHitVfx;
    [SerializeField] protected GameObject OnDeathVfx;

    protected List<Renderer> m_BodyRenderers;
    protected MaterialPropertyBlock m_BodyFlashProperties;
    protected bool m_BodyPropertiesRestored = false;
    protected float m_LastTimeDamaged = float.NegativeInfinity;
    protected bool m_WasDamagedThisFrame;
    protected bool m_InCoroutine = false;

    public abstract float MaxAgentSpeed { get; }
    public abstract float CurrAgentSpeed { get; }

    protected virtual void Start()
    {
        // Data structures for the flash on hit
        m_BodyRenderers = new List<Renderer>(BodyRenderers);
        m_BodyFlashProperties = new MaterialPropertyBlock();

        if (OnMovementSfx)
            OnMovementSfx.Play();

        RegisterToEvents();
    }

    protected abstract void RegisterToEvents();

    protected virtual void Update()
    {
        CheckBodyFlash();
        m_WasDamagedThisFrame = false;
        if (OnMovementSfx)
            ModifyMovementSfxPitch(CurrAgentSpeed, MaxAgentSpeed);
    }

    void CheckBodyFlash()
    {
        // Make body flash when hit (materials' emission option should be checked)
        float diff = Time.time - m_LastTimeDamaged;
        if (diff > FlashOnHitDuration + 1f)
        {
            if (!m_BodyPropertiesRestored)
            {
                foreach (var br in m_BodyRenderers)
                    br.SetPropertyBlock(null);
                m_BodyPropertiesRestored = true;
            }
            return;
        }

        float percentage = diff / FlashOnHitDuration;
        Color currentColor = OnDamageBodyGradient.Evaluate(percentage);
        m_BodyFlashProperties.SetColor("_BaseColor", currentColor);

        m_BodyPropertiesRestored = false;
        foreach (var br in m_BodyRenderers)
            br.SetPropertyBlock(m_BodyFlashProperties);
    }

    protected void ModifyMovementSfxPitch(float currSpeed, float maxSpeed)
    {
        // Changing the pitch of the movement sound depending on the movement speed
        const float min = 0f;
        const float max = 1f;
        float interpolator = currSpeed / maxSpeed;
        OnMovementSfx.pitch = Mathf.Lerp(min, max, interpolator);
    }

    protected virtual void OnDamaged()
    {
        if (OnDamageSfx && !m_WasDamagedThisFrame)
            OnDamageSfx.Play();

        m_LastTimeDamaged = Time.time;
        m_WasDamagedThisFrame = true;

        if (OnHitVfx.Length > 0)
        {
            int n = Random.Range(0, OnHitVfx.Length - 1);
            OnHitVfx[n].Play();
        }
    }

    protected virtual void OnDie()
    {
        if (OnDeathVfx)
        {
            var vfx = Instantiate(OnDeathVfx, transform.position, Quaternion.identity);
            Destroy(vfx, 5f);
        }

        if (OnDeathSfx)
	    {
            var sfx = Instantiate(OnDeathSfx, transform.position, Quaternion.identity);
            Destroy(sfx, 5f);
        }
    }
}
