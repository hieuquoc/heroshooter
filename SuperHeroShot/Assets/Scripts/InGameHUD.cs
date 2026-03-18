using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameHUD : BasePopup
{
    public const string PopupName = "InGameHUDPopup";

    public static InGameHUD Instance { get; private set; }

    public Image healthBarFill;

    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        base.Awake();
    }

    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.Register(PopupName, this);
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
}
