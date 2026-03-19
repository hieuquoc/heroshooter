using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace rescueforce
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SplashScreenController : MonoBehaviour
    {
        [Tooltip("CanvasGroup used to fade the whole splash UI. If null, will get component on the same GameObject.")]
        public CanvasGroup canvasGroup;

        public float fadeInDuration = 0.8f;
        public float holdDuration = 1.2f;
        public float fadeOutDuration = 0.8f;
        [Header("Start Options")]
        [Tooltip("If true the splash starts fully visible (no fade-in) and will hold for `startHoldDuration` before fading out.")]
        public bool startVisible = true;
        [Tooltip("Hold time in seconds when `startVisible` is true.")]
        public float startHoldDuration = 1f;

        [Tooltip("If true, load the specified scene after fading out.")]
        public bool loadNextScene = false;
        public string nextSceneName = "";

        void Reset()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        IEnumerator Start()
        {
            if (canvasGroup == null) yield break;

            if (startVisible)
            {
                // start fully visible, hold, then fade out
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                yield return new WaitForSeconds(startHoldDuration);

                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));
            }
            else
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                // Fade in
                yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;

                // Hold
                yield return new WaitForSeconds(holdDuration);

                // Fade out
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));
            }

            // Finished
            if (loadNextScene && !string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        IEnumerator Fade(float from, float to, float duration)
        {
            float t = 0f;
            if (duration <= 0f) { canvasGroup.alpha = to; yield break; }
            while (t < duration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
                yield return null;
            }
            canvasGroup.alpha = to;
        }
    }

}

