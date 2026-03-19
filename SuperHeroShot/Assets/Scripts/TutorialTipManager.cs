using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rescueforce
{
    public class TutorialTipManager : MonoBehaviour
    {
        public List<GameObject> tips;
        public static TutorialTipManager Instance { get; private set; }

        void OnEnable()
        {
            foreach (var tip in tips)
            {
                tip.SetActive(false);
            }
            
        }

        public void Show()
        {
            if(PlayerPrefs.GetInt("tutorial", 0) == 0)
            {
                PlayerPrefs.SetInt("tutorial", 1);
                Debug.Log("Showing tutorial tips...");
                StartCoroutine(DelayShow());
            }
        }

        IEnumerator DelayShow()
        {
            for(int i = 0; i < tips.Count; i++)
            {
                tips[i].SetActive(true);
                yield return new WaitForSeconds(3f);
                tips[i].SetActive(false);                
            }
        }
    }
}


