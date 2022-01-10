using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Manipulate_Animator
{
    public class Manipulate_Animator : MonoBehaviour
    {
        public Animator animator;
		public GameObject Observed_Object;

        public float wiggleroom = 0.05f;

        public Vector3 start;
        public Vector3 end;

        public enum dirtype
        {
            x = 0,
            y = 1,
            z = 2
        }

        public dirtype direction;
        public bool isRotation;


        public void Awake()
        {
        }
        public void Update()
        {
            float pos;
            if (!isRotation) pos = Mathf.InverseLerp(start[(int)direction], end[(int)direction], Observed_Object.transform.localPosition[(int)direction]);
            else pos = Mathf.InverseLerp(start[(int)direction], end[(int)direction], Observed_Object.transform.localEulerAngles[(int)direction]);
            animator.Play("animation", 0, pos);
        }
    }
}
