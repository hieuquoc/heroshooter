#if INVECTOR_BASIC
using Invector;
using Invector.vEventSystems;
using static Invector.vEventSystems.vAnimatorEvent;
#endif
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using static com.mobilin.games.MISAnimator;
using System.IO;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public partial class MISMainSetup
    {
#if MIS_FREEFLYING
        // ----------------------------------------------------------------------------------------------------
        // Animator StateMachine/State
        public const string STATE_FREEFLYING = "FreeFlying";

        // Base Layer - Airborne - Falling
        AnimatorState base_FF_HardLanding;

        // Base Layer - Airborne - Landing
        AnimatorState base_FF_HardLand;

        // Base Layer
        AnimatorStateMachine base_FF_SM;
        AnimatorState base_FF_Free;
        AnimatorState base_FF_Strafe;
        AnimatorStateMachine base_FF_StrafeEscapeSM;
        AnimatorState base_FF_EscapeL;
        AnimatorState base_FF_EscapeR;
        AnimatorState base_FF_EscapeF;
        AnimatorState base_FF_EscapeB;
        AnimatorStateMachine base_FF_IdleSM;
        AnimatorState base_FF_IdleHandsOnWaist;
        AnimatorState base_FF_IdleCrossArms;
        AnimatorState base_FF_IdleHasWeapon;
        AnimatorStateMachine base_FF_SprintSM;
        AnimatorState base_FF_SprintToFlying;
        AnimatorState base_FF_FlyingToSprint;
        AnimatorState base_FF_Sprint;
        AnimatorStateMachine base_FF_SprintRollSM;
        AnimatorState base_FF_SprintRollL;
        AnimatorState base_FF_SprintRollR;
        AnimatorState base_FF_SprintRollU;

        // UpperBody Layer
        AnimatorStateMachine upb_FF_SmallHitReactionSM;
        AnimatorState upb_FF_SmallHitReactionSM_GetHitFromBack;
        AnimatorState upb_FF_SmallHitReactionSM_GetHitFromFront;
        AnimatorState upb_FF_SmallHitReactionSM_GetHitFromLeft;
        AnimatorState upb_FF_SmallHitReactionSM_GetHitFromRight;

        // FullBody
        AnimatorStateMachine fb_FF_BigHitReactionSM;
        AnimatorState fb_FF_BigHitReactionSM_GetHitFromBack;
        AnimatorState fb_FF_BigHitReactionSM_GetHitFromFront;
        AnimatorState fb_FF_BigHitReactionSM_GetHitFromLeft;
        AnimatorState fb_FF_BigHitReactionSM_GetHitFromRight;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void FreeFlyingSetup(mvSetupOption setupOption, GameObject characterObj, GameObject cameraObj)
        {
            // ----------------------------------------------------------------------------------------------------
            // Setup Options
            // ----------------------------------------------------------------------------------------------------
            bool usePhysicalWindForce = setupOption.HasSetupOption(SetupOption.PhysicalWindForce);
            bool useWindForceVFX = setupOption.HasSetupOption(SetupOption.WindForceVFX);
            bool useJetStreamVFX = setupOption.HasSetupOption(SetupOption.JetStreamVFX);
            bool useAirTrailsFX = setupOption.HasSetupOption(SetupOption.AirTrails);
            bool useHardLandingFX = setupOption.HasSetupOption(SetupOption.HardLandingFx);


            // ----------------------------------------------------------------------------------------------------
            // Main Component
            // ----------------------------------------------------------------------------------------------------
            mvFreeFlying package = null;
            if (templateType == MISEditor.TemplateType.Shooter)
            {
                package = characterObj.GetComponent<mvFreeFlyingShooter>();
                if (package == null)
                    package = characterObj.AddComponent<mvFreeFlyingShooter>();
            }
            else if (templateType == MISEditor.TemplateType.Melee)
            {
                package = characterObj.GetComponent<mvFreeFlyingMelee>();
                if (package == null)
                    package = characterObj.AddComponent<mvFreeFlyingMelee>();
            }
            else
            {
                package = characterObj.GetComponent<mvFreeFlyingBasic>();
                if (package == null)
                    package = characterObj.AddComponent<mvFreeFlyingBasic>();
            }


            // ----------------------------------------------------------------------------------------------------
            // mvWindForce Component
            mvWindForce windForce = null;
            if (usePhysicalWindForce || useWindForceVFX)
            {
                if (characterObj.TryGetComponent(out windForce) == false)
                    windForce = characterObj.AddComponent<mvWindForce>();
            }


            // ----------------------------------------------------------------------------------------------------
            // WindForce VFX
            GameObject windForceVfxObj = null;
            if (useWindForceVFX)
            {
                Transform windForceVFXTransform = misComponentsParentObj.transform.Find("WindForceVFX");
                if (windForceVFXTransform == null)
                {
                    GameObject windForceVfxPrefab = 
                        AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/VFX/WindForce/WindForceVFX.prefab"));
                    windForceVfxObj = windForceVfxPrefab.Instantiate3D(Vector3.zero, misComponentsParentObj.transform);
                }
                else
                {
                    windForceVfxObj = windForceVFXTransform.gameObject;
                }

                // OnGround VFX
                var smokeParticleObject = windForceVfxObj.transform.Find("Smoke").gameObject;
                if (smokeParticleObject != null)
                {
                    smokeParticleObject.SetActive(false);
                    windForce.onGroundVfx = smokeParticleObject;
                }

                // OnAir VFX
                var shockwaveParticleObject = windForceVfxObj.transform.Find("Shockwave").gameObject;
                if (shockwaveParticleObject != null)
                {
                    shockwaveParticleObject.SetActive(false);
                    windForce.onAirVfx = shockwaveParticleObject;
                }
            }


            // ----------------------------------------------------------------------------------------------------
            // JetStream VFX
            GameObject leftJetStreamObject = null;
            GameObject rightJetStreamObject = null;
            mvParticlePlayer leftJetStreamParticlePlayer = null;
            mvParticlePlayer rightJetStreamParticlePlayer = null;
            if (useJetStreamVFX)
            {
                // BodySnap
                vBodySnappingControl bodySnappingControl = invectorComponentsParentObj.transform.GetBodySnappingControl();

                if (bodySnappingControl != null)
                {
                    // JetStreamVFX Object
                    leftJetStreamObject = SetJetStreamVFX(bodySnappingControl, "LeftFoot");
                    rightJetStreamObject = SetJetStreamVFX(bodySnappingControl, "RightFoot");

                    leftJetStreamParticlePlayer = leftJetStreamObject.GetComponent<mvParticlePlayer>();
                    rightJetStreamParticlePlayer = rightJetStreamObject.GetComponent<mvParticlePlayer>();
                }
            }


            // ----------------------------------------------------------------------------------------------------
            // Air Trails FX
            GameObject airTrailsFxObj = null;
            if (useAirTrailsFX)
            {
                Transform airTrailsFxTransform = misComponentsParentObj.transform.Find("FX_AirTrails");
                if (airTrailsFxTransform == null)
                {
                    GameObject airTrailsFxPrefab = 
                        AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/VFX/Wind/FX_AirTrails.prefab"));
                    airTrailsFxObj = airTrailsFxPrefab.Instantiate3D(Vector3.up, misComponentsParentObj.transform);
                }
                else
                {
                    airTrailsFxObj = airTrailsFxTransform.gameObject;
                }
                airTrailsFxObj.SetActive(false);

                package.airTrailsObj = airTrailsFxObj;
            }


            // ----------------------------------------------------------------------------------------------------
            // 
            if (useHardLandingFX)
            {
                if (package.hardLandingStartFxPrefab == null)
                    package.hardLandingStartFxPrefab = 
                        AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/VFX/Shockwave/FX_Shockwave.prefab"));

                if (package.hardLandingCraterFxPrefab == null)
                    package.hardLandingCraterFxPrefab = 
                        AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/VFX/HardLandingCrater/FX_HardLandingCrater.prefab"));
            }


            // ----------------------------------------------------------------------------------------------------
            // mvFreeFlying Events
            if (package.OnStartActionOnGround == null)
                package.OnStartActionOnGround = new UnityEvent();

            if (package.OnStartActionOnAir == null)
                package.OnStartActionOnAir = new UnityEvent();

            if (package.OnFinishAction == null)
                package.OnFinishAction = new UnityEvent();

            if (package.OnStartSprinting == null)
                package.OnStartSprinting = new UnityEvent();

            if (package.OnFinishSprinting == null)
                package.OnFinishSprinting = new UnityEvent();

            if (package.OnStartLanding == null)
                package.OnStartLanding = new UnityEvent();

            if (package.OnFinishLanding == null)
                package.OnFinishLanding = new UnityEvent();
            

            // ----------------------------------------------------------------------------------------------------
            // OnStartActionOnGround
            package.OnStartActionOnGround.RemoveMissingPersistents();

            if (usePhysicalWindForce && windForce != null)
            {
                if (package.OnStartActionOnGround.HasPersistent(windForce, windForce.GetType(), "StartOneTimeForce", typeof(bool)) == false)
                {
                    UnityAction<bool> startOneTimeForceDelegate = 
                        System.Delegate.CreateDelegate(typeof(UnityAction<bool>), windForce, "StartOneTimeForce") as UnityAction<bool>;
                    UnityEventTools.AddBoolPersistentListener(package.OnStartActionOnGround, startOneTimeForceDelegate, true);
                }
            }

            if (useJetStreamVFX)
            {
                if (package.OnStartActionOnGround.HasPersistent(leftJetStreamParticlePlayer, leftJetStreamParticlePlayer.GetType(), "Play", null) == false)
                {
                    UnityAction leftPlayDelegate = System.Delegate.CreateDelegate(typeof(UnityAction), leftJetStreamParticlePlayer, "Play") as UnityAction;
                    UnityEventTools.AddVoidPersistentListener(package.OnStartActionOnGround, leftPlayDelegate);
                }

                if (package.OnStartActionOnGround.HasPersistent(rightJetStreamParticlePlayer, rightJetStreamParticlePlayer.GetType(), "Play", null) == false)
                {
                    UnityAction rightPlayDelegate = System.Delegate.CreateDelegate(typeof(UnityAction), rightJetStreamParticlePlayer, "Play") as UnityAction;
                    UnityEventTools.AddVoidPersistentListener(package.OnStartActionOnGround, rightPlayDelegate);
                }
            }


            // ----------------------------------------------------------------------------------------------------
            // OnStartActionOnAir
            package.OnStartActionOnAir.RemoveMissingPersistents();

            if (usePhysicalWindForce && windForce != null)
            {
                if (package.OnStartActionOnAir.HasPersistent(windForce, windForce.GetType(), "StartOneTimeForce", typeof(bool)) == false)
                {
                    UnityAction<bool> startOneTimeForceDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<bool>), windForce, "StartOneTimeForce") as UnityAction<bool>;
                    UnityEventTools.AddBoolPersistentListener(package.OnStartActionOnAir, startOneTimeForceDelegate, false);
                }
            }

            if (useJetStreamVFX)
            {
                if (package.OnStartActionOnAir.HasPersistent(leftJetStreamParticlePlayer, leftJetStreamParticlePlayer.GetType(), "Play", null) == false)
                {
                    UnityAction leftPlayDelegate = System.Delegate.CreateDelegate(typeof(UnityAction), leftJetStreamParticlePlayer, "Play") as UnityAction;
                    UnityEventTools.AddVoidPersistentListener(package.OnStartActionOnAir, leftPlayDelegate);
                }

                if (package.OnStartActionOnAir.HasPersistent(rightJetStreamParticlePlayer, rightJetStreamParticlePlayer.GetType(), "Play", null) == false)
                {
                    UnityAction rightPlayDelegate = System.Delegate.CreateDelegate(typeof(UnityAction), rightJetStreamParticlePlayer, "Play") as UnityAction;
                    UnityEventTools.AddVoidPersistentListener(package.OnStartActionOnAir, rightPlayDelegate);
                }
            }


            // ----------------------------------------------------------------------------------------------------
            // OnFinishAction Event
            package.OnFinishAction.RemoveMissingPersistents();

            if (usePhysicalWindForce && windForce != null)
            {
                if (package.OnFinishAction.HasPersistent(windForce, windForce.GetType(), "StopWindForce", null) == false)
                {
                    UnityAction stopWindForceDelegate = System.Delegate.CreateDelegate(typeof(UnityAction), windForce, "StopWindForce") as UnityAction;
                    UnityEventTools.AddVoidPersistentListener(package.OnFinishAction, stopWindForceDelegate);
                }
            }

            if (useJetStreamVFX)
            {
                if (package.OnFinishAction.HasPersistent(leftJetStreamParticlePlayer, leftJetStreamParticlePlayer.GetType(), "Stop", null) == false)
                {
                    UnityAction leftPlayDelegate = System.Delegate.CreateDelegate(typeof(UnityAction), leftJetStreamParticlePlayer, "Stop") as UnityAction;
                    UnityEventTools.AddVoidPersistentListener(package.OnFinishAction, leftPlayDelegate);
                }

                if (package.OnFinishAction.HasPersistent(rightJetStreamParticlePlayer, rightJetStreamParticlePlayer.GetType(), "Stop", null) == false)
                {
                    UnityAction rightPlayDelegate = System.Delegate.CreateDelegate(typeof(UnityAction), rightJetStreamParticlePlayer, "Stop") as UnityAction;
                    UnityEventTools.AddVoidPersistentListener(package.OnFinishAction, rightPlayDelegate);
                }
            }

            // ----------------------------------------------------------------------------------------------------
            // OnStartSprinting Event
            package.OnStartSprinting.RemoveMissingPersistents();

            if (usePhysicalWindForce && windForce != null)
            {
                if (package.OnStartSprinting.HasPersistent(windForce, windForce.GetType(), "SetForceMultiplier", typeof(float)) == false)
                {
                    UnityAction<float> setForceMultiplierDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<float>), windForce, "SetForceMultiplier") as UnityAction<float>;
                    UnityEventTools.AddFloatPersistentListener(package.OnStartSprinting, setForceMultiplierDelegate, 2f);
                }
            }

            if (useJetStreamVFX)
            {
                if (package.OnStartSprinting.HasPersistent(leftJetStreamParticlePlayer, leftJetStreamParticlePlayer.GetType(), "Emit", typeof(float)) == false)
                {
                    UnityAction<float> leftPlayDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<float>), leftJetStreamParticlePlayer, "Emit") as UnityAction<float>;
                    UnityEventTools.AddFloatPersistentListener(package.OnStartSprinting, leftPlayDelegate, 5f);
                }

                if (package.OnStartSprinting.HasPersistent(rightJetStreamParticlePlayer, rightJetStreamParticlePlayer.GetType(), "Emit", typeof(float)) == false)
                {
                    UnityAction<float> rightPlayDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<float>), rightJetStreamParticlePlayer, "Emit") as UnityAction<float>;
                    UnityEventTools.AddFloatPersistentListener(package.OnStartSprinting, rightPlayDelegate, 5f);
                }
            }

            if (useAirTrailsFX)
            {
                if (package.OnStartSprinting.HasPersistent(airTrailsFxObj, airTrailsFxObj.GetType(), "SetActive", typeof(bool)) == false)
                {
                    UnityAction<bool> setActiveDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<bool>), airTrailsFxObj, "SetActive") as UnityAction<bool>;
                    UnityEventTools.AddBoolPersistentListener(package.OnStartSprinting, setActiveDelegate, true);
                }
            }


            // ----------------------------------------------------------------------------------------------------
            // OnFinishSprinting Event
            package.OnFinishSprinting.RemoveMissingPersistents();

            if (usePhysicalWindForce && windForce != null)
            {
                if (package.OnFinishSprinting.HasPersistent(windForce, windForce.GetType(), "ResetForce", null) == false)
                {
                    UnityAction resetForceDelegate = System.Delegate.CreateDelegate(typeof(UnityAction), windForce, "ResetForce") as UnityAction;
                    UnityEventTools.AddVoidPersistentListener(package.OnFinishSprinting, resetForceDelegate);
                }
            }

            if (useJetStreamVFX)
            {
                if (package.OnFinishSprinting.HasPersistent(leftJetStreamParticlePlayer, leftJetStreamParticlePlayer.GetType(), "Emit", typeof(float)) == false)
                {
                    UnityAction<float> leftPlayDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<float>), leftJetStreamParticlePlayer, "Emit") as UnityAction<float>;
                    UnityEventTools.AddFloatPersistentListener(package.OnFinishSprinting, leftPlayDelegate, 1f);
                }

                if (package.OnFinishSprinting.HasPersistent(rightJetStreamParticlePlayer, rightJetStreamParticlePlayer.GetType(), "Emit", typeof(float)) == false)
                {
                    UnityAction<float> rightPlayDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<float>), rightJetStreamParticlePlayer, "Emit") as UnityAction<float>;
                    UnityEventTools.AddFloatPersistentListener(package.OnFinishSprinting, rightPlayDelegate, 1f);
                }
            }

            if (useAirTrailsFX)
            {
                if (package.OnFinishSprinting.HasPersistent(airTrailsFxObj, airTrailsFxObj.GetType(), "SetActive", typeof(bool)) == false)
                {
                    UnityAction<bool> setActiveDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<bool>), airTrailsFxObj, "SetActive") as UnityAction<bool>;
                    UnityEventTools.AddBoolPersistentListener(package.OnFinishSprinting, setActiveDelegate, false);
                }
            }


            // ----------------------------------------------------------------------------------------------------
            // OnStartLanding Event
            package.OnStartLanding.RemoveMissingPersistents();


            // ----------------------------------------------------------------------------------------------------
            // OnFinishLanding Event
            package.OnFinishLanding.RemoveMissingPersistents();

            if (useAirTrailsFX)
            {
                if (package.OnFinishLanding.HasPersistent(airTrailsFxObj, airTrailsFxObj.GetType(), "SetActive", typeof(bool)) == false)
                {
                    UnityAction<bool> setActiveDelegate = System.Delegate.CreateDelegate(typeof(UnityAction<bool>), airTrailsFxObj, "SetActive") as UnityAction<bool>;
                    UnityEventTools.AddBoolPersistentListener(package.OnFinishLanding, setActiveDelegate, false);
                }
            }


            // ----------------------------------------------------------------------------------------------------
            // vAnimatorEventReceiver
            if (characterObj.TryGetComponent(out vAnimatorEventReceiver animatorEventReceiver) == false)
                animatorEventReceiver = characterObj.AddComponent<vAnimatorEventReceiver>();

            if (animatorEventReceiver.animatorEvents == null)
                animatorEventReceiver.animatorEvents = new List<vAnimatorEventReceiver.vAnimatorEvent>();

            // BeginSprint AnimatorEvent
            vAnimatorEventReceiver.vAnimatorEvent beginSprintAnimatorEvent = animatorEventReceiver.animatorEvents.Find(x => x.eventName.Equals("BeginSprint"));
            if (beginSprintAnimatorEvent != null)
                animatorEventReceiver.animatorEvents.Remove(beginSprintAnimatorEvent);

            beginSprintAnimatorEvent = new vAnimatorEventReceiver.vAnimatorEvent
            {
                eventName = "BeginSprint",
                onTriggerEvent = new vAnimatorEventReceiver.vAnimatorEvent.StateEvent()
            };
            animatorEventReceiver.animatorEvents.Add(beginSprintAnimatorEvent);

            UnityAction beginSprintDelegate = System.Delegate.CreateDelegate(typeof(UnityAction), package, "BeginSprint") as UnityAction;
            UnityEventTools.AddVoidPersistentListener(beginSprintAnimatorEvent.onTriggerEvent, beginSprintDelegate);


            // ----------------------------------------------------------------------------------------------------
            // Animator
            // ----------------------------------------------------------------------------------------------------
            FreeFlyingAnimatorParameters();
            FreeFlyingBaseLayer();
            FreeFlyingUpperBodyLayer();
            FreeFlyingFullBodyLayer();
            FreeFlyingAnimatorTransitions();
            FreeFlyingPosition();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        GameObject SetJetStreamVFX(vBodySnappingControl bodySnappingControl, string boneName)
        {
            GameObject jetStreamVfxObject = null;

            vSnapToBody snapToBody = bodySnappingControl.GetSnapToBody(boneName);
            Transform jetStreamTransform = snapToBody.gameObject.transform.Find("JetStreamHandler/FX_JetStream");

            if (jetStreamTransform == null)
            {
                GameObject jetStreamHandlerObject = new GameObject("JetStreamHandler");
                jetStreamHandlerObject.transform.parent = snapToBody.gameObject.transform;
                jetStreamHandlerObject.transform.localPosition = Vector3.zero;
                jetStreamHandlerObject.transform.localRotation = Quaternion.identity;

                GameObject jetStreamVfxPrefab = 
                    AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/VFX/JetStream/FX_JetStream.prefab"));
                jetStreamVfxObject = jetStreamVfxPrefab.Instantiate3D(Vector3.zero, jetStreamHandlerObject.transform);
                jetStreamVfxObject.transform.localPosition = boneName.Contains("Left") ? new Vector3(-0.1f, -0.03f, 0f) : new Vector3(0.1f, -0.03f, 0f);
                jetStreamVfxObject.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            }
            else
            {
                jetStreamVfxObject = jetStreamTransform.gameObject;
            }

            return jetStreamVfxObject;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void FreeFlyingAnimatorParameters()
        {
            // Input
            if (!animatorController.parameters.HasParameter(PARAM_HORIZONTAL_INPUT))
                animatorController.AddParameter(PARAM_HORIZONTAL_INPUT, AnimatorControllerParameterType.Float);
            if (!animatorController.parameters.HasParameter(PARAM_VERTICAL_INPUT))
                animatorController.AddParameter(PARAM_VERTICAL_INPUT, AnimatorControllerParameterType.Float);


            // FlyingState
            if (!animatorController.parameters.HasParameter(PARAM_FLYING_STATE))
                animatorController.AddParameter(PARAM_FLYING_STATE, AnimatorControllerParameterType.Int);


#if MIS_AIRDASH
            if (!animatorController.parameters.HasParameter(PARAM_IS_AIRDASH))
                animatorController.AddParameter(PARAM_IS_AIRDASH, AnimatorControllerParameterType.Bool);
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void FreeFlyingBaseLayer()
        {
            // ----------------------------------------------------------------------------------------------------
            // Animation Clips
            // ----------------------------------------------------------------------------------------------------
            AnimationClip flyingIdleHandsOnWaistClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Idle_HandsOnWaist.anim"));
            AnimationClip flyingIdleCrossArmsClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Idle_CrossArms.anim"));
            AnimationClip flyingIdleHasWeaponClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Idle.anim"));
            AnimationClip flyingIdleTriumphantClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Idle_Triumphant.anim"));

            AnimationClip flyingFClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@F.anim"));
            AnimationClip flyingFL45Clip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@FL45.anim"));
            AnimationClip flyingFR45Clip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@FR45.anim"));
            AnimationClip flyingBClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@B.anim"));
            AnimationClip flyingBL45Clip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@BL45.anim"));
            AnimationClip flyingBR45Clip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@BR45.anim"));
            AnimationClip flyingLClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@L.anim"));
            AnimationClip flyingRClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@R.anim"));

            AnimationClip flyingSprintFClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Sprint_F.anim"));
            AnimationClip flyingSprintFL45Clip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Sprint_FL45.anim"));
            AnimationClip flyingSprintFR45Clip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Sprint_FR45.anim"));

            AnimationClip flyingSprintF2FClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Sprint_F2F.anim"));
            AnimationClip flyingToSprintClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@F2Sprint_F.anim"));

            AnimationClip flyingSprintRollLClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@SprintRoll_L.anim"));
            AnimationClip flyingSprintRollRClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@SprintRoll_R.anim"));
            AnimationClip flyingSprintRollUClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@SprintRoll_U.anim"));

            AnimationClip flyingEscapeFClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Escape_F.anim"));
            AnimationClip flyingEscapeBClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Escape_B.anim"));
            AnimationClip flyingEscapeLClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Escape_L.anim"));
            AnimationClip flyingEscapeRClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@Escape_R.anim"));

            AnimationClip hardLandingClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@HardLanding.anim"));
            AnimationClip hardLandClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@HardLand.anim"));


            // ----------------------------------------------------------------------------------------------------
            // Base - Airborne - FallingSM
            // ----------------------------------------------------------------------------------------------------
            base_FF_HardLanding = base_FallingSM.CreateStateIfNotExist("FlyingHardLanding", hardLandingClip);


            // ----------------------------------------------------------------------------------------------------
            // Base - Airborne - LandingSM
            // ----------------------------------------------------------------------------------------------------
            base_FF_HardLand = base_LandingSM.CreateStateIfNotExist("FlyingHardLand", hardLandClip);


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion
            // ----------------------------------------------------------------------------------------------------

            // MIS
            base_Locomotion_MIS = base_LocomotionSM.CreateStateMachineIfNotExist(STATE_MIS);


            // ----------------------------------------------------------------------------------------------------
            // Base - FreeFlying
            // ----------------------------------------------------------------------------------------------------
            base_FF_SM = base_Locomotion_MIS.CreateStateMachineIfNotExist(STATE_FREEFLYING);


            // Base - FreeFlying - Free Flying
            base_FF_Free = base_FF_SM.FindState("Free Flying");

            if (base_FF_Free == null)
            {
                base_FF_Free = base_FF_SM.CreateBlendTree("Free Flying", out BlendTree blendTree);
                blendTree.blendType = BlendTreeType.FreeformCartesian2D;
                blendTree.blendParameter = PARAM_HORIZONTAL_INPUT;
                blendTree.blendParameterY = PARAM_VERTICAL_INPUT;

                blendTree.useAutomaticThresholds = false;
                blendTree.AddChild(flyingFClip, new Vector2(0f, 0.5f));
                blendTree.AddChild(flyingFL45Clip, new Vector2(-0.5f, 0.5f));
                blendTree.AddChild(flyingFR45Clip, new Vector2(0.5f, 0.5f));

                base_FF_Free.motion = blendTree;
            }


            // Base - MIS - FreeFlying - Strafe Flying
            base_FF_Strafe = base_FF_SM.FindState("Strafe Flying");

            if (base_FF_Strafe == null)
            {
                base_FF_Strafe = base_FF_SM.CreateBlendTree("Strafe Flying", out BlendTree blendTree);
                blendTree.blendType = BlendTreeType.FreeformCartesian2D;
                blendTree.blendParameter = PARAM_HORIZONTAL_INPUT;
                blendTree.blendParameterY = PARAM_VERTICAL_INPUT;

                blendTree.useAutomaticThresholds = false;
                blendTree.AddChild(flyingIdleTriumphantClip, new Vector2(0f, 0f));
                blendTree.AddChild(flyingLClip, new Vector2(-1f, 0f));
                blendTree.AddChild(flyingRClip, new Vector2(1f, 0f));
                blendTree.AddChild(flyingFClip, new Vector2(0f, 1f));
                blendTree.AddChild(flyingFL45Clip, new Vector2(-1f, 1f));
                blendTree.AddChild(flyingFR45Clip, new Vector2(1f, 1f));
                blendTree.AddChild(flyingBClip, new Vector2(0f, -1f));
                blendTree.AddChild(flyingBL45Clip, new Vector2(-1f, -1f));
                blendTree.AddChild(flyingBR45Clip, new Vector2(1f, -1f));

                base_FF_Strafe.motion = blendTree;
            }


            // ----------------------------------------------------------------------------------------------------
            // Base - FreeFlying - Strafe Escape
            base_FF_StrafeEscapeSM = base_FF_SM.CreateStateMachineIfNotExist("Strafe Escape");


            // Base - FreeFlying - Strafe Escape - FlyingEscape_L
            base_FF_EscapeL = base_FF_StrafeEscapeSM.CreateStateIfNotExist("FlyingEscape_L", flyingEscapeLClip);

            // vAnimatorTag
            if (!base_FF_EscapeL.TryGetStateMachineBehaviour(out vAnimatorTag base_FlyingEscapeLAnimatorTag))
                base_FlyingEscapeLAnimatorTag = base_FF_EscapeL.AddStateMachineBehaviour<vAnimatorTag>();

            base_FlyingEscapeLAnimatorTag.tags = base_FlyingEscapeLAnimatorTag.tags.RemoveStringIfExist(TAG_CUSTOM_ACTION);
            base_FlyingEscapeLAnimatorTag.tags = base_FlyingEscapeLAnimatorTag.tags.AddStringIfNotExist("IsFlyingEscape");
            base_FlyingEscapeLAnimatorTag.tags = base_FlyingEscapeLAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_IK);
            base_FlyingEscapeLAnimatorTag.tags = base_FlyingEscapeLAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_HEADTRACK);

            // vTriggerSoundByState
            base_FF_EscapeL.AddvTriggerSoundByState(
                new List<AudioClip>
                {
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Basic Locomotion/Audio/Others/rollFx.mp3"))
                });


            // Base - FreeFlying - Strafe Escape - FlyingEscape_R
            base_FF_EscapeR = base_FF_StrafeEscapeSM.CreateStateIfNotExist("FlyingEscape_R", flyingEscapeRClip);

            // vAnimatorTag
            if (!base_FF_EscapeR.TryGetStateMachineBehaviour(out vAnimatorTag base_FlyingEscapeRAnimatorTag))
                base_FlyingEscapeRAnimatorTag = base_FF_EscapeR.AddStateMachineBehaviour<vAnimatorTag>();

            base_FlyingEscapeRAnimatorTag.tags = base_FlyingEscapeRAnimatorTag.tags.RemoveStringIfExist(TAG_CUSTOM_ACTION);
            base_FlyingEscapeRAnimatorTag.tags = base_FlyingEscapeRAnimatorTag.tags.AddStringIfNotExist("IsFlyingEscape");
            base_FlyingEscapeRAnimatorTag.tags = base_FlyingEscapeRAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_IK);
            base_FlyingEscapeRAnimatorTag.tags = base_FlyingEscapeRAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_HEADTRACK);

            // vTriggerSoundByState
            base_FF_EscapeR.AddvTriggerSoundByState(
                new List<AudioClip>
                {
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Basic Locomotion/Audio/Others/rollFx.mp3"))
                });


            // Base - FreeFlying - Strafe Escape - FlyingEscape_F
            base_FF_EscapeF = base_FF_StrafeEscapeSM.CreateStateIfNotExist("FlyingEscape_F", flyingEscapeFClip);

            // vAnimatorTag
            if (!base_FF_EscapeF.TryGetStateMachineBehaviour(out vAnimatorTag base_FlyingEscapeFAnimatorTag))
                base_FlyingEscapeFAnimatorTag = base_FF_EscapeF.AddStateMachineBehaviour<vAnimatorTag>();

            base_FlyingEscapeFAnimatorTag.tags = base_FlyingEscapeFAnimatorTag.tags.RemoveStringIfExist(TAG_CUSTOM_ACTION);
            base_FlyingEscapeFAnimatorTag.tags = base_FlyingEscapeFAnimatorTag.tags.AddStringIfNotExist("IsFlyingEscape");
            base_FlyingEscapeFAnimatorTag.tags = base_FlyingEscapeFAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_IK);
            base_FlyingEscapeFAnimatorTag.tags = base_FlyingEscapeFAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_HEADTRACK);

            // vTriggerSoundByState
            base_FF_EscapeF.AddvTriggerSoundByState(
                new List<AudioClip>
                {
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Basic Locomotion/Audio/Others/rollFx.mp3"))
                });


            // Base - FreeFlying - Strafe Escape - FlyingEscape_B
            base_FF_EscapeB = base_FF_StrafeEscapeSM.CreateStateIfNotExist("FlyingEscape_B", flyingEscapeBClip);

            // vAnimatorTag
            if (!base_FF_EscapeB.TryGetStateMachineBehaviour(out vAnimatorTag base_FlyingEscapeBAnimatorTag))
                base_FlyingEscapeBAnimatorTag = base_FF_EscapeB.AddStateMachineBehaviour<vAnimatorTag>();

            base_FlyingEscapeBAnimatorTag.tags = base_FlyingEscapeBAnimatorTag.tags.RemoveStringIfExist(TAG_CUSTOM_ACTION);
            base_FlyingEscapeBAnimatorTag.tags = base_FlyingEscapeBAnimatorTag.tags.AddStringIfNotExist("IsFlyingEscape");
            base_FlyingEscapeBAnimatorTag.tags = base_FlyingEscapeBAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_IK);
            base_FlyingEscapeBAnimatorTag.tags = base_FlyingEscapeBAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_HEADTRACK);

            // vTriggerSoundByState
            base_FF_EscapeB.AddvTriggerSoundByState(
                new List<AudioClip>
                {
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Basic Locomotion/Audio/Others/rollFx.mp3"))
                });



            // ----------------------------------------------------------------------------------------------------
            // Base - FreeFlying - Flying Idle
            base_FF_IdleSM = base_FF_SM.CreateStateMachineIfNotExist("Flying Idle");
            base_FF_SM.AddExitTransitionIfNotExist(base_FF_IdleSM, null);


            // Base - FreeFlying - Flying Idle - FlyingIdle_HandsOnWaist
            base_FF_IdleHandsOnWaist = base_FF_IdleSM.CreateStateIfNotExist("FlyingIdle_HandsOnWaist", flyingIdleHandsOnWaistClip);

            // Base - FreeFlying - Flying Idle - FlyingIdle_CrossArms
            base_FF_IdleCrossArms = base_FF_IdleSM.CreateStateIfNotExist("FlyingIdle_CrossArms", flyingIdleCrossArmsClip);


            // Base - MIS - FreeFlying - Flying Idle - FlyingIdle_HasWeapon
            base_FF_IdleHasWeapon = base_FF_IdleSM.CreateStateIfNotExist("FlyingIdle_HasWeapon", flyingIdleHasWeaponClip);


            // ----------------------------------------------------------------------------------------------------
            // Base - FreeFlying - Sprint Flying
            base_FF_SprintSM = base_FF_SM.CreateStateMachineIfNotExist("Sprint Flying");


            // Base - FreeFlying - Sprint Flying - SprintToFlying
            base_FF_SprintToFlying = base_FF_SprintSM.CreateStateIfNotExist("SprintToFlying", flyingSprintF2FClip);


            // Base - FreeFlying - Sprint Flying - FlyingToSprint
            base_FF_FlyingToSprint = base_FF_SprintSM.CreateStateIfNotExist("FlyingToSprint", flyingToSprintClip);

            // vAnimatorEvent
            if (!base_FF_FlyingToSprint.TryGetStateMachineBehaviour(out vAnimatorEvent base_FlyingToSprintAnimatorEvent))
                base_FlyingToSprintAnimatorEvent = base_FF_FlyingToSprint.AddStateMachineBehaviour<vAnimatorEvent>();

            vAnimatorEventTrigger base_FlyingToSprintBeginSprintAnimatorEventTrigger = new vAnimatorEventTrigger()
            {
                eventName = "BeginSprint",
                eventTriggerType = vAnimatorEventTrigger.vAnimatorEventTriggerType.NormalizedTime,
                normalizedTime = 0.6f
            };

            if (base_FlyingToSprintAnimatorEvent.eventTriggers == null)
                base_FlyingToSprintAnimatorEvent.eventTriggers = new List<vAnimatorEventTrigger>();

            if (base_FlyingToSprintAnimatorEvent.eventTriggers.Find(x => x.eventName.Equals(base_FlyingToSprintBeginSprintAnimatorEventTrigger.eventName)) == null)
                base_FlyingToSprintAnimatorEvent.eventTriggers.Add(base_FlyingToSprintBeginSprintAnimatorEventTrigger);


            // Base - FreeFlying - Sprint Flying - Sprint Flying
            base_FF_Sprint = base_FF_SprintSM.FindState("Sprint Flying");

            if (base_FF_Sprint == null)
            {
                base_FF_Sprint = base_FF_SprintSM.CreateBlendTree("Sprint Flying", out BlendTree blendTree);
                blendTree.blendType = BlendTreeType.FreeformCartesian2D;
                blendTree.blendParameter = PARAM_HORIZONTAL_INPUT;
                blendTree.blendParameterY = PARAM_VERTICAL_INPUT;

                blendTree.useAutomaticThresholds = false;
                blendTree.AddChild(flyingSprintFClip, new Vector2(0f, 0.5f));
                blendTree.AddChild(flyingSprintFL45Clip, new Vector2(-0.5f, 0.5f));
                blendTree.AddChild(flyingSprintFR45Clip, new Vector2(0.5f, 0.5f));

                base_FF_Sprint.motion = blendTree;
            }


            // ----------------------------------------------------------------------------------------------------
            // Base - FreeFlying - Sprint Flying - Sprint Roll
            base_FF_SprintRollSM = base_FF_SprintSM.CreateStateMachineIfNotExist("Sprint Roll");


            // Base - FreeFlying - Sprint Flying - FlyingSprintRoll_L
            base_FF_SprintRollL = base_FF_SprintRollSM.CreateStateIfNotExist("FlyingSprintRoll_L", flyingSprintRollLClip);

            // vAnimatorTag
            if (!base_FF_SprintRollL.TryGetStateMachineBehaviour(out vAnimatorTag base_SprintRollLAnimatorTag))
                base_SprintRollLAnimatorTag = base_FF_SprintRollL.AddStateMachineBehaviour<vAnimatorTag>();

            base_SprintRollLAnimatorTag.tags = base_SprintRollLAnimatorTag.tags.RemoveStringIfExist(TAG_CUSTOM_ACTION);
            base_SprintRollLAnimatorTag.tags = base_SprintRollLAnimatorTag.tags.AddStringIfNotExist("IsFlyingEscape");
            base_SprintRollLAnimatorTag.tags = base_SprintRollLAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_IK);
            base_SprintRollLAnimatorTag.tags = base_SprintRollLAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_HEADTRACK);

            // vTriggerSoundByState
            base_FF_SprintRollL.AddvTriggerSoundByState(
                new List<AudioClip>
                {
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Basic Locomotion/Audio/Others/rollFx.mp3"))
                });


            // Base - FreeFlying - Sprint Flying - FlyingSprintRoll_R
            base_FF_SprintRollR = base_FF_SprintRollSM.CreateStateIfNotExist("FlyingSprintRoll_R", flyingSprintRollRClip);

            // vAnimatorTag
            if (!base_FF_SprintRollR.TryGetStateMachineBehaviour(out vAnimatorTag base_SprintRollRAnimatorTag))
                base_SprintRollRAnimatorTag = base_FF_SprintRollR.AddStateMachineBehaviour<vAnimatorTag>();

            base_SprintRollRAnimatorTag.tags = base_SprintRollRAnimatorTag.tags.RemoveStringIfExist(TAG_CUSTOM_ACTION);
            base_SprintRollRAnimatorTag.tags = base_SprintRollRAnimatorTag.tags.AddStringIfNotExist("IsFlyingEscape");
            base_SprintRollRAnimatorTag.tags = base_SprintRollRAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_IK);
            base_SprintRollRAnimatorTag.tags = base_SprintRollRAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_HEADTRACK);

            // vTriggerSoundByState
            base_FF_SprintRollR.AddvTriggerSoundByState(
                new List<AudioClip>
                {
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Basic Locomotion/Audio/Others/rollFx.mp3"))
                });


            // Base - FreeFlying - Sprint Flying - FlyingSprintRoll_U
            base_FF_SprintRollU = base_FF_SprintRollSM.CreateStateIfNotExist("FlyingSprintRoll_U", flyingSprintRollUClip);

            // vAnimatorTag
            if (!base_FF_SprintRollU.TryGetStateMachineBehaviour(out vAnimatorTag base_SprintRollUAnimatorTag))
                base_SprintRollUAnimatorTag = base_FF_SprintRollU.AddStateMachineBehaviour<vAnimatorTag>();

            base_SprintRollUAnimatorTag.tags = base_SprintRollUAnimatorTag.tags.RemoveStringIfExist(TAG_CUSTOM_ACTION);
            base_SprintRollUAnimatorTag.tags = base_SprintRollUAnimatorTag.tags.AddStringIfNotExist("IsFlyingEscape");
            base_SprintRollUAnimatorTag.tags = base_SprintRollUAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_IK);
            base_SprintRollUAnimatorTag.tags = base_SprintRollUAnimatorTag.tags.AddStringIfNotExist(TAG_IGNORE_HEADTRACK);

            // vTriggerSoundByState
            base_FF_SprintRollU.AddvTriggerSoundByState(
                new List<AudioClip>
                {
                    AssetDatabase.LoadAssetAtPath<AudioClip>(Path.Combine(MISEditor.INVECTOR_ASSETS_PATH, "Basic Locomotion/Audio/Others/rollFx.mp3"))
                });
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void FreeFlyingUpperBodyLayer()
        {
            if (templateType == MISEditor.TemplateType.Basic)
                return;

            base.SetupUpperBodyLayer();


            // ----------------------------------------------------------------------------------------------------
            // Animation Clips
            // ----------------------------------------------------------------------------------------------------
            AnimationClip flyingGetHitBClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@GetHit_B.anim"));
            AnimationClip flyingGetHitFClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@GetHit_F.anim"));
            AnimationClip flyingGetHitLClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@GetHit_L.anim"));
            AnimationClip flyingGetHitRClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@GetHit_R.anim"));


            // ----------------------------------------------------------------------------------------------------
            // UpperBody - Small Hit Reaction
            upb_SmallHitReactionSM = upb_Root.CreateStateMachineIfNotExist(SMALL_HIT_REACTION);


            // ----------------------------------------------------------------------------------------------------
            // UpperBody - Small Hit Reaction - MIS-Flying Small Reaction
            upb_FF_SmallHitReactionSM = upb_SmallHitReactionSM.CreateStateMachineIfNotExist(MISFeature.MIS_PACKAGE_FREEFLYING);
            upb_SmallHitReactionSM.AddExitTransitionIfNotExist(upb_FF_SmallHitReactionSM, null);


            // UpperBody - Small Hit Reaction - MIS-Flying Small Reaction - GetHit_From_Back
            upb_FF_SmallHitReactionSM_GetHitFromBack = upb_FF_SmallHitReactionSM.CreateStateIfNotExist("GetHit_From_Back", flyingGetHitBClip);

            // UpperBody - Small Hit Reaction - MIS-Flying Small Reaction - GetHit_From_Front
            upb_FF_SmallHitReactionSM_GetHitFromFront = upb_FF_SmallHitReactionSM.CreateStateIfNotExist("GetHit_From_Front", flyingGetHitFClip);

            // UpperBody - Small Hit Reaction - MIS-Flying Small Reaction - GetHit_From_Left
            upb_FF_SmallHitReactionSM_GetHitFromLeft = upb_FF_SmallHitReactionSM.CreateStateIfNotExist("GetHit_From_Left", flyingGetHitLClip);

            // UpperBody - Small Hit Reaction - MIS-Flying Small Reaction - GetHit_From_Right
            upb_FF_SmallHitReactionSM_GetHitFromRight = upb_FF_SmallHitReactionSM.CreateStateIfNotExist("GetHit_From_Right", flyingGetHitRClip);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void FreeFlyingFullBodyLayer()
        {
            if (templateType == MISEditor.TemplateType.Basic)
                return;

            base.SetupFullBodyLayer();


            // ----------------------------------------------------------------------------------------------------
            // Animation Clips
            // ----------------------------------------------------------------------------------------------------
            AnimationClip flyingGetHitBClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@GetHit_B.anim"));
            AnimationClip flyingGetHitFClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@GetHit_F.anim"));
            AnimationClip flyingGetHitLClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@GetHit_L.anim"));
            AnimationClip flyingGetHitRClip = 
                AssetDatabase.LoadAssetAtPath<AnimationClip>(Path.Combine(MISFeature.MIS_FREEFLYING_PATH, "Runtime/Animations/Flying@GetHit_R.anim"));


            // ----------------------------------------------------------------------------------------------------
            // FullBody - Big Hit Reaction
            fb_BigHitReactionSM = fb_Root.CreateStateMachineIfNotExist(BIG_HIT_REACTION);


            // ----------------------------------------------------------------------------------------------------
            // FullBody - Big Hit Reaction - MIS-Flying Big Reaction StateMachine
            fb_FF_BigHitReactionSM = fb_BigHitReactionSM.CreateStateMachineIfNotExist(MISFeature.MIS_PACKAGE_FREEFLYING);
            fb_BigHitReactionSM.AddExitTransitionIfNotExist(fb_FF_BigHitReactionSM, null);


            // GetHit_From_Back
            fb_FF_BigHitReactionSM_GetHitFromBack = fb_FF_BigHitReactionSM.CreateStateIfNotExist("GetHit_From_Back", flyingGetHitBClip);


            // GetHit_From_Front
            fb_FF_BigHitReactionSM_GetHitFromFront = fb_FF_BigHitReactionSM.CreateStateIfNotExist("GetHit_From_Front", flyingGetHitFClip);


            // GetHit_From_Left
            fb_FF_BigHitReactionSM_GetHitFromLeft = fb_FF_BigHitReactionSM.CreateStateIfNotExist("GetHit_From_Left", flyingGetHitLClip);


            // GetHit_From_Right
            fb_FF_BigHitReactionSM_GetHitFromRight = fb_FF_BigHitReactionSM.CreateStateIfNotExist("GetHit_From_Right", flyingGetHitRClip);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void FreeFlyingAnimatorTransitions()
        {
            // ----------------------------------------------------------------------------------------------------
            // Base - Airborne - FallingSM
            // ----------------------------------------------------------------------------------------------------
            List<AnimatorStateTransition> allAny2FallingTransitions = base_Root.FindAllAnyTransition(base_FallingSM_Falling);

            for (int i = 0; i < allAny2FallingTransitions.Count; i++)
            {
                if (!allAny2FallingTransitions[i].HasCondition(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.None)))
                    allAny2FallingTransitions[i].AddCondition(AnimatorConditionMode.Equals, (int)FreeFlyingState.None, PARAM_FLYING_STATE);
            }


            // Any To FlyingHardLanding
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_GROUNDED, AnimatorConditionMode.IfNot, 0f));  // false
            conditionList.Add(Condition(PARAM_GROUND_DISTANCE, AnimatorConditionMode.Greater, 0.25f));
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.HardLanding));
            var any2SoftLand = base_Root.FindAnyTransitionIfContains(base_FF_HardLanding, conditionList);
            if (any2SoftLand == null)
                any2SoftLand = base_Root.AddAnyTransition(base_FF_HardLanding, conditionList, false, 0.75f);


            // FlyingHardLanding To FlyingHardLand
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_GROUNDED, AnimatorConditionMode.If, 0f));  // true
            conditionList.Add(Condition(PARAM_IS_SLIDING, AnimatorConditionMode.IfNot, 0f));  // false
            base_FF_HardLanding.AddTransitionIfNotExist(base_FF_HardLand, conditionList);



            // ----------------------------------------------------------------------------------------------------
            // Base - Airborne - LandingSM
            // ----------------------------------------------------------------------------------------------------

            // FlyingHardLand To Exit1
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Greater, 0.45f));
            base_FF_HardLand.AddExitTransitionIfNotExist(conditionList);


            // FlyingHardLand To Exit2
            AnimatorStateTransition softLandToExit = base_FF_HardLand.FindSameExitTransition(null);
            if (softLandToExit == null)
                base_FF_HardLand.AddExitTransition(null, true, 0.9f);


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion
            // ----------------------------------------------------------------------------------------------------

            // ----------------------------------------------------------------------------------------------------
            // MIS

            // MIS to Exit
            base_LocomotionSM.AddExitTransitionIfNotExist(base_Locomotion_MIS, null);


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion - MIS
            // ----------------------------------------------------------------------------------------------------

            // FreeFlying
            base_Locomotion_MIS.AddExitTransitionIfNotExist(base_FF_SM, null);


            // ----------------------------------------------------------------------------------------------------
            // Base - Locomotion - MIS - FreeFlying
            // ----------------------------------------------------------------------------------------------------

            // FreeFlying To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.Flying));
            base_FF_Free.AddExitTransitionIfNotExist(conditionList);


            // FreeFlying To StrafeFlying
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.If, 0f));  // true
            conditionList.Add(Condition(PARAM_IS_SPRINTING, AnimatorConditionMode.IfNot, 0f));  // false
            base_FF_Free.AddTransitionIfNotExist(base_FF_Strafe, conditionList);


            // FreeFlying To FlyingIdle
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Less, 0.25f));
            base_FF_Free.AddTransitionIfNotExist(base_FF_IdleSM, conditionList);


            // FreeFlying To FlyingToSprint
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_SPRINTING, AnimatorConditionMode.If, 0f));  // true
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.IfNot, 0f));   // false
            conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Greater, 0.25f));
            base_FF_Free.AddTransitionIfNotExist(base_FF_FlyingToSprint, conditionList);


            // ----------------------------------------------------------------------------------------------------
            // Base - FreeFlying - StrafeFlying

            // StrafeFlying To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.Flying));
            base_FF_Strafe.AddExitTransitionIfNotExist(conditionList);


            // StrafeFlying To FreeFlying
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.IfNot, 0f));  // false
            conditionList.Add(Condition(PARAM_IS_SPRINTING, AnimatorConditionMode.IfNot, 0f));  // false
            conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Greater, 0.25f));  // false
            base_FF_Strafe.AddTransitionIfNotExist(base_FF_Free, conditionList);


            // StrafeFlying To FlyingIdle
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.IfNot, 0f));  // false
            conditionList.Add(Condition(PARAM_IS_SPRINTING, AnimatorConditionMode.IfNot, 0f));  // false
            conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Less, 0.25f));  // false
            base_FF_Strafe.AddTransitionIfNotExist(base_FF_IdleSM, conditionList);


            // StrafeFlying To FlyingToSprint
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.IfNot, 0f));  // false
            conditionList.Add(Condition(PARAM_IS_SPRINTING, AnimatorConditionMode.If, 0f));  // true
            base_FF_Strafe.AddTransitionIfNotExist(base_FF_FlyingToSprint, conditionList);


            // ----------------------------------------------------------------------------------------------------
            // Base - FreeFlying - StrafeEscape
            base_FF_SM.AddTransitionIfNotExist(base_FF_StrafeEscapeSM, base_FF_Strafe, null);


            // FlyingEscape_L To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.None));
            base_FF_EscapeL.AddExitTransitionIfNotExist(conditionList);
            base_FF_EscapeL.AddExitTransitionIfNotExist(null, true);


            // FlyingEscape_R To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.None));
            base_FF_EscapeR.AddExitTransitionIfNotExist(conditionList);
            base_FF_EscapeR.AddExitTransitionIfNotExist(null, true, 0.625f);


            // FlyingEscape_F To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.None));
            base_FF_EscapeF.AddExitTransitionIfNotExist(conditionList);
            base_FF_EscapeF.AddExitTransitionIfNotExist(null, true, 0.625f);


            // FlyingEscape_B To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.None));
            base_FF_EscapeB.AddExitTransitionIfNotExist(conditionList);
            base_FF_EscapeB.AddExitTransitionIfNotExist(null, true, 0.625f);


            // ----------------------------------------------------------------------------------------------------
            // Base - FreeFlying - FlyingIdle

            // FlyingIdle_HandsOnWaist To FlyingIdle_CrossArms
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IDLERANDOM, AnimatorConditionMode.Equals, 1));
            base_FF_IdleHandsOnWaist.AddTransitionIfNotExist(base_FF_IdleCrossArms, conditionList);


            // FlyingIdle_HandsOnWaist To FlyingIdle_HasWeapon
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IDLERANDOM, AnimatorConditionMode.Equals, 2));
            base_FF_IdleHandsOnWaist.AddTransitionIfNotExist(base_FF_IdleHasWeapon, conditionList);


            // FlyingIdle_HandsOnWaist To FreeFlying
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Greater, 0.1f));
            base_FF_IdleHandsOnWaist.AddTransitionIfNotExist(base_FF_Free, conditionList);


            // FlyingIdle_HandsOnWaist To StrafeFlying
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.If, 0f)); // true
            conditionList.Add(Condition(PARAM_IS_SPRINTING, AnimatorConditionMode.IfNot, 0f)); // false
            base_FF_IdleHandsOnWaist.AddTransitionIfNotExist(base_FF_Strafe, conditionList);


            // FlyingIdle_HandsOnWaist To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.None));
            base_FF_IdleHandsOnWaist.AddExitTransitionIfNotExist(conditionList);


            // FlyingIdle_CrossArms To FlyingIdle_HandsOnWaist
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IDLERANDOM, AnimatorConditionMode.Equals, 0));
            base_FF_IdleCrossArms.AddTransitionIfNotExist(base_FF_IdleHandsOnWaist, conditionList);


            // FlyingIdle_CrossArms To FlyingIdle_HasWeapon
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IDLERANDOM, AnimatorConditionMode.Equals, 2));
            base_FF_IdleCrossArms.AddTransitionIfNotExist(base_FF_IdleHasWeapon, conditionList);


            // FlyingIdle_CrossArms To FreeFlying
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Greater, 0.1f));
            base_FF_IdleCrossArms.AddTransitionIfNotExist(base_FF_Free, conditionList);


            // FlyingIdle_CrossArms To StrafeFlying
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.If, 0f)); // true
            conditionList.Add(Condition(PARAM_IS_SPRINTING, AnimatorConditionMode.IfNot, 0f)); // false
            base_FF_IdleCrossArms.AddTransitionIfNotExist(base_FF_Strafe, conditionList);


            // FlyingIdle_CrossArms To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.None));
            base_FF_IdleCrossArms.AddExitTransitionIfNotExist(conditionList);


            // FlyingIdle_HasWeapon To FlyingIdle_HandsOnWaist
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IDLERANDOM, AnimatorConditionMode.Equals, 0f));
            base_FF_IdleHasWeapon.AddTransitionIfNotExist(base_FF_IdleHandsOnWaist, conditionList);


            // FlyingIdle_HasWeapon To FlyingIdle_CrossArms
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IDLERANDOM, AnimatorConditionMode.Equals, 1));
            base_FF_IdleHasWeapon.AddTransitionIfNotExist(base_FF_IdleCrossArms, conditionList);


            // FlyingIdle_HasWeapon To FreeFlying
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_INPUT_MAGNITUDE, AnimatorConditionMode.Greater, 0.1f));
            base_FF_IdleHasWeapon.AddTransitionIfNotExist(base_FF_Free, conditionList);


            // FlyingIdle_HasWeapon To StrafeFlying
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.If, 0f)); // true
            conditionList.Add(Condition(PARAM_IS_SPRINTING, AnimatorConditionMode.IfNot, 0f)); // false
            base_FF_IdleHasWeapon.AddTransitionIfNotExist(base_FF_Strafe, conditionList);


            // FlyingIdle_HasWeapon To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.None));
            base_FF_IdleHasWeapon.AddExitTransitionIfNotExist(conditionList);


            // ----------------------------------------------------------------------------------------------------
            // Base - FreeFlying - SprintFlying

            // SprintToFlying To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.Flying));
            base_FF_SprintToFlying.AddExitTransitionIfNotExist(conditionList);


            // SprintToFlying To Exit
            base_FF_SprintToFlying.AddExitTransitionIfNotExist(null, true, 0.75f);


            // FlyingToSprint To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.Flying));
            base_FF_FlyingToSprint.AddExitTransitionIfNotExist(conditionList);


            // FlyingToSprint To SprintFlying
            base_FF_FlyingToSprint.AddTransitionIfNotExist(base_FF_Sprint, null, true, 0.625f);


            // SprintFlying To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_SPRINTING, AnimatorConditionMode.IfNot, 0.5f)); // false
            conditionList.Add(Condition(PARAM_INPUT_VERTICAL, AnimatorConditionMode.Less, 0.5f));
            base_FF_Sprint.AddExitTransitionIfNotExist(conditionList);


            // SprintFlying To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.Flying));
            base_FF_Sprint.AddExitTransitionIfNotExist(conditionList);


            // SprintFlying To SprintToFlying
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_SPRINTING, AnimatorConditionMode.IfNot, 0f)); // false
            conditionList.Add(Condition(PARAM_INPUT_VERTICAL, AnimatorConditionMode.Greater, 0.5f));
            base_FF_Sprint.AddTransitionIfNotExist(base_FF_SprintToFlying, conditionList);


            // SprintFlying To SprintToFlying
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_IS_STRAFING, AnimatorConditionMode.If, 0f)); // true
            base_FF_Sprint.AddTransitionIfNotExist(base_FF_SprintToFlying, conditionList);


            // ----------------------------------------------------------------------------------------------------
            // Base - MIS-FreeFlying - SprintRoll SubStateMachine
            base_FF_SprintSM.AddTransitionIfNotExist(base_FF_SprintRollSM, base_FF_Sprint, null);


            // FlyingSprintRoll_L To Exit
            base_FF_SprintRollL.AddExitTransitionIfNotExist(null, true, 0.85f);


            // FlyingSprintRoll_L To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.None));
            base_FF_SprintRollL.AddExitTransitionIfNotExist(conditionList);


            // FlyingSprintRoll_R To Exit
            base_FF_SprintRollR.AddExitTransitionIfNotExist(null, true, 0.85f);


            // FlyingSprintRoll_R To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.None));
            base_FF_SprintRollR.AddExitTransitionIfNotExist(conditionList);


            // FlyingSprintRoll_U To Exit
            base_FF_SprintRollU.AddExitTransitionIfNotExist(null, true, 0.85f);


            // FlyingSprintRoll_U To Exit
            conditionList.Clear();
            conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.None));
            base_FF_SprintRollU.AddExitTransitionIfNotExist(conditionList);


#if INVECTOR_SHOOTER || INVECTOR_MELEE
            if (templateType == MISEditor.TemplateType.Melee || templateType == MISEditor.TemplateType.Shooter)
            {
                // ----------------------------------------------------------------------------------------------------
                // UpperBody - Small Hit Reaction

                // UpperBody - Small Hit Reaction - MIS-Flying Small Reaction - GetHit_From_Back AnimatorState
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_TRIGGER_REACTION, AnimatorConditionMode.If, 0)); // Trigger
                conditionList.Add(Condition(PARAM_HIT_DIRECTION, AnimatorConditionMode.Equals, 180f));
                conditionList.Add(Condition(PARAM_REACTION_ID, AnimatorConditionMode.Equals, 0));
                conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.None));
                upb_Root.AddAnyTransitionIfNotExist(upb_FF_SmallHitReactionSM_GetHitFromBack, conditionList);

                upb_FF_SmallHitReactionSM_GetHitFromBack.AddExitTransitionIfNotExist(null, true);

                // UpperBody - Small Hit Reaction - MIS-Flying Small Reaction - GetHit_From_Front state
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_TRIGGER_REACTION, AnimatorConditionMode.If, 0)); // Trigger
                conditionList.Add(Condition(PARAM_HIT_DIRECTION, AnimatorConditionMode.Equals, 0f));
                conditionList.Add(Condition(PARAM_REACTION_ID, AnimatorConditionMode.Equals, 0));
                conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.None));
                upb_Root.AddAnyTransitionIfNotExist(upb_FF_SmallHitReactionSM_GetHitFromFront, conditionList);

                upb_FF_SmallHitReactionSM_GetHitFromFront.AddExitTransitionIfNotExist(null, true);

                // UpperBody - Small Hit Reaction - MIS-Flying Small Reaction - GetHit_From_Left state
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_TRIGGER_REACTION, AnimatorConditionMode.If, 0)); // Trigger
                conditionList.Add(Condition(PARAM_HIT_DIRECTION, AnimatorConditionMode.Equals, -90f));
                conditionList.Add(Condition(PARAM_REACTION_ID, AnimatorConditionMode.Equals, 0));
                conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.None));
                upb_Root.AddAnyTransitionIfNotExist(upb_FF_SmallHitReactionSM_GetHitFromLeft, conditionList);

                upb_FF_SmallHitReactionSM_GetHitFromLeft.AddExitTransitionIfNotExist(null, true);

                // UpperBody - Small Hit Reaction - MIS-Flying Small Reaction - GetHit_From_Right state
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_TRIGGER_REACTION, AnimatorConditionMode.If, 0)); // Trigger
                conditionList.Add(Condition(PARAM_HIT_DIRECTION, AnimatorConditionMode.Equals, 90f));
                conditionList.Add(Condition(PARAM_REACTION_ID, AnimatorConditionMode.Equals, 0));
                conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.None));
                upb_Root.AddAnyTransitionIfNotExist(upb_FF_SmallHitReactionSM_GetHitFromRight, conditionList);

                upb_FF_SmallHitReactionSM_GetHitFromRight.AddExitTransitionIfNotExist(null, true);


                // ----------------------------------------------------------------------------------------------------
                // FullBody
                // ----------------------------------------------------------------------------------------------------

                // FullBody - Big Hit Reaction - MIS-Flying Big Reaction - GetHit_From_Back
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_TRIGGER_REACTION, AnimatorConditionMode.If, 0)); // Trigger
                conditionList.Add(Condition(PARAM_HIT_DIRECTION, AnimatorConditionMode.Equals, 180f));
                conditionList.Add(Condition(PARAM_REACTION_ID, AnimatorConditionMode.Equals, 0));
                conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.None));
                fb_Root.AddAnyTransitionIfNotExist(fb_FF_BigHitReactionSM_GetHitFromBack, conditionList);

                fb_FF_BigHitReactionSM_GetHitFromBack.AddExitTransitionIfNotExist(null, true);

                // FullBody - Big Hit Reaction - MIS-Flying Big Reaction - GetHit_From_Front
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_TRIGGER_REACTION, AnimatorConditionMode.If, 0)); // Trigger
                conditionList.Add(Condition(PARAM_HIT_DIRECTION, AnimatorConditionMode.Equals, 0f));
                conditionList.Add(Condition(PARAM_REACTION_ID, AnimatorConditionMode.Equals, 0));
                conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.None));
                fb_Root.AddAnyTransitionIfNotExist(fb_FF_BigHitReactionSM_GetHitFromFront, conditionList);

                fb_FF_BigHitReactionSM_GetHitFromFront.AddExitTransitionIfNotExist(null, true);

                // FullBody - Big Hit Reaction - MIS-Flying Big Reaction - GetHit_From_Left
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_TRIGGER_REACTION, AnimatorConditionMode.If, 0)); // Trigger
                conditionList.Add(Condition(PARAM_HIT_DIRECTION, AnimatorConditionMode.Equals, -90f));
                conditionList.Add(Condition(PARAM_REACTION_ID, AnimatorConditionMode.Equals, 0));
                conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.None));
                fb_Root.AddAnyTransitionIfNotExist(fb_FF_BigHitReactionSM_GetHitFromLeft, conditionList);

                fb_FF_BigHitReactionSM_GetHitFromLeft.AddExitTransitionIfNotExist(null, true);

                // FullBody - Big Hit Reaction - MIS-Flying Big Reaction - GetHit_From_Right
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_TRIGGER_REACTION, AnimatorConditionMode.If, 0)); // Trigger
                conditionList.Add(Condition(PARAM_HIT_DIRECTION, AnimatorConditionMode.Equals, 90f));
                conditionList.Add(Condition(PARAM_REACTION_ID, AnimatorConditionMode.Equals, 0));
                conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.NotEqual, (int)FreeFlyingState.None));
                fb_Root.AddAnyTransitionIfNotExist(fb_FF_BigHitReactionSM_GetHitFromRight, conditionList);

                fb_FF_BigHitReactionSM_GetHitFromRight.AddExitTransitionIfNotExist(null, true);
            }
#endif

#if MIS_AIRDASH
            AnimatorState base_AirDash = base_ActionsSM.FindState("AirDash");
            if (base_AirDash != null)
            {
                // AirDash To FreeFlying
                conditionList.Clear();
                conditionList.Add(Condition(PARAM_IS_AIRDASH, AnimatorConditionMode.IfNot, 0)); // false
                conditionList.Add(Condition(PARAM_FLYING_STATE, AnimatorConditionMode.Equals, (int)FreeFlyingState.Flying));
                base_AirDash.AddTransitionIfNotExist(base_FF_Free, conditionList);
            }
#endif
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void FreeFlyingPosition()
        {
            // ----------------------------------------------------------------------------------------------------
            // Base Layer
            // ----------------------------------------------------------------------------------------------------


            // ----------------------------------------------------------------------------------------------------
            // Locomotion
            base_LocomotionSM.SetStateMachinePosition(base_Locomotion_MIS, BASE_LOCOMOTION_MIS_POS);
            base_Locomotion_MIS.SetDefaultLayerAllPosition();
            base_Locomotion_MIS.ArrangeStatemachines(0);


            // ----------------------------------------------------------------------------------------------------
            // Locomotion - MIS - FreeFlying
            base_FF_SM.SetDefaultLayerPosition();
            base_FF_SM.SetExitPosition(STATE_POS + (Vector3.right * 20));
            base_FF_SM.SetParentStateMachinePosition(ZERO_POS);

            base_FF_SM.SetStateRelativePosition(base_FF_Free, 2, -1);
            base_FF_SM.SetStateRelativePosition(base_FF_Strafe, 2, 1);
            base_FF_SM.SetStateMachineRelativePosition(base_FF_StrafeEscapeSM, 2, 2);
            base_FF_SM.SetStateMachineRelativePosition(base_FF_IdleSM, 4, 0);
            base_FF_SM.SetStateMachineRelativePosition(base_FF_SprintSM, 7, 0);
            base_FF_SM.SetStateMachineRelativePosition(base_FF_SprintRollSM, 7, 0);

            // ----------------------------------------------------------------------------------------------------
            // Strafe Escape
            base_FF_StrafeEscapeSM.SetDefaultLayerAllPosition();

            base_FF_StrafeEscapeSM.SetStateRelativePosition(base_FF_EscapeL, 0, -2);
            base_FF_StrafeEscapeSM.SetStateRelativePosition(base_FF_EscapeR, 0, -1);
            base_FF_StrafeEscapeSM.SetStateRelativePosition(base_FF_EscapeF, 0, 0);
            base_FF_StrafeEscapeSM.SetStateRelativePosition(base_FF_EscapeB, 0, 1);

            // ----------------------------------------------------------------------------------------------------
            // Flying Idle
            base_FF_IdleSM.SetDefaultLayerPosition(BASE_LD_POS, STATE_POS + (Vector3.right * 20) + (Vector3.up * 170));
            base_FF_IdleSM.SetParentStateMachinePosition(ZERO_POS);

            base_FF_IdleSM.SetStateRelativePosition(base_FF_IdleHandsOnWaist, 0, -1);
            base_FF_IdleSM.SetStateRelativePosition(base_FF_IdleHasWeapon, 3, 0);
            base_FF_IdleSM.SetStateRelativePosition(base_FF_IdleCrossArms, 0, 1);

            // ----------------------------------------------------------------------------------------------------
            // Sprint Flying
            base_FF_SprintSM.SetDefaultLayerPosition(BASE_LD_POS, BASE_RM_POS);
            base_FF_SprintSM.SetParentStateMachinePosition(ZERO_POS);

            base_FF_SprintSM.SetStateRelativePosition(base_FF_FlyingToSprint, 0, -1);
            base_FF_SprintSM.SetStateRelativePosition(base_FF_Sprint, 0, 0);
            base_FF_SprintSM.SetStateRelativePosition(base_FF_SprintToFlying, 0, 1);
            base_FF_SprintSM.SetStateMachineRelativePosition(base_FF_SprintRollSM, 3, -1);

            // ----------------------------------------------------------------------------------------------------
            // Sprint Roll
            base_FF_SprintRollSM.SetDefaultLayerPosition(BASE_LD_POS, BASE_RM_POS);
            base_FF_SprintRollSM.SetParentStateMachinePosition(ZERO_POS);

            base_FF_SprintRollSM.SetStateRelativePosition(base_FF_SprintRollL, 0, -1);
            base_FF_SprintRollSM.SetStateRelativePosition(base_FF_SprintRollR, 0, 0);
            base_FF_SprintRollSM.SetStateRelativePosition(base_FF_SprintRollU, 0, 1);


#if INVECTOR_SHOOTER || INVECTOR_MELEE
            if (templateType == MISEditor.TemplateType.Melee || templateType == MISEditor.TemplateType.Shooter)
            {
                // ----------------------------------------------------------------------------------------------------
                // UpperBody Layer
                // ----------------------------------------------------------------------------------------------------
                upb_SmallHitReactionSM.SetDefaultLayerAllPosition();

                upb_SmallHitReactionSM.FindStateSetRelativePosition(SMALL_FROM_BACK, 0, -3);
                upb_SmallHitReactionSM.FindStateSetRelativePosition(SMALL_FROM_FRONT, 0, -2);
                upb_SmallHitReactionSM.FindStateSetRelativePosition(SMALL_FROM_LEFT, 0, -1);
                upb_SmallHitReactionSM.FindStateSetRelativePosition(SMALL_FROM_RIGHT, 0, 0);

                Vector3 smallFromRightPosition = upb_SmallHitReactionSM.FindGetStatePosition(SMALL_FROM_RIGHT);
                upb_SmallHitReactionSM.SetStateMachinePosition(upb_FF_SmallHitReactionSM, smallFromRightPosition + (Vector3.up * VERTICAL_GAP));

                upb_FF_SmallHitReactionSM.SetDefaultLayerAllPosition();

                upb_FF_SmallHitReactionSM.FindStateSetRelativePosition("GetHit_From_Back", 0, -3);
                upb_FF_SmallHitReactionSM.FindStateSetRelativePosition("GetHit_From_Front", 0, -2);
                upb_FF_SmallHitReactionSM.FindStateSetRelativePosition("GetHit_From_Left", 0, -1);
                upb_FF_SmallHitReactionSM.FindStateSetRelativePosition("GetHit_From_Right", 0, 0);


                // ----------------------------------------------------------------------------------------------------
                // FullBody Layer
                // ----------------------------------------------------------------------------------------------------
                fb_BigHitReactionSM.SetDefaultLayerAllPosition();
                
                fb_BigHitReactionSM.FindStateSetRelativePosition(BIG_FROM_BACK, 0, -3);
                fb_BigHitReactionSM.FindStateSetRelativePosition(BIG_FROM_FRONT, 0, -2);
                fb_BigHitReactionSM.FindStateSetRelativePosition(BIG_FROM_LEFT, 0, -1);
                fb_BigHitReactionSM.FindStateSetRelativePosition(BIG_FROM_RIGHT, 0, 0);

                Vector3 bigFromRightPosition = fb_BigHitReactionSM.FindGetStatePosition(BIG_FROM_RIGHT);
                fb_BigHitReactionSM.SetStateMachinePosition(fb_FF_BigHitReactionSM, bigFromRightPosition + (Vector3.up * VERTICAL_GAP));

                fb_FF_BigHitReactionSM.SetDefaultLayerAllPosition();
                
                fb_FF_BigHitReactionSM.FindStateSetRelativePosition("GetHit_From_Back", 0, -3);
                fb_FF_BigHitReactionSM.FindStateSetRelativePosition("GetHit_From_Front", 0, -2);
                fb_FF_BigHitReactionSM.FindStateSetRelativePosition("GetHit_From_Left", 0, -1);
                fb_FF_BigHitReactionSM.FindStateSetRelativePosition("GetHit_From_Right", 0, 0);
            }
#endif
        }
#endif
    }
}
