using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Manipulate_Animator
{
    public class Manipulate_BlendShape : MonoBehaviour
    {
        public SkinnedMeshRenderer SkinnedMeshRenderer;
        public int BlendShapeIndex = 0;

		public Transform ObservedObject;

        public Vector3 ObservedObject_Start;
        public Vector3 ObservedObject_End;

        public enum Axis
        {
            X = 0,
            Y = 1,
            Z = 2
        }

        public Axis Direction;

        private float _lastLerp;
        public void Awake()
        {
            gameObject.SetActive(false);
            OpenScripts2.ManipulateBlendShape newComponent = gameObject.AddComponent<OpenScripts2.ManipulateBlendShape>();
            newComponent.skinnedMeshRenderer = SkinnedMeshRenderer;
            newComponent.ObservedObject = ObservedObject;
            newComponent.BlendShapeIndex = BlendShapeIndex;

            newComponent.ObservedObject_Start = ObservedObject_Start;
            newComponent.ObservedObject_End = ObservedObject_End;
            newComponent.Direction = (OpenScripts2.OpenScripts2_BasePlugin.Axis)(int)Direction;
            gameObject.SetActive(true);

            Destroy(this);
        }

        //public void Update()
        //{
        //    float lerp;
        //    lerp = Mathf.InverseLerp(ObservedObject_Start[(int)Direction], ObservedObject_End[(int)Direction], ObservedObject.localPosition[(int)Direction]);
        //    if (!Mathf.Approximately(lerp, _lastLerp)) SkinnedMeshRenderer.SetBlendShapeWeight(BlendShapeIndex, lerp);

        //    _lastLerp = lerp;
        //}
    }
}
