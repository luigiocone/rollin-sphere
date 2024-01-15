using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using EZCameraShake;

public class Dasher : MonoBehaviour
{
    //[SerializeField] Damage DashDamage;

    [SerializeField, Range(10f, 50f)]
    float DashSpeed = 25f;

    [SerializeField, Range(0.1f, 1.5f)]
    float MaxDashDuration = 0.5f;

    [SerializeField, Range(5f, 30f)]
    float ForceAfterDash = 5f;

    [SerializeField, Range(0.1f, 1f)]
    float CooldownDuration = 0.25f;

    [SerializeField]
    LayerMask WhatIsAgent = -1;

    public float SqrMaxDashDistance { get; private set; }

    public UnityAction<bool> OnDash;

    float m_MaxDashDistance;
    bool m_IsDashing;
    float m_LastDash;
    InputAction m_DashAction;
    Collision m_LastCollision;
    readonly List<Dashable> m_Dashables = new();
    Dashable m_LastAimed;
    SphereShield m_Shield;
    HealthManager m_HealthManager;
    Rigidbody m_Body;
    GameObject m_Camera;

    public void Register(Dashable d) => m_Dashables.Add(d);
    public void Unregister(Dashable d)
    {
        ReplaceLastAimed(null);
        m_Dashables.Remove(d);
    }

    void OnValidate()
    {
        m_MaxDashDistance = DashSpeed * MaxDashDuration;
        SqrMaxDashDistance = m_MaxDashDistance * m_MaxDashDistance;
    }

    void Start()
    {
        OnValidate();

        var references = FindObjectOfType<GlobalReferences>();
        m_Camera = references.Camera;
        m_DashAction = references.InputManager.SphereInputActions.Player.Dash;

        var stats = GetComponentInChildren<StatsCollectionManager>();
        m_HealthManager = stats.GetStatManager<HealthManager>();

        m_Body = GetComponent<Rigidbody>();
        m_Shield = GetComponentInChildren<SphereShield>();
    }

    void Update()
    {
        bool cannotDash = m_IsDashing || Time.time - m_LastDash < CooldownDuration;
        if (cannotDash) 
	        return;

        var dashable = GetAimedDashable();
        ReplaceLastAimed(dashable);

        cannotDash = !dashable
            || m_Shield.IsShieldOn
            || !m_DashAction.WasPressedThisFrame();

        if (cannotDash) 
	        return;

        StartCoroutine(MakeDash(dashable));
    }


    Dashable GetAimedDashable()
    {
        Vector3 cameraForward = m_Camera.transform.forward;
        float min = float.MaxValue;
        Dashable toReturn = null;

        foreach (var d in m_Dashables)
        {
            float angle = Vector3.Angle(cameraForward, d.ToCamera);
            if (angle > min)
                continue;

            min = angle;
            toReturn = d;
        }
        return toReturn;
    }

    void ReplaceLastAimed(Dashable d)
    {
        if (m_LastAimed == d) 
	        return;

        if (m_LastAimed) 
	        m_LastAimed.OnAimedEnd();

        m_LastAimed = d;
        if (m_LastAimed) 
	        m_LastAimed.OnAimedStart();
	}

    IEnumerator MakeDash(Dashable dashable)
    {
        InitDashState();

        Vector3 direction = Vector3.zero;
        float start = Time.time;
        while (m_LastCollision == null && Time.time - start < MaxDashDuration)
        {
            direction = dashable.ToTargetNormalized;
            m_Body.velocity = direction * DashSpeed;
            yield return null;
        }

        if (m_LastCollision != null)
        {
            m_Body.velocity = Vector3.zero;
            m_Body.AddForce(-direction * ForceAfterDash, ForceMode.Impulse);
        }

        ClearDashState();
    }

    void InitDashState()
    {
        m_IsDashing = true;
        m_HealthManager.Invincible = true;
        OnDash?.Invoke(true);
        m_LastCollision = null;
        //movingScript.enabled = false;
    }

    void ClearDashState()
    {
        m_IsDashing = false;
        m_HealthManager.Invincible = false;
        OnDash?.Invoke(false);
        m_LastDash = Time.time;
        //movingScript.enabled = true;
    }

    void OnCollisionEnter(Collision collision) =>
        InflictDamage(collision);

    void OnCollisionStay(Collision collision) =>
        InflictDamage(collision);

    void InflictDamage(Collision collision)
    {
        if (!m_IsDashing) return;

        bool isAgent = (WhatIsAgent & (1 << collision.gameObject.layer)) == 0;
        if (isAgent) return;

        m_LastCollision = collision;

        CameraShaker.Instance.ShakeOnce(3f, 3f, 0.1f, 1f);

        var stats = collision.gameObject.GetComponentInParent<StatsCollectionManager>();
        if (stats == null) return;
        var hm = stats.GetStatManager<HealthManager>();
        if (hm == null) return;
        hm.DamageManager.Kill();
    }
}
