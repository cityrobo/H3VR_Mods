using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;


namespace Cityrobo
{
    public class AttachmentMountPicatinnyRail : MonoBehaviour
    {
        public FVRFireArmAttachmentMount Mount;
        public int NumberOfPicatinnySlots = 0;
        [Tooltip("Sound generated while moving attachment between slots.")]
        public AudioEvent SlotSound;

        [HideInInspector]
        public static Dictionary<FVRFireArmAttachment, AttachmentPicatinnyRailForwardStop> ExistingForwardStops = new();

        private static Dictionary<FVRFireArmAttachmentMount, AttachmentMountPicatinnyRail> _exisingAttachmentMountPicatinnyRail = new();
        
        private FVRFireArmAttachment _currentlyPatchedAttachment = null;
        private bool _isPatched = false;

        private float _slotLerpFactor = 0f;
        private static IntPtr _methodPointer;

        private int _lastPosIndex = -1;
#if (!DEBUG)
        /*
        static AttachmentMountPicatinnyRail()
        {
            _methodPointer = IntPtr.Zero;
#if !DEBUG
            On.FistVR.FVRFireArmAttachmentSensor.SetHoveredMount += FVRFireArmAttachmentSensor_SetHoveredMount;
            On.FistVR.FVRFireArmAttachment.AttachToMount += FVRFireArmAttachment_AttachToMount;
            On.FistVR.FVRFireArmAttachment.DetachFromMount += FVRFireArmAttachment_DetachFromMount;
#endif
        }
#if !DEBUG
        private static void FVRFireArmAttachmentSensor_SetHoveredMount(On.FistVR.FVRFireArmAttachmentSensor.orig_SetHoveredMount orig, FVRFireArmAttachmentSensor self, FVRFireArmAttachmentMount newMount)
        {
            AttachmentMountPicatinnyRail attachmentMountPicatinnyRail;
            if (newMount != null && _exisingAttachmentMountPicatinnyRail.TryGetValue(newMount, out attachmentMountPicatinnyRail))
            {
                attachmentMountPicatinnyRail._currentlyPatchedAttachment = self.Attachment;
            }
            else if (newMount == null && _exisingAttachmentMountPicatinnyRail.TryGetValue(self.CurHoveredMount, out attachmentMountPicatinnyRail))
            {
                attachmentMountPicatinnyRail._currentlyPatchedAttachment = null;
            }
            orig(self, newMount);
        }

        private static void FVRFireArmAttachment_AttachToMount(On.FistVR.FVRFireArmAttachment.orig_AttachToMount orig, FVRFireArmAttachment self, FVRFireArmAttachmentMount m, bool playSound)
        {
            AttachmentMountPicatinnyRail picatinnyRail;

            if (_exisingAttachmentMountPicatinnyRail.TryGetValue(m, out picatinnyRail))
            {
                self.curMount = m;
                self.StoreAndDestroyRigidbody();
                if (self.curMount.GetRootMount().ParentToThis)
                {
                    self.SetParentage(self.curMount.GetRootMount().transform);
                }
                else
                {
                    self.SetParentage(self.curMount.MyObject.transform);
                }
                if (self.IsBiDirectional)
                {
                    if (Vector3.Dot(self.transform.forward, self.curMount.transform.forward) >= 0f) self.transform.rotation = self.curMount.transform.rotation;
                    else self.transform.rotation = Quaternion.LookRotation(-self.curMount.transform.forward, self.curMount.transform.up);
                }
                else self.transform.rotation = self.curMount.transform.rotation;

                // New Code
                Vector3 front = self.curMount.Point_Front.position;
                Vector3 rear = self.curMount.Point_Rear.position;
                Vector3 closestValidPoint = self.GetClosestValidPoint(front, rear, self.transform.position);
                float inverseLerp = Vector3Utils.InverseLerp(front, rear, closestValidPoint);
                float posIndex = Mathf.Round(inverseLerp / picatinnyRail._slotLerpFactor);
                AttachmentPicatinnyRailForwardStop forwardStop;
                if (ExistingForwardStops.TryGetValue(self, out forwardStop))
                {
                    // Vector3 closestValidPointLimit = GetClosestValidPointUnclamped(front, rear, forwardStop.transform.position);
                    Vector3 closestValidPointLimit = closestValidPoint + self.transform.forward * Vector3.Distance(forwardStop.transform.position, self.transform.position);
                    float inverseLerpUnclamped = Vector3Utils.InverseLerpUnclamped(front, rear, closestValidPointLimit);
                    int posIndexLimit = (int)Mathf.Round(inverseLerpUnclamped / picatinnyRail._slotLerpFactor);
                    if (posIndexLimit < 0) posIndex -= posIndexLimit;
                    else if (posIndexLimit >= picatinnyRail.NumberOfPicatinnySlots) posIndex -= (posIndexLimit - (picatinnyRail.NumberOfPicatinnySlots - 1));
                }
                Vector3 snapPos = Vector3.Lerp(front, rear, posIndex * picatinnyRail._slotLerpFactor);
                self.transform.position = snapPos;
                //

                if (self.curMount.Parent != null)
                {
                    self.curMount.Parent.RegisterAttachment(self);
                }
                self.curMount.RegisterAttachment(self);
                if (self.curMount.Parent != null && self.curMount.Parent.QuickbeltSlot != null)
                {
                    self.SetAllCollidersToLayer(false, "NoCol");
                }
                else
                {
                    self.SetAllCollidersToLayer(false, "Default");
                }
                if (self.AttachmentInterface != null)
                {
                    self.AttachmentInterface.OnAttach();
                    self.AttachmentInterface.gameObject.SetActive(true);
                }
                self.SetTriggerState(false);
                if (self.DisableOnAttached != null)
                {
                    self.DisableOnAttached.SetActive(false);
                }
                picatinnyRail._currentlyPatchedAttachment = null;
            }
            else orig(self,m,playSound);
        }

        private static void FVRFireArmAttachment_DetachFromMount(On.FistVR.FVRFireArmAttachment.orig_DetachFromMount orig, FVRFireArmAttachment self)
        {
            AttachmentMountPicatinnyRail attachmentMountPicatinnyRail;
            if (_exisingAttachmentMountPicatinnyRail.TryGetValue(self.curMount, out attachmentMountPicatinnyRail))
            {
                attachmentMountPicatinnyRail._currentlyPatchedAttachment = self;
            }
            orig(self);
        }
#endif
        */
        public void Start()
        {
            StartCoroutine(ReplaceScript());
            /* _exisingAttachmentMountPicatinnyRail.Add(Mount, this);
            if (_methodPointer == IntPtr.Zero)
            {
                var _methodInfo = typeof(FVRPhysicalObject).GetMethod("GetPosTarget", BindingFlags.NonPublic | BindingFlags.Instance);
                _methodPointer = _methodInfo.MethodHandle.GetFunctionPointer();
            }

            _slotLerpFactor = 1f/(NumberOfPicatinnySlots - 1);
            */
        }


        IEnumerator ReplaceScript()
        {
            yield return null;

            OpenScripts2.AttachmentMountPicatinnyRail newRail = GetComponent<OpenScripts2.AttachmentMountPicatinnyRail>();
            if (newRail != null) Destroy(newRail);
            yield return null;

            gameObject.SetActive(false);
            newRail = gameObject.AddComponent<OpenScripts2.AttachmentMountPicatinnyRail>();

            newRail.Mount = Mount;
            newRail.NumberOfPicatinnySlots = NumberOfPicatinnySlots;
            newRail.SlotSound = SlotSound;

            gameObject.SetActive(true);
            Destroy(this);
        }
        /*
        public void OnDestoy()
        {
             _exisingAttachmentMountPicatinnyRail.Remove(Mount);
        }
        
        public void Update()
        {
            if (!_isPatched && _currentlyPatchedAttachment != null)
            {
#if !DEBUG
                On.FistVR.FVRFireArmAttachment.GetPosTarget += FVRFireArmAttachment_GetPosTarget;
#endif
                _isPatched = true;
            }
            else if (_isPatched && _currentlyPatchedAttachment == null)
            {
#if !DEBUG
                On.FistVR.FVRFireArmAttachment.GetPosTarget -= FVRFireArmAttachment_GetPosTarget;
#endif
                _isPatched = false;
                _lastPosIndex = -1;
            }
        }
#if !DEBUG
        private Vector3 FVRFireArmAttachment_GetPosTarget(On.FistVR.FVRFireArmAttachment.orig_GetPosTarget orig, FVRFireArmAttachment self)
        {
            if (_currentlyPatchedAttachment == self)
            {
                var func = (Func<Vector3>)Activator.CreateInstance(typeof(Func<Vector3>), self, _methodPointer);

                if (!(self.Sensor.CurHoveredMount != null))
                {
                    return func();
                }
                Vector3 front = self.Sensor.CurHoveredMount.Point_Front.position;
                Vector3 rear = self.Sensor.CurHoveredMount.Point_Rear.position;
                Vector3 closestValidPoint = self.GetClosestValidPoint(front, rear, self.m_handPos);

                if (Vector3.Distance(closestValidPoint, self.m_handPos) < 0.15f)
                {
                    float inverseLerp = Vector3Utils.InverseLerp(front, rear, closestValidPoint);
                    int posIndex = (int) Mathf.Round(inverseLerp / _slotLerpFactor);

                    AttachmentPicatinnyRailForwardStop forwardStop;
                    //Log($"posIndex Before Limit: {posIndex}");
                    if (ExistingForwardStops.TryGetValue(self, out forwardStop))
                    {
                        //Vector3 closestValidPointLimit = GetClosestValidPointUnclamped(front, rear, forwardStop.transform.position);

                        Vector3 closestValidPointLimit = closestValidPoint + self.transform.forward * Vector3.Distance(forwardStop.transform.position,self.transform.position);
                        float inverseLerpUnclamped = Vector3Utils.InverseLerpUnclamped(front, rear, closestValidPointLimit);
                        int posIndexLimit = (int)Mathf.Round(inverseLerpUnclamped / _slotLerpFactor);
                        //Log($"posIndexLimit: {posIndexLimit}");
                        if (posIndexLimit < 0) posIndex -= posIndexLimit;
                        else if (posIndexLimit >= NumberOfPicatinnySlots) posIndex -= (posIndexLimit - (NumberOfPicatinnySlots - 1));
                    }
                    //Log($"posIndex After Limit: {posIndex}");
                    Vector3 snapPos = Vector3.Lerp(front, rear, posIndex * _slotLerpFactor);

                    if (posIndex != _lastPosIndex)
                    {
                        SM.PlayGenericSound(SlotSound, self.transform.position);
                    }
                    _lastPosIndex = posIndex;
                    return snapPos;
                }
                return func();
            }

            return orig(self);
        }
#endif
    }
    public static class Vector3Utils
    {
        public static Vector3 ProjectOnPlaneThroughPoint(Vector3 vector, Vector3 point, Vector3 planeNormal)
        {
            return Vector3.ProjectOnPlane(vector, planeNormal) + Vector3.Dot(point, planeNormal) * planeNormal;
        }

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Mathf.Clamp01(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB));
        }

        public static float InverseLerpUnclamped(Vector3 a, Vector3 b, Vector3 value)
        {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }
        */
#endif
    }
}