using UnityEngine;

public class InstantDeathTrigger : MonoBehaviour, ITrigger
{
    public void Trigger(object obj)
    {
        Collider other = obj as Collider;
        if (!other) return;
        Kill(other.gameObject);
    }

    void Kill(GameObject victim)
    {
        var stats = victim.GetComponentInParent<StatsCollectionManager>();
        if (stats == null) 
	        return;

        var hm = stats.GetStatManager<HealthManager>();
        if (hm == null) 
	        return;

        hm.DamageManager.Kill();
    }
}
