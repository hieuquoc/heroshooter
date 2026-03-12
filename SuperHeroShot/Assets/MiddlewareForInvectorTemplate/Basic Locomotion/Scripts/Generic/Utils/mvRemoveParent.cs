using Invector;
using UnityEngine;
using UnityEngine.Events;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("RemoveParent", iconName = "misIconRed")]
    public class mvRemoveParent : vMonoBehaviour
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Settings", order = mvToolbarOrder.SETTINGS)]
        public bool removeOnStart = true;
        public bool resetScale = true;


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Events", order = 99)]
        public UnityEvent OnRemoved;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Start()
        {
            if (removeOnStart)
                RemoveParent();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void RemoveParent()
        {
            transform.SetParent(null);

            if (resetScale)
                transform.localScale = Vector3.one;

            OnRemoved?.Invoke();
        }
    }
}