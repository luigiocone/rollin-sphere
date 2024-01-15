using UnityEngine.InputSystem;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [field: SerializeField] 
    public string NextLevel { get; private set; } = string.Empty;

    public bool GameIsEnding { get; private set; }
    public bool GameIsPausing { get; private set; }

    MenuManager m_MenuManager;
    InputManager m_InputManager;
    CursorManager m_CursorManager;

    void Awake()
    {
        #if UNITY_EDITOR
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 144;
        #endif

        EventManager.AddListener<PlayerDeathEvent>(OnPlayerDeath);
        EventManager.AddListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
    }

    void Start()
    {
        var references = FindObjectOfType<GlobalReferences>();
        m_InputManager = references.InputManager;
        m_CursorManager = references.CursorManager;
        m_MenuManager = references.MenuManager;

        var actions = m_InputManager.SphereInputActions;
        actions.Player.Pause.performed += OnPause;
        
        #if UNITY_EDITOR
            actions.Player.Debug.performed += OnDebug;
        #endif
    }

    void OnDebug(InputAction.CallbackContext context)
    {
        if (context.action.WasPressedThisFrame())
            return;

        // Toggle 
        Time.timeScale = (Time.timeScale == 0f) ? 1f : 0f;
    }

    void OnAllObjectivesCompleted(AllObjectivesCompletedEvent e)
    {
        EventManager.RemoveListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);

        GameOverEvent evt = Events.GameOverEvent;
        evt.Win = true;
        OnEndGame(evt);
        EventManager.Broadcast(evt);
    }

    void OnPlayerDeath(PlayerDeathEvent e)
    {
        EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);

        GameOverEvent evt = Events.GameOverEvent;
        evt.Win = false;
        OnEndGame(evt);
        EventManager.Broadcast(evt);
    }

    void OnEndGame(GameOverEvent evt)
    {
        GameIsEnding = true;
        GameIsPausing = false;
        m_CursorManager.UpdateCursorState();
        m_MenuManager.ShowGameOverMenu(evt);
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (GameIsEnding) return;
        GameIsPausing = !GameIsPausing;
        m_CursorManager.UpdateCursorState();

        if (GameIsPausing)
        {
            Time.timeScale = 0f;
            m_MenuManager.ShowFirstMenu();
            return;
        }
        // else resume
        m_MenuManager.Hide();
        Time.timeScale = 1f;
    }

    public void LoadLevel(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            sceneName = NextLevel;

        Time.timeScale = 1f;
        var evt = Events.ChangeSceneEvent;
        evt.sceneName = sceneName;
        EventManager.Broadcast(evt);
    }

    public void LoadNextLevel() => LoadLevel(NextLevel);

    private void OnDestroy()
    {
        EventManager.RemoveListener<PlayerDeathEvent>(OnPlayerDeath);
        EventManager.RemoveListener<AllObjectivesCompletedEvent>(OnAllObjectivesCompleted);
    }
}

