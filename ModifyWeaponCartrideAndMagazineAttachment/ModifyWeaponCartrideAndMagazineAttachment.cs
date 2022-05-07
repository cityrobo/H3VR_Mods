using FistVR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Cityrobo
{
    public class ModifyWeaponCartrideAndMagazineAttachment : MonoBehaviour
    {
        public FVRFireArmAttachment attachment;

        [Header("Caliber Modification")]
        public bool changesCaliber = true;
        [SearchableEnum]
        public FireArmRoundType roundType;

        [Header("MagazineType Modification")]
        public bool changesMagType = true;
        [SearchableEnum]
        public FireArmMagazineType magType;

        [Header("MagPos Calculation Modification")]
        [Header("Place Firearm and the new mag pos into these fields and use the context menu to calculate the position.")]
        public FVRFireArm temp_firearm;
        public Transform magMountPos;
        public Transform magEjectPos;

        [Header("Recoil Modification")]
        public bool changesRecoil = true;
        public FVRFireArmRecoilProfile recoilProfile;
        public FVRFireArmRecoilProfile recoilProfileStocked;

        public Vector3 relativeMagPos;
        public Quaternion relativeMagRot;

        public Vector3 relativeMagEjectPos;
        public Quaternion relativeMagEjectRot;

        private Vector3 origMagPos;
        private Quaternion origMagRot;

        private Vector3 origMagEjectPos;
        private Quaternion origMagEjectRot;

        private FVRFireArm fireArm = null;

        private FireArmRoundType origRoundType;
        private FireArmMagazineType origMagType;

        private FVRFireArmRecoilProfile origRecoilProfile;
        private FVRFireArmRecoilProfile origRecoilProfileStocked;

#if!DEBUG
        public void Update()
        {
            if (attachment.curMount != null && fireArm == null)
            {
                fireArm = attachment.curMount.GetRootMount().MyObject as FVRFireArm;

                if (changesMagType)
                {
                    origMagType = fireArm.MagazineType;
                    if (magMountPos != null)
                    {
                        origMagPos = fireArm.MagazineMountPos.localPosition;
                        origMagRot = fireArm.MagazineMountPos.localRotation;

                        origMagEjectPos = fireArm.MagazineEjectPos.localPosition;
                        origMagEjectRot = fireArm.MagazineEjectPos.localRotation;

                        fireArm.MagazineMountPos.localPosition = relativeMagPos;
                        fireArm.MagazineMountPos.localRotation = relativeMagRot;

                        fireArm.MagazineEjectPos.localPosition = relativeMagEjectPos;
                        fireArm.MagazineEjectPos.localRotation = relativeMagEjectRot;
                    }
                    fireArm.MagazineType = magType;
                }
                if (changesCaliber)
                {
                    origRoundType = fireArm.RoundType;

                    fireArm.RoundType = roundType;

                    switch (fireArm)
                    {
                        case ClosedBoltWeapon w:
                            w.Chamber.RoundType = roundType;
                            break;
                        case OpenBoltReceiver w:
                            w.Chamber.RoundType = roundType;
                            break;
                        case Handgun w:
                            w.Chamber.RoundType = roundType;
                            break;
                        case BoltActionRifle w:
                            w.Chamber.RoundType = roundType;
                            break;
                        case TubeFedShotgun w:
                            w.Chamber.RoundType = roundType;
                            break;
                        default:
                            Debug.LogWarning("ModifyWeaponCartrideAndMagazineAttachment: FireArm type not supported!");
                            break;
                    }
                }
                if (changesRecoil)
                {
                    origRecoilProfile = fireArm.RecoilProfile;
                    origRecoilProfileStocked = fireArm.RecoilProfileStocked;

                    fireArm.RecoilProfile = recoilProfile;
                    fireArm.RecoilProfileStocked = recoilProfileStocked;
                }
            }
            else if (attachment.curMount == null && fireArm != null)
            {
                if (changesMagType)
                {
                    fireArm.MagazineType = origMagType;
                    if (magMountPos != null)
                    {
                        fireArm.MagazineMountPos.localPosition = origMagPos;
                        fireArm.MagazineMountPos.localRotation = origMagRot;

                        fireArm.MagazineEjectPos.localPosition = origMagEjectPos;
                        fireArm.MagazineEjectPos.localRotation = origMagEjectRot;
                    }
                }
                if (changesCaliber)
                {
                    fireArm.RoundType = origRoundType;

                    switch (fireArm)
                    {
                        case ClosedBoltWeapon w:
                            w.Chamber.RoundType = origRoundType;
                            break;
                        case OpenBoltReceiver w:
                            w.Chamber.RoundType = origRoundType;
                            break;
                        case Handgun w:
                            w.Chamber.RoundType = origRoundType;
                            break;
                        case BoltActionRifle w:
                            w.Chamber.RoundType = origRoundType;
                            break;
                        case TubeFedShotgun w:
                            w.Chamber.RoundType = origRoundType;
                            break;
                        default:
                            Debug.LogWarning("ModifyWeaponCartrideAndMagazineAttachment: FireArm type not supported!");
                            break;
                    }
                }
                if (changesRecoil)
                { 
                    fireArm.RecoilProfile = origRecoilProfile;
                    fireArm.RecoilProfileStocked = origRecoilProfileStocked;
                }

                fireArm = null;
            }
        }
#endif

        [ContextMenu("Calculate relative magazine transforms")]
        public void CaluculateRelativeMagPos()
        {
            relativeMagPos = temp_firearm.transform.InverseTransformPoint(magMountPos.position);
            relativeMagRot = Quaternion.Inverse(temp_firearm.transform.rotation) * magMountPos.rotation;

            relativeMagEjectPos = temp_firearm.transform.InverseTransformPoint(magEjectPos.position);
            relativeMagEjectRot = Quaternion.Inverse(temp_firearm.transform.rotation) * magEjectPos.rotation;
        }
    }
}
