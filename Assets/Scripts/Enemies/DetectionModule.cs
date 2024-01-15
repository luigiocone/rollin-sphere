using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DetectionModule : MonoBehaviour
{
    [field: SerializeField, Tooltip("The point representing the source of target-detection raycasts for the enemy AI")]
    public Transform DetectionSourcePoint { get; private set; }

    [field: SerializeField, Min(10f), Tooltip("The max distance at which the enemy can see targets")]
    public float DetectionRange { get; private set; } = 20f;

    [field: SerializeField, Min(5f), Tooltip("The max distance at which the enemy can attack its target")]
    public float AttackRange { get; private set; } = 15f;

    [field: SerializeField, Min(3f), Tooltip("The max distance at which the enemy can see targets")]
    public float FleeRange { get; private set; } = 10f;

    [SerializeField, Range(3f, 20f), Tooltip("Time before an enemy abandons a known target that it can't see anymore")]
    float KnownTargetTimeout = 4f;

    [Header("Debug Display")]
    [SerializeField, Tooltip("Color of the sphere gizmo representing the detection range")]
    Color DetectionRangeColor = Color.blue;

    [SerializeField, Tooltip("Color of the sphere gizmo representing the attack range")]
    Color AttackRangeColor = Color.red;

    [SerializeField, Tooltip("Color of the sphere gizmo representing the flee range")]
    Color FleeRangeColor = Color.green;


    public UnityAction OnDetectedTarget;
    public UnityAction OnLostTarget;

    public GameObject Target { get; private set; }
    public GameObject KnownDetectedTarget { get; private set; }
    public bool IsTargetInAttackRange { get; private set; }
    public bool IsSeeingTarget { get; private set; }
    public bool HadKnownTarget { get; private set; }
    public Vector3 ToTarget { get; private set; }
    public Vector3 ToTargetNormalized { get; private set; }
    public float ToTargetSqrMagnitude { get; private set; }
    public RaycastHit ClosestHit { get => closestHit; }

    RaycastHit closestHit;
    float TimeLastSeenTarget = Mathf.NegativeInfinity;
    float sqrAttackRange;
    float sqrDetectionRange;
    List<Collider> targetColliders;

    void OnValidate()
    {
        if (FleeRange >= AttackRange) FleeRange = AttackRange - 3f;
        if (AttackRange >= DetectionRange) AttackRange = DetectionRange - 5f;
    }

    void Start()
    {
        var references = FindObjectOfType<GlobalReferences>();
        Target = references.Player;
        sqrAttackRange = AttackRange * AttackRange;
        sqrDetectionRange = DetectionRange * DetectionRange;
        targetColliders = new(Target.GetComponentsInChildren<Collider>());
    }

    public void OnDamaged(GameObject damageSource)
    {
        TimeLastSeenTarget = Time.time;
        KnownDetectedTarget = damageSource;
    }

    public void HandleTargetDetection(List<Collider> selfColliders)
    {
        // Handle known target detection timeout
        bool IsDetectionTimedOut = KnownDetectedTarget 
	        && !IsSeeingTarget 
	        && (Time.time - TimeLastSeenTarget) > KnownTargetTimeout;

        if (IsDetectionTimedOut)
            KnownDetectedTarget = null;

        // Check if target is inside attack range
        ToTarget = Target.transform.position - DetectionSourcePoint.position;
        ToTargetSqrMagnitude = ToTarget.sqrMagnitude;
        ToTargetNormalized = ToTarget.normalized;

        IsSeeingTarget = false;
        if (ToTargetSqrMagnitude > sqrDetectionRange)
        {
            if (IsDetectionTimedOut) 
		        UpdateState(ToTargetSqrMagnitude);
            return;
        }

        IsSeeingTarget = CanSeeTarget(DetectionSourcePoint.position, 
	        out closestHit, selfColliders);

        if (IsSeeingTarget)
        {
            TimeLastSeenTarget = Time.time;
            KnownDetectedTarget = closestHit.collider.gameObject;
        }
        UpdateState(ToTargetSqrMagnitude);
    }

    void UpdateState(float sqrDistance)
    {
        IsTargetInAttackRange = KnownDetectedTarget != null 
	        && sqrDistance <= sqrAttackRange;

        // Detection events
        if (!HadKnownTarget && KnownDetectedTarget != null)
            OnDetectedTarget?.Invoke();

        if (HadKnownTarget && KnownDetectedTarget == null)
            OnLostTarget?.Invoke();

        HadKnownTarget = KnownDetectedTarget != null;
    }

    public bool CanSeeTarget(Vector3 origin, out RaycastHit closestValidHit, List<Collider> toIgnore)
    {
        // Check for obstructions
        RaycastHit[] hits = Physics.SphereCastAll(
            origin,
            0.01f,
            ToTargetNormalized, 
	        DetectionRange,
            -1, QueryTriggerInteraction.Ignore
            );

        closestValidHit = new();
        closestValidHit.distance = Mathf.Infinity;
        bool foundValidHit = false;

        foreach (var hit in hits)
        {
            if (!toIgnore.Contains(hit.collider) && hit.distance < closestValidHit.distance)
            {
                closestValidHit = hit;
                foundValidHit = true;
            }
        }

        if (!foundValidHit) 
	        return false;

        // Return if the closest hit occurred with target
        return targetColliders.Contains(closestValidHit.collider);
    }

    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = DetectionRangeColor;
        Gizmos.DrawWireSphere(transform.position, DetectionRange);

        // Attack range
        Gizmos.color = AttackRangeColor;
        Gizmos.DrawWireSphere(transform.position, AttackRange);

        // Flee range
        Gizmos.color = FleeRangeColor;
        Gizmos.DrawWireSphere(transform.position, FleeRange);
    }
}
