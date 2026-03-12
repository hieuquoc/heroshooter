using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class mvRayCast : mvCast
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public mvRayCast() : base()
        {
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
            Vector3 p1 = tr.TransformPoint(castOffset) + (backOffset * -direction);
            hits.Clear();

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetClosestHit(Physics.RaycastNonAlloc(p1, direction, hits, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance - backOffset;
                hitObject = hit.collider.gameObject;
            }
            else
            {
                isDetected = false;
                distance = hit.distance - backOffset;
                hitObject = null;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
            {
                //if (!string.IsNullOrEmpty(castName))

                MISDebugDrawExt.Point(p1, 0.1f, drawGizmo.color);
                this.DrawRayCast(p1, direction);
            }
#endif

            return isDetected;
        }

        // ----------------------------------------------------------------------------------------------------
        // The dedicated direction version
        // ----------------------------------------------------------------------------------------------------
        public bool Cast(
            Transform tr, Vector3 direction,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = tr.TransformPoint(castOffset) + (backOffset * -direction);
            hits.Clear();

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetClosestHit(Physics.RaycastNonAlloc(p1, direction, hits, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance - backOffset;
                hitObject = hit.collider.gameObject;
            }
            else
            {
                isDetected = false;
                distance = hit.distance - backOffset;
                hitObject = null;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
            {
                //if (!string.IsNullOrEmpty(castName))

                MISDebugDrawExt.Point(p1, 0.1f, drawGizmo.color);
                this.DrawRayCast(p1, direction);
            }
#endif

            return isDetected;
        }
        public bool Cast(
            Vector3 position, Vector3 direction,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = position + (backOffset * -direction);
            hits.Clear();

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetClosestHit(Physics.RaycastNonAlloc(p1, direction, hits, backOffset + maxDistance, layerMask, query), tagMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance - backOffset;
                hitObject = hit.collider.gameObject;
            }
            else
            {
                isDetected = false;
                distance = hit.distance - backOffset;
                hitObject = null;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
            {
                MISDebugDrawExt.Point(p1, 0.1f, drawGizmo.color);
                this.DrawRayCast(p1, direction);
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
            hits.Clear();

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetBlockedClosestHit(Physics.RaycastNonAlloc(p1, direction, hits, backOffset + maxDistance, Physics.AllLayers, query), tagMask, layerMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance - backOffset;
                hitObject = hit.collider.gameObject;
            }
            else
            {
                isDetected = false;
                distance = hit.distance - backOffset;
                hitObject = null;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
            {
                MISDebugDrawExt.Point(p1, 0.1f, drawGizmo.color);
                this.DrawRayCast(p1, direction);
            }
#endif

            return isDetected;
        }
        public bool CastBlocked(
            Vector3 position, Vector3 direction,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = position + (backOffset * -direction);
            hits.Clear();

            if (maxDistance == -1f)
                maxDistance = Mathf.Infinity;

            if (GetBlockedClosestHit(Physics.RaycastNonAlloc(p1, direction, hits, backOffset + maxDistance, Physics.AllLayers, query), tagMask, layerMask, filter, hits, out hit))
            {
                isDetected = true;
                distance = hit.distance - backOffset;
                hitObject = hit.collider.gameObject;
            }
            else
            {
                isDetected = false;
                distance = hit.distance - backOffset;
                hitObject = null;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
            {
                MISDebugDrawExt.Point(p1, 0.1f, drawGizmo.color);
                this.DrawRayCast(p1, direction);
            }
#endif

            return isDetected;
        }
        */
    }
}