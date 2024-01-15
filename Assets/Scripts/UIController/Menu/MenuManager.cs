using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour
{
    [field: SerializeField] public GameObject MenusRoot { get; private set; }
    [SerializeField] Menu FirstMenu;
    [SerializeField] GameOverMenu GameOverMenu;
    [SerializeField] GameObject BackButton;
    [SerializeField] bool HideOnAwake = true;

    [Header("SFX")]
    [SerializeField] AudioClip ClickSfx;

    public Menu CurrentMenu { get; private set; }
    public bool IsMenuActive => MenusRoot.activeInHierarchy;

    AudioSource m_AudioSource;

    void Awake()
    {
        if (BackButton)
        {
            BackButton.GetComponent<Button>().onClick.AddListener(GoBack);
            BackButton.SetActive(FirstMenu.PreviousMenu != null);
        }

        MenusRoot.SetActive(!HideOnAwake);
        CurrentMenu = FirstMenu;
    }

    void Start()
    {
        var references = FindObjectOfType<GlobalReferences>();
        if (!references)
            return;
        var actions = references.InputManager.SphereInputActions;
        actions.UI.Navigate.performed += SelectFirstSelectable;

        m_AudioSource = GetComponent<AudioSource>();
        if (m_AudioSource && ClickSfx)
            actions.UI.Click.performed += PlayClickSfx;
    }

    void SelectFirstSelectable(InputAction.CallbackContext context)
    { 
        var selection = EventSystem.current.currentSelectedGameObject;
        if (!selection)
            CurrentMenu.Focus();
    }

    public void Transition(Menu curr, Menu next)
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (curr) curr.gameObject.SetActive(false);
        next.gameObject.SetActive(true);

        BackButton.SetActive(next.PreviousMenu != null);
        CurrentMenu = next;
    }

    public void GoBack() =>
        Transition(CurrentMenu, CurrentMenu.PreviousMenu);

    public void Hide() => 
	    MenusRoot.SetActive(false);

    public void ShowFirstMenu()
    {
        MenusRoot.SetActive(true);
        Transition(CurrentMenu, FirstMenu);
    }

    public void ShowGameOverMenu(GameOverEvent evt)
    {
        MenusRoot.SetActive(true);
        Transition(CurrentMenu, GameOverMenu);
        GameOverMenu.LoadAnimation(evt.Win);
    }

    public void PlayClickSfx(InputAction.CallbackContext ctx)
    {
        if (!IsMenuActive || !ctx.ReadValueAsButton()) return;
        m_AudioSource.PlayOneShot(ClickSfx);
    }
}

