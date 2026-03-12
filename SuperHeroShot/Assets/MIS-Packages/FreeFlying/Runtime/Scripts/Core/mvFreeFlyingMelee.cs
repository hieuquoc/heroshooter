using Invector;
#if INVECTOR_MELEE
using Invector.vMelee;
#endif
using System.Collections;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("FreeFlying Melee", iconName = "misIconRed")]
    public class mvFreeFlyingMelee : mvFreeFlyingBasic
    {
#if MIS && MIS_FREEFLYING && INVECTOR_MELEE
        // ----------------------------------------------------------------------------------------------------
        // 
        protected mvMeleeCombatInput meleeCombatInput;
        protected vMeleeManager meleeManager;
        protected vDrawHideMeleeWeapons drawHideWeapons;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override IEnumerator Start()
        {
            yield return StartCoroutine(base.Start());

            if (IsAvailable)
            {
                meleeCombatInput = tpInput as mvMeleeCombatInput;
                TryGetComponent(out meleeManager);
                TryGetComponent(out drawHideWeapons);
            }
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
                            if (drawHideWeapons && autoDrawHideWeapon)
                                drawHideWeapons.HideWeapons(true);

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
                        if (drawHideWeapons && autoDrawHideWeapon)
                            drawHideWeapons.HideWeapons(true);

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
            if (base.StartSprintImmediately())
            {
                if (drawHideWeapons && autoDrawHideWeapon)
                    drawHideWeapons.HideWeapons(true);

                return true;
            }

            return false;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void FinishSprint()
        {
            if (drawHideWeapons && autoDrawHideWeapon)
                drawHideWeapons.ReturnToLastState(true);

            base.FinishSprint();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override bool HasWeapon()
        {
            if (meleeManager != null)
            {
                if (meleeManager.rightWeapon && meleeManager.rightWeapon.gameObject.activeInHierarchy)
                    return true;
                if (meleeManager.leftWeapon && meleeManager.leftWeapon.gameObject.activeInHierarchy)
                    return true;
            }

            return false;
        }
#endif
    }
}