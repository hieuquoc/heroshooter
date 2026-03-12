using Invector;
using System.Collections.Generic;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("Audio Select Player", iconName = "misIconRed")]
    public class mvAudioSelectPlayer : mvAudioPlayer
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Settings", order = 0)]
        [Space]
        public List<AudioClip> audioClipList;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void Play()
        {
            if (Source == null || audioClipList == null || audioClipList.Count == 0)
                return;

            Play(audioClipList[Random.Range(0, audioClipList.Count)], Random.Range(1f - pitchRange, 1f + pitchRange), loop, volume.now);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void Play(string audioClipName)
        {
            if (audioClipList.Count == 0)
                return;

            AudioClip clip = audioClipList.Find(x => x.name == audioClipName);

            if (clip)
                Play(clip);
        }
    }
}
