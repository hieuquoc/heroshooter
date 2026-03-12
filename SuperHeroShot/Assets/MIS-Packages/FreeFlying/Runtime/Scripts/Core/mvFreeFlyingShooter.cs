using Invector;
#if INVECTOR_SHOOTER
using Invector.vShooter;
#endif
using System.Collections;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("FreeFlying Shooter", iconName = "misIconRed")]
    public class mvFreeFlyingShooter : mvFreeFlyingMelee
    {
#if MIS && MIS_FREEFLYING && INVECTOR_SHOOTER
        // ----------------------------------------------------------------------------------------------------
        // 
        protected mvShooterMeleeInput shooterMeleeInput;
        protected vShooterManager shooterManager;

        protected bool oldUseLeftIK, oldUseRightIK;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override IEnumerator Start()
        {
            yield return StartCoroutine(base.Start());

            if (IsAvailable)
            {
                shooterMeleeInput = tpInput as mvShooterMeleeInput;

                shooterMeleeInput.onLateUpdate -= OnLateUpdate;
                shooterMeleeInput.onLateUpdate += OnLateUpdate;

                TryGetComponent(out shooterManager);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void OnLateUpdate()
        {
            if (!IsAvailable || !IsOnAction)
                return;

            if (shooterManager != null)
            {
                if (!shooterMeleeInput.IsAiming && !shooterMeleeInput.isAttacking)
                {
                    shooterManager.useLeftIK = false;
                    shooterManager.useRightIK = false;
                }
                else
                {
                    shooterManager.useLeftIK = oldUseLeftIK;
                    shooterManager.useRightIK = oldUseRightIK;
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void UpdateCameraState()
        {
            if ((tpInput as mvShooterMeleeInput).isAimingByInput)
                tpInput.ChangeCameraState(aimingCameraState, true);
            else
                base.UpdateCameraState();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override bool HasWeapon()
        {
            if (shooterManager != null)
            {
                if (shooterManager.rWeapon && shooterManager.rWeapon.gameObject.activeInHierarchy)
                    return true;
                if (shooterManager.lWeapon && shooterManager.lWeapon.gameObject.activeInHierarchy)
                    return true;
            }
             
            return base.HasWeapon();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected override void EnterActionState(bool fromGround = true)
        {
            base.EnterActionState(fromGround);

            oldUseLeftIK = shooterManager.useLeftIK;
            oldUseRightIK = shooterManager.useRightIK;

            shooterMeleeInput.SetCustomIKAdjustState("FreeFlying");
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public override void ExitActionState()
        {
            base.ExitActionState();

            shooterMeleeInput.ResetCustomIKAdjustState();

            shooterManager.useLeftIK = oldUseLeftIK;
            shooterManager.useRightIK = oldUseRightIK;
        }
#endif
    }
}