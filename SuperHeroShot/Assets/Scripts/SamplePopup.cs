using UnityEngine;

public class SamplePopup : MonoBehaviour
{
    public BasePopup popup;

    public void OpenPopup()
    {
        if (popup != null) popup.Open();
    }

    public void ClosePopup()
    {
        if (popup != null) popup.Close();
    }
}
