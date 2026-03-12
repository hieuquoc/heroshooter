using Invector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [vClassHeader("Wind Force", iconName = "misIconRed")]
    public class mvWindForce : vMonoBehaviour
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("Pysical Force")]
        public bool usePhysicalForce = true;
        public float forceRadius = 2f;
        public int maxForceTargets = 10;
        public float force = 300f;
        public float continuousForceDelay = 1f;
        public LayerMask forceTargetLayerMaks = 1 << MISRuntimeTagLayer.LAYER_DEFAULT;
        List<mvWindForceTarget> forceTargetList = new List<mvWindForceTarget>();
        Collider[] forceTargetCollider;
        int defaultForceTargetCount = 20;

        // ----------------------------------------------------------------------------------------------------
        // 
        [vEditorToolbar("VFX")]
        public GameObject onGroundVfx;
        public GameObject onAirVfx;

        // ----------------------------------------------------------------------------------------------------
        // 
        float originalForce;
        int originalMaxForceTargets;
        Coroutine coroutine = null;
        WaitForSeconds delaySeconds;

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void Start()
        {
            originalForce = force;
            originalMaxForceTargets = maxForceTargets;

            if (onGroundVfx != null)
                onGroundVfx.SetActive(false);

            if (onAirVfx != null)
                onAirVfx.SetActive(false);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void OnDisable()
        {
            StopWindForce();
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void SetForceMultiplier(float multiplier = 1.5f)
        {
            force *= multiplier;
        }
        public void ResetForce()
        {
            force = originalForce;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void SetMaxForceTargetsMultiplier(float multiplier = 1.5f)
        {
            maxForceTargets = (int)(maxForceTargets * multiplier);
        }
        public void ResetMaxForceTargets()
        {
            maxForceTargets = originalMaxForceTargets;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void StartOneTimeForce(bool fromGround = true)
        {
            if (fromGround)
                onGroundVfx.SetActive(true);
            else
                onAirVfx.SetActive(true);

            if (usePhysicalForce && FindWindStormTargets())
            {
                ResetForce();
                ResetMaxForceTargets();
                ApplyForce();
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void StartContinuousForce(bool fromGround = true)
        {
            if (fromGround)
                onGroundVfx.SetActive(true);
            else
                onAirVfx.SetActive(true);

            if (usePhysicalForce)
            {
                ResetForce();
                ResetMaxForceTargets();

                if (coroutine != null)
                    StopCoroutine(coroutine);
                coroutine = StartCoroutine(WindForce());
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public void StopWindForce()
        {
            force = originalForce;
            maxForceTargets = originalMaxForceTargets;

            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        bool FindWindStormTargets()
        {
            forceTargetCollider = new Collider[defaultForceTargetCount];

            int count = Physics.OverlapSphereNonAlloc(transform.position, forceRadius, forceTargetCollider, forceTargetLayerMaks);
            if (count <= 0)
                return false;

            forceTargetList.Clear();

            for (int i = 0; i < count; i++)
            {
                forceTargetCollider[i].gameObject.TryGetComponent(out mvWindForceTarget windForceTarget);
                if (windForceTarget == null)
                    continue;

                forceTargetList.Add(windForceTarget);

                if (forceTargetList.Count >= maxForceTargets)
                    break;
            }

            return forceTargetList.Count > 0;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        IEnumerator WindForce()
        {
            yield return null;
            delaySeconds = new WaitForSeconds(continuousForceDelay);

            while (true)
            {
                if (FindWindStormTargets())
                    ApplyForce();

                yield return delaySeconds;
            }
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        void ApplyForce()
        {
            for (int i = 0; i < forceTargetList.Count; i++)
            {
                mvWindForceTarget windForceTarget = forceTargetList[i];

                if (windForceTarget != null)
                {
                    Vector3 direction = windForceTarget.Position - transform.position;
                    direction = Vector3.ProjectOnPlane(direction, Vector3.up);

                    windForceTarget.WindStorm(transform.position, direction.normalized, force);
                }
            }
        }
    }
}