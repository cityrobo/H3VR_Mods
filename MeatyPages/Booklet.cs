using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class Booklet : FVRPhysicalObject
    {
        [Header("Booklet Config")]
        public GameObject[] pages;

        public float startAngle = 0f;
        public float flipAngle = 180f;
        public float flipSpeed = 180f;

        public Axis pageAxis;
        [Header("Booklet Sounds")]
        public AudioEvent flipPageLeft;
        public AudioEvent flipPageRight;
        public AudioEvent closeBooklet;

        int currentPage = 0;
        bool isFlipping = false;
        bool isClosing = false;
#if !(UNITY_EDITOR || UNITY_5)
        public override void UpdateInteraction(FVRViveHand hand)
        {
            base.UpdateInteraction(hand);
            UpdateInputsAndAnimate(hand);
        }

        void UpdateInputsAndAnimate(FVRViveHand hand)
        {
            if (hand != null)
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) FlipLeft();
                else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) FlipRight();
                else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f) CloseBooklet();
            }
        }

        void FlipLeft()
        {
            if (isClosing || currentPage >= pages.Length) return;
            SM.PlayGenericSound(flipPageLeft,this.transform.position);
            if (isFlipping)
            {
                StopAllCoroutines();
                pages[currentPage - 1].transform.localRotation = Quaternion.Euler(GetRotationalVector(flipAngle));
            }
            StartCoroutine(FlipPage(pages[currentPage], flipAngle));
            currentPage++;
        }
        void FlipRight()
        {
            if (isClosing || currentPage <= 0) return;
            SM.PlayGenericSound(flipPageRight, this.transform.position);
            if (isFlipping)
            {
                StopAllCoroutines();
                pages[currentPage].transform.localRotation = Quaternion.Euler(GetRotationalVector(startAngle));
            }
            StartCoroutine(FlipPage(pages[currentPage - 1], startAngle));
            currentPage--;
        }
        void CloseBooklet()
        {
            if (isClosing) return;
            SM.PlayGenericSound(closeBooklet, this.transform.position);

            if (isFlipping)
            {
                StopAllCoroutines();
            }

            StartCoroutine(ClosingBooklet());

            currentPage = 0;
        }

        IEnumerator FlipPage(GameObject page, float angle)
        {
            isFlipping = true;
            Vector3 angleVector = GetRotationalVector(angle);
            Quaternion targetRotation = Quaternion.Euler(angleVector);

            while (page.transform.localRotation != targetRotation)
            {
                page.transform.localRotation = Quaternion.RotateTowards(page.transform.localRotation, targetRotation, flipSpeed * Time.deltaTime);
                yield return null;
            }

            isFlipping = false;
        }

        IEnumerator ClosingBooklet()
        {
            isClosing = true;
            for (int i = 0; i < pages.Length; i++)
            {
                StartCoroutine(FlipPage(pages[i], startAngle));
                yield return null;
            }
            isClosing = false;
        }

        Vector3 GetRotationalVector(float angle)
        {
            Vector3 angleVector;
            switch (pageAxis)
            {
                case Axis.X:
                    angleVector = new Vector3(angle, 0f, 0f);
                    break;
                case Axis.Y:
                    angleVector = new Vector3(0f, angle, 0f);
                    break;
                case Axis.Z:
                    angleVector = new Vector3(0f, 0f, angle);
                    break;
                default:
                    angleVector = new Vector3();
                    break;
            }

            return angleVector;
        }
#endif
    }
}
