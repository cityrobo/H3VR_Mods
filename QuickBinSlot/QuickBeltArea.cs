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
    public class QuickBeltArea : StandaloneQBSlot
	{
        public FVRPhysicalObject MainObject;
        [Tooltip("Preconfigured QBSlot that will be used as a reference to create all other slots.")]
        public GameObject SubQBSlotPrefab;
        [Tooltip("Please try to use base 2 numbers, like 2 ,4 ,8 ,16 ,32 ,64 etc.")]
        public int ItemLimit = 32;

        [Header("Advanced Size Options")]
        public bool UsesAdvancedSizeMode = false;
        [Tooltip("Capacity requirement for items of size Small, Medium, Large, Massive, CantCarryBig")]
        public int[] Sizes = { 1, 2, 5, 10, 25 };
        public int TotalCapacity = 50;

        [Header("Collision Settings")]
        public bool ObjectsKeepCollision = false;
        [Tooltip("This setting requires a manually placed QuickBeltAreaCollisionDetector on the FVRPhysicalObject.")]
        public bool CollisionActivatedFreeze = false;
        public QuickBeltAreaCollisionDetector CollisionDetector;
        public bool SetKinematic = false;

        [HideInInspector]
        public bool ItemDidCollide = false;
		private Dictionary<FVRQuickBeltSlot, FVRPhysicalObject> _quickBeltSlots;
        private FVRPhysicalObject.FVRPhysicalObjectSize[] _sizes =
        {
            FVRPhysicalObject.FVRPhysicalObjectSize.Small,
            FVRPhysicalObject.FVRPhysicalObjectSize.Medium,
            FVRPhysicalObject.FVRPhysicalObjectSize.Large,
            FVRPhysicalObject.FVRPhysicalObjectSize.Massive,
            FVRPhysicalObject.FVRPhysicalObjectSize.CantCarryBig
        };

        private Dictionary<FVRPhysicalObject.FVRPhysicalObjectSize, int> _SizeRequirements;
        private int _currentLoad = 0;

        private class SubQBSlot
        {
            SubQBSlot(StandaloneQBSlot slot, Vector3 localPos, Quaternion localRot)
            {
                this.slot = slot;
                this.localPos = localPos;
                this.localRot = localRot;
            }

            public StandaloneQBSlot slot;
            public Vector3 localPos;
            public Quaternion localRot;
        }
#if !(UNITY_EDITOR || UNITY_5)
        /*
        public void Awake()
        {
            Hook();
        }
        */
        public override void OnDestroy()
        {
            Unhook();
        }
        public override void Start()
        {
            base.Start();
            Hook();
            if (MainObject == null)
            {
                FVRPhysicalObject myObject = GetComponent<FVRPhysicalObject>();

                while (myObject == null)
                {
                    Transform parent = transform.parent;
                    myObject = parent.GetComponent<FVRPhysicalObject>();
                }

                MainObject = myObject;
            }

            _quickBeltSlots = new Dictionary<FVRQuickBeltSlot, FVRPhysicalObject>();
            

            _SizeRequirements = new Dictionary<FVRPhysicalObject.FVRPhysicalObjectSize, int>();
            for (int i = 0; i < _sizes.Length; i++)
            {
                _SizeRequirements.Add(_sizes[i], Sizes[i]);
            }

            List<FVRQuickBeltSlot> qbSlots = new List<FVRQuickBeltSlot>();

            for (int i = 0; i < ItemLimit; i++)
            {
                FVRQuickBeltSlot qbSlot = Instantiate(SubQBSlotPrefab).GetComponent<FVRQuickBeltSlot>();

                qbSlot.gameObject.name = "QuickBeltAreaSubSlot_" + _quickBeltSlots.Count;

                
                qbSlot.gameObject.transform.parent = transform;
                qbSlot.gameObject.transform.localPosition = Vector3.zero;
                qbSlot.gameObject.transform.localRotation = Quaternion.identity;

                MainObject.GetFlagDic().Add("QuickBeltAreaSubSlot_" + _quickBeltSlots.Count, Vector3.zero.ToString("F6") + ";" + Quaternion.identity.ToString("F6"));

                _quickBeltSlots.Add(qbSlot, null);
                qbSlots.Add(qbSlot);
            }
            MainObject.Slots.Concat(qbSlots.ToArray());

            SubQBSlotPrefab.SetActive(false);
        }

        public void LateUpdate()
        {
            if (!CollisionActivatedFreeze && CurObject != null)
            {
                CreateNewQBSlotPos(CurObject);
            }
            else if (CurObject != null)
            {
                StartCoroutine(WaitForCollision(CurObject));
            }

            //List<GameObject> slotsToDelete = new List<GameObject>();
            List<FVRQuickBeltSlot> slotsToEmpty = new List<FVRQuickBeltSlot>();
            foreach (var quickBeltSlot in _quickBeltSlots)
            {
                
                FVRQuickBeltSlot slot = quickBeltSlot.Key;
                
                //if (slot.CurObject == null) slotsToDelete.Add(quickBeltSlot.Key);
                if (slot.CurObject != null && !slot.CurObject.m_isSpawnLock && !slot.CurObject.m_isHardnessed) slot.HoverGeo.SetActive(false);
                
                if (ObjectsKeepCollision && slot.CurObject != null) slot.CurObject.SetAllCollidersToLayer(false, "Default");
                
                if (SetKinematic && slot.CurObject != null && slot.CurObject.transform.localPosition != Vector3.zero) slot.CurObject.transform.localPosition = Vector3.zero;
                if (SetKinematic && slot.CurObject != null && slot.CurObject.transform.localRotation != Quaternion.identity) slot.CurObject.transform.localRotation = Quaternion.identity;
            }
            /*
            foreach (var slotToDelete in slotsToDelete)
            {
                _quickBeltSlots.Remove(slotToDelete);

                FVRPhysicalObject physicalObject = slotToDelete.GetComponentInChildren<FVRPhysicalObject>();
                if (physicalObject != null && physicalObject.m_hand != null)
                {
                    physicalObject.SetParentage(physicalObject.m_hand.WholeRig);
                }
                if (UsesAdvancedSizeMode)
                {
                    FVRPhysicalObject.FVRPhysicalObjectSize size = slotToDelete.GetComponent<FVRQuickBeltSlot>().SizeLimit;
                    int sizeRequirement = 0;
                    _SizeRequirements.TryGetValue(size, out sizeRequirement);

                    _currentLoad -= sizeRequirement;
                }
                Destroy(slotToDelete);
            }
            

            if (ItemLimit != 0)
            {
                if (_quickBeltSlots.Count >= ItemLimit) this.IsSelectable = false;
                else this.IsSelectable = true;
            }
            */

            foreach (var qbSlot in _quickBeltSlots)
            {
                if (qbSlot.Value != qbSlot.Key.CurObject)
                {
                    slotsToEmpty.Add(qbSlot.Key);
                }
            }

            foreach (var clearQB in slotsToEmpty)
            {
                if (UsesAdvancedSizeMode)
                {
                    FVRPhysicalObject.FVRPhysicalObjectSize size = clearQB.SizeLimit;
                    int sizeRequirement = 0;
                    _SizeRequirements.TryGetValue(size, out sizeRequirement);

                    _currentLoad -= sizeRequirement;
                }

                _quickBeltSlots[clearQB] = null;
            }


            if (UsesAdvancedSizeMode)
            {
                if (_currentLoad >= TotalCapacity) this.IsSelectable = false;
                else this.IsSelectable = true;
            }
        }

        public void CreateNewQBSlotPos(FVRPhysicalObject physicalObject)
        {
            FVRPhysicalObject.FVRPhysicalObjectSize size = physicalObject.Size;

            Vector3 pos = physicalObject.transform.position;
            Quaternion rot = physicalObject.transform.rotation;
            Quaternion localRot = rot;
            if (!SetKinematic && physicalObject.QBPoseOverride != null)
            {
                pos = physicalObject.QBPoseOverride.position;
                rot = physicalObject.QBPoseOverride.rotation;
                localRot = physicalObject.QBPoseOverride.localRotation;
            }
            else if (!SetKinematic && physicalObject.PoseOverride_Touch != null && (GM.HMDMode == ControlMode.Oculus || GM.HMDMode == ControlMode.Index))
            {
                pos = physicalObject.PoseOverride_Touch.position;
                rot = physicalObject.PoseOverride_Touch.rotation;
                localRot = physicalObject.PoseOverride_Touch.localRotation;
            }
            else if (!SetKinematic && physicalObject.PoseOverride != null)
            {
                pos = physicalObject.PoseOverride.position;
                rot = physicalObject.PoseOverride.rotation;
                localRot = physicalObject.PoseOverride.localRotation;
            }

            if (UsesAdvancedSizeMode)
            {
                int sizeRequirement = 0;
                _SizeRequirements.TryGetValue(size, out sizeRequirement);
                if (_currentLoad + sizeRequirement > TotalCapacity)
                {
                    physicalObject.ForceObjectIntoInventorySlot(null);
                    return;
                }
                else
                {
                    _currentLoad += sizeRequirement;
                }
            }

            FVRQuickBeltSlot slot = GetEmptySlot();

            if (slot == null) physicalObject.ForceObjectIntoInventorySlot(null);
            slot.transform.SetPositionAndRotation(pos, rot);
            slot.SizeLimit = size;
            physicalObject.ForceObjectIntoInventorySlot(slot);

            _quickBeltSlots[slot] = physicalObject;
            if (SetKinematic)
            {
                physicalObject.RootRigidbody.isKinematic = true;
                slot.transform.rotation = rot;
            }
            else slot.PoseOverride.rotation = rot;
        }

        IEnumerator WaitForCollision(FVRPhysicalObject physicalObject)
        {
            physicalObject.SetParentage(null);
            physicalObject.SetQuickBeltSlot(null);
            ItemDidCollide = false;
            CollisionDetector.PhysicalObjectToDetect = physicalObject;
            while (!ItemDidCollide) yield return null;
            ItemDidCollide = false;
            CreateNewQBSlotPos(physicalObject);
        }

        FVRQuickBeltSlot GetEmptySlot()
        {
            foreach (var qbSlot in _quickBeltSlots)
            {
                if (qbSlot.Value == null) return qbSlot.Key;
            }

            return null;
        }

        private void Unhook()
        {
#if !(DEBUG || UNITY_EDITOR || UNITY_5)
            On.FistVR.FVRPhysicalObject.ConfigureFromFlagDic -= FVRPhysicalObject_ConfigureFromFlagDic;
#endif
        }

        private void Hook()
        {
#if !(DEBUG || UNITY_EDITOR || UNITY_5)
            On.FistVR.FVRPhysicalObject.ConfigureFromFlagDic += FVRPhysicalObject_ConfigureFromFlagDic;
#endif
        }
#if !(DEBUG || UNITY_EDITOR || UNITY_5)
        private void FVRPhysicalObject_ConfigureFromFlagDic(On.FistVR.FVRPhysicalObject.orig_ConfigureFromFlagDic orig, FVRPhysicalObject self, Dictionary<string, string> f)
        {
            orig(self, f);
            if (MainObject == self)
            {
                for (int i = 0; i < ItemLimit; i++)
                {
                    string posRot = f["QuickBeltAreaSubSlot_" + i];
                    posRot = posRot.Replace(" ", "");
                    string[] posRotSep = posRot.Split(';');

                    string[] posString = posRotSep[0].Split(',');
                    string[] rotString = posRotSep[1].Split(',');

                    Vector3 pos = new Vector3(float.Parse(posString[0]), float.Parse(posString[1]), float.Parse(posString[2]));
                    Quaternion rot = new Quaternion(float.Parse(rotString[0]), float.Parse(rotString[1]), float.Parse(rotString[2]), float.Parse(rotString[3]));
                    _quickBeltSlots.ElementAt(i).Key.transform.localPosition = pos;
                    _quickBeltSlots.ElementAt(i).Key.transform.localRotation = rot;
                }
            }
        }
#endif
#endif
    }
}
