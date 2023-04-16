using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BepInEx;
using System.Net.Mail;

namespace Cityrobo
{
    public class MultiBarrelAttachment : MonoBehaviour 
    {
        public FVRFireArmAttachment Attachment = null;
        public BreakActionWeapon breakAction = null;
        public Derringer derringer = null;
        public MultiBarrelMount Mount;

        public GameObject Viz = null;
        public List<GameObject> VizCopies = new List<GameObject>();
        
        private Vector3 _origMuzzlePos;

        private readonly Dictionary<MuzzleEffect ,Vector3> _origMuzzleEffectPos = new Dictionary<MuzzleEffect, Vector3>();
        
        private readonly List<FVRFireArmAttachment> _lastSubAttachments = new List<FVRFireArmAttachment>();

        private bool _wasSuppressorWellMounted = false;
        public void Awake()
        {
            if (Attachment == null) Attachment = GetComponent<FVRFireArmAttachment>();
            
            if (Attachment is MuzzleDevice muzzleDevice)
            {
                _origMuzzlePos = muzzleDevice.Muzzle.localPosition;

                foreach (var muzzleEffect in muzzleDevice.MuzzleEffects)
                {
                    if (muzzleEffect.OverridePoint != null) _origMuzzleEffectPos.Add(muzzleEffect, muzzleEffect.OverridePoint.localPosition);
                }
            }

            if (Attachment is Suppressor suppressor && suppressor.CatchRot >= 359f) _wasSuppressorWellMounted = true; 
        }

        public void Update()
        {
            if (Attachment.curMount == null)
            {
                Viz?.SetActive(true);

                for (int i = 0; i < VizCopies.Count; i++)
                {
                    Destroy(VizCopies[i]);
                }
                
                if (Attachment is MuzzleDevice muzzleDevice)
                {
                    muzzleDevice.Muzzle.localPosition = _origMuzzlePos;

                    foreach (var muzzleEffectPos in _origMuzzleEffectPos)
                    {
                        muzzleEffectPos.Key.OverridePoint.localPosition = muzzleEffectPos.Value;
                    }
                }
                
                Destroy(this);
            }

            List<FVRFireArmAttachment> subAttachments = GetAllSubAttachments();

            if (_lastSubAttachments.Count < subAttachments.Count)
            {
                foreach (var subAttachment in subAttachments)
                {
                    if (!_lastSubAttachments.Contains(subAttachment)) FixNewSubAttachment(subAttachment);
                }
                _lastSubAttachments.Clear();
                _lastSubAttachments.AddRange(subAttachments);
            }
            else if (_lastSubAttachments.Count > subAttachments.Count)
            {
                _lastSubAttachments.Clear();
                _lastSubAttachments.AddRange(subAttachments);
            }

            if (Attachment is Suppressor suppressor)
            {
                if (suppressor.CatchRot < 359f && _wasSuppressorWellMounted)
                {
                    foreach (var VizCopy in VizCopies)
                    {
                        VizCopy.SetActive(false);
                    }

                    _wasSuppressorWellMounted = false;
                    Viz?.SetActive(true);
                }
                else if (suppressor.CatchRot >= 359f && !_wasSuppressorWellMounted)
                {
                    foreach (var VizCopy in VizCopies)
                    {
                        VizCopy.SetActive(true);
                    }

                    Viz?.SetActive(false);
                    _wasSuppressorWellMounted = true;
                    suppressor.ForceBreakInteraction();
                    suppressor.AttachmentInterface.ForceBreakInteraction();
                }
            }
        }

        private void FixNewSubAttachment(FVRFireArmAttachment newAttachment)
        {
            if (Attachment.curMount.GetRootMount().ScaleModifier != 1 && !Attachment.CanScaleToMount && newAttachment.CanScaleToMount) newAttachment.ScaleToMount(Attachment.curMount.GetRootMount());

            if ((breakAction != null || derringer != null) && newAttachment != null && newAttachment is MuzzleDevice && newAttachment.GetComponent<MultiBarrelAttachment>() == null)
            {
                Renderer[] meshRenderers = newAttachment.GetComponentsInChildren<Renderer>().Where(obj => !(obj is ParticleSystemRenderer) && obj.sharedMaterials.Length > 0 && obj.sharedMaterials[0] != null && !obj.sharedMaterials[0].name.Contains("Default-Material")).ToArray();

                if (meshRenderers.Length > 0)
                {
                    MultiBarrelAttachment multiBarrelAttachment = newAttachment.gameObject.AddComponent<MultiBarrelAttachment>();
                    multiBarrelAttachment.Attachment = newAttachment;
                    multiBarrelAttachment.Mount = Mount;

                    if (breakAction != null) multiBarrelAttachment.breakAction = breakAction;
                    else if (derringer != null) multiBarrelAttachment.derringer = derringer;

                    if (newAttachment is MuzzleDevice)
                    {
                        if (meshRenderers.Length == 1) multiBarrelAttachment.Viz = meshRenderers[0].gameObject;
                        else
                        {
                            Transform viz = newAttachment.transform.Find("Viz");
                            if (viz != null) multiBarrelAttachment.Viz = viz.gameObject;
                            if (multiBarrelAttachment.Viz == null)
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
                        }

                        Vector3 attachmentOffset;

                        Suppressor suppressor = newAttachment as Suppressor;
                        if (breakAction != null && multiBarrelAttachment.Viz != null)
                        {
                            multiBarrelAttachment.breakAction = breakAction;

                            foreach (var barrel in breakAction.Barrels)
                            {
                                attachmentOffset = barrel.Muzzle.position - Mount.transform.position;
                                GameObject vizCopy = Instantiate(multiBarrelAttachment.Viz, multiBarrelAttachment.Viz.transform.position + attachmentOffset, multiBarrelAttachment.Viz.transform.rotation, newAttachment.transform);
                                multiBarrelAttachment.VizCopies.Add(vizCopy);
                                if (suppressor != null && suppressor.CatchRot < 359f) vizCopy.SetActive(false);
                            }
                            if (!(suppressor != null && suppressor.CatchRot < 359f)) multiBarrelAttachment.Viz.SetActive(false);
                        }
                        else if (derringer != null && multiBarrelAttachment.Viz != null)
                        {
                            multiBarrelAttachment.derringer = derringer;

                            foreach (var barrel in derringer.Barrels)
                            {
                                attachmentOffset = barrel.MuzzlePoint.position - Mount.transform.position;
                                GameObject vizCopy = Instantiate(multiBarrelAttachment.Viz, multiBarrelAttachment.Viz.transform.position + attachmentOffset, multiBarrelAttachment.Viz.transform.rotation, newAttachment.transform);
                                multiBarrelAttachment.VizCopies.Add(vizCopy);
                                if (suppressor != null && suppressor.CatchRot < 359f) vizCopy.SetActive(false);
                            }
                            if (!(suppressor != null && suppressor.CatchRot < 359f)) multiBarrelAttachment.Viz.SetActive(false);
                        }
                        if (suppressor != null && suppressor.CatchRot >= 359f)
                        {
                            suppressor.ForceBreakInteraction();
                            suppressor.AttachmentInterface.ForceBreakInteraction();
                        }
                    }
                }
            }
        }

        private List<FVRFireArmAttachment> GetAllSubAttachments()
        {
            List <FVRFireArmAttachment> subAttachments = new List<FVRFireArmAttachment>();
            foreach (var SubMount in Attachment.AttachmentInterface.SubMounts)
            {
                subAttachments.AddRange(SubMount.AttachmentsList);
            }

            return subAttachments;
        }
    }
}
