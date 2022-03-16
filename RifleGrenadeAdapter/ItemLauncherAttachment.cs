using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Cityrobo
{
    public class ItemLauncherAttachment : MuzzleDevice
    {
        [Header("ItemLauncher Config")]
        public ItemLauncherQBSlot ItemHolder;
        public Transform ItemLaunchPoint;

        public float SpeedMultiplier = 1f;

        public AudioEvent GrenadeShot;

        private Vector3 _origMuzzlePos;
        private Quaternion _origMuzzleRot;
        private FVRFireArm _fireArm;

#if !(UNITY_EDITOR || UNITY_5)
        public override void Awake()
        {
            base.Awake();

            _origMuzzlePos = this.Muzzle.localPosition;
            _origMuzzleRot = this.Muzzle.localRotation;
        }

        public override void FVRUpdate()
        {
            base.FVRUpdate();

            if (ItemHolder.HeldObject != null)
            {
                this.Muzzle.position = Vector3.down * 3 + this.transform.TransformPoint(_origMuzzlePos);
                this.Muzzle.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
            }
            else
            {
                this.Muzzle.localPosition = _origMuzzlePos;
                this.Muzzle.localRotation = _origMuzzleRot;
            }
        }

        public override void AttachToMount(FVRFireArmAttachmentMount m, bool playSound)
        {
            base.AttachToMount(m, playSound);

            _fireArm = m.GetRootMount().MyObject as FVRFireArm;
        }

        public override void OnShot(FVRFireArm f, FVRTailSoundClass tailClass)
        {
            base.OnShot(f, tailClass);

            if (ItemHolder.HeldObject != null) FireItem();
        }

        void FireItem()
        {
            float speed = CalculateLaunchSpeed();

            bool launched = ItemHolder.LaunchHeldObject(speed * SpeedMultiplier, ItemLaunchPoint.position);
            if (launched) SM.PlayCoreSound(FVRPooledAudioType.GunShot, GrenadeShot, ItemHolder.transform.position);
        }

        float CalculateLaunchSpeed()
        {
            FVRFireArmChamber chamber = GetCurentChamber();
            if (chamber == null) return 5f;
            GameObject roundPrefab = chamber.GetRound().BallisticProjectilePrefab;
            BallisticProjectile ballisticProjectile = roundPrefab.GetComponent<BallisticProjectile>();

            float kinecticEnergy = 0.5f * ballisticProjectile.Mass * Mathf.Pow(ballisticProjectile.MuzzleVelocityBase, 2);

            float ItemMass = ItemHolder.CurObject.RootRigidbody.mass;

            return Mathf.Sqrt(kinecticEnergy / (0.5f * ItemMass));
        }

        FVRFireArmChamber GetCurentChamber()
        {
            switch (_fireArm)
            {
                case Handgun w:
                    return w.Chamber;
                case ClosedBoltWeapon w:
                    return w.Chamber;
                case OpenBoltReceiver w:
                    return w.Chamber;
                case TubeFedShotgun w:
                    return w.Chamber;
                case BoltActionRifle w:
                    return w.Chamber;
                case BreakActionWeapon w:
                    return w.Barrels[w.m_curBarrel].Chamber;
                case Revolver w:
                    return w.Chambers[w.CurChamber];
                case SingleActionRevolver w:
                    return w.Cylinder.Chambers[w.CurChamber];
                case RevolvingShotgun w:
                    return w.Chambers[w.CurChamber];
                case Flaregun w:
                    return w.Chamber;
                default:
                    if (_fireArm.FChambers.Count > 0) return _fireArm.FChambers[0];
                    else return null;
            }
        }
#endif
    }
}
