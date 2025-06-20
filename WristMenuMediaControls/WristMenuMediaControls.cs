using BepInEx;
using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    [BepInPlugin("h3vr.cityrobo.WristMenuMediaControls", "WristMenu Media Controls", "1.0.0")]
    public class WristMenuMediaControls : BaseUnityPlugin
    {
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const byte VK_MEDIA_PLAY_PAUSE = 0xB3; // Media Play/Pause key
        private const byte VK_MEDIA_NEXT_TRACK = 0xB0; // Media Next Track key
        private const byte VK_MEDIA_PREV_TRACK = 0xB1; // Media Previous Track key

        private const byte VK_VOLUME_UP = 0xAF;   // Volume Up key
        private const byte VK_VOLUME_DOWN = 0xAE; // Volume Down key
        private const byte VK_VOLUME_MUTE = 0xAD; // Volume Mute key

        private const uint KEYEVENTF_KEYDOWN = 0x0000;
        private const uint KEYEVENTF_KEYUP = 0x0002; // Key Up flag

        // Simulate Play/Pause Button
        public static void SimulateMediaPlayPause()
        {
            if (keybd_event == null)
            {
                Debug.LogError("Failed to import keybd_event function from user32.dll.");
                return;
            }

            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        // Simulate Next Track Button
        public static void SimulateMediaNextTrack()
        {
            if (keybd_event == null)
            {
                Debug.LogError("Failed to import keybd_event function from user32.dll.");
                return;
            }

            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        // Simulate Previous Track Button
        public static void SimulateMediaPrevTrack()
        {
            if (keybd_event == null)
            {
                Debug.LogError("Failed to import keybd_event function from user32.dll.");
                return;
            }

            keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        // Simulate volume up button
        public static void SimulateVolumeUp()
        {
            keybd_event(VK_VOLUME_UP, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event(VK_VOLUME_UP, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        // Simulate volume down button
        public static void SimulateVolumeDown()
        {
            keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event(VK_VOLUME_DOWN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        // Simulate volume mute/unmute button
        public static void SimulateVolumeMute()
        {
            keybd_event(VK_VOLUME_MUTE, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event(VK_VOLUME_MUTE, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }
#if !DEBUG

#endif
    }
}
