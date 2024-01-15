using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    GameFlowManager m_GameFlowManager;
    InputManager m_InputManager;
    bool m_DeviceNeedCursor = true;

    void Start()
    {
        var references = FindObjectOfType<GlobalReferences>();
        m_GameFlowManager = references.GameFlowManager;
        m_InputManager = references.InputManager;
        m_InputManager.InputDeviceChanged += OnInputDeviceChange;
        m_InputManager.SphereInputActions.Player.Dash.performed 
	        += (InputAction.CallbackContext obj) => UpdateCursorState();
        Cursor.visible = true;
    }

    void OnInputDeviceChange(InputDevice device)
    {
        m_DeviceNeedCursor = device is Mouse || device is Keyboard;
        UpdateCursorState();
    }

    public void UpdateCursorState()
    {
        bool lockCursor = m_DeviceNeedCursor 
            && !(m_GameFlowManager.GameIsEnding || m_GameFlowManager.GameIsPausing);

        Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockCursor;
    }
}
