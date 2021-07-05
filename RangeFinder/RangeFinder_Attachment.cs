using System;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using FistVR;
namespace Cityrobo
{
    class RangeFinder_Attachment : MonoBehaviour
    {
        public FVRFireArmAttachmentInterface attachmentInterface;
        public RangeFinder_Raycast raycast;

        public void Update()
        {
            FVRViveHand hand = attachmentInterface.m_hand;
            if (hand != null)
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) RotateScreenLeft();
                else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) RotateScreenRight();
            }
        }

        public void RotateScreenLeft()
        {
            switch (raycast.chosenScreen)
            {
                case RangeFinder_Raycast.ChosenScreen.Up:
                    raycast.chosenScreen = RangeFinder_Raycast.ChosenScreen.Left;
                    raycast.ChangeActiveScreen();
                    break;
                case RangeFinder_Raycast.ChosenScreen.Left:
                    raycast.chosenScreen = RangeFinder_Raycast.ChosenScreen.Down;
                    raycast.ChangeActiveScreen();
                    break;
                case RangeFinder_Raycast.ChosenScreen.Down:
                    raycast.chosenScreen = RangeFinder_Raycast.ChosenScreen.Right;
                    raycast.ChangeActiveScreen();
                    break;
                case RangeFinder_Raycast.ChosenScreen.Right:
                    raycast.chosenScreen = RangeFinder_Raycast.ChosenScreen.Up;
                    raycast.ChangeActiveScreen();
                    break;
                default:
                    raycast.chosenScreen = RangeFinder_Raycast.ChosenScreen.Up;
                    raycast.ChangeActiveScreen();
                    break;
            }
        }

        public void RotateScreenRight()
        {
            switch (raycast.chosenScreen)
            {
                case RangeFinder_Raycast.ChosenScreen.Up:
                    raycast.chosenScreen = RangeFinder_Raycast.ChosenScreen.Right;
                    raycast.ChangeActiveScreen();
                    break;
                case RangeFinder_Raycast.ChosenScreen.Left:
                    raycast.chosenScreen = RangeFinder_Raycast.ChosenScreen.Up;
                    raycast.ChangeActiveScreen();
                    break;
                case RangeFinder_Raycast.ChosenScreen.Down:
                    raycast.chosenScreen = RangeFinder_Raycast.ChosenScreen.Left;
                    raycast.ChangeActiveScreen();
                    break;
                case RangeFinder_Raycast.ChosenScreen.Right:
                    raycast.chosenScreen = RangeFinder_Raycast.ChosenScreen.Down;
                    raycast.ChangeActiveScreen();
                    break;
                default:
                    raycast.chosenScreen = RangeFinder_Raycast.ChosenScreen.Up;
                    raycast.ChangeActiveScreen();
                    break;
            }
        }
    }
}
