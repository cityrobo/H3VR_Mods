using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class MagPoseCycler : MonoBehaviour
    {
        public FVRFireArmMagazine magazine;

        public List<Transform> alternatePoseOverrides;

        private int poseIndex = 0;

        private Vector3 positionalOffset = new Vector3(0,0,0);
        private Quaternion rotationalOffset = new Quaternion();

        //private string lastMessage;

        private bool offsetCalculated = false;

#if!(DEBUG || MEATKIT)
        public void Awake()
        {
            //Debug.Log("MagPoseCycler awoken!");
            gameObject.SetActive(false);
            OpenScripts2.MagazinePoseCycler magazinePoseCycler = gameObject.AddComponent<OpenScripts2.MagazinePoseCycler>();
            magazinePoseCycler.Magazine = magazine;
            magazinePoseCycler.AlternatePoseOverrides = alternatePoseOverrides;
            gameObject.SetActive(true);

            Destroy(this);

            //alternatePoseOverrides.Add(Instantiate(magazine.PoseOverride));
            //poseIndex = alternatePoseOverrides.Count - 1;

            //Hook();
        }

        //public void OnDestroy()
        //{
        //    Unhook();
        //}

        //public void Update()
        //{
        //    FVRViveHand hand = magazine.m_hand;

        //    /*
        //    string message = "Current PoseOverride pos: " + magazine.PoseOverride.localPosition.ToString() + "\n" + "Current PoseOverride rot: " + magazine.PoseOverride.localRotation.ToString();
        //    if (lastMessage != message)
        //    {
        //        Debug.Log(message);

        //        lastMessage = message;
        //    }
        //    */

        //    if (hand != null)
        //    {
        //        if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f)
        //        {
        //            NextPose();
        //        }
        //        UpdatePose();
        //    }
        //}

        //void NextPose()
        //{
        //    poseIndex++;
        //    if (poseIndex >= alternatePoseOverrides.Count) poseIndex = 0;

        //    //Debug.Log("poseIndex: " + poseIndex);
        //    //Debug.Log("Selected PoseOverride pos: " + alternatePoseOverrides[poseIndex].localPosition);
        //    //Debug.Log("Selected PoseOverride rot: " + alternatePoseOverrides[poseIndex].localRotation);

        //    UpdatePose();
        //}

        //void UpdatePose()
        //{
            
        //    magazine.PoseOverride.localPosition = alternatePoseOverrides[poseIndex].localPosition + positionalOffset;
        //    magazine.PoseOverride.localRotation = rotationalOffset * alternatePoseOverrides[poseIndex].localRotation;
        //    /*
        //    magazine.PoseOverride.localPosition = alternatePoseOverrides[poseIndex].localPosition;
        //    magazine.PoseOverride.localRotation = alternatePoseOverrides[poseIndex].localRotation;
        //    */
        //    /*
        //    string message = "Current PoseOverride pos: " + magazine.PoseOverride.localPosition.ToString() + "\n" + "Current PoseOverride rot: " + magazine.PoseOverride.localRotation.ToString();
        //    if (lastMessage != message)
        //    {
        //        Debug.Log(message);

        //        lastMessage = message;
        //    }
        //    */
        //}

        //void CalculateOffset(FVRFireArmMagazine magazine)
        //{
        //    positionalOffset = magazine.PoseOverride_Touch.localPosition - magazine.PoseOverride.localPosition;
        //    rotationalOffset = magazine.PoseOverride_Touch.localRotation * Quaternion.Inverse(magazine.PoseOverride.localRotation);
        //    offsetCalculated = true;
        //}

        //public void Unhook()
        //{
        //    On.FistVR.FVRPhysicalObject.UpdatePosesBasedOnCMode -= FVRPhysicalObject_UpdatePosesBasedOnCMode;
        //}

        //public void Hook()
        //{
        //    On.FistVR.FVRPhysicalObject.UpdatePosesBasedOnCMode += FVRPhysicalObject_UpdatePosesBasedOnCMode;
        //}

        //private void FVRPhysicalObject_UpdatePosesBasedOnCMode(On.FistVR.FVRPhysicalObject.orig_UpdatePosesBasedOnCMode orig, FVRPhysicalObject self, FVRViveHand hand)
        //{
        //    if (self as FVRFireArmMagazine == magazine)
        //    {
        //        if (!offsetCalculated && (hand.CMode == ControlMode.Oculus || hand.CMode == ControlMode.Index) && (self as FVRFireArmMagazine).PoseOverride_Touch != null)
        //        {
        //            CalculateOffset(self as FVRFireArmMagazine);
        //        }
        //    }
        //    orig(self, hand);
        //}

#endif
    }
}
