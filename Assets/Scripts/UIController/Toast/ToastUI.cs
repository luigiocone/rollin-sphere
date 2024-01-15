using UnityEngine;

public class ToastUI : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI body;

    RectTransform m_Background;
    const float k_MaxHeight = 250f;
    const float k_VerticalPad = 100f;

    void Awake()
    {
        m_Background = GetComponent<RectTransform>();
    }

    public void Display(string text)
    {
        string prev = body.text;
        body.text = text;

        float height = body.preferredHeight + k_VerticalPad;
        if (height >= k_MaxHeight)
        {
            Debug.Log("Desired toast height (" + height + ") is too big (>" + k_MaxHeight + ")");
            body.text = prev;
            return;
	    }

        m_Background.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        //StopAllCoroutines();
        //StartCoroutine(TypeSentence(text));
    }

    /*
    IEnumerator TypeSentence(string sentence)
    {
        float TypingSpeed = 100f;
        float m_TypingWaitTime = 1f / TypingSpeed;
        this.enabled = true;
        body.text = "";

        foreach (char c in sentence)
        {
            body.text += c;
            yield return new WaitForSeconds(m_TypingWaitTime);
        }
    }
    */
}
