using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public abstract class mvAudioPlayer : vMonoBehaviour
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Settings", order = 0)]
        public AudioSource Source;
        public mvFloatOrigin volume = new(1f);
        [Range(0f, 1f)] public float pitchRange = 0.2f;
        public bool loop = false;


        // ----------------------------------------------------------------------------------------------------
        // 
        public bool IsPlaying => Source != null && Source.isPlaying;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public abstract void Play();

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Enable()
        {
            ResetVolume();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void Play(AudioClip clip)
        {
            Play(clip, Random.Range(1f - pitchRange, 1f + pitchRange), loop, volume.now);
        }
        public virtual void Play(AudioClip clip, float pitchRange = 0.2f, float volume = 1f)
        {
            Play(clip, Random.Range(1f - pitchRange, 1f + pitchRange), loop, volume);
        }
        protected virtual void Play(AudioClip clip, float pitch, bool loop, float volume)
        {
            Source.clip = clip;
            Source.pitch = pitch;
            Source.loop = loop;
            Source.volume = volume;
            Source.Play();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void StopAudio()
        {
            if (Source == null)
                return;

            Source.Stop();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ResetVolume()
        {
            volume.now = volume.origin;
        }
    }
}