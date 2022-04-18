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
        public FVRFireArmRecoilProfile OverrideRecoilProfile;
        public FVRFireArmRecoilProfile OverrideRecoilProfileStocked;

        public AudioEvent GrenadeShot;

        private Vector3 _origMuzzlePos;
        private Quaternion _origMuzzleRot;
        private FVRFireArm _fireArm;
        private FVRFireArmRecoilProfile _origRecoilProfile;
        private FVRFireArmRecoilProfile _origRecoilProfileStocked;
        private bool _recoilProfileSet = false;

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

                if (!_recoilProfileSet && _fireArm != null && OverrideRecoilProfile != null)
                {
                    _origRecoilProfile = _fireArm.RecoilProfile;
                    _origRecoilProfileStocked = _fireArm.RecoilProfileStocked;

                    if (OverrideRecoilProfileStocked != null)
                    {
                        _fireArm.RecoilProfile = OverrideRecoilProfile;
                        _fireArm.RecoilProfileStocked = OverrideRecoilProfileStocked;
                    }
                    else
                    {
                        _fireArm.RecoilProfile = OverrideRecoilProfile;
                        _fireArm.RecoilProfileStocked = OverrideRecoilProfile;
                    }

                    _recoilProfileSet = true;
                }

            }
            else
            {
                if (_recoilProfileSet && _fireArm != null)
                {

                    _fireArm.RecoilProfile = _origRecoilProfile;
                    _fireArm.RecoilProfileStocked = _origRecoilProfileStocked;

                    _recoilProfileSet = false;
                }

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
                case RollingBlock w:
                    return w.Chamber;
                case Derringer w:
                    return w.Barrels[w.m_curBarrel].Chamber;
                case LAPD2019 w:
                    return w.Chambers[w.CurChamber];
                case BAP w:
                    return w.Chamber;
                case HCB w:
                    return w.Chamber;
                case M72 w:
                    return w.Chamber;
                case MF2_RL w:
                    return w.Chamber;
                case RGM40 w:
                    return w.Chamber;
                case RPG7 w:
                    return w.Chamber;
                case SimpleLauncher w:
                    return w.Chamber;
                case SimpleLauncher2 w:
                    return w.Chamber;
                case RemoteMissileLauncher w:
                    return w.Chamber;
                case PotatoGun w:
                    return w.Chamber;
                case GrappleGun w:
                    return w.Chambers[w.m_curChamber];
                default:
                    if (_fireArm.FChambers.Count > 0) return _fireArm.FChambers[0];
                    else return null;
            }
        }
#endif
    }
}
