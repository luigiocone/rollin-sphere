using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] string LoadingSceneName = "Loading Scene";

    static string NextSceneName;
    static AsyncOperation CurrOperation;

    public float LoadingPercentage { get; private set; } = 0f;

    void Start()
    {
        if (NextSceneName != null)
        {
            StartCoroutine(LoadSceneAsync());
            return;
	    }
        EventManager.AddListener<ChangeSceneEvent>(ChangeScene);
    }

    public void ChangeScene(ChangeSceneEvent evt)
    {
        NextSceneName = evt.sceneName;
        if (NextSceneName == string.Empty)
            NextSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(LoadingSceneName);
    }

    IEnumerator LoadSceneAsync()
    {
        CurrOperation = SceneManager.LoadSceneAsync(NextSceneName);
        CurrOperation.allowSceneActivation = false;
        NextSceneName = null;

        LoadingPercentage = 0f;
        while (LoadingPercentage < 0.9f)
        {
            LoadingPercentage = CurrOperation.progress;
            yield return null;
	    }

        LoadingPercentage = 1f;
        yield return null;
        CurrOperation.allowSceneActivation = true;
    }
}

