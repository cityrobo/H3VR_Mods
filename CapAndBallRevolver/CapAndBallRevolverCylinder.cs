using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FistVR;
using UnityEngine;

namespace Cityrobo
{
    public class CapAndBallRevolverCylinder : SingleActionRevolverCylinder
    {
        public FVRFireArmChamber[] capNipples;

        public float unrammedPos;
        public float rammedPos;

        private bool[] chamberRammed;

        private float[] lastLerp;

#if !(UNITY_EDITOR || UNITY_5)
        public void Awake()
        {
            chamberRammed = new bool[NumChambers];
            lastLerp = new float[NumChambers];
            for (int i = 0; i < NumChambers; i++)
            {
                Chambers[i].transform.localPosition = new Vector3(Chambers[i].transform.localPosition.x, Chambers[i].transform.localPosition.y, unrammedPos);
                chamberRammed[i] = false;
                lastLerp[i] = 0f;
            }
        }

        public bool ChamberRammed(int chamber, bool set = false, bool value = false)
        {
            if (set)
            {
                chamberRammed[chamber] = value;

                if (value)
                {
                    Chambers[chamber].transform.localPosition = new Vector3(Chambers[chamber].transform.localPosition.x, Chambers[chamber].transform.localPosition.y, rammedPos);
                }
                else Chambers[chamber].transform.localPosition = new Vector3(Chambers[chamber].transform.localPosition.x, Chambers[chamber].transform.localPosition.y, unrammedPos);
            }
            return chamberRammed[chamber];
        }

        public void RamChamber(int chamber, float lerp) 
        {
            if (!chamberRammed[chamber] && lerp > lastLerp[chamber])
            {
                Vector3 lerpPos = Vector3.Lerp(new Vector3(Chambers[chamber].transform.localPosition.x, Chambers[chamber].transform.localPosition.y, unrammedPos), new Vector3(Chambers[chamber].transform.localPosition.x, Chambers[chamber].transform.localPosition.y, rammedPos), lerp);
                Chambers[chamber].transform.localPosition = lerpPos;
                lastLerp[chamber] = lerp;
                if (lerp == 1f)
                {
                    chamberRammed[chamber] = true;
                    lastLerp[chamber] = 0f;
                }
            }
        }

#endif
    }
}
