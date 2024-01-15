using System.Collections;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField]
    Modifier[] Modifiers;

    [SerializeField, Tooltip("Follow agent if close enough, else collect immediately")]
    bool FollowAgent = true;

    [SerializeField, Range(10f, 50f)]
    float FollowingAcceleration = 20f;

    [SerializeField]
    GameObject OnCollectVfx;

    [SerializeField]
    Color VfxColor = new(0f, 1f, 0.25f, 1f);

    Collider m_Collider;
    StatsCollectionManager m_Stats;
    Transform m_Collector;
    
    const float k_MinDistanceToCollect = 0.55f;

    void Awake()
    {
        m_Collider = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        // Only agents with stats can collect a collectable -> TODO: Create a specific layer
        m_Stats = other.GetComponentInChildren<StatsCollectionManager>();
        if (!m_Stats) 
	        return;

        // Don't collect a medikit (or something else) if the health is full
        bool needToCollect = Modifiers.Length == 0;
        foreach (Modifier m in Modifiers)
        {
            if (needToCollect) break;
            var stat = m_Stats.GetStat(m.StatId);
            bool persistent = stat.Settings.IsPersistent;
            bool notFull = persistent && stat.GetCurrValue() < stat.GetMaxValue();
            needToCollect = !persistent || notFull;
        }

        if (!needToCollect)
            return;

        if (!FollowAgent)
        {
            // Collect immediately 
            m_Stats.CopyModifiers(Modifiers);
            return;
	    }

        m_Collider.enabled = false;
        m_Collector = m_Stats.transform;
        StartCoroutine(Follow(m_Collector));
    }

    IEnumerator Follow(Transform collector)
    {
        float speed = 0f;
        float distance = Vector3.Distance(transform.position, collector.position);
        while (distance >= k_MinDistanceToCollect)
        {
            speed += FollowingAcceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(
                transform.position, collector.position, speed * Time.deltaTime
            );
            yield return null;

            distance = Vector3.Distance(transform.position, collector.position);
        }
        m_Stats.CopyModifiers(Modifiers);
        OnCollect();
    }

    void OnCollect()
    {
        PickupEvent evt = Events.PickupEvent;
        evt.Pickup = this;
        EventManager.Broadcast(evt);

        if (OnCollectVfx)
        {
            var vfx = Instantiate(OnCollectVfx, m_Collector.position, Quaternion.identity);
            var ps = vfx.GetComponent<ParticleSystem>().main;
	        ps.startColor = VfxColor;
            vfx.transform.SetParent(m_Collector);
            Destroy(vfx, 5f);
        }
        Destroy(this.gameObject);
    }
}

