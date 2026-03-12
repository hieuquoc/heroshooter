using Invector;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class mvBoxCast : mvCast
    {
        [vHideInInspector("useLocalDirection", false)] public Vector3 localEulerAngles;

        [Space]
        public Vector3 halfExtents;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public mvBoxCast() : base()
        {
            halfExtents = new Vector3(1f, 0.1f, 0f);
        }
        public mvBoxCast(Vector3 castOffset, float backOffset, float maxDistance, Vector3 halfExtents) : base(castOffset, backOffset, maxDistance)
        {
            this.halfExtents = halfExtents;
        }
        public mvBoxCast(Vector3 castOffset, float backOffset, float maxDistance, bool useLocalDirection, Vector3 localDirection, Vector3 localEulerAngles, Vector3 halfExtents) : this(castOffset, backOffset, maxDistance, halfExtents)
        {
            this.useLocalDirection = useLocalDirection;
            this.localDirection = localDirection;
            this.localEulerAngles = localEulerAngles;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual Vector3 GetP1Position(Transform tr)
        {
            Vector3 direction = tr.TransformDirection(localDirection);
            return tr.TransformPoint(castOffset) + (backOffset * -direction);
        }

        // ----------------------------------------------------------------------------------------------------
        // The localDirection version
        // ----------------------------------------------------------------------------------------------------
        public bool Cast(
            Transform tr, 
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 direction = tr.TransformDirection(localDirection);
            Quaternion orientation = Quaternion.Euler(localEulerAngles);
            Vector3 p1 = tr.TransformPoint(castOffset) + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetClosestHit(Physics.BoxCastNonAlloc(p1, halfExtents, direction, hits, orientation, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance - backOffset;
                hitObject = hit.collider.gameObject;
            }
            else
            {
                isDetected = false;
                distance = 0f;
                hitObject = null;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
                this.DrawBoxCast(p1, halfExtents, orientation, Vector3.one, direction);
#endif

            return isDetected;
        }

        // ----------------------------------------------------------------------------------------------------
        // The dedicated direction version
        // ----------------------------------------------------------------------------------------------------
        public bool Cast(
            Transform tr, Quaternion orientation, Vector3 direction,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = tr.TransformPoint(castOffset) + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetClosestHit(Physics.BoxCastNonAlloc(p1, halfExtents, direction, hits, orientation, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance - backOffset;
                hitObject = hit.collider.gameObject;
            }
            else
            {
                isDetected = false;
                distance = 0f;
                hitObject = null;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
                this.DrawBoxCast(p1, halfExtents, orientation, Vector3.one, direction);
#endif

            return isDetected;
        }
        /*
        public bool Cast(
            Vector3 position, Quaternion orientation, Vector3 lossyScale, Vector3 direction,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = position + (backOffset * -direction);

            if (GetClosestHit(Physics.BoxCastNonAlloc(p1, halfExtents, direction, hits, orientation, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance - backOffset;
                hitObject = hit.collider.gameObject;
            }
            else
            {
                isDetected = false;
                distance = 0f;
                hitObject = null;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
            {
                this.DrawBoxCast(p1, halfExtents, orientation, lossyScale, direction);
            }
#endif

            return isDetected;
        }
        */

        /*
        // ----------------------------------------------------------------------------------------------------
        // Detects objects that meet given conditions. If there is an object blocking the target object, it fails.
        // ----------------------------------------------------------------------------------------------------
        public bool CastBlocked(
            Transform tr, Quaternion orientation, Vector3 lossyScale, Vector3 direction,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = tr.TransformPoint(castOffset) + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetBlockedClosestHit(Physics.BoxCastNonAlloc(p1, halfExtents, direction, hits, orientation, backOffset + maxDistance, Physics.AllLayers, query), tagMask, layerMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance - backOffset;
                hitObject = hit.collider.gameObject;
            }
            else
            {
                isDetected = false;
                distance = 0f;
                hitObject = null;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
            {
                this.DrawBoxCast(p1, halfExtents, orientation, lossyScale, direction);
            }
#endif

            return isDetected;
        }
        */
        /*
        public bool CastBlocked(
            Vector3 position, Quaternion orientation, Vector3 lossyScale, Vector3 direction,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = position + (backOffset * -direction);

            if (GetBlockedClosestHit(Physics.BoxCastNonAlloc(p1, halfExtents, direction, hits, orientation, backOffset + maxDistance, Physics.AllLayers, query), tagMask, layerMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance - backOffset;
                hitObject = hit.collider.gameObject;
            }
            else
            {
                isDetected = false;
                distance = 0f;
                hitObject = null;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
            {
                this.DrawBoxCast(p1, halfExtents, orientation, lossyScale, direction);
            }
#endif

            return isDetected;
        }
        */
    }
}