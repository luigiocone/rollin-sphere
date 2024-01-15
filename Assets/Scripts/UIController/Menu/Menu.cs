using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Menu : MonoBehaviour 
{
    [field: SerializeField] public Selectable FirstSelectable { get; protected set; }
    [field: SerializeField] public Menu PreviousMenu { get; private set; }

    MenuManager MenuManager;

    protected virtual void Awake()
    {
        MenuManager = FindObjectOfType<MenuManager>();
    }

    public void Transition(Menu next) =>
        MenuManager.Transition(this, next);

    public virtual void Focus()
    {
        if (FirstSelectable != null) 
            FirstSelectable.Select();
    }
}

