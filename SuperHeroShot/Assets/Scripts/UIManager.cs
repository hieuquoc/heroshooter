using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rescueforce
{
    public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [System.Serializable]
    public struct NamedPopup { public string name; public BasePopup popup; }

    [Tooltip("Initial popups to register (optional)")]
    public List<NamedPopup> initialPopups = new List<NamedPopup>();

    Dictionary<string, BasePopup> popups = new Dictionary<string, BasePopup>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (var np in initialPopups)
        {
            if (np.popup != null && !string.IsNullOrEmpty(np.name))
                Register(np.name, np.popup);
        }
    }

    void Start()
    {
        Open("HomePopup");
    }

    public void Register(string name, BasePopup popup)
    {
        if (string.IsNullOrEmpty(name) || popup == null) return;
        popups[name] = popup;
    }

    public void Unregister(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        popups.Remove(name);
    }

    public bool Open(string name)
    {
        if (popups.TryGetValue(name, out var p))
        {
            if(p.IsActionInProgress) return false; // prevent opening if already animating
            p.Open();
            return true;
        }
        Debug.LogWarning($"UIManager: popup '{name}' not found");
        return false;
    }

    public void OpenPopupDelay(string name, float delay)
    {
        StartCoroutine(OpenWithDelay(name, delay));
    }
    
    IEnumerator OpenWithDelay(string name, float delay)
    {
        yield return new WaitForSeconds(delay);
        Open(name);
    }

    public bool Close(string name)
    {
        if (popups.TryGetValue(name, out var p))
        {
            if(!p.gameObject.activeSelf) return false; // already closed
            if(p.IsActionInProgress) return false; // prevent closing if already animating
            p.Close();
            return true;
        }
        Debug.LogWarning($"UIManager: popup '{name}' not found");
        return false;
    }

    public bool Toggle(string name)
    {
        if (popups.TryGetValue(name, out var p))
        {
            // simple toggle based on alpha
            if (p == null) return false;
            var cg = p.GetComponent<CanvasGroup>();
            if (cg != null && cg.alpha > 0.5f) p.Close(); else p.Open();
            return true;
        }
        Debug.LogWarning($"UIManager: popup '{name}' not found");
        return false;
    }

    public bool OpenImmediate(string name)
    {
        if (popups.TryGetValue(name, out var p))
        {
            p.OpenImmediate();
            return true;
        }
        Debug.LogWarning($"UIManager: popup '{name}' not found");
        return false;
    }

    public bool CloseImmediate(string name)
    {
        if (popups.TryGetValue(name, out var p))
        {
            p.CloseImmediate();
            return true;
        }
        Debug.LogWarning($"UIManager: popup '{name}' not found");
        return false;
    }
}

}

