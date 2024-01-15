using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ObjectiveBox : MonoBehaviour
{
    [Header("References")]
    public TMPro.TextMeshProUGUI Title;
    public TMPro.TextMeshProUGUI Description;
    public TMPro.TextMeshProUGUI Counter;

    [SerializeField, Tooltip("Rect that will display the description")]
    RectTransform DescriptionRect;

    [SerializeField, Tooltip("Used to fade in and out the content")]
    CanvasGroup CanvasGroup;

    [Header("Transitions")]
    [SerializeField] float ShowDuration = 3f;
    [SerializeField] float FadeInDuration = 0.5f;
    [SerializeField] float FadeOutDuration = 2f;
    [SerializeField] float OnShowingDelay = 0.5f;
    [SerializeField] float CompletionDelay = 2f;

    [Header("Sound")]
    [SerializeField] AudioClip InitSound;
    [SerializeField] AudioClip CompletedSound;

    AudioSource m_AudioSource;

    public void Initialize(Objective objective, string counter)
    {
        // set the description for the objective, and forces the content size fitter to be recalculated
        Canvas.ForceUpdateCanvases();

        Title.text = objective.Title;
        Description.text = objective.Description;
        Counter.text = counter;

        if (GetComponent<RectTransform>())
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        DescriptionRect.gameObject.SetActive(!string.IsNullOrEmpty(Description.text));
        StartCoroutine(FadeIn(playSound: true, fadeout: true));
    }

    public void Complete()
    {
        StopAllCoroutines();
        PlaySound(CompletedSound);
        StartCoroutine(FadeOut(true, CompletionDelay));
    }

    public void Show(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(TimedShow(duration));
        CanvasGroup.alpha = 1f;
    }

    public void ShowImmediate()
    {
        StopAllCoroutines();
        CanvasGroup.alpha = 1f;
    }

    public void HideImmediate()
    {
        StopAllCoroutines();
        CanvasGroup.alpha = 0f;
    }

    IEnumerator TimedShow(float duration)
    {
        yield return new WaitForSeconds(duration);
        StartCoroutine(FadeOut(false, 0f));
    }

    IEnumerator FadeIn(bool playSound, bool fadeout)
    {
        float start = Time.time + OnShowingDelay;
        float elapsed = Time.time - start;

        while (elapsed < FadeInDuration)
        {
            CanvasGroup.alpha = elapsed / FadeInDuration;
            yield return null;
            elapsed = Time.time - start;
        }
        CanvasGroup.alpha = 1f;

        if (playSound)
	        PlaySound(InitSound);

        if (fadeout)
        {
            yield return new WaitForSeconds(ShowDuration);
            StartCoroutine(FadeOut(false, 0f));
        }
    }

    IEnumerator FadeOut(bool destroy, float delay)
    {
        float start = Time.time + delay;
        float elapsed = Time.time - start;

        while (elapsed < FadeOutDuration)
        {
            CanvasGroup.alpha = 1 - (elapsed / FadeOutDuration);
            yield return null;
            elapsed = Time.time - start;
        }
        CanvasGroup.alpha = 0f;

        if (destroy)
            Destroy(gameObject);

    }

    void PlaySound(AudioClip sound)
    {
        if (!sound)
            return;

        if (!m_AudioSource)
        {
            // TODO
            //m_AudioSource = gameObject.AddComponent<AudioSource>();
            //m_AudioSource.outputAudioMixerGroup = AudioUtility.GetAudioGroup(AudioUtility.AudioGroups.HUDObjective);
        }

        m_AudioSource.PlayOneShot(sound);
    }
}
