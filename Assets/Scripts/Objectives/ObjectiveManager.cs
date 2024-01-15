using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    [SerializeField] GameObject ObjectiveUIPrefab;
    [SerializeField] RectTransform Background;

    bool WasPaused;
    GameFlowManager GameFlowManager;
    Dictionary<Objective, ObjectiveBox> m_ObjectivesDictionary = new();

    void Awake()
    {
        GameFlowManager = FindObjectOfType<GameFlowManager>();
        EventManager.AddListener<ObjectiveUpdateEvent>(OnUpdateObjective);
        Objective.OnObjectiveCreated += RegisterObjective;
        Objective.OnObjectiveCompleted += UnregisterObjective;
    }

    void OnDestroy()
    {
        EventManager.RemoveListener<ObjectiveUpdateEvent>(OnUpdateObjective);
        Objective.OnObjectiveCreated -= RegisterObjective;
        Objective.OnObjectiveCompleted -= UnregisterObjective;
    }

    void Update()
    {
        if (!WasPaused && GameFlowManager.GameIsPausing)
        {
            WasPaused = true;
            StartCoroutine(ShowAll());
        }
    }

    IEnumerator ShowAll()
    {
        foreach (var obj in m_ObjectivesDictionary.Values)
            obj.ShowImmediate();

        // Wait until no more in pause
        while (GameFlowManager.GameIsPausing)
            yield return null;
        WasPaused = false;

        foreach (var obj in m_ObjectivesDictionary.Values)
            obj.HideImmediate();
    }

    public void RegisterObjective(Objective objective)
    {
        // UI elements for the new objective
        GameObject ui = Instantiate(ObjectiveUIPrefab, Background);
        ObjectiveBox toast = ui.GetComponent<ObjectiveBox>();
        ui.transform.SetParent(Background);

        // Initialize and put on top of vertical layout the toast
        toast.Initialize(objective, "");
        toast.transform.SetAsFirstSibling();

        m_ObjectivesDictionary.Add(objective, toast);

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(Background);
    }

    public void UnregisterObjective(Objective objective)
    {
        bool validKey = m_ObjectivesDictionary.TryGetValue(objective, out ObjectiveBox toast);
        if (!validKey || toast == null)
            return;

        toast.Complete();
        toast.transform.SetAsFirstSibling();
        m_ObjectivesDictionary.Remove(objective);

        if (m_ObjectivesDictionary.Count == 0)
            EventManager.Broadcast(Events.AllObjectivesCompletedEvent);
    }

    void OnUpdateObjective(ObjectiveUpdateEvent evt)
    {
        bool validKey = m_ObjectivesDictionary.TryGetValue(evt.Objective, out ObjectiveBox toast);
        if (!validKey || toast == null)
            return;

        toast.transform.SetAsFirstSibling();

        // Set the new updated description for the objective, and forces the content size fitter to be recalculated
        Canvas.ForceUpdateCanvases();
        if (!string.IsNullOrEmpty(evt.DescriptionText))
            toast.Description.text = evt.DescriptionText;

        if (!string.IsNullOrEmpty(evt.CounterText))
            toast.Counter.text = evt.CounterText;

        if (toast.GetComponent<RectTransform>())
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(toast.GetComponent<RectTransform>());

        const float showDuration = 2f;
        toast.Show(showDuration);
    }
}
