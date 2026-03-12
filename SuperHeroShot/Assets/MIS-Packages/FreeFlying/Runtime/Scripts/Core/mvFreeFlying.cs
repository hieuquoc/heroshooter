#pragma warning disable 0414

using Invector;
using Invector.vCharacterController;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public abstract class mvFreeFlying : vMonoBehaviour, IMISColliderFilter
    {
#if MIS_FREEFLYING && INVECTOR_BASIC
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Input", order = 0)]
        [Header("Flying Toggle")]
        public GenericInput enterFlyingInput = new GenericInput("Space", "", "");
        public GenericInput exitFlyingInput = new GenericInput("Space", "", "");
        [vHelpBox("If this input is used, you should hold this button down when you press flyingInput")]
        public GenericInput flyingSubInput = new GenericInput("LeftControl", "", "");

        [Header("Movement")]
        [vHelpBox("Flies upward only when there is no movement input")]
        public GenericInput straightUpInput = new GenericInput("Space", "", "");
        [vHelpBox("Flies downward only when there is no movement input")]
        public GenericInput straightDownInput = new GenericInput("LeftControl", "", "");

        [Header("Movement Mode")]
        [vHelpBox("Toggles Strafe mode")]
        public GenericInput strafeInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");
        [vHelpBox("If useSubInput is enabled, immediateSprintInput key is also enabled.")]
        public GenericInput immediateSprintInput = new GenericInput("LeftControl", "", "");
        [vHelpBox("Flies in Sprint speed")]
        public GenericInput sprintInput = new GenericInput("LeftShift", "", "");    // while holding down a flyingSubInput key
        [vHelpBox("Flies in Slow speed")]
        public GenericInput slowInput = new GenericInput("LeftAlt", "", "");  // while holding down

        [Header("Escape")]
        [vHelpBox("Flying Escape action")]
        public GenericInput escapeInput = new GenericInput("Q", "", "");

        [Header("Sprint Roll")]
        [vHelpBox("Sprint Flying Roll action")]
        public GenericInput sprintRollLeftInput = new GenericInput("A", "", "");
        public GenericInput sprintRollRightInput = new GenericInput("D", "", "");
        public GenericInput sprintRollUpInput = new GenericInput("W", "", "");

        [Header("Camera")]
        public string cameraState = "FreeFlying";
        public string strafeCameraState = "FreeFlyingStrafing";
        public string sprintCameraState = "FreeFlyingSprint";
        public string aimingCameraState = "FreeFlyingAiming";


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Flying", order = 1)]
        [Header("Flying Setting")]
        public LayerMask moveStopLayerMask = 1 << MISRuntimeTagLayer.LAYER_DEFAULT;
        public float maxMoveStopCheckDistance = 1.5f;
        float moveStopWeight;

        [Space(10)]
        [vHelpBox("Flying starts at a distance higher than min value. Cannot fly upward than max value.")]
        public mvFloatMinMax flyingGroundDistance = new mvFloatMinMax(0.15f, 100f);

        [Space(10)]
        [vHelpBox("If true, automatically hide weapons on Flying Sprint then redraw when finished.")]
        public bool autoDrawHideWeapon = true;

        [Space(10)]
        [vHelpBox("Sets the CapsuleCollider Center on the Z axis on Flying Sprint.")]
        public Vector3 capsuleColliderCenterOnSprint = new Vector3(0f, 1.1f, 0f);

        [Header("Jump Up for Flying")]
        public float flyJumpUpVelocity = 30f;

        [Header("Landing")]
        [vHelpBox("If true, Flying will be canceled when the character collides obstacle.")]
        public bool autoCancelFlyingOnCollision = false;
        public LayerMask obstacleLayerMask = 1 << MISRuntimeTagLayer.LAYER_DEFAULT;
        [Tooltip("If you want to increase this value, you should also increase groundDetectionDistance of VThirdPersonMotor.cs")]
        public float minHardLandingGroundDistance = 5f;
        public float hardLandingForce = 150f;

        [Header("Flying Escape")]
        public bool useFlyingEscapeRootMotion = true;
        public float flyingEscapeSpeedMultiplier = 2f;
        Vector3 flyingEscapeDirection;

        [Header("Flying SprintRoll")]
        public bool useFlyingSprintRollRootMotion = true;
        public float flyingSprintRollSpeedMultiplier = 2f;


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Movement", order = 2)]
        [vHelpBox("The higher, the faster the character stops.")]
        public float stopMoveSpeed = 18f;

        [Tooltip("Speed in Free Flying mode")]
        public mvFreeFlyingSpeed freeSpeed = new mvFreeFlyingSpeed(10f, 6f, 3f, 4f);
        [Tooltip("Speed in Free Sprint Flying mode")]
        public mvFreeFlyingSpeed freeSprintSpeed = new mvFreeFlyingSpeed(15f, 10f, 4f, 50f);
        [Tooltip("Speed in Strafe Flying mode")]
        public mvFreeFlyingSpeed strafeSpeed = new mvFreeFlyingSpeed(10f, 5f, 15f, 10f);
        [Tooltip("Speed in Strafe Sprint Flying mode")]
        public mvFreeFlyingSpeed strafeSprintSpeed = new mvFreeFlyingSpeed(15f, 8f, 15f, 30f);

        public mvFloatMinMax rotationSpeed = new mvFloatMinMax(3f, 6f);    // min: Sprint, max: Free

        [Header("Slow Mode")]
        [vHelpBox("The lower, the more the character loose speed when holding down SlowInput key.")]
        public float slowModeMultiplier = 0.4f;

        [Header("Multi-Sprint")]
        [vHelpBox("The maximum Multi-Sprint count")]
        public mvIntOrigin multiSprintCount = new mvIntOrigin(3);

        [Space(10)]
        [vHelpBox("The higher, the faster on next sprint")]
        public float multiSprintSpeedMultiplier = 1.2f;

        [Header("Rolling")]
        public float resetRollingSpeed = 2f;
        public float rollingSpeed = 5f;
        public float rollingThreshold = 0.15f;
        public mvFloatOrigin rolling = new mvFloatOrigin(45f, 0f);

        [Header("Idle Animation")]
        [vHelpBox("If the Origin is greater than 0, a Random Flying Idle animation is executed when the idle state lasts as long as the Origin time.")]
        public mvFloatOrigin randomIdleTime = new mvFloatOrigin(5f);
        public enum FlyingIdleType
        {
            HandsOnWaist = 0,
            CrossArms,
            HasWeapon
        };
        FlyingIdleType flyingIdle;
        FlyingIdleType FlyingIdle
        {
            get
            {
                return flyingIdle;
            }
            set
            {
                if (flyingIdle != value)
                {
                    flyingIdle = value;
                    tpInput.cc.animator.SetInteger(idleRandomStateHash, (int)flyingIdle);

                    switch (flyingIdle)
                    {
                        case FlyingIdleType.HandsOnWaist:
                            tpInput.cc.animator.CrossFadeInFixedTime("FlyingIdle_HandsOnWaist", 0.25f);
                            break;
                        case FlyingIdleType.CrossArms:
                            tpInput.cc.animator.CrossFadeInFixedTime("FlyingIdle_CrossArms", 0.25f);
                            break;
                        case FlyingIdleType.HasWeapon:
                            tpInput.cc.animator.CrossFadeInFixedTime("FlyingIdle_HasWeapon", 0.25f);
                            break;
                    }
                }
            }
        }


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Stamina", order = 3)]
        [vHelpBox("Stamina consumption in Free Flying mode. If 0, no stamina is required for Flying")]
        public float stamina = 2f;

        [Space(10)]
        [vHelpBox("Stamina consumption in Sprint Flying mode. If 0, no stamina is required for Sprint Flying. IMPORTANT: sprintStamina in vThirdPersonMotor will be consumed at the same time.")]
        public float sprintStamina = 0f;

        [Space(10)]
        [vHelpBox("Stamina consumption in Flying Escape. If 0, no stamina is required for Flying Escape")]
        public float escapeStamina = 10f;

        [Space(10)]
        [vHelpBox("Stamina consumption in Flying Sprint Roll. If 0, no stamina is required for Flying Sprint Roll")]
        public float sprintRollStamina = 10f;

        [Space(10)]
        public float staminaRecoveryDelay = 2.5f;


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Damage", order = 4)]
        [vHelpBox("Ignore all damage while flying, include Damage that ignore defence")]
        public bool noDamageOnAction = false;

        [Space(10)]
        [vHelpBox("Ignore damage that needs to activate ragdoll")]
        public bool noRagdollOnAction = false;

        [Space(10)]
        [vHelpBox("Ignore all damage while Flying Escape, include Damage that ignore defence")]
        public bool noDamageOnEscape = true;

        [Space(10)]
        [vHelpBox("Ignore damage that needs to activate ragdoll")]
        public bool noRagdolOnEscape = true;

        [Space(10)]
        [vHelpBox("Ignore all damage while SprintRoll, include Damage that ignore defence")]
        public bool noDamageOnSprintRoll = true;

        [Space(10)]
        [vHelpBox("Ignore damage that needs to activate ragdoll")]
        public bool noRagdollOnSprintRoll = true;

        [Space(10)]
        [vHelpBox("When a collision occurs during Sprint Flying, if the CrashVelocity and CrashAngle conditions are met, it will switch to Ragdoll.")]
        public bool ragdollOnSprintCrash = true;
        public mvFloatOrigin crashVelocity = new mvFloatOrigin(3f);
        public mvFloatMinMax crashAngle = new mvFloatMinMax(40f, 90f);


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Effect", order = 5)]
        public GameObject airTrailsObj;
        public GameObject hardLandingStartFxPrefab;
        public GameObject hardLandingCraterFxPrefab;


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("ChainedAction", order = 98)]
        [mvReadOnly][SerializeField] string chainedAction = "Chained-Actions are provided as an option";
#if MIS_AIRDASH
        public bool allowFromAirDash = true;
#endif
#if MIS_CARRIDER_EVP || MIS_CARRIDER_RCC || MIS_HELICOPTER || MIS_ROWINGBOAT
        public bool allowFromVehicleRider = true;
#endif
#if MIS_CRAWLING
        public bool allowFromCrawling = true;
#endif
#if MIS_GRAPPLINGHOOK
        public bool allowFromGrapplingHook = true;
#endif
#if MIS_GRAPPLINGROPE
        public bool allowFromGrapplingRope = true;
#endif
#if MIS_GROUNDDASH
        public bool allowFromGroundDash = true;
#endif
#if MIS_MOTORCYCLE
        public bool allowFromMotorcycle = true;
#endif
#if MIS_WALLRUN
        public bool allowFromWallRun = true;
#endif

#if MIS_INVECTOR_FREECLIMB
        public bool allowFromFreeClimb = true;
#endif
#if MIS_INVECTOR_SHOOTERCOVER
        public bool allowFromShooterCover = true;
#endif


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Events", order = 99)]
        public UnityEvent OnStartActionOnGround;
        public UnityEvent OnStartActionOnAir;
        public UnityEvent OnFinishAction;
        public UnityEvent OnStartSprinting;
        public UnityEvent OnFinishSprinting;
        public UnityEvent OnStartLanding;
        public UnityEvent OnFinishLanding;

        public UnityEvent OnStartEscape;
        public UnityEvent OnFinshEscape;
        public UnityEvent OnStartSprintRoll;
        public UnityEvent OnFinshSprintRoll;


        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Debug", order = 100)]
        [SerializeField] protected bool debugMode = false;
        [mvReadOnly][SerializeField] protected bool isAvailable;
        [mvReadOnly][SerializeField] protected bool isOnAction;
        [mvReadOnly][SerializeField] protected float inputSqrt, inputSmoothSqrt;
        [mvReadOnly][SerializeField] protected float currentMoveSpeed, currentRotationSpeed, currentMaxRollingAngle;
        [mvReadOnly][SerializeField] protected float currentAngle, targetAngle, deltaAngle;
        [mvReadOnly][SerializeField] protected float currentRollingSensitivity;
        [mvReadOnly][SerializeField] protected float rollingProportion;
        [mvReadOnly][SerializeField] protected float crashedAngle;


        // ----------------------------------------------------------------------------------------------------
        // if true, it means this action is not blocked and can be used
        public virtual bool IsAvailable
        {
            get => isAvailable;
            set => isAvailable = value;
        }

        // ----------------------------------------------------------------------------------------------------
        // if true, it means this action is currently being used
        public bool IsOnAction
        {
            get => isOnAction;
            set => isOnAction = value;
        }


        // ----------------------------------------------------------------------------------------------------
        // 
        protected mvThirdPersonInput tpInput;
        protected Rigidbody rb;
        vRagdoll ragdoll;

        Transform tr;
        protected float upDownInput, upDownInputSmooth, upDownInputSmoothAbs;
        protected float originalMoveSpeed;
        protected float originalRotationSpeed;

        Vector3 targetDirection;
        Quaternion targetRotation;
        internal float horizontalDelta, verticalDelta;

        internal bool lockInput = false;
        internal bool lockMovement = false;
        internal bool lockRotation = false;

        internal Collider[] colliders;
        protected mvMISSphereCast groundCaster;


        // ----------------------------------------------------------------------------------------------------
        // Animator
        int flyingStateHash = Animator.StringToHash("FlyingState");
        int rollingMagnitudeHash = Animator.StringToHash("RollingMagnitude");
        int idleRandomStateHash = Animator.StringToHash("IdleRandom");

        // ----------------------------------------------------------------------------------------------------
        // 
        int flyingState;
        public virtual int FlyingState
        {
            get => flyingState;
            set
            {
                if (flyingState != value)
                {
                    flyingState = value;
                    tpInput.cc.animator.SetInteger(flyingStateHash, value);
                }
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        Vector3 AimDirection
        {
            get => tpInput.cameraMain.transform.forward;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        RaycastHit[] forwardHits = new RaycastHit[1];
        Vector3 ForwardCollisionCheckPivot
        {
            get => tr.TransformPoint(tr.up * tpInput.cc._capsuleCollider.height);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        bool HasInput
        {
            get
            {
                if (tpInput.cc.isStrafing)
                {
                    if (inputSqrt < 0.1f)
                        return false;
                }
                else
                {
                    if (inputSqrt < 0.1f)
                        return false;
                }

                return true;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        bool HasSmoothInput
        {
            get
            {
                if (tpInput.cc.isStrafing)
                {
                    if (inputSqrt < 0.1f)
                        return false;
                }
                else
                {
                    if (inputSmoothSqrt < 0.1f)
                        return false;
                }

                return true;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        bool HasUpDownInput
        {
            get => upDownInput > 0.1f || upDownInput < -0.1f;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        bool HasUpDownSmoothInput
        {
            get => upDownInputSmoothAbs > 0.1f;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        bool isSlowMode = false;
        bool HasSlowInput
        {
            get => (slowInput.useInput && slowInput.GetButton()) || isSlowMode;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        bool isFlyingEscape, prevFlyingEscape;
        public bool IsFlyingEscape
        {
            get
            {
                isFlyingEscape = tpInput.cc.IsAnimatorTag("IsFlyingEscape");

                if (isFlyingEscape != prevFlyingEscape)
                {
                    prevFlyingEscape = isFlyingEscape;

                    if (isFlyingEscape)
                    {
                        OnStartEscape.Invoke();
                    }
                    else
                    {
                        SetCapsuleCollider();

                        OnFinshEscape.Invoke();
                    }
                }

                return isFlyingEscape;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        bool isFlyingSprintRoll, prevFlyingSprintRoll;
        public bool IsFlyingSprintRoll
        {
            get
            {
                isFlyingSprintRoll = tpInput.cc.IsAnimatorTag("IsFlyingSprintRoll");

                if (isFlyingSprintRoll != prevFlyingSprintRoll)
                {
                    prevFlyingSprintRoll = isFlyingSprintRoll;

                    if (isFlyingSprintRoll)
                    {
                        OnStartSprintRoll.Invoke();
                    }
                    else
                    {
                        SetCapsuleCollider();

                        OnFinshSprintRoll.Invoke();
                    }
                }

                return isFlyingSprintRoll;
            }
        }


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Awake()
        {
            // IMISColliderFilter
            colliders = GetComponentsInChildren<Collider>();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            tr = transform;

            if (TryGetComponent(out tpInput) && TryGetComponent(out rb))
            {
                TryGetComponent(out ragdoll);

                originalMoveSpeed = tpInput.cc.moveSpeed;
                originalRotationSpeed = tpInput.cc.freeSpeed.rotationSpeed;

                if (airTrailsObj)
                    airTrailsObj.SetActive(false);

                groundCaster = new mvMISSphereCast();
                groundCaster.useCast = true;
                groundCaster.backOff = 0.2f;
                groundCaster.radius = 0.2f;
                groundCaster.maxDistance = Mathf.Infinity;

                IsAvailable = true;
            }
            else
            {
                IsAvailable = false;
                enabled = false;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void FixedUpdate()
        {
            if (!IsAvailable || !IsOnAction)
                return;

            if (IsFlyingEscape)
            {
                UpdateEscapeMovement();
                return;
            }

            if (IsFlyingSprintRoll)
            {
                UpdateSprintRollMovement();
                return;
            }

            UpdateRotation();
            UpdateMovement();
            UpdateHardLanding();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void Update()
        {
            if (!IsAvailable || lockInput || Time.timeScale == 0)
                return;

            if (ImmediateSprintToggleInput())
                return;

            ToggleFlyingInput();

            if (!IsOnAction)
                return;

            UpdateCameraState();

            if (!FlyingCondition() || !StaminaConsumption())
            {
                ExitActionState();
                return;
            }

            TriggerRandomIdle();

            MoveInput();
            StrafeInput();
            SprintInput();
            EscapeInput();
            SprintRollInput();
        }

        // ----------------------------------------------------------------------------------------------------
        // IMISColliderFilter
        // ----------------------------------------------------------------------------------------------------
        public bool FilterCollider(Collider other)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].Equals(other))
                    return true;
            }

            return false;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual bool FlyingCondition()
        {
            return
                tpInput.cc.currentHealth > 0
                && tpInput.cc.currentStamina >= stamina
#if MIS_AIRDASH
                && (tpInput.cc.IsAirDashOnAction ? (allowFromAirDash ? true : false) : true)
#endif
#if MIS_CARRIDER_EVP || MIS_CARRIDER_RCC || MIS_HELICOPTER || MIS_ROWINGBOAT
                && (tpInput.cc.IsVehicleRiderOnAction ? (allowFromVehicleRider ? true : false) : true)
#endif
#if MIS_CRAWLING
                && (tpInput.cc.IsCrawlingOnAction ? (allowFromCrawling ? true : false) : true)
#endif
#if MIS_GRAPPLINGHOOK
                && (tpInput.cc.IsGrapplingHookOnAction ? (allowFromGrapplingHook ? true : false) : true)
#endif
#if MIS_GRAPPLINGROPE
                && ((tpInput.cc.IsGrapplingRopeOnAction || tpInput.cc.IsGrapplingRopeOnMoveAction) ? (allowFromGrapplingRope ? true : false) : true)
#endif
#if MIS_GROUNDDASH
                && (tpInput.cc.IsGroundDashOnAction ? (allowFromGroundDash ? true : false) : true)
#endif
#if MIS_LEDGECLIMB1 || MIS_LEDGECLIMB2
                && !tpInput.cc.IsLedgeClimbOnAction
#endif
#if MIS_MOTORCYCLE
                && (tpInput.cc.IsRiderOnAction ? (allowFromMotorcycle ? true : false) : true)
#endif
#if MIS_SOFTFLYING
                && !tpInput.cc.IsSoftFlyingOnAction
#endif
#if MIS_SWIMMING
                && !tpInput.cc.IsSwimOnAction
#endif
#if MIS_WALLRUN
                && (tpInput.cc.IsWallRunOnAction ? (allowFromWallRun ? true : false) : true)
#endif
#if MIS_WATERDASH
                && !tpInput.cc.IsWaterDashOnAction
#endif

#if MIS_INVECTOR_FREECLIMB
                && (tpInput.cc.IsVFreeClimbOnAction ? (allowFromFreeClimb ? true : false) : true)
#endif
#if MIS_INVECTOR_PUSH
                && !tpInput.cc.IsVPushOnAction
#endif
#if MIS_INVECTOR_SHOOTERCOVER
                && (tpInput.cc.IsVShooterCoverOnAction ? (allowFromShooterCover ? true : false) : true)
#endif
#if MIS_INVECTOR_ZIPLINE
                && !tpInput.cc.IsVZiplineOnAction   // This Chained-Action is not possible because inExitZipline of vZipline is not accessible
#endif
                ;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void CheckChainedAction()
        {
#if MIS_AIRDASH
            if (tpInput.cc.IsAirDashOnAction && allowFromAirDash)
                tpInput.cc.misAirDash.ExitActionState();
#endif
#if MIS_CARRIDER_EVP || MIS_CARRIDER_RCC || MIS_HELICOPTER || MIS_ROWINGBOAT
            if (tpInput.cc.IsVehicleRiderOnAction && allowFromVehicleRider)
                tpInput.cc.misVehicleRider.Interrupt();
#endif
#if MIS_CRAWLING
            if (tpInput.cc.IsCrawlingOnAction && allowFromCrawling)
                tpInput.cc.misCrawling.Interrupt();
#endif
#if MIS_GRAPPLINGHOOK
            if (tpInput.cc.IsGrapplingHookOnAction && allowFromGrapplingHook)
                tpInput.cc.misGrapplingHook.Interrupt();
#endif
#if MIS_GRAPPLINGROPE
            if (tpInput.cc.IsGrapplingRopeOnAction && allowFromGrapplingRope)
                tpInput.cc.misGrapplingRope.Interrupt();
#endif
#if MIS_GROUNDDASH
            if (tpInput.cc.IsGroundDashOnAction && allowFromGroundDash)
                tpInput.cc.misGroundDash.ExitActionState();
#endif
#if MIS_MOTORCYCLE
            if (tpInput.cc.IsRiderOnAction && allowFromMotorcycle)
                tpInput.cc.misRider.ExitByForce(false);
#endif
#if MIS_WALLRUN
            if (tpInput.cc.IsWallRunOnAction && allowFromWallRun)
                tpInput.cc.misWallRun.ExitActionState();
#endif

#if MIS_INVECTOR_FREECLIMB
            if (tpInput.cc.IsVFreeClimbOnAction && allowFromFreeClimb)
                tpInput.cc.vmisFreeClimb.Interrupt();
#endif
#if MIS_INVECTOR_SHOOTERCOVER
            if (tpInput.cc.IsVShooterCoverOnAction && allowFromShooterCover)
                tpInput.cc.vmisShooterCover.ExitActionState(false);
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual bool HardLandingCondition()
        {
            UpdateGroundDetection();

            return
                tpInput.cc.groundDistance > tpInput.cc.groundMinDistance
                && (groundCaster.isDetected && groundCaster.distance >= minHardLandingGroundDistance)
                ;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void ToggleFlyingInput()
        {
            if ((flyingSubInput.useInput && flyingSubInput.GetButton() && enterFlyingInput.GetButtonDown()) ||
                (!flyingSubInput.useInput && enterFlyingInput.GetButtonDown()))
            {
                if (!IsOnAction)
                {
                    EnterActionState(tpInput.cc.groundDistance <= flyingGroundDistance.min);
                    return;
                }
            }

            if ((flyingSubInput.useInput && flyingSubInput.GetButton() && exitFlyingInput.GetButtonDown()) ||
                (!flyingSubInput.useInput && exitFlyingInput.GetButtonDown()))
            {
                if (IsOnAction && HardLandingCondition())
                    EnterHardLandingState();
                else
                    ExitActionState();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected abstract void UpdateCameraState();

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void StrafeInput()
        {
            if (strafeInput.useInput && strafeInput.GetButtonDown())
                tpInput.cc.Strafe();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual bool ImmediateSprintFlyingStartCondition()
        {
            return
                !tpInput.cc.customAction
#if MIS_AIRDASH
                && (tpInput.cc.IsAirDashOnAction ? (allowFromAirDash ? true : false) : true)
#endif
#if MIS_GROUNDDASH
                && (tpInput.cc.IsGroundDashOnAction ? (allowFromGroundDash ? true : false) : true)
#endif
#if MIS_CRAWLING
                && (tpInput.cc.IsCrawlingOnAction ? (allowFromCrawling ? true : false) : true)
#endif
#if MIS_GRAPPLINGHOOK
                && (tpInput.cc.IsGrapplingHookOnAction ? (allowFromGrapplingHook ? true : false) : true)
#endif
#if MIS_GRAPPLINGROPE
                && ((tpInput.cc.IsGrapplingRopeOnAction || tpInput.cc.IsGrapplingRopeOnMoveAction) ? (allowFromGrapplingRope ? true : false) : true)
#endif
#if MIS_MOTORCYCLE
                && (tpInput.cc.IsRiderOnAction ? (allowFromMotorcycle ? true : false) : true)
#endif
#if MIS_SOFTFLYING
                && !tpInput.cc.IsSoftFlyingOnAction
#endif
#if MIS_SWIMMING
                && !tpInput.cc.IsSwimOnAction
#endif
#if MIS_WALLRUN
                && (tpInput.cc.IsWallRunOnAction ? (allowFromWallRun ? true : false) : true)
#endif
#if MIS_WATERDASH
                && !tpInput.cc.IsWaterDashOnAction
#endif

#if MIS_INVECTOR_SHOOTERCOVER
                && (tpInput.cc.IsVShooterCoverOnAction ? (allowFromShooterCover ? true : false) : true)
#endif
#if MIS_INVECTOR_ZIPLINE
                && !tpInput.cc.IsVZiplineOnAction   // This Chained-Action is not possible because inExitZipline of vZipline is not accessible
#endif
                ;
        }

        // ----------------------------------------------------------------------------------------------------
        // When this method returns true, SprintInput() should not be called in this same frame.
        // ----------------------------------------------------------------------------------------------------
        protected virtual bool ImmediateSprintToggleInput()
        {
            if (((flyingSubInput.useInput && flyingSubInput.GetButton() && sprintInput.GetButtonDown()) ||
                (!flyingSubInput.useInput && immediateSprintInput.GetButtonDown())))
            {
                if (IsOnAction)
                {
                    FinishSprint();
                    return true;
                }
                else
                {
                    if (tpInput.cc.currentStamina <= 0
                        || (tpInput.cc.currentStamina < tpInput.cc.sprintStamina + sprintStamina)
                        || tpInput.cc.customAction)
                        return false;

                    if (/*flyingSubInput.GetButton() && sprintInput.GetButtonDown() && */ImmediateSprintFlyingStartCondition())
                    {
                        CheckChainedAction();

                        StartSprintImmediately();
                        return true;
                    }

                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void SprintInput()
        {
            if (!sprintInput.useInput)
                return;

            if (tpInput.cc.sprintOnlyFree && tpInput.cc.isStrafing)
                return;

            Sprint(tpInput.cc.useContinuousSprint ? sprintInput.GetButtonDown() : sprintInput.GetButton());
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected abstract void Sprint(bool hasSprintInput);

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void SetSlowMode(bool activate)
        {
            isSlowMode = activate;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void EscapeInput()
        {
            if (!HasInput || tpInput.cc.isSprinting || tpInput.cc.customAction)
                return;

            if (escapeStamina > 0 && escapeStamina > tpInput.cc.currentStamina)
                return;

            if (escapeInput.useInput && escapeInput.GetButtonDown())
            {
                if (tpInput.cc.isStrafing)
                {
                    if (tpInput.cc.input.z > 0.1f)
                    {
                        flyingEscapeDirection = tr.forward;
                        tpInput.cc.animator.CrossFadeInFixedTime("FlyingEscape_F", 0.25f);
                    }
                    else if (tpInput.cc.input.z < -0.1f)
                    {
                        flyingEscapeDirection = -tr.forward;
                        tpInput.cc.animator.CrossFadeInFixedTime("FlyingEscape_B", 0.25f);
                    }
                    else if (tpInput.cc.input.x > 0.1f)
                    {
                        flyingEscapeDirection = tr.right;
                        tpInput.cc.animator.CrossFadeInFixedTime("FlyingEscape_R", 0.25f);
                    }
                    else if (tpInput.cc.input.x < -0.1f)
                    {
                        flyingEscapeDirection = -tr.right;
                        tpInput.cc.animator.CrossFadeInFixedTime("FlyingEscape_L", 0.25f);
                    }
                    else
                    {
                        flyingEscapeDirection = Vector3.zero;
                        return;
                    }
                }
                else
                {
                    flyingEscapeDirection = tr.forward;
                    tpInput.cc.animator.CrossFadeInFixedTime("FlyingEscape_F", 0.25f);
                }

                tpInput.cc.currentStaminaRecoveryDelay = staminaRecoveryDelay;
                tpInput.cc.ReduceStamina(escapeStamina, true);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void SprintRollInput()
        {
            if (!HasInput || !tpInput.cc.isSprinting || tpInput.cc.customAction)
                return;

            if (sprintRollStamina > 0 && sprintRollStamina > tpInput.cc.currentStamina)
                return;

            if (sprintRollLeftInput.useInput && sprintRollLeftInput.GetButtonDown())
            {
                tpInput.cc.animator.CrossFadeInFixedTime("FlyingSprintRoll_L", 0.25f);

                tpInput.cc.currentStaminaRecoveryDelay = staminaRecoveryDelay;
                tpInput.cc.ReduceStamina(sprintRollStamina, true);
            }
            else if (sprintRollRightInput.useInput && sprintRollRightInput.GetButtonDown())
            {
                tpInput.cc.animator.CrossFadeInFixedTime("FlyingSprintRoll_R", 0.25f);

                tpInput.cc.currentStaminaRecoveryDelay = staminaRecoveryDelay;
                tpInput.cc.ReduceStamina(sprintRollStamina, true);
            }
            else if (sprintRollUpInput.useInput && sprintRollUpInput.GetButtonDown())
            {
                tpInput.cc.animator.CrossFadeInFixedTime("FlyingSprintRoll_U", 0.25f);

                tpInput.cc.currentStaminaRecoveryDelay = staminaRecoveryDelay;
                tpInput.cc.ReduceStamina(sprintRollStamina, true);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void MoveInput()
        {
            Vector3 input = tpInput.cc.input;
            input.x = tpInput.horizontalInput.GetAxisRaw();
            input.z = tpInput.verticalInput.GetAxisRaw();
            tpInput.cc.input = input;

            tpInput.cc.inputSmooth = Vector3.Lerp(tpInput.cc.inputSmooth, tpInput.cc.input,
                (tpInput.cc.isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.fixedDeltaTime);

            inputSqrt = tpInput.cc.input.sqrMagnitude;
            inputSmoothSqrt = tpInput.cc.inputSmooth.sqrMagnitude;

            // Up Down
            if (straightUpInput.useInput && straightUpInput.GetButton())
                upDownInput = 1f;
            else if (straightDownInput.useInput && straightDownInput.GetButton())
                upDownInput = -1f;
            else
                upDownInput = 0f;

            upDownInputSmooth = Mathf.Lerp(upDownInputSmooth, upDownInput,
                (tpInput.cc.isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.fixedDeltaTime);
            upDownInputSmoothAbs = Mathf.Abs(upDownInputSmooth);

            if (tpInput.cc.isStrafing)
            {
                Vector3 right = tr.right;
                Vector3 forward = Quaternion.AngleAxis(-90, tr.up) * right;
                targetDirection = (tpInput.cc.inputSmooth.x * right) + (tpInput.cc.inputSmooth.z * forward);
            }
            else
            {
                if (tpInput.cc.isSprinting)
                {
                    targetDirection = AimDirection;
                }
                else
                {
                    if (HasInput)
                    {
                        var right = tpInput.cameraMain.transform.right;
                        var forward = Quaternion.AngleAxis(-90, tpInput.cameraMain.transform.up) * right;
                        targetDirection = (tpInput.cc.inputSmooth.x * right) + (tpInput.cc.inputSmooth.z * forward);
                    }
                    else
                    {
                        Vector3 dir = AimDirection;
                        dir.y = 0f;
                        targetDirection = Vector3.Lerp(targetDirection, dir, freeSpeed.movementSmooth * Time.deltaTime);
                    }
                }
            }
            targetDirection.Normalize();

            UpdateAnimator(targetDirection);
            UpdateFlyingSpeed();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateAnimator(Vector3 direction)
        {
            if (lockInput)
            {
                tpInput.cc.inputMagnitude = 0f;
                return;
            }

            var mag = tpInput.cc.inputSmooth.magnitude;
            if (mag > 0.1f && mag < 0.5f)
                mag = 0.5f;
            else if (mag > 0.6f)
                mag = 1f;

            tpInput.cc.inputMagnitude = Mathf.Clamp(tpInput.cc.isSprinting ? mag + 0.5f : mag, 0, tpInput.cc.isSprinting ? 1.5f : 1f);

            tpInput.cc.animator.SetFloat(mvAnimatorParameters.HorizontalInput, horizontalDelta, tpInput.cc.freeSpeed.animationSmooth, Time.fixedDeltaTime);
            tpInput.cc.animator.SetFloat(mvAnimatorParameters.VerticalInput, verticalDelta, tpInput.cc.freeSpeed.animationSmooth, Time.fixedDeltaTime);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void BeginSprint()
        {
            if (tpInput.cc.isStrafing)
            {
                currentMoveSpeed = strafeSprintSpeed.moveSpeed;
                currentRotationSpeed = strafeSprintSpeed.rotationSpeed;
                currentRollingSensitivity = strafeSprintSpeed.rollingSensitivity;
            }
            else
            {
                currentMoveSpeed = freeSprintSpeed.moveSpeed;
                currentRotationSpeed = freeSprintSpeed.rotationSpeed;
                currentRollingSensitivity = freeSprintSpeed.rollingSensitivity;

                if (multiSprintCount.origin > 0 && multiSprintCount.now > 1 && multiSprintCount.now <= multiSprintCount.origin)
                    currentMoveSpeed *= multiSprintSpeedMultiplier * multiSprintCount.now;
            }

            SetCapsuleCollider();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public abstract bool StartSprintImmediately();

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public abstract void FinishSprint();

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateFlyingSpeed()
        {
            float t = Time.fixedDeltaTime;

            currentMaxRollingAngle = Mathf.Lerp(currentMaxRollingAngle, rolling.origin, t * stopMoveSpeed);

            if (tpInput.cc.isSprinting)
                return;

            if (HasSmoothInput || HasUpDownSmoothInput)
            {
                if (tpInput.cc.isStrafing)
                {
                    t = (HasSmoothInput ? inputSmoothSqrt : upDownInputSmoothAbs) * Time.fixedDeltaTime;
                    currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, strafeSpeed.moveSpeed, t);
                    currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, strafeSpeed.rotationSpeed, t);
                    currentRollingSensitivity = Mathf.Lerp(currentRollingSensitivity, strafeSpeed.rollingSensitivity, t);
                }
                else
                {
                    t = (HasSmoothInput ? inputSmoothSqrt : upDownInputSmoothAbs) * Time.fixedDeltaTime;
                    currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, freeSpeed.moveSpeed, t);
                    currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, freeSpeed.rotationSpeed, t);
                    currentRollingSensitivity = Mathf.Lerp(currentRollingSensitivity, freeSpeed.rollingSensitivity, t);
                }
            }
            else
            {
                t = Time.fixedDeltaTime * stopMoveSpeed;
                currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, 0f, t);
                currentRotationSpeed = Mathf.Lerp(currentRotationSpeed, freeSpeed.rotationSpeed, t);
                currentRollingSensitivity = Mathf.Lerp(currentRollingSensitivity, freeSpeed.rollingSensitivity, t);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void ResetPose(bool immediate = true)
        {
            currentMoveSpeed = 0;

            Vector3 direction = Vector3.ProjectOnPlane(tr.forward, Vector3.up);

            if (immediate)
                tr.rotation = Quaternion.LookRotation(direction);
            else
                tr.rotation = Quaternion.Slerp(tr.rotation, Quaternion.LookRotation(direction), rotationSpeed.max * Time.fixedDeltaTime);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateRotation()
        {
            if (lockRotation || tpInput.cc.customAction)
                return;

            if (tpInput.cc.isStrafing)
            {
                targetRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(AimDirection, Vector3.up).normalized);
                tr.rotation = Quaternion.Slerp(tr.rotation, targetRotation, Time.fixedDeltaTime * strafeSpeed.rotationSpeed);
            }
            else
            {
                Vector3 cross = Vector3.Cross(tr.forward, targetDirection);

                verticalDelta = cross.x;
                horizontalDelta = cross.y;

                if (tpInput.cc.isSprinting)
                {
                    if (Mathf.Abs(horizontalDelta) < rollingThreshold)
                    {
                        rolling.now = Mathf.Lerp(rolling.now, 0f, resetRollingSpeed * Time.fixedDeltaTime);
                    }
                    else
                    {
                        rolling.now += -horizontalDelta * rollingSpeed;
                        rolling.now = Mathf.Clamp(rolling.now, -rolling.origin, rolling.origin);
                    }

                    targetRotation = Quaternion.LookRotation(targetDirection);
                    targetRotation *= Quaternion.AngleAxis(rolling.now, Vector3.forward);
                }
                else
                {
                    targetRotation = Quaternion.LookRotation(targetDirection);
                }

                tr.rotation = Quaternion.Slerp(tr.rotation, targetRotation, rotationSpeed.max * Time.fixedDeltaTime);
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateMovement()
        {
            if (lockMovement || tpInput.cc.customAction)
                return;

            Vector3 targetPosition = rb.position + targetDirection * currentMoveSpeed * (HasSlowInput ? slowModeMultiplier : 1f) * Time.fixedDeltaTime;
            Vector3 targetVelocity = (targetPosition - tr.position) / Time.fixedDeltaTime;

            if (flyingGroundDistance.min > 0f && tpInput.cc.groundDistance >= flyingGroundDistance.max)
            {
                targetVelocity.y = 0f;
            }
            else
            {
                if (HasUpDownInput && !tpInput.cc.isSprinting)
                    targetVelocity.y += upDownInputSmooth;   // Up/Down
            }

            rb.velocity = targetVelocity;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateHardLanding()
        {
            if (FlyingState != (int)FreeFlyingState.HardLanding)
                return;

            rb.AddForce(Vector3.down * hardLandingForce, ForceMode.Acceleration);

            UpdateGroundDetection();

            if (groundCaster.isDetected && groundCaster.distance <= 0.1f)
            {
                OnFinishLanding.Invoke();

                tpInput.cc.verticalVelocity = 0f;   // to prevent ragdoll transition

                if (hardLandingCraterFxPrefab)
                    Instantiate(hardLandingCraterFxPrefab, new Vector3(tr.position.x, groundCaster.hit.point.y + 0.1f, tr.position.z), Quaternion.identity);

                ExitActionState();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateEscapeMovement()
        {
            Vector3 deltaPosition;
            Vector3 targetVelocity;

            float smooth = tpInput.cc.isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth;
            rb.CheckMoveStopWeight(tpInput.cc._capsuleCollider, moveStopLayerMask, out RaycastHit hit, ref moveStopWeight, maxMoveStopCheckDistance, smooth);

            if (useFlyingEscapeRootMotion)
            {
                deltaPosition = tpInput.cc.animator.deltaPosition;
                targetVelocity = (deltaPosition * flyingEscapeSpeedMultiplier) / Time.fixedDeltaTime * (1f - moveStopWeight);
            }
            else
            {
                deltaPosition = flyingEscapeDirection * Time.fixedDeltaTime;
                targetVelocity = (deltaPosition * currentMoveSpeed * flyingEscapeSpeedMultiplier) / Time.fixedDeltaTime * (1f - moveStopWeight);
            }

            rb.velocity = targetVelocity;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateSprintRollMovement()
        {
            Vector3 deltaPosition;
            Vector3 targetVelocity;

            float smooth = tpInput.cc.isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth;
            rb.CheckMoveStopWeight(tpInput.cc._capsuleCollider, moveStopLayerMask, out RaycastHit hit, ref moveStopWeight, maxMoveStopCheckDistance, smooth);

            if (useFlyingSprintRollRootMotion)
            {
                deltaPosition = tpInput.cc.animator.deltaPosition;
                targetVelocity = (deltaPosition * flyingSprintRollSpeedMultiplier) / Time.fixedDeltaTime * (1f - moveStopWeight);
            }
            else
            {
                deltaPosition = flyingEscapeDirection * Time.fixedDeltaTime;
                targetVelocity = (deltaPosition * currentMoveSpeed * flyingSprintRollSpeedMultiplier) / Time.fixedDeltaTime * (1f - moveStopWeight);
            }

            rb.velocity = targetVelocity;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void UpdateGroundDetection()
        {
            if (!groundCaster.useCast)
                return;

            groundCaster.Cast(tr, Vector3.down, tpInput.cc.groundLayer, QueryTriggerInteraction.Ignore, this, 0f, true, debugMode);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual bool StaminaConsumption()
        {
            if (tpInput.cc.isSprinting)
                tpInput.cc.ReduceStamina(sprintStamina, true);
            else
                tpInput.cc.ReduceStamina(stamina, true);

            return tpInput.cc.currentStamina > 0;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected abstract bool HasWeapon();

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void TriggerRandomIdle()
        {
            if (tpInput.cc.customAction || tpInput.cc.isStrafing || tpInput.cc.isSprinting || HasInput)
                return;

            if (HasWeapon())
            {
                FlyingIdle = FlyingIdleType.HasWeapon;
            }
            else
            {
                if (randomIdleTime.origin > 0)
                {
                    if (tpInput.cc._capsuleCollider.enabled)
                    {
                        randomIdleTime.now += Time.deltaTime;

                        if (randomIdleTime.now >= randomIdleTime.origin)
                        {
                            randomIdleTime.now = 0;
                            FlyingIdle = (FlyingIdleType)UnityEngine.Random.Range(0, 2);
                        }
                    }
                    else
                    {
                        randomIdleTime.now = 0;
                    }
                }
            }

            SetCapsuleCollider();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void EnterActionState(bool fromGround = true)
        {
            if (IsOnAction)
                return;

            if (!FlyingCondition())
                return;

            CheckChainedAction();

            tpInput.cc.isJumping = false;
            tpInput.cc.isSprinting = false;
            tpInput.cc.isCrouching = false;
            tpInput.cc.isGrounded = true;
            tpInput.cc.animator.SetBool(vAnimatorParameters.IsGrounded, true);
            tpInput.cc.animator.SetFloat(vAnimatorParameters.InputHorizontal, 0);
            tpInput.cc.animator.SetFloat(vAnimatorParameters.InputVertical, 0);
            tpInput.cc.animator.SetFloat(mvAnimatorParameters.HorizontalInput, 0);
            tpInput.cc.animator.SetFloat(mvAnimatorParameters.VerticalInput, 0);

            tpInput.SetLockBasicInput(true);
            tpInput.cc.lockMovement = true;
            tpInput.cc.lockRotation = true;
            tpInput.cc.disableCheckGround = true;
            tpInput.cc.disableSnapToGround = true;
            tpInput.cc.lockSetMoveSpeed = true;
            tpInput.cc.verticalVelocity = 0f;
            rb.useGravity = false;
            rb.drag = 1f;
            tpInput.cc._capsuleCollider.isTrigger = false;
            SetCapsuleCollider();

            tpInput.cc.currentStaminaRecoveryDelay = staminaRecoveryDelay;

            ResetPose();
            IsOnAction = true;  // must be called after SetLockBasicInput()

            FlyingState = 1;
            tpInput.cc.animator.CrossFadeInFixedTime("Free Flying", 0.25f);

            if (fromGround)
            {
                Vector3 current = tr.eulerAngles.normalized;
                current.y += flyJumpUpVelocity;
                rb.AddForce(current * freeSpeed.moveSpeed * 2f, ForceMode.Impulse);

                OnStartActionOnGround.Invoke();
            }
            else
            {
                OnStartActionOnAir.Invoke();
            }

#if UNITY_EDITOR
            if (debugMode)
                Debug.LogWarning("EnterActionState() fromGround = " + fromGround);
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void EnterHardLandingState()
        {
            lockInput = true;
            lockMovement = true;
            lockRotation = true;

            tpInput.cc.isJumping = false;
            tpInput.cc.isSprinting = false;
            tpInput.cc.isCrouching = false;
            tpInput.cc.isGrounded = false;
            tpInput.cc.disableCheckGround = false;
            tpInput.cc.disableSnapToGround = false;
            tpInput.cc.animator.SetBool(vAnimatorParameters.IsGrounded, false);
            tpInput.cc.animator.SetFloat(vAnimatorParameters.InputHorizontal, 0);
            tpInput.cc.animator.SetFloat(vAnimatorParameters.InputVertical, 0);
            tpInput.cc.animator.SetFloat(mvAnimatorParameters.HorizontalInput, 0);
            tpInput.cc.animator.SetFloat(mvAnimatorParameters.VerticalInput, 0);

            tpInput.cc.lockMovement = false;
            tpInput.cc.lockRotation = false;
            rb.useGravity = true;

            SetCapsuleCollider();
            ResetPose(true);

            if (hardLandingStartFxPrefab)
                Instantiate(hardLandingStartFxPrefab, tr.position, Quaternion.identity, tr);

            FlyingState = (int)FreeFlyingState.HardLanding;

            OnStartLanding.Invoke();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void ExitActionState()
        {
            if (!IsOnAction)
                return;
            IsOnAction = false; // must be called before SetLockBasicInput()

            if (tpInput.cc.isSprinting)
                OnFinishSprinting.Invoke();

            tpInput.cc.isJumping = false;
            tpInput.cc.isSprinting = false;
            tpInput.cc.isCrouching = false;
            tpInput.cc.isGrounded = false;
            tpInput.cc.animator.SetBool(vAnimatorParameters.IsGrounded, false);
            tpInput.cc.animator.SetFloat(vAnimatorParameters.InputHorizontal, 0);
            tpInput.cc.animator.SetFloat(vAnimatorParameters.InputVertical, 0);
            tpInput.cc.animator.SetFloat(mvAnimatorParameters.HorizontalInput, 0);
            tpInput.cc.animator.SetFloat(mvAnimatorParameters.VerticalInput, 0);

            tpInput.SetLockBasicInput(false);
            tpInput.cc.lockMovement = false;
            tpInput.cc.lockRotation = false;
            tpInput.cc.moveSpeed = originalMoveSpeed;
            tpInput.cc.freeSpeed.rotationSpeed = originalRotationSpeed;
            tpInput.cc.disableCheckGround = false;
            tpInput.cc.disableSnapToGround = false;
            tpInput.cc.lockSetMoveSpeed = false;
            tpInput.cc.moveSpeed = originalMoveSpeed;
            tpInput.cc.freeSpeed.rotationSpeed = originalRotationSpeed;
            rb.useGravity = true;
            rb.drag = 0f;
            rb.angularVelocity = Vector3.zero;

            SetCapsuleCollider();

            ResetPose();
            multiSprintCount.now = 0;

            lockInput = false;
            lockMovement = false;
            lockRotation = false;

            FlyingState = 0;
            tpInput.ResetCameraState();

#if UNITY_EDITOR
            if (debugMode)
                Debug.LogWarning("ExitActionState()");
#endif

            OnFinishAction.Invoke();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void SetCapsuleCollider()
        {
            if (IsOnAction)
            {
                if (tpInput.cc.isSprinting)
                {
                    // Flying Sprint
                    tpInput.cc._capsuleCollider.direction = 2; // Z axis
                    tpInput.cc._capsuleCollider.center = capsuleColliderCenterOnSprint;
                    tpInput.cc._capsuleCollider.height = tpInput.cc.colliderHeightDefault;
                }
                else if (HasInput)
                {
                    // Free Flying
                    tpInput.cc._capsuleCollider.direction = 1; // Y axis
                    tpInput.cc._capsuleCollider.center = tpInput.cc.colliderCenterDefault;
                    tpInput.cc._capsuleCollider.height = tpInput.cc.colliderHeightDefault * 0.5f;
                }
                else
                {
                    // Idle
                    tpInput.cc._capsuleCollider.direction = 1; // Y axis
                    tpInput.cc._capsuleCollider.center = tpInput.cc.colliderCenterDefault;
                    tpInput.cc._capsuleCollider.height = tpInput.cc.colliderHeightDefault;
                }
            }
            else
            {
                tpInput.cc._capsuleCollider.direction = 1; // Y axis
                tpInput.cc._capsuleCollider.center = tpInput.cc.colliderCenterDefault;
                tpInput.cc._capsuleCollider.height = tpInput.cc.colliderHeightDefault;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void OnCollisionEnter(Collision collision)
        {
            if (!IsOnAction)
                return;

            if (!obstacleLayerMask.ContainsLayer(collision.gameObject.layer))
                return;

            if (IsFlyingEscape && noRagdolOnEscape)
                return;

            if (IsFlyingSprintRoll && noRagdollOnSprintRoll)
                return;

            if (!tpInput.cc.isSprinting)
                return;

            // Use Raycast again in order to get the exact collision angle
            if (tr.CheckForwardRayCollision(tr.forward, tpInput.cc._capsuleCollider, out RaycastHit hit, obstacleLayerMask, 1f, true))
            {
                crashAngle.now = Mathf.Abs(90f - Vector3.Angle(tr.forward, -hit.normal));
                crashVelocity.now = rb.velocity.magnitude;

                if (crashAngle.IsInRange(crashAngle.now) && crashVelocity.now >= crashVelocity.origin)
                {
                    if (ragdollOnSprintCrash && ragdoll)
                    {
                        ragdoll.ActivateRagdoll();
                        ExitActionState();
                    }
                    else if (autoCancelFlyingOnCollision)
                    {
                        ExitActionState();
                    }
                }
            }
        }

#if UNITY_EDITOR
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual void OnDrawGizmosSelected()
        {
            if (!debugMode)
                return;

            if (colliders == null)
                colliders = GetComponentsInChildren<Collider>();


            if (groundCaster is null)
                groundCaster = new mvMISSphereCast();

            groundCaster.backOff = 0.2f;
            groundCaster.radius = 0.2f;
            groundCaster.maxDistance = Mathf.Infinity;

            if (!Application.isPlaying)
                groundCaster.Cast(transform, Vector3.down, 0/*tpInput.cc.groundLayer*/, QueryTriggerInteraction.Ignore, this, 0f, true, true);
        }
#endif
#else
        // ----------------------------------------------------------------------------------------------------
        // IMISColliderFilter
        // ----------------------------------------------------------------------------------------------------
        public bool FilterCollider(Collider other)
        {
            return false;
        }
#endif
    }

    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [Serializable]
    public class mvFreeFlyingSpeed
    {
        [Range(1f, 20f)]
        [Tooltip("The higher, the faster input keys response")]
        public float movementSmooth = 10f;

        [Tooltip("Movement speed")]
        public float moveSpeed = 6f;
        [Tooltip("Rotation(Yaw) speed")]
        public float rotationSpeed = 4f;
        [Tooltip("The higher, the more often the rolling occurrs")]
        public float rollingSensitivity = 15f;

        public mvFreeFlyingSpeed(float movementSmooth, float moveSpeed, float rotationSpeed, float rollingSensitivity)
        {
            this.movementSmooth = movementSmooth;
            this.moveSpeed = moveSpeed;
            this.rotationSpeed = rotationSpeed;
            this.rollingSensitivity = rollingSensitivity;
        }
    }
}