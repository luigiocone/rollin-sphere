using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class Toast
{
    [TextArea(minLines: 3, maxLines: 5)]
    public string text;

    [Range(0f, 5f)] public float MinShowDuration = 2f;
    [Range(3f, 10f)] public float MaxShowDuration = 5f;

    [Tooltip("The toast should be hidden when this action event occurs")]
    public InputActionReference HideAction;

    private void OnValidate()
    {
        if (MinShowDuration > MaxShowDuration)
            MinShowDuration = MaxShowDuration;
    }
}

