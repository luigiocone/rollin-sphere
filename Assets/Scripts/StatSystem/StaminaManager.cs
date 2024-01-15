using System.Collections;
using UnityEngine;

public class StaminaManager : StatManager
{
    public readonly PersistentStat Stamina;
    readonly Modifier m_ConstantIncrease;
    Modifier m_ConstantIncreaseCopy; 
    const float k_PauseDuration = 1f;

    public StaminaManager(StatsCollectionManager stats, Stat stat) : base(stats, stat)
    {
        Stamina = stat as PersistentStat;
        m_ConstantIncrease = new();
        m_ConstantIncrease.StatId = StatId.STAMINA;
        m_ConstantIncrease.Type = Modifier.ModifierType.ADDER;
        m_ConstantIncrease.Amount = 5f;
        m_ConstantIncrease.Duration = float.PositiveInfinity;
        m_ConstantIncrease.ApplyPeriod = 0.1f;
        m_ConstantIncreaseCopy = Stamina.CopyModifier(m_ConstantIncrease);

        Stamina.OnZero += (StatValueChangeEvent evt) => OnZero();
    }

    void OnZero()
    {
        // Wait a bit before starting to increase stamina again
        Stats.StartCoroutine(TimedStop());
    }

    public IEnumerator TimedStop()
    {
        Stamina.RemoveModifier(m_ConstantIncreaseCopy);

        var start = Time.time;
        while (start + k_PauseDuration > Time.time)
            yield return null;

        m_ConstantIncreaseCopy = Stamina.CopyModifier(m_ConstantIncrease);
    }

}
