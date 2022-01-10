using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace Cityrobo
{
    public class PoseTester : FVRPhysicalObject
    {
        [Header("PoseTester Config")]
        public GameObject Display_Controller_Vive;
        public GameObject Display_Controller_Touch;
        public GameObject Display_Controller_Index;
        public GameObject Display_Controller_WMR;
        public GameObject Display_Controller_RiftS;
        public GameObject Display_Controller_Cosmos;
        public GameObject Display_Controller_HPR2;
        public GameObject Display_Controller_Quest2;

		public Transform OtherHand;

        private DisplayMode DMode;

#if!(UNITY_EDITOR || UNITY_5)
		public override void Start()
        {
            base.Start();
            if (OtherHand != null)
            {
				OtherHand.parent = null;
			}

			//DisplayOffsets();
        }

        public override void BeginInteraction(FVRViveHand hand)
        {
            base.BeginInteraction(hand);

            DMode = hand.DMode;

			if (this.DMode == DisplayMode.Vive)
			{
				this.Display_Controller_Vive.SetActive(true);
				this.Display_Controller_RiftS.SetActive(false);
				this.Display_Controller_Touch.SetActive(false);
				this.Display_Controller_Index.SetActive(false);
				this.Display_Controller_WMR.SetActive(false);
				this.Display_Controller_Cosmos.SetActive(false);
				this.Display_Controller_HPR2.SetActive(false);
				this.Display_Controller_Quest2.SetActive(false);
				this.ConfigureFromControllerDefinition(0);
			}
			else if (this.DMode == DisplayMode.Rift)
			{
				this.Display_Controller_Touch.SetActive(true);
				this.Display_Controller_RiftS.SetActive(false);
				this.Display_Controller_Vive.SetActive(false);
				this.Display_Controller_Index.SetActive(false);
				this.Display_Controller_WMR.SetActive(false);
				this.Display_Controller_Cosmos.SetActive(false);
				this.Display_Controller_HPR2.SetActive(false);
				this.Display_Controller_Quest2.SetActive(false);
				this.ConfigureFromControllerDefinition(1);
			}
			else if (this.DMode == DisplayMode.RiftS)
			{
				this.Display_Controller_RiftS.SetActive(true);
				this.Display_Controller_Touch.SetActive(false);
				this.Display_Controller_Vive.SetActive(false);
				this.Display_Controller_Index.SetActive(false);
				this.Display_Controller_WMR.SetActive(false);
				this.Display_Controller_Cosmos.SetActive(false);
				this.Display_Controller_HPR2.SetActive(false);
				this.Display_Controller_Quest2.SetActive(false);
				this.ConfigureFromControllerDefinition(1);
			}
			else if (this.DMode == DisplayMode.Index)
			{
				this.Display_Controller_Index.SetActive(true);
				this.Display_Controller_RiftS.SetActive(false);
				this.Display_Controller_Vive.SetActive(false);
				this.Display_Controller_Touch.SetActive(false);
				this.Display_Controller_WMR.SetActive(false);
				this.Display_Controller_Cosmos.SetActive(false);
				this.Display_Controller_HPR2.SetActive(false);
				this.Display_Controller_Quest2.SetActive(false);
				this.ConfigureFromControllerDefinition(1);
			}
			else if (this.DMode == DisplayMode.Quest2)
			{
				this.Display_Controller_Quest2.SetActive(true);
				this.Display_Controller_RiftS.SetActive(false);
				this.Display_Controller_Vive.SetActive(false);
				this.Display_Controller_Touch.SetActive(false);
				this.Display_Controller_Index.SetActive(false);
				this.Display_Controller_Cosmos.SetActive(false);
				this.Display_Controller_HPR2.SetActive(false);
				this.Display_Controller_WMR.SetActive(false);
				this.ConfigureFromControllerDefinition(1);
			}
			else if (this.DMode == DisplayMode.WMR)
			{
				this.Display_Controller_WMR.SetActive(true);
				this.Display_Controller_RiftS.SetActive(false);
				this.Display_Controller_Vive.SetActive(false);
				this.Display_Controller_Touch.SetActive(false);
				this.Display_Controller_Index.SetActive(false);
				this.Display_Controller_Cosmos.SetActive(false);
				this.Display_Controller_HPR2.SetActive(false);
				this.Display_Controller_Quest2.SetActive(false);
				this.ConfigureFromControllerDefinition(0);
			}
			else if (this.DMode == DisplayMode.ViveCosmos)
			{
				this.Display_Controller_Cosmos.SetActive(true);
				this.Display_Controller_WMR.SetActive(false);
				this.Display_Controller_RiftS.SetActive(false);
				this.Display_Controller_Vive.SetActive(false);
				this.Display_Controller_Touch.SetActive(false);
				this.Display_Controller_Index.SetActive(false);
				this.Display_Controller_HPR2.SetActive(false);
				this.Display_Controller_Quest2.SetActive(false);
				this.ConfigureFromControllerDefinition(1);
			}
			else if (this.DMode == DisplayMode.WMRHPRb2)
			{
				this.Display_Controller_HPR2.SetActive(true);
				this.Display_Controller_RiftS.SetActive(false);
				this.Display_Controller_Vive.SetActive(false);
				this.Display_Controller_Touch.SetActive(false);
				this.Display_Controller_Index.SetActive(false);
				this.Display_Controller_Cosmos.SetActive(false);
				this.Display_Controller_WMR.SetActive(false);
				this.Display_Controller_Quest2.SetActive(false);
				this.ConfigureFromControllerDefinition(0);
			}
		}

		private void ConfigureFromControllerDefinition(int i)
        {
            switch (i)
            {
				case 0:
                    Debug.Log("PoseTester using PoseOverride");
                    break;
				case 1:
					Debug.Log("PoseTester using PoseOverride_Touch");
					break;
				default:
                    Debug.LogError("Unknown Controller Definition!");
                    break;
            }
		}

        private void DisplayOffsets()
        {
			int i = 0;
			foreach (var ControllerDefinition in ManagerSingleton<GM>.Instance.ControllerDefinitions)
            {
				Vector3 PoseTransformOffset = ControllerDefinition.PoseTransformOffset;
				Vector3 PoseTransformRotOffset = ControllerDefinition.PoseTransformRotOffset;

                Debug.Log("ControllerDefinition Nr." + i + ":");

                Debug.Log("PoseOverride Position x: " + PoseTransformOffset.x);
				Debug.Log("PoseOverride Position y: " + PoseTransformOffset.y);
				Debug.Log("PoseOverride Position z: " + PoseTransformOffset.z);
				Debug.Log("PoseOverride EulerAngles x: " + PoseTransformRotOffset.x);
				Debug.Log("PoseOverride EulerAngles x: " + PoseTransformRotOffset.y);
				Debug.Log("PoseOverride EulerAngles x: " + PoseTransformRotOffset.z);
			}
		}
#endif
	}
}
