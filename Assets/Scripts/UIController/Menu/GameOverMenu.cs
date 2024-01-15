using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameOverMenu : Menu
{
    [SerializeField, Range(1f, 10f)] float LoadingSpeed = 1.5f;
    [SerializeField] Image Background;

    [Header("On Win")]
    [SerializeField] Color OnWinColor = new(0.68f, 1, 0.47f, 0.39f);
    [SerializeField] GameObject OnWinMenu;

    [Header("On Lose")]
    [SerializeField] Color OnLoseColor = new(1f, 0.47f, 0.56f, 0.39f);
    [SerializeField] GameObject OnLoseMenu;

    GameFlowManager m_GameFlowManager;
    Selectable m_WinFirstSelectable, m_LoseFirstSelectable;

    protected override void Awake()
    {
        base.Awake();

        m_GameFlowManager = FindObjectOfType<GameFlowManager>();

        m_WinFirstSelectable = 
	        OnWinMenu.GetComponentInChildren<Menu>().FirstSelectable;
        m_LoseFirstSelectable = 
	        OnLoseMenu.GetComponentInChildren<Menu>().FirstSelectable;

        OnWinMenu.SetActive(false);
        OnLoseMenu.SetActive(false);

    }

    public void LoadAnimation(bool win)
    {
        if (win)
        {
            FirstSelectable = m_WinFirstSelectable;
            StartCoroutine(LoadGameOverMenu(OnWinColor, OnWinMenu));
        }
        else
        {
            FirstSelectable = m_LoseFirstSelectable;
            StartCoroutine(LoadGameOverMenu(OnLoseColor, OnLoseMenu));
        }
    }

    public void ChangeScene(string sceneName)
    {
        Time.timeScale = 1f;
        var evt = Events.ChangeSceneEvent;
        evt.sceneName = sceneName;
        EventManager.Broadcast(evt);
    }

    public void NextLevel()
    {
        m_GameFlowManager.LoadNextLevel();
    }

    IEnumerator LoadGameOverMenu(Color color, GameObject menu)
    {
        float targetAlpha = color.a;
        color.a = 0f;
        Background.color = color;

        float scale = Time.timeScale;

        var time = 0f;
        var exit = false;

        while (!exit)
        {
            time += Time.unscaledDeltaTime * LoadingSpeed;

            // Lower the transparency level 
            bool cond1 = color.a < targetAlpha;
            if (cond1)
            {
                color.a = Mathf.Lerp(0f, targetAlpha, time);
                Background.color = color;
            }

            // Slow down time
            bool cond2 = scale > 0f;
            if (cond2)
            {
                scale = Mathf.Lerp(1f, 0f, time);
                Time.timeScale = scale;
            }

            exit = !cond1 && !cond2;
            yield return null;
        }

        menu.SetActive(true);
    }
}

