using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameHUD : BasePopup
{
    public const string PopupName = "InGameHUDPopup";

    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.Register(PopupName, this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
