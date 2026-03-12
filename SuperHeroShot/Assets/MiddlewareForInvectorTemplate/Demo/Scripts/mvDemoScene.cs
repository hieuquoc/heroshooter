using Invector;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("Demo Scene", iconName = "misIconRed")]
    public class mvDemoScene : vMonoBehaviour
    {
#if MIS
        [Header("Scene")]
        [SerializeField] protected GenericInput quitInput = new("Escape", "", "");
        [SerializeField] protected GenericInput restartInput = new("Insert", "", "");
        [SerializeField] protected GenericInput debugInput = new("Delete", "", "");

        [Header("Time Scale")]
        [SerializeField, Range(0.01f, 1f)] protected float timeScale = 0.2f;
        [SerializeField] protected GenericInput setTimeScaleInput = new("Home", "", "");
        [SerializeField] protected GenericInput resetTimeScaleInput = new("End", "", "");

        [Header("Character")]
        [vHelpBox("Freeze Animator Input would be useful during Shooter IK Adjust")]
        [SerializeField] protected GenericInput freezeAnimatorInput = new("Backspace", "", "");
        [SerializeField] protected GenericInput killInput = new("Home", "", "");
        public vHealthController character;


        protected bool isFreezedAnimator;
        protected float lastAnimatorSpeed;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void Update()
        {
            // Quit
            if (quitInput.useInput && quitInput.GetButtonDown())
            {
                if (Application.isPlaying)
                {
#if UNITY_EDITOR
                    EditorApplication.ExitPlaymode();
#else
                    Application.Quit();
#endif
                }
            }


            // Restart scene
            if (restartInput.useInput && restartInput.GetButtonDown())
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);


            // Debug break
            if (debugInput.useInput && debugInput.GetButtonDown())
                Debug.Break();


            // Time scale
            if (setTimeScaleInput.useInput && setTimeScaleInput.GetButtonDown())
                Time.timeScale = timeScale;
            else if (resetTimeScaleInput.useInput && resetTimeScaleInput.GetButtonDown())
                Time.timeScale = 1f;


            if (character != null)
            {
                // Freeze Animator
                if (freezeAnimatorInput.useInput && freezeAnimatorInput.GetButtonDown())
                {
                    if (character.gameObject.TryGetComponent(out Animator animator))
                    {
                        if (isFreezedAnimator)
                        {
                            isFreezedAnimator = false;
                            animator.speed = lastAnimatorSpeed;
                        }
                        else
                        {
                            isFreezedAnimator = true;
                            lastAnimatorSpeed = animator.speed;
                            animator.speed = 0;
                        }
                    }
                }


                // Kill a character
                if (killInput.useInput && killInput.GetButtonDown())
                    character.TakeDamage(new vDamage(1000000));
            }
        }
#endif
    }
}