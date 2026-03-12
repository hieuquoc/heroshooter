using System.Collections.Generic;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    [System.Serializable]
    public class mvOverlapBox : mvOverlapCast
    {
        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public mvOverlapBox() : base()
        {
            resultCount = DEFAULT_RESULT_COUNT;
        }
        public mvOverlapBox(int resultCount = DEFAULT_RESULT_COUNT) : base(resultCount)
        {
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual Vector3 GetP1Position(Transform tr, Vector3 offset)
        {
            return tr.TransformPoint(offset);
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public bool Overlap(
            Transform tr, Vector3 offset, Quaternion orientation, Vector3 halfExtents,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = tr.TransformPoint(offset);

            Collider[] overlaps = new Collider[resultCount];

            if (GetOverlaps(Physics.OverlapBoxNonAlloc(p1, halfExtents, overlaps, orientation, layerMask, query), tagMask, filter, overlaps, out overlapList))
            {
                isDetected = true;
                distance = 0f;
            }
            else
            {
                isDetected = false;
                distance = 0f;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
                MISDebugDrawExt.WireCube(p1, halfExtents, orientation, isDetected ? drawGizmo.color : Color.cyan);
#endif

            return isDetected;
        }
        public bool Overlap(
            Vector3 position, Quaternion orientation, Vector3 halfExtents,
            vTagMask tagMask, LayerMask layerMask, QueryTriggerInteraction query, IMISColliderFilter filter)
        {
            Vector3 p1 = position;

            Collider[] overlaps = new Collider[resultCount];

            if (GetOverlaps(Physics.OverlapBoxNonAlloc(p1, halfExtents, overlaps, orientation, layerMask, query), tagMask, filter, overlaps, out overlapList))
            {
                isDetected = true;
                distance = 0f;
            }
            else
            {
                isDetected = false;
                distance = 0f;
            }

#if UNITY_EDITOR
            if (drawGizmo.draw)
                MISDebugDrawExt.WireCube(p1, halfExtents, orientation, isDetected ? drawGizmo.color : Color.cyan);
#endif

            return isDetected;
        }
    }
}