using BepInEx;
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
    public class SpawnlockCloneTimeoutController : MonoBehaviour
    {
        public FVRFireArmMagazine Magazine;
        public Speedloader Speedloader;
        public FVRFireArmClip Clip;
        public FVRFireArmRound Round;
        public FVRPhysicalObject PhysicalObject;

        public float Timout = 10f;

        private float _timer = 0f;

        public void Update()
        {
            if (Magazine != null)
            {
                if (Magazine.State == FVRFireArmMagazine.MagazineState.Free && !Magazine.IsHeld && Magazine.QuickbeltSlot == null)
                {
                    _timer += Time.deltaTime;
                }
                else
                {
                    _timer = 0f;
                }

                if (_timer >= Timout)
                {
                    Destroy(Magazine.gameObject);
                }

                if (Magazine.m_isSpawnLock)
                {
                    Destroy(this);
                }
            }
            else if (Speedloader != null)
            {
                if (!Speedloader.IsHeld && Speedloader.QuickbeltSlot == null)
                {
                    _timer += Time.deltaTime;
                }
                else
                {
                    _timer = 0f;
                }

                if (_timer >= Timout)
                {
                    Destroy(Speedloader.gameObject);
                }

                if (Speedloader.m_isSpawnLock)
                {
                    Destroy(this);
                }
            }
            else if (Clip != null)
            {
                if (Clip.State == FVRFireArmClip.ClipState.Free && !Clip.IsHeld && Clip.QuickbeltSlot == null)
                {
                    _timer += Time.deltaTime;
                }
                else
                {
                    _timer = 0f;
                }

                if (_timer >= Timout)
                {
                    Destroy(Clip.gameObject);
                }

                if (Clip.m_isSpawnLock)
                {
                    Destroy(this);
                }
            }
            else if (Round != null)
            {
                if (!Round.IsHeld && Round.QuickbeltSlot == null)
                {
                    _timer += Time.deltaTime;
                }
                else
                {
                    _timer = 0f;
                }

                if (_timer >= Timout)
                {
                    Destroy(Round.gameObject);
                }

                if (Round.m_isSpawnLock)
                {
                    Destroy(this);
                }
            }
            else if (PhysicalObject != null)
            {
                if (!PhysicalObject.IsHeld && PhysicalObject.QuickbeltSlot == null)
                {
                    _timer += Time.deltaTime;
                }
                else
                {
                    _timer = 0f;
                }

                if (_timer >= Timout)
                {
                    Destroy(PhysicalObject.gameObject);
                }

                if (PhysicalObject.m_isSpawnLock)
                {
                    Destroy(this);
                }
            }
        }
#if !DEBUG
#endif
    }
}
