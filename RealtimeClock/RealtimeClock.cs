using System;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class RealtimeClock : MonoBehaviour
    {
        public Text screen;

        public string timeFormat = "hh:mm:sstt";

        public void FixedUpdate()
        {
            screen.text = DateTime.Now.ToString(timeFormat);
        }
    }
}
