using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class LaserGuidanceSystem : MonoBehaviour
    {
        public LayerMask TargetMask;
        [HideInInspector]
        public static List<Vector3> LaserTargets = new List<Vector3>();
        public float Range;

        private Vector3 _lastTarget;

#if !DEBUG
        public void Update()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, Range, TargetMask,QueryTriggerInteraction.Collide))
            {
                LaserTargets.Remove(_lastTarget);
                LaserTargets.Add(hit.point);
                _lastTarget = hit.point;
            }
            else
            {
                LaserTargets.Remove(_lastTarget);
            }
        }

        public void OnDestroy()
        {
            LaserTargets.Remove(_lastTarget);
        }
#endif
	}
}
