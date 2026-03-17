using System.Collections;
using UnityEngine;

public class BasePopup : MonoBehaviour
{
    [Tooltip("Duration of open/close animation in seconds")]
    public float animationDuration = 0.25f;

    [Tooltip("Animation curve for open/close progress")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    CanvasGroup canvasGroup;
    RectTransform rectTransform;
    Vector3 targetScale = Vector3.one;

    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null) rectTransform = gameObject.AddComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Ensure start state is closed/invisible
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        rectTransform.localScale = Vector3.zero;
    }

    public virtual void Open()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        StartCoroutine(Animate(true));
    }

    public virtual void Close()
    {
        StopAllCoroutines();
        StartCoroutine(Animate(false));
    }

    IEnumerator Animate(bool opening)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.0001f, animationDuration);

        Vector3 fromScale = opening ? Vector3.zero : targetScale;
        Vector3 toScale = opening ? targetScale : Vector3.zero;
        float fromAlpha = opening ? 0f : 1f;
        float toAlpha = opening ? 1f : 0f;

        if (opening)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float f = curve.Evaluate(t);
            rectTransform.localScale = Vector3.LerpUnclamped(fromScale, toScale, f);
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, f);
            yield return null;
        }

        rectTransform.localScale = toScale;
        canvasGroup.alpha = toAlpha;

        if (!opening)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }

    // Immediate helpers
    public void OpenImmediate()
    {
        StopAllCoroutines();
        gameObject.SetActive(true);
        rectTransform.localScale = targetScale;
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void CloseImmediate()
    {
        StopAllCoroutines();
        rectTransform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }
}
