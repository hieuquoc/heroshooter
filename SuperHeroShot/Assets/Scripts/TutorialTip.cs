using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rescueforce
{
    public class TutorialTip : MonoBehaviour
    {
        [SerializeField] private int showIndex;
        [SerializeField] private GameObject tipObject;
        private bool isShown = false;
        private bool isInitialized = false;

        public int ShowIndex => showIndex;

        public bool ShowTip()
        {
            if (!isInitialized)
            {
                isShown = PlayerPrefs.GetInt("tutorial", 0) >= showIndex; // check if already shown
                isInitialized = true;
            }
            if (isShown) return false; // already shown
            tipObject.SetActive(true);
            PlayerPrefs.SetInt("tutorial", showIndex++);
            isShown = true;
            return true;
        }

        public void HideTip()
        {
            if(!isShown) return;
            if(tipObject.activeSelf)
            tipObject.SetActive(false);
        }
    }
}


