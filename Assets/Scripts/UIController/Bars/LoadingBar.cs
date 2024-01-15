using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingBar : Bar
{
    SceneSwitcher m_Switcher;
    Slider m_Slider;
    TextMeshProUGUI m_Text;
    const float k_Max = 100f;

    void Awake()
    {
        m_Switcher = FindObjectOfType<SceneSwitcher>();
        m_Slider = GetComponentInChildren<Slider>();
        m_Text = GetComponentInChildren<TextMeshProUGUI>();
        this.max = k_Max;
    }

    protected override float GetCurrValue() =>
        m_Switcher.LoadingPercentage;

    protected override float GetMaxValue() =>
        k_Max;

    protected override void UpdateCurrValue(float next)
    {
        value = next;

        float rounded = Mathf.Round(value * 100f);
        m_Slider.value = rounded;
        m_Text.text = rounded + "%";
    }

    protected override void UpdateMaxValue(float next) 
    { }
}

