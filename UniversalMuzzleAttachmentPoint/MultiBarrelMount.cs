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
    public class MultiBarrelMount : FVRFireArmAttachmentMount 
    {
        public BreakActionWeapon breakAction = null;
        public Derringer derringer = null;

        public void Start()
        {
            ParentToThis = true;
        }
        public void Update()
        {
            FVRFireArmAttachment attachment = GetComponentInChildren<FVRFireArmAttachment>();

            if ((breakAction != null || derringer != null) && attachment != null && attachment.GetComponent<MultiBarrelAttachment>() == null)
            {
                MeshRenderer[] meshRenderers = attachment.GetComponentsInChildren<MeshRenderer>();

                if (meshRenderers.Length > 0)
                {
                    MultiBarrelAttachment multiBarrelAttachment = attachment.gameObject.AddComponent<MultiBarrelAttachment>();
                    multiBarrelAttachment.FireArmAttachment = attachment;
                    multiBarrelAttachment.Mount = this;

                    if (meshRenderers.Length == 1) multiBarrelAttachment.Viz = meshRenderers[0].gameObject;
                    else
                    {
                        multiBarrelAttachment.Viz = new GameObject("Viz");
                        multiBarrelAttachment.Viz.transform.SetParent(attachment.transform);
                        multiBarrelAttachment.Viz.transform.localPosition = Vector3.zero;
                        multiBarrelAttachment.Viz.transform.localRotation = Quaternion.identity;

                        foreach (var meshRenderer in meshRenderers)
                        {
                            meshRenderer.transform.SetParent(multiBarrelAttachment.Viz.transform);
                        }
                    }

                    if (breakAction != null)
                    {
                        multiBarrelAttachment.breakAction = breakAction;

                        foreach (var barrel in breakAction.Barrels)
                        {
                            multiBarrelAttachment.VizCopies.Add(Instantiate(multiBarrelAttachment.Viz, barrel.Muzzle.position, barrel.Muzzle.rotation, attachment.transform));
                        }
                        multiBarrelAttachment.Viz.SetActive(false);
                    }
                    else if (derringer != null)
                    {
                        multiBarrelAttachment.derringer = derringer;

                        foreach (var barrel in derringer.Barrels)
                        {
                            multiBarrelAttachment.VizCopies.Add(Instantiate(multiBarrelAttachment.Viz, barrel.MuzzlePoint.position, barrel.MuzzlePoint.rotation, attachment.transform));
                        }
                        multiBarrelAttachment.Viz.SetActive(false);
                    }
                }
            }
        }
    }
}
