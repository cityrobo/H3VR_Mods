using BepInEx;
using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using OpenScripts2;

namespace Cityrobo
{
    public class RailHighlightController : MonoBehaviour
    {
        private GameObject _alternativeBoxMesh;
        private MultipleHideOnAttach _multipleHideOnAttach;
        private FVRFireArmAttachmentMount _mount;
        private FVRViveHand[] _hands;
        private bool _highlightEnabled = false;
        private readonly Dictionary<MeshRenderer, Material[]> _originalMeshRendererMaterials = new();

        private Material HighlightMaterial => HighlightHiddenRails_BepInEx._highlightMaterial;
        private bool _isHovered = false;

        private static readonly Dictionary<FVRFireArmAttachmentMount, RailHighlightController> _existingHighlightControllers = new();

        public void Awake()
        {
            // Either grab the vanilla mount or my own script
            _multipleHideOnAttach = GetComponent<MultipleHideOnAttach>();
            if (_multipleHideOnAttach != null)
            {
                _mount = _multipleHideOnAttach.attachmentMount;
            }
            else
            {
                _mount = GetComponent<FVRFireArmAttachmentMount>();
            }

            if (HighlightHiddenRails_BepInEx.HighlightMountsWithoutAHiddenMesh.Value && !_mount.HasHoverEnablePiece)
            {
                CreateProxyMesh();
            }

            // Grab the players hands so we can test them for held objects later
            _hands = GM.CurrentMovementManager.Hands;

            // Store all the default materials of the hidden rails so we can swap them out later but also restore them
            MeshRenderer[] meshRenderers;
            if (_multipleHideOnAttach != null)
            {
                meshRenderers = _multipleHideOnAttach.ObjectToHideOrShow.SelectMany(o => o.GetComponentsInChildren<MeshRenderer>(true)).ToArray();
            }
            else if (_mount.HasHoverEnablePiece)
            {
                meshRenderers = _mount.EnableOnHover.GetComponentsInChildren<MeshRenderer>(true);
            }
            else
            {
                meshRenderers = [_alternativeBoxMesh.GetComponent<MeshRenderer>()] ;
            }

            foreach (var meshRenderer in meshRenderers)
            {
                _originalMeshRendererMaterials.Add(meshRenderer, meshRenderer.sharedMaterials);
            }

            _existingHighlightControllers.Add(_mount, this);
        }

        public void OnDestroy()
        {
            _existingHighlightControllers.Remove(_mount);
        }

        public void Update()
        {
            // either the config setting is not applied or we need to check if the mount's parent is being held
            bool isHeld = !HighlightHiddenRails_BepInEx.OnlyHighlightOnHeldObject.Value || HighlightHiddenRails_BepInEx.OnlyHighlightOnHeldObject.Value && _mount.GetRootMount().Parent.IsHeld;

            // We don't want our code to run if our mount is already being hovered by an attachment
            if (isHeld && !_isHovered)
            {
                // If we're not already highlighting the rail, the mount is empty, and we are holding a compatible attachment, highlight the rail
                if (!_highlightEnabled && !_mount.HasAttachmentsOnIt() && _hands.Any(h => h.CurrentInteractable != null && h.CurrentInteractable is FVRFireArmAttachment attachment && attachment.Type == _mount.Type))
                {
                    EnableHighlighting();
                }
                // If we're highlighting the rail, the mount is empty, and we are not holding a compatible attachment or no attachment at all, disable the highlighting
                else if (_highlightEnabled && !_mount.HasAttachmentsOnIt() && _hands.All(h => h.CurrentInteractable is not FVRFireArmAttachment || h.CurrentInteractable is FVRFireArmAttachment attachment && attachment.Type != _mount.Type))
                {
                    DisableHighlighting(true);
                }
                // If we're highlighting the rail and the mount is not empty anyore, disable the highlighting but keep the hidden part active.
                else if (_highlightEnabled && _mount.HasAttachmentsOnIt())
                {
                    DisableHighlighting(false);
                }
            }
            // Disable highlighting according to mount status when not held, if required
            else if (!isHeld)
            {
                if (_highlightEnabled && !_mount.HasAttachmentsOnIt())
                {
                    DisableHighlighting(true);
                }
                else if (_highlightEnabled && _mount.HasAttachmentsOnIt())
                {
                    DisableHighlighting(false);
                }
            }
        }

        private void EnableHighlighting()
        {
            _highlightEnabled = true;

            foreach (var meshRenderer in _originalMeshRendererMaterials.Keys)
            {
                // Couldn't replace the entries in the sharedMaterials array one by one because the changes didn't "stick". only replacing the entire array worked.
                Material[] replacementMats = new Material[meshRenderer.sharedMaterials.Length];
                for (var i = 0; i < replacementMats.Length; i++)
                {
                    replacementMats[i] = HighlightMaterial;
                }
                meshRenderer.sharedMaterials = replacementMats;
            }

            // Enable the hidden part outside of their normal operation
            if (_multipleHideOnAttach != null) _multipleHideOnAttach.ManualOverride = true;
            else if (_mount.HasHoverEnablePiece) _mount.EnableOnHover.SetActive(true);
            else _alternativeBoxMesh?.SetActive(true);
        }

        private void DisableHighlighting(bool disableVis)
        {
            _highlightEnabled = false;

            // If we put the attachment on the rail, we don't wanna hide it again
            if (disableVis)
            {
                if (_multipleHideOnAttach != null) _multipleHideOnAttach.ManualOverride = false;
                else if (_mount.HasHoverEnablePiece) _mount.EnableOnHover.SetActive(false);
            }

            _alternativeBoxMesh?.SetActive(false);

            // Reset materials to default
            foreach (var meshRendererPair in _originalMeshRendererMaterials)
            {
                meshRendererPair.Key.sharedMaterials = meshRendererPair.Value;
            }
        }

        private void CreateProxyMesh()
        {
            Collider collider = _mount.GetComponent<Collider>();

            if (collider != null)
            {
                // Create a cube and parent it to the mount
                _alternativeBoxMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _alternativeBoxMesh.transform.parent = _mount.transform;
                _alternativeBoxMesh.transform.position = _mount.transform.position;
                _alternativeBoxMesh.transform.rotation = _mount.transform.rotation;
                
                // Scale cube according to mount collider size
                Vector3 colliderSize = GetColliderSize(collider);
                _alternativeBoxMesh.transform.localScale = colliderSize;

                // Disable the cube so it's hidden by default
                _alternativeBoxMesh.SetActive(false);
            }
            else
            {
                // If we didn't find a collider means that this mount isn't going to work anyway cause it doesn't have a trigger!
                Destroy(this);
            }
        }

        Vector3 GetColliderSize(Collider collider)
        {
            if (collider is BoxCollider boxCollider)
            {
                return boxCollider.size;
            }
            else if (collider is SphereCollider sphereCollider)
            {
                float diameter = sphereCollider.radius * 2;
                return new Vector3(diameter, diameter, diameter);
            }
            else if (collider is CapsuleCollider capsuleCollider)
            {
                float diameter = capsuleCollider.radius * 2;
                float height = capsuleCollider.height;

                // Approximate the size of the capsule as a box dependent on direction of the capsule
                return capsuleCollider.direction switch
                {
                    0 => new Vector3(height, diameter, diameter),
                    1 => new Vector3(diameter, height, diameter),
                    2 => new Vector3(diameter, diameter, height),
                    _ => new Vector3(),
                };
            }
            else
            {
                // For other types of colliders, just use the bounds size
                return collider.bounds.size;
            }
        }

#if !DEBUG
        static RailHighlightController()
        {
            On.FistVR.FVRFireArmAttachmentMount.BeginHover += FVRFireArmAttachmentMount_BeginHover;
            On.FistVR.FVRFireArmAttachmentMount.EndHover += FVRFireArmAttachmentMount_EndHover;
        }

        // Patching the Hovering code so that we can "physicalize" the hidden rail once you bring an attachment near it
        private static void FVRFireArmAttachmentMount_BeginHover(On.FistVR.FVRFireArmAttachmentMount.orig_BeginHover orig, FVRFireArmAttachmentMount self)
        {
            orig(self);

            if (_existingHighlightControllers.TryGetValue(self, out RailHighlightController highlightController))
            {
                highlightController.DisableHighlighting(false);
                highlightController._isHovered = true;
            }
        }
        // Same as above, but the reverse, turning the rail back into a ghost once you bring the held attachment far enough away
        private static void FVRFireArmAttachmentMount_EndHover(On.FistVR.FVRFireArmAttachmentMount.orig_EndHover orig, FVRFireArmAttachmentMount self)
        {
            orig(self);

            if (_existingHighlightControllers.TryGetValue(self, out RailHighlightController highlightController))
            {
                highlightController.EnableHighlighting();
                highlightController._isHovered = false;
            }
        }
#endif
    }
}