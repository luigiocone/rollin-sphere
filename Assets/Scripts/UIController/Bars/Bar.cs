using UnityEngine;
using UnityEngine.Events;

public abstract class Bar : MonoBehaviour
{
    [SerializeField, Range(0.5f, 10f)]
    protected float AnimationSpeed = 4f;

    [SerializeField]
    protected UnityEvent OnBarUpdate;

    // Displayed values
    protected float max, value;

    protected abstract float GetMaxValue();
    protected abstract float GetCurrValue(); 

    void Update()
    {
        bool change = false;

        float newMax = GetMaxValue();
        change |= newMax != max;
        if (change)
            UpdateMaxValue(newMax);

        float newValue = GetCurrValue();
        change |= value != newValue;
        if (change) 
	    { 
            UpdateCurrValue(newValue);
            OnBarUpdate?.Invoke();
	    }
    }

    protected abstract void UpdateMaxValue(float next);
    protected abstract void UpdateCurrValue(float next);

    protected float Interpolate(float curr, float end)
    {
        const float epsilon = 0.05f;
        bool interpolate = curr < (end - epsilon) || curr > (end + epsilon);

        if (interpolate)
            return Mathf.Lerp(curr, end, Time.deltaTime * AnimationSpeed);
        return end;
    }
}

