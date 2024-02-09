using System.Collections;
using UnityEngine;

public class StaminaManager : StatManager
{
    public readonly PersistentStat Stamina;
    readonly Modifier m_ConstantIncrease;
    Modifier m_ConstantIncreaseCopy; 
    const float k_PauseDuration = 1f;
    const string k_ConstantIncreaseResource = "Modifiers/Stamina-Up-Constant-Increase";

    public StaminaManager(StatsCollectionManager stats, Stat stat) : base(stats, stat)
    {
        Stamina = stat as PersistentStat;
        m_ConstantIncrease = Resources.Load<Modifier>(k_ConstantIncreaseResource);
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
