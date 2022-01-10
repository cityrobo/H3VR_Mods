using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using FistVR;

namespace Cityrobo
{
    class RangeFinder_HandHeld : MonoBehaviour
    {
        public FVRFireArmAttachment attachment;
        public GameObject laserSystem;
        public AudioSource audioSource;
        public AudioClip audioClip;

        private bool isOn = false;
        private bool lockControls = false;

        public void Start()
        {
            attachment = this.gameObject.GetComponent<FVRFireArmAttachment>();
        }
        public void Update()
        {
            FVRViveHand hand = attachment.m_hand;
            if (hand != null && attachment.curMount == null)
            {
                if (hand.Input.TriggerDown && !lockControls) StartCoroutine("MeasureOnce");
                else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes,Vector2.up) < 45f && !lockControls) ToggleMeasure();
                else if (hand.Input.TouchpadUp && lockControls) lockControls = false;
            }
            else if (attachment.curMount != null)
            {
                if (attachment.AttachmentInterface.m_hand != null)
                {
                    if (attachment.AttachmentInterface.m_hand.Input.TouchpadDown && Vector2.Angle(attachment.AttachmentInterface.m_hand.Input.TouchpadAxes, Vector2.up) < 45f) ToggleMeasure();
                    else if (attachment.AttachmentInterface.m_hand.Input.TouchpadDown && Vector2.Angle(attachment.AttachmentInterface.m_hand.Input.TouchpadAxes, Vector2.down) < 45f) lockControls = true;
                }
            }
        }

        public IEnumerator MeasureOnce()
        {
            if (!isOn)
            {
                ToggleMeasure();
            }
            yield return 0;
            ToggleMeasure();
        }

        public void ToggleMeasure()
        {
            switch (isOn)
            {
                case false:
                    laserSystem.SetActive(true);
                    audioSource.PlayOneShot(audioClip);
                    isOn = true;
                    break;
                case true:
                    laserSystem.SetActive(false);
                    isOn = false;
                    break;
                default:
                    break;
            }
        }
    }
}
