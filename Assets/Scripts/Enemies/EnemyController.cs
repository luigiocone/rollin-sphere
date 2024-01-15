using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class EnemyController : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField, Tooltip("The speed at which the enemy rotates")]
    float OrientationSpeed = 10f;

    [SerializeField, Range(0f, 2f), Tooltip("Delay after death where the GameObject is destroyed (to allow for animation)")]
    float DeathDuration = 0f;

    [Header("Loot")]
    [SerializeField, Tooltip("The object this enemy can drop when dying")]
    GameObject LootPrefab;

    [SerializeField, Range(0, 1), Tooltip("The chance the object has to drop")]
    float DropRate = 1f;

    const float FleeUnits = 3f;

    public UnityAction onAttack;
    public UnityAction onDamage;
    public UnityAction onDie;

    public PatrolPath PatrolPath { get; set; }
    public NavMeshAgent NavMeshAgent { get; private set; }
    public DetectionModule DetectionModule { get; private set; }
    public List<Collider> SelfColliders { get; private set; }
    public GameObject KnownDetectedTarget => DetectionModule.KnownDetectedTarget;
    public bool IsTargetInAttackRange => DetectionModule.IsTargetInAttackRange;
    public bool IsSeeingTarget => DetectionModule.IsSeeingTarget;
    public bool HadKnownTarget => DetectionModule.HadKnownTarget;

    EnemyManager m_EnemyManager;
    WeaponController m_Weapon;

    void Awake()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
        DetectionModule = GetComponentInChildren<DetectionModule>();

        m_Weapon = GetComponentInChildren<WeaponController>();
        SelfColliders = new(GetComponentsInChildren<Collider>());
    }

    void Start()
    {
        //m_GameFlowManager = FindObjectOfType<GameFlowManager>();
        m_EnemyManager = FindObjectOfType<EnemyManager>();
        m_EnemyManager.RegisterEnemy(this);

        // Subscribe to damage & death actions
        var stats = GetComponentInChildren<StatsCollectionManager>();
        if (stats == null) return;
        var hm = stats.GetStatManager<HealthManager>();
        if (hm == null) return;
        hm.DamageManager.OnDamage += OnDamage;
        hm.Health.OnZero += (StatValueChangeEvent evt) => OnDie();
    }

    void Update()
    {
        if (DetectionModule)
            DetectionModule.HandleTargetDetection(SelfColliders);
    }

    public Vector3 GetFleeDestination()
    {
        Vector3 flee = FleeUnits * DetectionModule.ToTargetNormalized;
        Vector3 destination = transform.position - flee;
        bool canSeeTarget = DetectionModule.CanSeeTarget(
	        destination, out RaycastHit _, SelfColliders
	    );
        return (canSeeTarget) ? destination : transform.position;
    }

    public bool HasPatrolPath()
    {
        return PatrolPath && PatrolPath.Count > 0;
    }

    public void RestartPatrolling()
    {
        if (!HasPatrolPath())
            return;

        PatrolPath.GoToClosestNode(transform.position);
    }

    public Vector3 GetPatrolDestination()
    {
        if (HasPatrolPath())
            return PatrolPath.Destination;

        return transform.position;
    }

    public void SetNavDestination(Vector3 destination)
    {
        if (NavMeshAgent && NavMeshAgent.enabled)
            NavMeshAgent.SetDestination(destination);
    }

    public void OrientTowards(Vector3 lookPosition)
    {
        CustomGravity.GetGravity(transform.position, out Vector3 upAxis);
        Vector3 toEnemy = lookPosition - transform.position;

        // Project to rotate enemy only on the upAxis
        Vector3 projectedLookDirection = 
	        Vector3.ProjectOnPlane(toEnemy, upAxis).normalized;

        if (projectedLookDirection.sqrMagnitude != 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(projectedLookDirection);
            transform.rotation =
                Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * OrientationSpeed);
        }
    }

    public void OrientWeaponTowards(Vector3 lookPosition)
    {
        // Orient weapon towards player
        Vector3 weaponPosition = m_Weapon.WeaponRoot.transform.position;
        Vector3 weaponForward = (lookPosition - weaponPosition).normalized;
        m_Weapon.transform.forward = weaponForward;
    }

    void OnDamage(Damage damage)
    {
        onDamage?.Invoke();

        GameObject source = damage.Source;
        if (DetectionModule && source && source == DetectionModule.Target)
        {
            // Pursue the player
            DetectionModule.OnDamaged(source);
        }
    }

    void OnDie()
    {
        onDie?.Invoke();

        m_EnemyManager.UnregisterEnemy(this);

        // Loot an object
        if (TryDropItem())
            Instantiate(LootPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject, DeathDuration);
    }

    public bool IsTargetShieldFarEnough()
    {
        if (!DetectionModule) return true;
        var hit = DetectionModule.ClosestHit;

        // Is target far?
        if (hit.distance > DetectionModule.FleeRange/2) return true;

        // Does target have a shield? 
        SphereShield shield = hit.collider.gameObject.GetComponent<SphereShield>();
        if (!shield) return true;

        // Is the shield active?
        return !shield.IsShieldOn;
    }

    public bool TryAttack(Vector3 enemyPosition)
    {
        //if (m_GameFlowManager.GameIsEnding) return false;

        OrientWeaponTowards(enemyPosition);

        // Shoot the weapon
        bool didFire = IsTargetShieldFarEnough() && m_Weapon.TryShoot();
        if (didFire && onAttack != null)
            onAttack.Invoke();

        return didFire;
    }

    public bool TryDropItem()
    {
        if (DropRate == 0f || LootPrefab == null)
            return false;
        if (DropRate == 1f)
            return true;
        return (Random.value <= DropRate);
    }

    private void OnDrawGizmos()
    {
        if (!NavMeshAgent)
            return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(NavMeshAgent.destination, 2.5f);
    }
}
