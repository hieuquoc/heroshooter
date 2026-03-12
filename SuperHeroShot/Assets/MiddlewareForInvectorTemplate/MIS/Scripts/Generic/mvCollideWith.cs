using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("Collide With", iconName = "misIconRed")]
    public class mvCollideWith : vMonoBehaviour
    {
#if MIS
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Settings", order = 1)]
        public LayerMask targetLayerMask = 1 << MISRuntimeTagLayer.LAYER_DEFAULT;
#endif
    }
}