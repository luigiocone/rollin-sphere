using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InputManager : MonoBehaviour
{
    public SphereInputActions SphereInputActions { get; private set; }
    public InputDevice LastDevice { get; private set; }

    public UnityAction<InputDevice> InputDeviceChanged;

    void Awake()
    {
        SphereInputActions = new();
        InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);
    }

    void OnEnable()
    {
        SphereInputActions.Enable();
    }

    void OnDisable()
    {
        SphereInputActions.Disable();
    }

    void OnAnyButtonPress(InputControl control)
    {
        var newDevice = control.device;
        if (newDevice == LastDevice)
            return;

        InputDeviceChanged?.Invoke(newDevice);
        LastDevice = newDevice;
    }
}

