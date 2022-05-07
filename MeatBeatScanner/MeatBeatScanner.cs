using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class MeatBeatScanner : FVRFireArmAttachment
    {
        [Header("MeatBeat Scanner Config")]
        public float range;
		public float EngageAngle;
        public RectTransform currentScreen;
        public GameObject pingDotReference;

		public LayerMask LatchingMask;

        public bool canRotateScreen = false;
        [Tooltip("Different Screen orientations")]
        public GameObject[] images;
		//public LayerMask BlockingMask;

        private Dictionary<SosigLink,GameObject> pings;
        private GameObject directionReference;

        private int currentImage;

#if !(UNITY_EDITOR || UNITY_5)

        public override void Awake()
        {
            base.Awake();

            pingDotReference.SetActive(false);
            pings = new Dictionary<SosigLink, GameObject>();

            directionReference = new GameObject("MeatBeatScanner DirectionReference");

            directionReference.transform.parent = transform;
            directionReference.transform.localPosition = new Vector3();

            if (canRotateScreen) currentScreen = images[0].GetComponent<RectTransform>();
        }

        public override void Start()
        {
            base.Start();
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();

            if (canRotateScreen && curMount != null && AttachmentInterface.m_hand != null) UpdateInputs(AttachmentInterface.m_hand);

            UpdateScreen();
        }

        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);

            if(canRotateScreen) UpdateInputs(hand);
        }

        public void UpdateScreen()
        {
			List<SosigLink> sosigs = FindSosigs();
            ClearPings(sosigs);
            foreach (var sosig in sosigs)
            {
                Vector3 sosigPos = sosig.transform.position;
                Vector3 correctedForwardDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

                directionReference.transform.rotation = Quaternion.LookRotation(correctedForwardDir, Vector3.up);

                Vector3 projectedPos = directionReference.transform.InverseTransformPoint(sosigPos);
                float distance = projectedPos.magnitude;
                GameObject pingObject;
                if (!pings.TryGetValue(sosig, out pingObject))
                {
                    pingObject = Instantiate(pingDotReference);
                    pingObject.transform.SetParent(currentScreen);

                    pings.Add(sosig, pingObject);
                }

                Vector3 screenTransform = new Vector3(0f, 0f, -0.0001f);

                float xMax = currentScreen.rect.width / 2;
                float yMax = currentScreen.rect.height;
                float max = Mathf.Max(xMax, yMax);
                screenTransform.x = (projectedPos.x / range) * max;
                screenTransform.y = (projectedPos.z / range) * max;

                if (Mathf.Abs(screenTransform.x) > xMax || Mathf.Abs(screenTransform.y) > yMax)
                {
                    pingObject.SetActive(false);
                }
                else pingObject.SetActive(true);

                pingObject.transform.localPosition = screenTransform;
                pingObject.transform.localRotation = pingDotReference.transform.localRotation;
                pingObject.transform.localScale = pingDotReference.transform.localScale;
            }
        }

        List<SosigLink> FindSosigs()
        {
            List<SosigLink> sosigs = new List<SosigLink>();
            
            Collider[] array = Physics.OverlapSphere(transform.position, range, LatchingMask);
			List<Rigidbody> list = new List<Rigidbody>();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].attachedRigidbody != null && !list.Contains(array[i].attachedRigidbody))
				{
					list.Add(array[i].attachedRigidbody);
				}
			}
			SosigLink sosigLink = null;
			float maxAngle = EngageAngle / 2;
			for (int j = 0; j < list.Count; j++)
			{
                SosigLink component = list[j].GetComponent<SosigLink>();
                
                if (!(component == null))
				{
					if (component.S.BodyState != Sosig.SosigBodyState.Dead)
					{
                        Vector3 toTarget = component.transform.position - transform.position;
                        float angle;
                        try
                        {
                            angle = Vector3.Angle(new Vector3(transform.forward.x, 0, transform.forward.z), new Vector3(toTarget.x, 0, toTarget.z));
                        }
                        catch (Exception)
                        {
                            angle = 360;
                        }

                        //Debug.Log("angle: " + angle);
						Sosig s = component.S;
						sosigLink = s.Links[0];

                        if (angle < maxAngle)
                        {
                            if (!sosigs.Contains(sosigLink)) sosigs.Add(sosigLink);
                        }
					}
				}
			}
			return sosigs;
		}

        void ClearPings(List<SosigLink> sosigs)
        {
            if (pings == null) pings = new Dictionary<SosigLink, GameObject>();
            if (sosigs.Count > 0)
            {
                for (int i = 0; i < pings.Count; i++)
                {
                    SosigLink toRemove = pings.ElementAt(i).Key;
                    if (!sosigs.Contains(toRemove))
                    {
                        GameObject toRemoveValue;
                        pings.TryGetValue(toRemove, out toRemoveValue);
                        Destroy(toRemoveValue);
                        pings.Remove(toRemove);
                    }
                }
            }
            else
            {
                foreach (var ping in pings)
                {
                    Destroy(ping.Value);
                }
                pings.Clear();
            }
        }

        void UpdateInputs(FVRViveHand hand)
        {
            if (!hand.IsInStreamlinedMode)
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) NextRotation();
                else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) PreviousRotation();
            }
            else
            {
                if (hand.Input.BYButtonDown) NextRotation();
            }
        }

        void NextRotation()
        {
            currentImage++;
            if (currentImage >= images.Length) currentImage = 0;

            for (int i = 0; i < images.Length; i++)
            {
                images[i].SetActive(i == (int)currentImage);
            }

            currentScreen = images[(int)currentImage].GetComponent<RectTransform>();

            foreach (var ping in pings)
            {
                Vector3 pos = ping.Value.transform.localPosition;
                ping.Value.transform.SetParent(currentScreen);
                ping.Value.transform.localPosition = pos;
            }
        }

        void PreviousRotation()
        {
            currentImage--;
            if (currentImage < 0) currentImage = images.Length - 1;

            for (int i = 0; i < images.Length; i++)
            {
                images[i].SetActive(i == (int)currentImage);
            }

            currentScreen = images[(int)currentImage].GetComponent<RectTransform>();

            foreach (var ping in pings)
            {
                Vector3 pos = ping.Value.transform.localPosition;
                ping.Value.transform.SetParent(currentScreen);
                ping.Value.transform.localPosition = pos;
            }
        }
#endif
    }
}
