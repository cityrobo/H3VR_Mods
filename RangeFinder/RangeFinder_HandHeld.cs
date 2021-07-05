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
        public Flashlight flashlight;

        public void Update()
        {
            FVRViveHand hand = flashlight.m_hand;
            if (hand != null)
            {
                if (hand.Input.TriggerDown) StartCoroutine("Measure");
            }
        }

        public IEnumerator Measure()
        {
            if (flashlight.LightParts.activeSelf == false)
            {
                flashlight.Aud.PlayOneShot(flashlight.LaserOnClip);
                flashlight.LightParts.SetActive(true);
            }
            yield return 0;
            flashlight.LightParts.SetActive(false);
        }
    }
}
