using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : StatBar
{
    [SerializeField] GameObject HeartPrefab;

    class Heart
    {
        public Heart(GameObject g, Image i)
        {
            this.GameObject = g;
            this.Image = i;
        }
        public GameObject GameObject { get; }
        public Image Image { get; }
    }

    readonly List<Heart> m_Hearts = new();
    const int k_HealthPerHeart = 20;
    public int NumHearts { get; protected set; }


    private void OnValidate() =>
        this.StatToMonitor = StatId.HEALTH;

    protected override void Awake() =>
        OnValidate();

    protected override void Start()
    {
        base.Start();
        UpdateNoOfHearts();
        UpdateDisplayedHealth();
    }

    protected override void UpdateCurrValue(float next)
    {
        value = this.Interpolate(value, next);
        UpdateDisplayedHealth();
    }

    protected override void UpdateMaxValue(float next)
    {
        max = next;
        UpdateNoOfHearts();
    }

    int ComputeNumHearts()
    {
        float div = max / k_HealthPerHeart;
        return (int) Mathf.Ceil(div);
    }

    void CreateHeart()
    {
        GameObject go = Instantiate(HeartPrefab);
        go.transform.SetParent(this.transform);

        Heart heart = new(go, go.GetComponent<Image>());
        m_Hearts.Add(heart);
    }

    void DestroyHeart()
    {
        Heart heart = m_Hearts[^1];
        m_Hearts.Remove(heart);
        Destroy(heart.GameObject); 
    }

    void UpdateDisplayedHealth()
    {
        // Which heart is not completely full or empty
        int heartIndex = (value >= max) ? 
            NumHearts-1 : (int) Mathf.Floor(value / k_HealthPerHeart);

        // Check if agent is dead
        if (heartIndex < 0)
        {
            SetAmountInRange(0, NumHearts, 0f);
            return;
        }

        // Fill the hearth at the computed index with following amount
        float fillAmount = value - heartIndex * k_HealthPerHeart;
        float percentage = fillAmount / k_HealthPerHeart;
        m_Hearts[heartIndex].Image.fillAmount = percentage;

        // Fill or empty completely the other hearts
        SetAmountInRange(0, heartIndex, 1f);
        SetAmountInRange(heartIndex + 1, NumHearts, 0f);
    }

    void SetAmountInRange(int start, int end, float amount)
    { 
        for(int i = start; i < end; i++)
            m_Hearts[i].Image.fillAmount = amount;
    }

    void UpdateNoOfHearts()
    {
        NumHearts = ComputeNumHearts();

        // Fill or empty the hearts array to match NumHearts 
        for (int i = m_Hearts.Count; i >= NumHearts; i--) DestroyHeart();
        for (int i = m_Hearts.Count; i <  NumHearts; i++) CreateHeart();
    }
}

