using Invector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace com.mobilin.games
{
    // ====================================================================================================
    // 
    // ====================================================================================================
    [vClassHeader("ParticleSystem Player", iconName = "misIconRed")]
    public class mvParticleSystemPlayer : vMonoBehaviour
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Settings", order = mvToolbarOrder.SETTINGS)]
        [Header("Life")]
        [SerializeField] protected bool enableEmitOnAwake = true;
        [SerializeField] protected bool playOnAwake = false;
        [SerializeField] protected EAfterPlayAction afterPlayAction = EAfterPlayAction.None;

        [Header("Audio")]
        [SerializeField] protected List<AudioClip> audioClipList = new();
        [SerializeField] protected bool waitForAudio = true;


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Events", order = mvToolbarOrder.EVENTS)]
        public UnityEvent OnStartPlay;
        public UnityEvent OnEndPlay;


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Debug", order = mvToolbarOrder.DEBUG)]
        [mvReadOnly] public float defaultEmitRateOverTimeMultiplier;
        [mvReadOnly] public float defaultEmitRateOverDistanceMultiplier;


        // ----------------------------------------------------------------------------------------------------
        // 
        protected ParticleSystem[] particleSystems;
        protected int particleCount;

        protected ParticleSystem.EmissionModule[] emissionModules;

        protected AudioSource audioSource;

        protected WaitForSeconds Delay100MS = new WaitForSeconds(0.1f);
        protected Coroutine coroutine = null;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Awake()
        {
            Initialize();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void OnEnable()
        {
            if (particleSystems == null)
                Initialize();

            if (playOnAwake)
                Play();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Initialize()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>();

            if (particleSystems != null)
            {
                particleCount = particleSystems.Length;
                emissionModules = new ParticleSystem.EmissionModule[particleCount];

                for (int i = 0; i < particleCount; i++)
                    emissionModules[i] = particleSystems[i].emission;

                defaultEmitRateOverTimeMultiplier = emissionModules[0].rateOverTimeMultiplier;
                defaultEmitRateOverDistanceMultiplier = emissionModules[0].rateOverDistanceMultiplier;
            }

            TryGetComponent(out audioSource);

            SetEmissionEnable(enableEmitOnAwake);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void Play()
        {
            for (int i = 0; i < particleCount; i++)
                particleSystems[i].Play();

            if (audioSource != null)
            {
                if (audioClipList.Count > 0)
                    audioSource.clip = audioClipList[Random.Range(0, audioClipList.Count - 1)];

                audioSource.Play();
            }

            OnStartPlay.Invoke();

            if (afterPlayAction != EAfterPlayAction.None)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);

                coroutine = StartCoroutine(CheckIfAlive());
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void Stop()
        {
            for (int i = 0; i < particleCount; i++)
                particleSystems[i].Stop();

            if (audioSource != null)
                audioSource?.Stop();

            OnEndPlay.Invoke();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void SetEmissionEnable(bool enable)
        {
            for (int i = 0; i < particleCount; i++)
                emissionModules[i].enabled = enable;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void SetRateOverTime(float value)
        {
            for (int i = 0; i < particleCount; i++)
                emissionModules[i].rateOverTime = value;
        }
        public virtual void SetRateOverDistance(float value)
        {
            for (int i = 0; i < particleCount; i++)
                emissionModules[i].rateOverDistance = value;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual IEnumerator CheckIfAlive()
        {
            for (int i = 0; i < particleCount; i++)
            {
                while (particleSystems[i].IsAlive())
                    yield return Delay100MS;
            }

            while (waitForAudio && audioSource != null && audioSource.isPlaying)
                yield return Delay100MS;

            coroutine = null;

            OnEndPlay.Invoke();

            if (afterPlayAction == EAfterPlayAction.Deactivate)
                gameObject.SetActive(false);
            else if (afterPlayAction == EAfterPlayAction.Destroy)
                Destroy(gameObject);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        public enum EAfterPlayAction
        { 
            None = 0,

            Deactivate,
            Destroy
        }
    }
}