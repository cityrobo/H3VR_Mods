using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;

namespace Cityrobo
{
    public class MultiBarrelAttachment : MonoBehaviour 
    {
        public FVRFireArmAttachment FireArmAttachment = null;
        public BreakActionWeapon breakAction = null;
        public Derringer derringer = null;
        public MultiBarrelMount Mount;

        public GameObject Viz = null;
        public List<GameObject> VizCopies = new List<GameObject>();
        
        private Vector3 _origMuzzlePos;

        private Dictionary<MuzzleEffect ,Vector3> _origMuzzleEffectPos = new Dictionary<MuzzleEffect, Vector3>();
        
        private List<FVRFireArmAttachment> _lastSubAttachments = new List<FVRFireArmAttachment>();
        public void Awake()
        {
            if (FireArmAttachment == null) FireArmAttachment = this.gameObject.GetComponent<FVRFireArmAttachment>();
            
            if (FireArmAttachment is MuzzleDevice muzzleDevice)
            {
                _origMuzzlePos = muzzleDevice.Muzzle.localPosition;

                foreach (var muzzleEffect in muzzleDevice.MuzzleEffects)
                {
                    if (muzzleEffect.OverridePoint != null) _origMuzzleEffectPos.Add(muzzleEffect, muzzleEffect.OverridePoint.localPosition);
                }
            }
        }

        public void Update()
        {
            if (FireArmAttachment.curMount == null)
            {
                Viz.SetActive(true);

                for (int i = 0; i < VizCopies.Count; i++)
                {
                    Destroy(VizCopies[i]);
                }
                
                if (FireArmAttachment is MuzzleDevice muzzleDevice)
                {
                    muzzleDevice.Muzzle.localPosition = _origMuzzlePos;

                    foreach (var muzzleEffect in _origMuzzleEffectPos)
                    {
                        muzzleEffect.Key.OverridePoint.localPosition = muzzleEffect.Value;
                    }
                }
                
                Destroy(this);
            }

            List<FVRFireArmAttachment> subAttachments = GetAllSubAttachments();

            foreach (var subAttachment in subAttachments)
            {
                if (!_lastSubAttachments.Contains(subAttachment)) FixNewSubAttachment(subAttachment);
            }

            _lastSubAttachments.Clear();
            _lastSubAttachments.AddRange(subAttachments);
        }

        private void FixNewSubAttachment(FVRFireArmAttachment newAttachment)
        {
            if ((breakAction != null || derringer != null) && newAttachment != null && newAttachment.GetComponent<MultiBarrelAttachment>() == null)
            {
                MeshRenderer[] meshRenderers = newAttachment.GetComponentsInChildren<MeshRenderer>();

                if (meshRenderers.Length > 0)
                {
                    MultiBarrelAttachment multiBarrelAttachment = newAttachment.gameObject.AddComponent<MultiBarrelAttachment>();
                    multiBarrelAttachment.FireArmAttachment = newAttachment;
                    multiBarrelAttachment.Mount = Mount;

                    if (meshRenderers.Length == 1) multiBarrelAttachment.Viz = meshRenderers[0].gameObject;
                    else
                    {
                        multiBarrelAttachment.Viz = new GameObject("Viz");
                        multiBarrelAttachment.Viz.transform.SetParent(newAttachment.transform);
                        multiBarrelAttachment.Viz.transform.localPosition = Vector3.zero;
                        multiBarrelAttachment.Viz.transform.localRotation = Quaternion.identity;

                        foreach (var meshRenderer in meshRenderers)
                        {
                            meshRenderer.transform.SetParent(multiBarrelAttachment.Viz.transform);
                        }
                    }

                    Vector3 attachmentOffset; 
                    if (breakAction != null)
                    {
                        multiBarrelAttachment.breakAction = breakAction;

                        foreach (var barrel in breakAction.Barrels)
                        {
                            attachmentOffset = barrel.Muzzle.position - Mount.transform.position;
                            multiBarrelAttachment.VizCopies.Add(Instantiate(multiBarrelAttachment.Viz, multiBarrelAttachment.Viz.transform.position + attachmentOffset, multiBarrelAttachment.Viz.transform.rotation, newAttachment.transform));
                        }
                        multiBarrelAttachment.Viz.SetActive(false);
                    }
                    else if (derringer != null)
                    {
                        multiBarrelAttachment.derringer = derringer;

                        foreach (var barrel in derringer.Barrels)
                        {
                            attachmentOffset = barrel.MuzzlePoint.position - Mount.transform.position;
                            multiBarrelAttachment.VizCopies.Add(Instantiate(multiBarrelAttachment.Viz, multiBarrelAttachment.Viz.transform.position + attachmentOffset, multiBarrelAttachment.Viz.transform.rotation, newAttachment.transform));
                        }
                        multiBarrelAttachment.Viz.SetActive(false);
                    }
                }
            }
        }

        private List<FVRFireArmAttachment> GetAllSubAttachments()
        {
            List <FVRFireArmAttachment> subAttachments = new List<FVRFireArmAttachment>();
            foreach (var SubMount in FireArmAttachment.AttachmentInterface.SubMounts)
            {
                subAttachments.AddRange(SubMount.AttachmentsList);
            }

            return subAttachments;
        }
    }
}
