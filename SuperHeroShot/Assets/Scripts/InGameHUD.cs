using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace rescueforce
{
    public class InGameHUD : BasePopup
{
    public const string PopupName = "InGameHUDPopup";

    public static InGameHUD Instance { get; private set; }
    public Image hurtOverlay;

    public Image healthBarFill;
    public TutorialTipManager tutorialTipManager;
    [SerializeField] private TextMeshProUGUI enemyCountText;

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        base.Awake();
        if (hurtOverlay != null)
        {
            Color col = hurtOverlay.color;
            col.a = 0f;
            hurtOverlay.color = col;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.Register(PopupName, this);
    }

    public override void Open()
    {
        base.Open();
        if (tutorialTipManager != null)
        {
            tutorialTipManager.Show();
        }
    }

    Coroutine _hurtRoutine;
    [SerializeField] private float hurtDuration = 0.4f;
    [SerializeField] private float hurtMaxAlpha = 0.6f;

    /// <summary>
    /// Show a brief hurt flash. `amount` can scale intensity.
    /// </summary>
    public void ShowHurt(float amount)
    {
        if (hurtOverlay == null) return;
        float alpha = Mathf.Clamp01((amount / 20f) * hurtMaxAlpha);
        if (alpha <= 0f) alpha = hurtMaxAlpha * 0.4f;
        if (_hurtRoutine != null) StopCoroutine(_hurtRoutine);
        _hurtRoutine = StartCoroutine(HurtFlash(alpha, hurtDuration));
    }

    IEnumerator HurtFlash(float startAlpha, float duration)
    {
        float t = 0f;
        Color baseCol = hurtOverlay.color;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = 1f - (t / duration);
            float a = startAlpha * p;
            hurtOverlay.color = new Color(baseCol.r, baseCol.g, baseCol.b, a);
            yield return null;
        }
        hurtOverlay.color = new Color(baseCol.r, baseCol.g, baseCol.b, 0f);
        _hurtRoutine = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHealthBar(float currentHp, float maxHp)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Clamp01(currentHp / maxHp);
        }
    }

    public void UpdateEnemyCount(int count, int max)
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = $"{count} / {max}";
        }
    }

    
}

}

