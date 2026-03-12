using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class mvSphereCast : mvCast
    {
        [Space]
        [Min(0f)] public float radius = 0.1f;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public mvSphereCast() : base()
        {
        }
        public mvSphereCast(float radius) : base()
        {
            this.radius = radius;
        }
        public mvSphereCast(float radius, Vector3 castOffset, float backOffset, float maxDistance) : base(castOffset, backOffset, maxDistance)
        {
            this.radius = radius;
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
            vTagMask tagMask, int layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 direction = tr.TransformDirection(localDirection);
            Vector3 p1 = tr.TransformPoint(castOffset) + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetClosestHit(Physics.SphereCastNonAlloc(p1, radius, direction, hits, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance + radius - backOffset;
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
                MISDebugDrawExt.Point(p1, radius, drawGizmo.color);
                this.DrawSphereCast(p1, direction);
            }
#endif

            return isDetected;
        }

        // ----------------------------------------------------------------------------------------------------
        // The dedicated direction version
        // ----------------------------------------------------------------------------------------------------
        public bool Cast(
            Transform tr, Vector3 direction,
            vTagMask tagMask, int layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = tr.TransformPoint(castOffset) + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetClosestHit(Physics.SphereCastNonAlloc(p1, radius, direction, hits, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance + radius - backOffset;
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
                MISDebugDrawExt.Point(p1, radius, drawGizmo.color);
                this.DrawSphereCast(p1, direction);
            }
#endif

            return isDetected;
        }
        public bool Cast(
            Vector3 position, Vector3 direction,
            vTagMask tagMask, int layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = position + castOffset + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetClosestHit(Physics.SphereCastNonAlloc(p1, radius, direction, hits, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance + radius - backOffset;
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
                MISDebugDrawExt.Point(p1, radius, drawGizmo.color);
                this.DrawSphereCast(p1, direction);
            }
#endif

            return isDetected;
        }

        /*
        // ----------------------------------------------------------------------------------------------------
        // Detects objects that meet given conditions. If there is an object blocking the target object, it fails.
        // ----------------------------------------------------------------------------------------------------
        public bool CastBlocked(
            Transform tr, Vector3 direction,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = tr.TransformPoint(castOffset) + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetBlockedClosestHit(Physics.SphereCastNonAlloc(p1, radius, direction, hits, backOffset + maxDistance, Physics.AllLayers, query), tagMask, layerMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance + radius - backOffset;
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
                MISDebugDrawExt.Point(p1, radius, drawGizmo.color);
                this.DrawSphereCast(p1, direction);
            }
#endif

            return isDetected;
        }
        public bool CastBlocked(
            Vector3 position, Vector3 direction,
            vTagMask tagMask, int layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = position + castOffset + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetBlockedClosestHit(Physics.SphereCastNonAlloc(p1, radius, direction, hits, backOffset + maxDistance, Physics.AllLayers, query), tagMask, layerMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance + radius - backOffset;
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
                MISDebugDrawExt.Point(p1, radius, drawGizmo.color);
                this.DrawSphereCast(p1, direction);
            }
#endif

            return isDetected;
        }
        */
    }
}