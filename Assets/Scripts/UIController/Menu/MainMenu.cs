using UnityEngine;

public class MainMenu : Menu
{
    GameFlowManager m_GameFlowManager;

    protected override void Awake()
    {
        base.Awake();
        m_GameFlowManager = FindObjectOfType<GameFlowManager>();
    }

    public void Resume()
    {
        if (m_GameFlowManager) m_GameFlowManager.OnPause(default);
    }

    public void LoadLevel(string sceneName)
    {
        m_GameFlowManager.LoadLevel(sceneName);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit(0);
        #endif
    }
}
