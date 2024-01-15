using UnityEngine;
using UnityEngine.Events;

public class AutomaticSlider : MonoBehaviour {

    [SerializeField, Min(0.01f)]
    float Duration = 1f;

    [SerializeField, Tooltip("If interpolation goes back and forth")]
    bool AutoReverse = false;
    
    [SerializeField, Tooltip(
        "If unchecked, linear interpolation will be used. It may appear too " +
        "rigid when direction is reversed"
    )]
    bool Smoothstep = false;

    // Unity cannot serialize generic (<float>) event types, it's necessary to 
    // create a concrete serializable event type that extends UnityEvent
    [System.Serializable] public class OnValueChangedEvent : UnityEvent<float> { }

    // This will be seen in inspector
    [SerializeField] OnValueChangedEvent onValueChanged = default;

    public bool Reversed { get; set; }
    
    // Smootstep function applied to value v --> 3v^2 - 2v^3
    float SmoothedValue => 3f * m_Value * m_Value - 2f * m_Value * m_Value * m_Value;

    // Slider having values in [0, 1], modified by interpolation
    float m_Value;

    void FixedUpdate () {
        float delta = Time.deltaTime / Duration;
        if (Reversed) {
            m_Value -= delta;
            if (m_Value <= 0f) {
                if (AutoReverse) {
                    // Clamping to avoid overshoot of the [0, 1] range
                    m_Value = Mathf.Min(1f, -m_Value);
                    Reversed = false;
                }
                else {
                    m_Value = 0f;
                    this.enabled = false;
                }
            }
        }
        else {
            m_Value += delta;
            if (m_Value >= 1f) {
                if (AutoReverse) {
                    // Clamping
                    m_Value = Mathf.Max(0f, 2f - m_Value);
                    Reversed = true;
                }
                else {
                    m_Value = 1f;
                    this.enabled = false;
                }
            }
        }

        onValueChanged.Invoke(Smoothstep ? SmoothedValue : m_Value);
    }
}
