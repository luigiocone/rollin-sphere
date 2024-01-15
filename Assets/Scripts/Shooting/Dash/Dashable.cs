using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Dashable : MonoBehaviour
{
    [SerializeField] GameObject placeholder;

    public UnityAction onAimedStart;
    public UnityAction onAimedEnd;

    public Vector3 ToCamera { get; private set; }
    public Vector3 ToTarget { get; private set; }
    public Vector3 ToTargetNormalized { get; private set; }
    public float SqrDistance { get; private set; }
    public List<Collider> Colliders { get => m_EnemyController.SelfColliders; }

    bool isRegistered = false;
    bool hasDetectionModule;
    EnemyController m_EnemyController;
    Dasher m_Dasher;
    GameObject m_Camera;

    void Start()
    {
        var references = FindObjectOfType<GlobalReferences>();
        m_Dasher = references.Player.GetComponent<Dasher>();
        m_Camera = references.Camera;

        m_EnemyController = GetComponentInParent<EnemyController>();
        hasDetectionModule = m_EnemyController 
	        && m_EnemyController.enabled && m_EnemyController.DetectionModule;

        if (placeholder)
            placeholder.SetActive(false);

        var stats = GetComponent<StatsCollectionManager>();
        if (stats)
        {
            if (stats.GetStat(StatId.HEALTH) is PersistentStat health)
                health.OnZero += OnDie;
        }
    }

    void Update()
    {
        UpdateDistanceVariables();

        if (SqrDistance < m_Dasher.SqrMaxDashDistance)
            Register();
        else
            Unregister();
    }

    void UpdateDistanceVariables()
    { 
        if (hasDetectionModule)
        {
            var detector = m_EnemyController.DetectionModule;
            ToTarget = -detector.ToTarget;
            ToTargetNormalized = -detector.ToTargetNormalized;
            SqrDistance = detector.ToTargetSqrMagnitude;
            return;
	    }

        ToCamera = this.transform.position - m_Camera.transform.position ;
        ToTarget = this.transform.position - m_Dasher.transform.position ;
        SqrDistance = ToTarget.sqrMagnitude;
        ToTargetNormalized = ToTarget.normalized;
    }

    void Register()
    {
        if (isRegistered) return;
        isRegistered = true;
        m_Dasher.Register(this);
    }

    void Unregister()
    { 
        if (!isRegistered) return;
        isRegistered = false;
        m_Dasher.Unregister(this);
    }

    public void OnAimedStart()
    {
        if (placeholder) 
	        placeholder.SetActive(true);
        onAimedStart?.Invoke();
    }

    public void OnAimedEnd()
    {
        if (placeholder)
            placeholder.SetActive(false);
        onAimedEnd?.Invoke();
    }

    void OnDie(StatValueChangeEvent evt) => Unregister();
}
