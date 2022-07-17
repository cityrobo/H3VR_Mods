using FistVR;
//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class CapAndBallRevolver : SingleActionRevolver
    {
        [Header("Cap and Ball Revolver Config")]
        public Transform ramRodLever;
        public Vector3 lowerLimit;
        public Vector3 upperLimit;

        public Vector3 touchingRot;
        public float wiggleRoom = 5f;

        public int numberOfChambersBackwardsToRam;

        private CapAndBallRevolverCylinder CapCylinder;

        private int lastChamber = -1;

        private bool isRamRodExtended = false;

#if!(UNITY_EDITOR || UNITY_5 || DEBUG)
        public override void Start()
        {
            base.Start();
            Hook();
            CapCylinder = base.Cylinder as CapAndBallRevolverCylinder;

            numberOfChambersBackwardsToRam = Mathf.Abs(numberOfChambersBackwardsToRam);
        }

        public override void OnDestroy()
        {
            Unhook();
            base.OnDestroy();
        }
        private void Unhook()
        {
            On.FistVR.SingleActionRevolver.Fire -= SingleActionRevolver_Fire;
            On.FistVR.SingleActionRevolver.EjectPrevCylinder -= SingleActionRevolver_EjectPrevCylinder;
            On.FistVR.SingleActionRevolver.UpdateCylinderRot -= SingleActionRevolver_UpdateCylinderRot;
            On.FistVR.SingleActionRevolver.AdvanceCylinder -= SingleActionRevolver_AdvanceCylinder;
        }

        private void Hook()
        {
            On.FistVR.SingleActionRevolver.Fire += SingleActionRevolver_Fire;
            On.FistVR.SingleActionRevolver.EjectPrevCylinder += SingleActionRevolver_EjectPrevCylinder;
            On.FistVR.SingleActionRevolver.UpdateCylinderRot += SingleActionRevolver_UpdateCylinderRot;
            On.FistVR.SingleActionRevolver.AdvanceCylinder += SingleActionRevolver_AdvanceCylinder;
        }

        public int RammingChamber
        {
            get
            {
                int num = this.CurChamber - numberOfChambersBackwardsToRam;
                if (num < 0)
                {
                    return this.Cylinder.NumChambers + num;
                }
                return num;
            }
        }

        public int PrevChamber3
        {
            get
            {
                int num = this.CurChamber - 3;
                if (num < 0)
                {
                    return this.Cylinder.NumChambers + num;
                }
                return num;
            }
        }
        public override void FVRUpdate()
        {
            base.FVRUpdate();

            Vector3 wiggleRoomVector = new Vector3(wiggleRoom, wiggleRoom, wiggleRoom);
            if (ramRodLever.localEulerAngles.IsLesserOrEqual( lowerLimit + wiggleRoomVector) && ramRodLever.localEulerAngles.IsGreaterOrEqual(lowerLimit - wiggleRoomVector))
            {
                isRamRodExtended = false;
            }
            else isRamRodExtended = true;

            float lerp = ExtendingVector3.InverseLerp(touchingRot,upperLimit, ramRodLever.localEulerAngles);
            if (CapCylinder.Chambers[RammingChamber].IsFull && lerp > 0f)
            {
                CapCylinder.RamChamber(RammingChamber, lerp);
            }
        }

        private void SingleActionRevolver_AdvanceCylinder(On.FistVR.SingleActionRevolver.orig_AdvanceCylinder orig, SingleActionRevolver self)
        {
            if (self == this)
            {
                if (isRamRodExtended || (!CapCylinder.ChamberRammed(RammingChamber) && CapCylinder.Chambers[RammingChamber].IsFull))
                {
                    return;
                }
                
                if (lastChamber == this.CurChamber)
                {
                    lastChamber--;
                }
                else
                {
                    this.CurChamber++;
                    lastChamber = this.CurChamber;
                }

                this.PlayAudioEvent(FirearmAudioEventType.FireSelector, 1f);
            }
            else orig(self);
        }

        private void SingleActionRevolver_UpdateCylinderRot(On.FistVR.SingleActionRevolver.orig_UpdateCylinderRot orig, SingleActionRevolver self)
        {
            if (self == this)
            {
                if (this.m_isStateToggled)
                {
                    int num = this.PrevChamber;
                    if (this.IsAccessTwoChambersBack)
                        num = this.PrevChamber2;
                    for (int index = 0; index < this.CapCylinder.Chambers.Length; ++index)
                    {
                        this.CapCylinder.Chambers[index].IsAccessible = index == num;
                        this.CapCylinder.capNipples[index].IsAccessible = index == num;

                        if (lastChamber == this.CurChamber)
                        {
                            if (!this.IsAccessTwoChambersBack)
                            {
                                this.CapCylinder.Chambers[index].IsAccessible = index == this.PrevChamber2;
                                this.CapCylinder.capNipples[index].IsAccessible = index == this.PrevChamber2;
                            }
                            else
                            {
                                this.CapCylinder.Chambers[index].IsAccessible = index == this.PrevChamber3;
                                this.CapCylinder.capNipples[index].IsAccessible = index == this.PrevChamber3;
                            }
                        }
                    }
                    if (this.DoesHalfCockHalfRotCylinder)
                    {
                        int cylinder = (this.CurChamber + 1) % this.CapCylinder.NumChambers;
                        this.CapCylinder.transform.localRotation = Quaternion.Slerp(this.CapCylinder.GetLocalRotationFromCylinder(this.CurChamber), this.CapCylinder.GetLocalRotationFromCylinder(cylinder), 0.5f);

                        if (lastChamber == this.CurChamber) this.CapCylinder.transform.localRotation = Quaternion.Slerp(this.CapCylinder.GetLocalRotationFromCylinder(this.CurChamber), this.CapCylinder.GetLocalRotationFromCylinder(cylinder), 0f);
                    }
                    else
                    {
                        int cylinder = (this.CurChamber + 1) % this.CapCylinder.NumChambers;
                        this.CapCylinder.transform.localRotation = this.CapCylinder.GetLocalRotationFromCylinder(this.CurChamber);
                        if (lastChamber == this.CurChamber) this.CapCylinder.transform.localRotation = Quaternion.Slerp(this.CapCylinder.GetLocalRotationFromCylinder(this.CurChamber), this.CapCylinder.GetLocalRotationFromCylinder(cylinder), 0.5f);
                    }
                    if (this.DoesCylinderTranslateForward)
                        this.CapCylinder.transform.localPosition = this.CylinderBackPos;
                    
                }
                else
                {
                    for (int index = 0; index < this.CapCylinder.Chambers.Length; ++index)
                    {
                        this.CapCylinder.Chambers[index].IsAccessible = false;
                        this.CapCylinder.capNipples[index].IsAccessible = false;
                    }
                    this.m_tarChamberLerp = !this.m_isHammerCocking ? 0.0f : this.m_hammerCockLerp;
                    this.m_curChamberLerp = Mathf.Lerp(this.m_curChamberLerp, this.m_tarChamberLerp, Time.deltaTime * 16f);
                    int cylinder = (this.CurChamber + 1) % this.CapCylinder.NumChambers;
                    this.CapCylinder.transform.localRotation = Quaternion.Slerp(this.CapCylinder.GetLocalRotationFromCylinder(this.CurChamber), this.CapCylinder.GetLocalRotationFromCylinder(cylinder), this.m_curChamberLerp);

                    if (this.DoesCylinderTranslateForward)
                        this.CapCylinder.transform.localPosition = Vector3.Lerp(this.CylinderBackPos, this.CylinderFrontPos, this.m_hammerCockLerp);


                    return;
                }
            }
            else orig(self);
        }

        private void SingleActionRevolver_EjectPrevCylinder(On.FistVR.SingleActionRevolver.orig_EjectPrevCylinder orig, SingleActionRevolver self)
        {
            if (self != this)
            {
                orig(self);
            }
        }

        private void SingleActionRevolver_Fire(On.FistVR.SingleActionRevolver.orig_Fire orig, SingleActionRevolver self)
        {
            if (self == this)
            {
                //Debug.Log("new fire");
                this.PlayAudioEvent(FirearmAudioEventType.HammerHit);

                bool capFired = this.CapCylinder.capNipples[this.CurChamber].Fire();

                if (capFired)
                {
                    this.PlayAudioEvent(FirearmAudioEventType.Shots_LowPressure);
                }

                if (!capFired || !this.CapCylinder.ChamberRammed(this.CurChamber) || !this.CapCylinder.Chambers[this.CurChamber].Fire())
                    return;

                FVRFireArmChamber chamber = this.CapCylinder.Chambers[this.CurChamber];
                this.Fire(chamber, this.GetMuzzle(), true);
                this.FireMuzzleSmoke();
                this.Recoil(this.IsTwoHandStabilized(), (Object)this.AltGrip != (Object)null, this.IsShoulderStabilized());
                this.PlayAudioGunShot(chamber.GetRound(), GM.CurrentPlayerBody.GetCurrentSoundEnvironment());

                if (GM.CurrentSceneSettings.IsAmmoInfinite && GM.CurrentPlayerBody.IsInfiniteAmmo)
                {
                    chamber.IsSpent = false;
                    this.CapCylinder.capNipples[this.CurChamber].IsSpent = false;

                    chamber.UpdateProxyDisplay();
                }
                else
                {
                    chamber.SetRound(null);

                    this.CapCylinder.ChamberRammed(this.CurChamber, true, false);
                }
            }
            else orig(self);
        }
#endif
    }
#if !(UNITY_EDITOR || UNITY_5 || DEBUG)
    public static class ExtendingVector3
    {
        public static bool IsGreaterOrEqual(this Vector3 local, Vector3 other)
        {
            if (local.x >= other.x && local.y >= other.y && local.z >= other.z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IsLesserOrEqual(this Vector3 local, Vector3 other)
        {
            if (local.x <= other.x && local.y <= other.y && local.z <= other.z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            /*
            float lerpx = Mathf.InverseLerp(a.x, b.x, value.x);
            float lerpy = Mathf.InverseLerp(a.y, b.y, value.y);
            float lerpz = Mathf.InverseLerp(a.z, b.z, value.z);

            Vector3 lerp = new Vector3(lerpx, lerpy, lerpz);
            return lerp.magnitude;
            */

            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Mathf.Clamp01(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB));
        }
    }
#endif
}
