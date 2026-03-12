using Invector;
using System.Collections;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("FreeFlying", iconName = "misIconRed")]
    public class mvFreeFlyingBasic : mvFreeFlying
    {
#if MIS_FREEFLYING && INVECTOR_BASIC
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override IEnumerator Start()
        {
            yield return StartCoroutine(base.Start());
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void UpdateCameraState()
        {
            if (tpInput.cc.isStrafing)
                tpInput.ChangeCameraState(strafeCameraState, true);
            else if (tpInput.cc.isSprinting)
                tpInput.ChangeCameraState(sprintCameraState, true);
            else
                tpInput.ChangeCameraState(cameraState, true);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void Sprint(bool hasSprintInput)
        {
            bool sprintConditions =
                tpInput.cc.currentStamina > 0
                && !tpInput.cc.customAction
                && !(tpInput.cc.isStrafing && (tpInput.cc.horizontalSpeed >= 0.5 || tpInput.cc.horizontalSpeed <= -0.5 || tpInput.cc.verticalSpeed <= 0.1f));

            if (hasSprintInput && sprintConditions)
            {
                if (tpInput.cc.currentStamina > 0)
                {
                    if (tpInput.cc.useContinuousSprint)
                    {
                        if (tpInput.cc.isSprinting && multiSprintCount.origin > 0)
                        {
                            if (multiSprintCount.now < multiSprintCount.origin)
                            {
                                multiSprintCount.now++;
                                if (multiSprintCount.now >= multiSprintCount.origin)
                                    multiSprintCount.now = multiSprintCount.origin;

                                tpInput.cc.animator.CrossFadeInFixedTime("FlyingToSprint", 0.25f);

                                OnStartSprinting.Invoke();
                                return;
                            }
                        }

                        tpInput.cc.isSprinting = !tpInput.cc.isSprinting;

                        if (tpInput.cc.isSprinting)
                        {
                            multiSprintCount.now++;
                            OnStartSprinting.Invoke();
                        }
                        else
                        {
                            FinishSprint();
                        }
                    }
                    else if (!tpInput.cc.isSprinting)
                    {
                        tpInput.cc.isSprinting = true;
                        OnStartSprinting.Invoke();
                    }
                }
                else if (tpInput.cc.isSprinting)
                {
                    FinishSprint();
                }
            }
            else if (tpInput.cc.isSprinting && (!sprintConditions || (!tpInput.cc.useContinuousSprint && !hasSprintInput)))
            {
                FinishSprint();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override bool StartSprintImmediately()
        {
            if (tpInput.cc.isSprinting)
                return false;

            EnterActionState(false);

            if (!IsOnAction)
                return false;

            tpInput.cc.isSprinting = true;

            multiSprintCount.now++;
            OnStartSprinting.Invoke();

            return true;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void FinishSprint()
        {
            multiSprintCount.now = 0;

            if (!tpInput.cc.isSprinting)
                return;
            tpInput.cc.isSprinting = false;

            OnFinishSprinting.Invoke();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override bool HasWeapon()
        {
            return false;
        }
#endif
    }
}