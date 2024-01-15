using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class SphereShield : MonoBehaviour
{
    [SerializeField]
    GameObject shieldOwner;

    [SerializeField]
    Material shieldMaterial, counterattackMaterial;

    [SerializeField]
    LayerMask WhatIsProjectile;

    [SerializeField, Range(1f, 20f), Tooltip("Shield growing and dissolving speed")]
    float speed = 4f;

    [SerializeField, Range(0.05f, 0.7f)]
    float counterAttackTimeWindow = 0.2f;

    [SerializeField, Range(0f, 0.25f), Tooltip("A counter attack can triggers a slow motion")]
    float slowMotionDuration = 0.05f;

    [SerializeField, Range(0f, 5f), Tooltip("Time to wait to use shield again")]
    float cooldownTime = 0f;

    [SerializeField, Range(0.5f, 3f), Tooltip("Used to compute the max radius of the shield")]
    float fullShieldAddedRadius = 1.5f;

    [SerializeField, Range(0.5f, 5f), Tooltip("Used to compute the max radius of the shield")]
    float BulletReflectionSpreadAngle = 1f;

    [SerializeField]
    Modifier StaminaConsumption;

    [SerializeField, Tooltip("Modifiers to add on reflected object")]
    Modifier[] ModifiersOnReflected;

    public bool IsShieldOn { get; private set; }
    public bool IsCounterAttackOn { get; private set; }

    public UnityAction OnCounterAttack;
    public UnityAction OnReflection;

    const float MIN_TRANSPARENCY = 0.01f;
    const float MIN_REFLECTION_SPEED = 60f;
    const float SLOW_MOTION_TIME_SCALE = 0.1f;

    float m_LastShieldUsage;
    bool m_DesiresShield;
    bool m_IsCoroutineActive;
    bool m_IsDissolving;
    Modifier m_StaminaConsumptionCopy;
    PersistentStat m_Stamina;
    InputAction m_ShieldAction;
    MeshRenderer m_Renderer;
    SphereCollider m_Collider;
    Vector3 m_StartingScale, m_EndingScale;
    Coroutine m_Coroutine;


    void Awake()
    {
        m_Collider = GetComponent<SphereCollider>();
        m_Renderer = GetComponent<MeshRenderer>();

        if (!shieldOwner) shieldOwner = this.gameObject;

        m_StartingScale = this.transform.localScale;
        m_EndingScale = m_StartingScale + Vector3.one * fullShieldAddedRadius;
        m_Collider.enabled = false;
    }

    void Start()
    {
        var inputs = FindObjectOfType<InputManager>();
        m_ShieldAction = inputs.SphereInputActions.Player.Shield;

        var stats = GetComponentInParent<StatsCollectionManager>();
        var sm = stats.GetStatManager<StaminaManager>();
        var hm = stats.GetStatManager<HealthManager>();

        m_Stamina = sm.Stamina;
        hm.DamageManager.Shield = this;
    }

    void Update()
    {
        if (Time.time < m_LastShieldUsage + cooldownTime)
            return;

        // Stop here if no input was received
        m_DesiresShield = m_ShieldAction.IsPressed() && m_Stamina.CurrValue > 0f;
        if (!m_DesiresShield)
            return;

        if (!m_IsCoroutineActive)
        {
            m_Coroutine = StartCoroutine(UseShield());
            return;
        }

        if (m_IsDissolving)
        {
            // Shield requested while dissolving previous shield
            StopCoroutine(m_Coroutine);
            ClearState();
            m_Coroutine = StartCoroutine(UseShield());
        }
    }

    IEnumerator UseShield()
    {
        IsShieldOn = true;
        m_Renderer.enabled = m_Collider.enabled = true;
        m_IsCoroutineActive = true;
        m_StaminaConsumptionCopy = m_Stamina.CopyModifier(StaminaConsumption);

        // Counter-attack available during a small fraction of time
        IsCounterAttackOn = true;
        m_Renderer.material = counterattackMaterial;
        float end = Time.time + counterAttackTimeWindow;
        while (m_DesiresShield && Time.time < end)
        {
            IncreaseScale();
            yield return null;
        }

        // Disable counter-attack and switch to a normal shield
        IsCounterAttackOn = false;
        if (m_DesiresShield) m_Renderer.material = shieldMaterial;
        while (m_DesiresShield)
        {
            IncreaseScale();
            yield return null;
        }
        m_LastShieldUsage = Time.time;

        // Dissolve shield and reset it
        m_IsDissolving = true;

		IsShieldOn = m_Collider.enabled = false;
        m_Stamina.RemoveModifier(m_StaminaConsumptionCopy);
        while (m_Renderer.material.color.a >= MIN_TRANSPARENCY)
        {
            Dissolve();
            yield return null;
        }

        ClearState();
    }

    IEnumerator SlowMotion()
    {
        if (slowMotionDuration > 0f)
        { 
            Time.timeScale = SLOW_MOTION_TIME_SCALE;
            yield return new WaitForSeconds(slowMotionDuration);
            Time.timeScale = 1f;
	    }
    }

    void IncreaseScale()
    {
        Vector3 current = this.transform.localScale;
        this.transform.localScale =
            Vector3.Lerp(current, m_EndingScale, speed * Time.deltaTime);
    }

    void Dissolve()
    {
        Color color = m_Renderer.material.color;
        float transparency = Mathf.Lerp(color.a, 0f, speed * Time.deltaTime);
        m_Renderer.material.color = new Color(
            color.r, color.g, color.b, transparency
        );
    }

    void ClearState()
    {
        m_Stamina.RemoveModifier(m_StaminaConsumptionCopy);
        transform.localScale = m_StartingScale;
        m_Renderer.enabled = m_Collider.enabled = false;
        IsShieldOn = IsCounterAttackOn = m_IsDissolving = false;
        m_IsCoroutineActive = false;
    }

    public Vector3 AddRandomSpread(Vector3 forward)
    {
        float angle = UnityEngine.Random.Range(-BulletReflectionSpreadAngle, BulletReflectionSpreadAngle);
        var quaternion = Quaternion.Euler(angle, angle, angle);
        var newDirection = quaternion * forward;
        return newDirection;
    }

    public void OnProjectileHit(Projectile projectile, RaycastHit hit)
    {
        Vector3 direction;
        if (IsCounterAttackOn && projectile.Owner)
        {
            OnCounterAttack?.Invoke();

            // Start slow motion and reflect projectile towards the enemy
            if (slowMotionDuration > 0f)
                StartCoroutine(SlowMotion());
            direction = (projectile.Owner.transform.position - 
		        projectile.transform.position).normalized;
        }
        else
        {
            OnReflection?.Invoke();
            direction = Vector3.Reflect(projectile.transform.forward, hit.normal).normalized;
            //direction = AddRandomSpread(direction);
        }

        Vector3 addedVelocity = direction * MIN_REFLECTION_SPEED;

        // Join two arrays
        var m1 = projectile.Damage.Modifiers;
        var m2 = ModifiersOnReflected;
        var array = new Modifier[m1.Length + m2.Length];
        Array.Copy(m1, array, m1.Length);
        Array.Copy(m2, 0, array, m1.Length, m2.Length);

        projectile.Damage.Modifiers = array;
        projectile.Shoot(shieldOwner, hit.point, direction, addedVelocity);
    }
}

