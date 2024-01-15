using UnityEngine;

public class ToastTrigger : MonoBehaviour, ITrigger
{
    [SerializeField] protected Toast toast;

    public virtual void Trigger(object obj)
    {
        var evt = Events.DisplayToastEvent;
        evt.toast = toast;
        EventManager.Broadcast(evt);
    }
}
