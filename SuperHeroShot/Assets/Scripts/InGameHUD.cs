using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameHUD : BasePopup
{
    public const string PopupName = "InGameHUDPopup";

     protected override void Awake()
    {
        base.Awake();
        UIManager.Instance.Register(PopupName, this);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
