using UnityEngine;
using UnityEngine.UI;

public class StatBar : Bar 
{
    [SerializeField]
    protected StatsCollectionManager Stats;

    [SerializeField]
    protected StatId StatToMonitor;

    Stat m_Stat;
    Slider m_Slider;
    RectTransform m_Rect;

    protected virtual void Awake()
    {
        m_Slider = GetComponentInChildren<Slider>();
        m_Rect = GetComponent<RectTransform>();
    }

    protected virtual void Start()
    {
        if (Stats != null)
            m_Stat = Stats.GetStat(StatToMonitor);   
        
        if (m_Stat == null)
        {
            this.gameObject.SetActive(false);
            return;
        }
        value = m_Stat.GetCurrValue();
        max = m_Stat.GetMaxValue();
    }

    protected override float GetCurrValue() =>
        m_Stat.GetCurrValue();

    protected override float GetMaxValue() =>
        m_Stat.GetMaxValue();

    protected override void UpdateCurrValue(float next)
    {
        value = Interpolate(value, next);
        m_Slider.value = 100 * value / max;
    }

    protected override void UpdateMaxValue(float next)
    {
        max = next;
        m_Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, next);
    }
}
