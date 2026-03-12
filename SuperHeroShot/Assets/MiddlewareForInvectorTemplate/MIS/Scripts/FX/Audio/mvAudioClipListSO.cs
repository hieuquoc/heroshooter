using System.Collections.Generic;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [CreateAssetMenu(menuName = "MIS/FX/Audio/New AudioClipListSO", fileName = "AudioClipListSO@")]
    public class mvAudioClipListSO : mvScriptableObject
    {
        public List<AudioClip> list;
    }
}