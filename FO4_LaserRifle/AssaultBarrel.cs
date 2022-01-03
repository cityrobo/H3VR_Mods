using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo

{
    public class AssaultBarrel : MonoBehaviour
    {
        public FVRFireArmAttachment attachment;
        public Transform boltProxy;
        public Transform rotatingBit;

        public float forwardBoltPostion;
        public float backwardBoltPostion;
        public float wiggleroom = 0.005f;

        public float rotationalAngle;

        private bool attached = false;
        private OpenBoltReceiver firearm;
        private OpenBoltReceiver.FireSelectorMode[] originalFireModes;

        private int index = 0;

        private enum LastPos
        {
            forward,
            backward
        }

        private LastPos lastPos;

        public void Start()
        {
            lastPos = LastPos.backward;
        }
        public void Update()
        {
            if (attachment.curMount != null && !attached)
            {
                ChangeFireMode(true);
                attached = true;
            }
            else if (attachment.curMount == null && attached)
            {
                ChangeFireMode(false);
                attached = false;
            }

            if (attachment.curMount != null && attached)
            {
                float boltPos = firearm.Bolt.Transform.localPosition.z;

                float lerp = Mathf.InverseLerp(forwardBoltPostion, backwardBoltPostion, boltPos);

                if (lerp >= 1f && lastPos == LastPos.forward)
                {
                    lastPos = LastPos.backward;
                    index++;
                    if (index >= 3) index = 0;

                    Vector3 rot = rotatingBit.localEulerAngles;
                    float zRotation = rotationalAngle * index;
                    rot = new Vector3(rot.x, rot.y, zRotation);
                    rotatingBit.localEulerAngles = rot;
                }
                else if(lerp < wiggleroom && lastPos == LastPos.backward)
                {
                    lastPos = LastPos.forward;
                }

                if (lastPos == LastPos.forward)
                {
                    Vector3 rot = rotatingBit.localEulerAngles;
                    float zRotation = rotationalAngle * index + rotationalAngle * lerp;
                    rot = new Vector3(rot.x, rot.y, zRotation);
                    rotatingBit.localEulerAngles = rot;
                }
            }
        }



        public void ChangeFireMode(bool activate)
        {
            if (activate)
            {
                firearm = attachment.curMount.GetRootMount().MyObject as OpenBoltReceiver;
                try
                {
                    originalFireModes = firearm.FireSelector_Modes;
                    OpenBoltReceiver.FireSelectorMode newFireSelectorMode = new OpenBoltReceiver.FireSelectorMode();
                    newFireSelectorMode.ModeType = OpenBoltReceiver.FireSelectorModeType.FullAuto;
                    newFireSelectorMode.SelectorPosition = originalFireModes[1].SelectorPosition;
                    firearm.FireSelector_Modes = originalFireModes.Concat(new OpenBoltReceiver.FireSelectorMode[] {newFireSelectorMode}).ToArray();
                }
                catch (Exception)
                {
                    Debug.LogError("AssaultBarrel: Firearm == null or not castable to OpenBoltReceiver!");
                    throw;
                }
            }
            else
            {
                firearm.m_fireSelectorMode = 1;
                firearm.FireSelector_Modes = originalFireModes;
            }
        }
    }
}
