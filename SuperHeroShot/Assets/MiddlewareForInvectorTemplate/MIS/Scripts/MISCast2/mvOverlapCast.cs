using System.Collections.Generic;
using UnityEngine;

namespace com.mobilin.games
{
    // ----------------------------------------------------------------------------------------------------
    // 
    // ----------------------------------------------------------------------------------------------------
    public class mvOverlapCast : mvCast
    {
        [HideInInspector] public List<Collider> overlapList;


        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public mvOverlapCast() : base()
        {
        }
        public mvOverlapCast(int resultCount = DEFAULT_RESULT_COUNT) : base(resultCount)
        {
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        protected virtual bool GetOverlaps(int hitCount, vTagMask tagMask, IMISColliderFilter filter, Collider[] others, out List<Collider> overlapList)
        {
            overlapList = new();

            if (hitCount == 0)
                return false;

            for (int i = 0; i < hitCount; i++)
            {
                if ((tagMask != null && tagMask.Count > 0 && !tagMask.Contains(others[i].gameObject.tag))
                    || (filter != null && filter.FilterCollider(others[i])))
                {
                    continue;
                }

                overlapList.Add(others[i]);
            }

            return overlapList.Count > 0;
        }

        // ----------------------------------------------------------------------------------------------------
        // 
        // ----------------------------------------------------------------------------------------------------
        public virtual void SortOverlapsByDistance(Transform reference)
        {
            overlapList.Sort((collider1, collider2) =>
            {
                float collider1Distance = (collider1.gameObject.transform.position - reference.position).sqrMagnitude;
                float collider2Distance = (collider2.gameObject.transform.position - reference.position).sqrMagnitude;

                return collider1Distance.CompareTo(collider2Distance);
            });
        }
    }
}