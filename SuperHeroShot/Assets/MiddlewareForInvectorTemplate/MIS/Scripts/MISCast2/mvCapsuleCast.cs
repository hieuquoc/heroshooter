using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class mvCapsuleCast : mvCast
    {
        [Space]
        [Min(0f)] public float radius = 0.1f;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public mvCapsuleCast() : base()
        {
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual Vector3 GetP1Position(Transform tr, float capsuleHeight)
        {
            SetCapsuleDimension(tr.position, radius, capsuleHeight, out Vector3 topCenter, out Vector3 bottomCenter);

            Vector3 direction = tr.TransformDirection(localDirection);
            return topCenter + (backOffset * -direction);
        }

        // ----------------------------------------------------------------------------------------------------
        // The localDirection version
        // ----------------------------------------------------------------------------------------------------
        public bool Cast(
            Transform tr, Vector3 upVector, float capsuleHeight,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            SetCapsuleDimension(tr.position, radius, capsuleHeight, out Vector3 topCenter, out Vector3 bottomCenter);

            Vector3 direction = tr.TransformDirection(localDirection);
            Vector3 p1 = topCenter + (backOffset * -direction);
            Vector3 p2 = bottomCenter + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetClosestHit(Physics.CapsuleCastNonAlloc(p1, p2, radius, direction, hits, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
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
                MISDebugDrawExt.Point(p2, radius, drawGizmo.color);

                this.DrawCapsuleCast(p1, p2, direction);
            }
#endif

            return isDetected;
        }

        // ----------------------------------------------------------------------------------------------------
        // The dedicated direction version
        // ----------------------------------------------------------------------------------------------------
        public bool Cast(
            Transform tr, Vector3 upVector, Vector3 direction, float capsuleHeight,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            SetCapsuleDimension(tr.position, radius, capsuleHeight, out Vector3 topCenter, out Vector3 bottomCenter);

            Vector3 p1 = topCenter + (backOffset * -direction);
            Vector3 p2 = bottomCenter + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetClosestHit(Physics.CapsuleCastNonAlloc(p1, p2, radius, direction, hits, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
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
                MISDebugDrawExt.Point(p2, radius, drawGizmo.color);

                this.DrawCapsuleCast(p1, p2, direction);
            }
#endif

            return isDetected;
        }
        public bool Cast(
            Vector3 position, Vector3 upVector, Vector3 direction, float capsuleHeight,
            vTagMask tagMask, int layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            SetCapsuleDimension(position, radius, capsuleHeight, out Vector3 topCenter, out Vector3 bottomCenter);

            Vector3 p1 = topCenter + (backOffset * -direction);
            Vector3 p2 = bottomCenter + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetClosestHit(Physics.CapsuleCastNonAlloc(p1, p2, radius, direction, hits, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
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
                MISDebugDrawExt.Point(p2, radius, drawGizmo.color);

                this.DrawCapsuleCast(p1, p2, direction);
            }
#endif

            return isDetected;
        }

        /*
        // ----------------------------------------------------------------------------------------------------
        // Detects objects that meet given conditions. If there is an object blocking the target object, it fails.
        // ----------------------------------------------------------------------------------------------------
        public bool CastBlocked(
            Transform tr, Vector3 upVector, Vector3 direction, float capsuleHeight,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            SetCapsuleDimension(tr.position, radius, capsuleHeight, out Vector3 topCenter, out Vector3 bottomCenter);

            Vector3 p1 = topCenter + (backOffset * -direction);
            Vector3 p2 = bottomCenter + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetBlockedClosestHit(Physics.CapsuleCastNonAlloc(p1, p2, radius, direction, hits, backOffset + maxDistance, Physics.AllLayers, query), tagMask, layerMask, filter, hits, out hit))
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
                MISDebugDrawExt.Point(p2, radius, drawGizmo.color);

                this.DrawCapsuleCast(p1, p2, direction);
            }
#endif

            return isDetected;
        }
        public bool CastBlocked(
            Vector3 position, Vector3 upVector, Vector3 direction, float capsuleHeight,
            vTagMask tagMask, int layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            SetCapsuleDimension(position, radius, capsuleHeight, out Vector3 topCenter, out Vector3 bottomCenter);

            Vector3 p1 = topCenter + (backOffset * -direction);
            Vector3 p2 = bottomCenter + (backOffset * -direction);

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetBlockedClosestHit(Physics.CapsuleCastNonAlloc(p1, p2, radius, direction, hits, backOffset + maxDistance, Physics.AllLayers, query), tagMask, layerMask, filter, hits, out hit))
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
                MISDebugDrawExt.Point(p2, radius, drawGizmo.color);

                this.DrawCapsuleCast(p1, p2, direction);
            }
#endif

            return isDetected;
        }
        */
    }
}