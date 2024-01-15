using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class FadeCanvasGroup : MonoBehaviour
{
    [SerializeField, Range(1f, 5f)]
    float ShowDuration = 3f;

    [SerializeField, Range(1f, 5f)]
    float fadeSpeed = 3f;

    [SerializeField]
    CanvasGroup group;

    Coroutine coroutine;

    float startingAlpha;
    float lastChange;
    bool isFading;

    void Awake()
    {
        startingAlpha = group.alpha;
    }

    void Update()
    {
        bool shouldStartFade =
            group.alpha != 0f    // Already faded
            && !isFading
            && Time.time - lastChange > ShowDuration;

        if (shouldStartFade)
            coroutine = StartCoroutine(Fade());
    }

    IEnumerator Fade()
    {
        isFading = true;

        const float epsilon = 0.01f;
        while (group.alpha > epsilon)
        {
            yield return null;
	        group.alpha -= fadeSpeed * Time.deltaTime;
        }
        group.alpha = 0f;

        isFading = false;
    }

    public void OnCanvasGroupChange()
    {
        lastChange = Time.time;
        if (isFading)
        {
            StopCoroutine(coroutine);
            ClearState();
        }
        group.alpha = startingAlpha;
    }

    void ClearState()
    {
        isFading = false;
    }
}
