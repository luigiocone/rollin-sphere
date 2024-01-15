using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToastManager : MonoBehaviour
{
    ToastUI m_ToastUI;
    Toast m_CurrToast;
    float m_LastEventTimestamp;

    void Awake()
    {
        m_ToastUI = FindObjectOfType<ToastUI>();
        m_ToastUI.gameObject.SetActive(false);
        EventManager.AddListener<DisplayToastEvent>(OnDisplayToastEvent);
        EventManager.AddListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);
    }

    void OnDestroy()
    {
        EventManager.RemoveListener<DisplayToastEvent>(OnDisplayToastEvent);
        EventManager.RemoveListener<ObjectiveUpdateEvent>(OnObjectiveUpdateEvent);
    }

    void OnDisplayToastEvent(DisplayToastEvent evt) =>
        DisplayToast(evt.toast);

    void OnObjectiveUpdateEvent(ObjectiveUpdateEvent evt)
    {
        if (string.IsNullOrEmpty(evt.NotificationText))
            return;
        Toast toast = new();
        toast.text = evt.NotificationText;
        DisplayToast(toast);
    }
    
    void DisplayToast(Toast toast)
    { 
        StopAllCoroutines();
        ResetToastUI();
        m_LastEventTimestamp = Time.time;
        m_CurrToast = toast;

        // Hide this toast when player performs a specific input
        if (m_CurrToast.HideAction != null)
            m_CurrToast.HideAction.action.performed += OnHide;

        // Show the toast
        m_ToastUI.gameObject.SetActive(true);
        m_ToastUI.Display(m_CurrToast.text);

        // Hide the toast after some seconds
        StartCoroutine(CountdownHide());
    }

    IEnumerator CountdownHide()
    {
        yield return new WaitForSeconds(m_CurrToast.MaxShowDuration);
        ResetToastUI();
    }

    void OnHide(InputAction.CallbackContext context)
    {
        // Hide toast after at least 'min show duration' seconds
        float elapsed = Time.time - m_LastEventTimestamp;
        bool canHide = elapsed > m_CurrToast.MinShowDuration;
        if (canHide) 
	        ResetToastUI();
    }

    public void ResetToastUI()
    {
        if (m_CurrToast == null) 
	        return;

        if (m_CurrToast.HideAction != null)
            m_CurrToast.HideAction.action.performed -= OnHide;

	    m_ToastUI.Display(string.Empty);
        m_ToastUI.gameObject.SetActive(false);
        m_CurrToast = null;
    }
}
