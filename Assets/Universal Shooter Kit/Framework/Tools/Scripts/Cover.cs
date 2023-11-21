using System.Collections;
using System.Collections.Generic;
using GercStudio.USK.Scripts;
using UnityEngine;

namespace GercStudio.USK.Scripts
{
    public class Cover : MonoBehaviour
    {
        public List<AIHelper.CoverPoint> points = new List<AIHelper.CoverPoint>();
        public int countOfEnemiesBehindThisCover;

        public List<int> instanceIDs = new List<int>();

        void Awake()
        {
            foreach (var point in points)
            {
                point.parentCover = this;
            }
            
            gameObject.tag = "Cover";
            instanceIDs.Add(gameObject.GetInstanceID());

            foreach (Transform child in transform)
            {
                child.gameObject.tag = "Cover";
                instanceIDs.Add(child.gameObject.GetInstanceID());
            }
        }

        void OnDrawGizmos()
        {
            if(!Application.isPlaying) return;
            
            foreach (var point in points)
            {
                Gizmos.color = point.isSuitablePoint ? Color.green : Color.red;
                Gizmos.DrawSphere(point.pointTransform.position, 1);
            }
        }
    }
}
