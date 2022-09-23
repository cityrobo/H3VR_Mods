using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Cityrobo
{
    public class QuickBottomlessSlot : FVRQuickBeltSlot
    {
        [Header("QuickBottomlessSlot settings")]
        public int maxItems = 30;
        public float timeBetweenMagSwitch = 0.25f;
        public Color hoverColor;
        public bool storesMagazines = true;
        public bool storesClips = true;
        public bool storesSpeedloaders = true;
        public bool onlyStoresEmpty = false;

        public AudioEvent insertSound;
        public AudioEvent extractSound;
        public AudioEvent failureSound;

        public GameObject canvas;
        public string textPrefix = "Items: ";
        public Text numberOfItemsDisplay;
        public bool textTurnsOffOnNoItemsStored = true;

        [ContextMenu("CopyQBSlot")]
        public void CopyQBSlot()
        {
            FVRQuickBeltSlot QBS = GetComponent<FVRQuickBeltSlot>();

            this.QuickbeltRoot = QBS.QuickbeltRoot;
            this.PoseOverride = QBS.PoseOverride;
            this.SizeLimit = QBS.SizeLimit;
            this.Shape = QBS.Shape;
            this.Type = QBS.Type;
            this.HoverGeo = QBS.HoverGeo;
            this.RectBounds = QBS.RectBounds;
            this.CurObject = QBS.CurObject;
            this.IsSelectable = QBS.IsSelectable;
            this.IsPlayer = QBS.IsPlayer;
            this.UseStraightAxisAlignment = QBS.UseStraightAxisAlignment;
            this.HeldObject = QBS.HeldObject;
        }

#if !(UNITY_EDITOR || UNITY_5)
        private GameObject currentSelectedObject;

        private List<GameObject> storedGameObjects;

        private bool switchingObject = false;

        private int selectedObjectIndex = 0;

        private float timeWaited = 0f;

        public void Start()
        {
            storedGameObjects = new List<GameObject>();

            Hook();
        }

        public void OnDestroy()
        {
            Unhook();
        }
        void Unhook()
        {
#if !(DEBUG || MEATKIT)
            On.FistVR.FVRQuickBeltSlot.Update -= FVRQuickBeltSlot_Update;
            On.FistVR.FVRQuickBeltSlot.MoveContents -= FVRQuickBeltSlot_MoveContents;
            On.FistVR.FVRQuickBeltSlot.MoveContentsInstant -= FVRQuickBeltSlot_MoveContentsInstant;
            On.FistVR.FVRQuickBeltSlot.MoveContentsCheap -= FVRQuickBeltSlot_MoveContentsCheap;
#endif
        }
        void Hook()
        {
#if !(DEBUG || MEATKIT)
            On.FistVR.FVRQuickBeltSlot.Update += FVRQuickBeltSlot_Update;
            On.FistVR.FVRQuickBeltSlot.MoveContents += FVRQuickBeltSlot_MoveContents;
            On.FistVR.FVRQuickBeltSlot.MoveContentsInstant += FVRQuickBeltSlot_MoveContentsInstant;
            On.FistVR.FVRQuickBeltSlot.MoveContentsCheap += FVRQuickBeltSlot_MoveContentsCheap;
#endif
        }
#if !(DEBUG || MEATKIT)
        private void FVRQuickBeltSlot_MoveContentsCheap(On.FistVR.FVRQuickBeltSlot.orig_MoveContentsCheap orig, FVRQuickBeltSlot self, Vector3 dir)
        {
            if (self == this)
            {
                if (this.currentSelectedObject != null)
                {
                    FVRPhysicalObject mag = this.currentSelectedObject.GetComponent<FVRPhysicalObject>();
                    if (mag.IsHeld)
                    {
                        return;
                    }
                    mag.RootRigidbody.position = mag.RootRigidbody.position + dir;
                    mag.RootRigidbody.velocity = Vector3.zero;
                }
            }
            else orig(self, dir);
        }

        private void FVRQuickBeltSlot_MoveContentsInstant(On.FistVR.FVRQuickBeltSlot.orig_MoveContentsInstant orig, FVRQuickBeltSlot self, Vector3 dir)
        {
            if (self == this)
            {
                if (this.currentSelectedObject != null)
                {
                    FVRPhysicalObject mag = this.currentSelectedObject.GetComponent<FVRPhysicalObject>();
                    if (mag.IsHeld)
                    {
                        return;
                    }
                    mag.transform.position = mag.transform.position + dir;
                    mag.RootRigidbody.velocity = Vector3.zero;
                }
            }
            else orig(self, dir);
        }

        private void FVRQuickBeltSlot_MoveContents(On.FistVR.FVRQuickBeltSlot.orig_MoveContents orig, FVRQuickBeltSlot self, Vector3 dir)
        {
            if (self == this)
            {
                if (this.currentSelectedObject != null)
                {
                    FVRPhysicalObject mag = this.currentSelectedObject.GetComponent<FVRPhysicalObject>();
                    if (mag.IsHeld)
                    {
                        return;
                    }
                    mag.transform.position = mag.transform.position + dir;
                    mag.RootRigidbody.velocity = Vector3.zero;
                }
            }
            else orig(self, dir);
        }

        private void FVRQuickBeltSlot_Update(On.FistVR.FVRQuickBeltSlot.orig_Update orig, FVRQuickBeltSlot self)
        {
            if (this == self)
            {
                /*
                //Debug.Log("currentSelectedObject: " + currentSelectedObject);
                List<GameObject> rootObjectsInScene = new List<GameObject>();
                Scene scene = SceneManager.GetActiveScene();
                scene.GetRootGameObjects(rootObjectsInScene);
                bool flag = false;
                foreach (var item in storedGameObjects)
                {

                    if (item == null) flag = true;

                }
                if (flag || storedGameObjects.Count != storedItemIDs.Count)
                {
                    Debug.Log("flag: " + flag);

                    storedGameObjects = new List<GameObject>();
                    for (int i = 0; i < rootObjectsInScene.Count; i++)
                    {
                        BottomlessSlotItem[] allComponents = rootObjectsInScene[i].GetComponentsInChildren<BottomlessSlotItem>(true);
                        for (int j = 0; j < allComponents.Length; j++)
                        {
                            if (allComponents[j].IDs.slotID == this.slotID && storedItemIDs.Contains(allComponents[j].IDs.itemID))
                            {
                                storedGameObjects.Add(allComponents[j].gameObject);
                            }
                        }
                    }
                }
                */



                if (!GM.CurrentSceneSettings.IsSpawnLockingEnabled && this.HeldObject != null && (this.HeldObject as FVRPhysicalObject).m_isSpawnLock)
                {
                    (this.HeldObject as FVRPhysicalObject).m_isSpawnLock = false;
                }
                if (this.HeldObject != null)
                {
                    if ((this.HeldObject as FVRPhysicalObject).m_isSpawnLock)
                    {
                        if (!this.HoverGeo.activeSelf)
                        {
                            this.HoverGeo.SetActive(true);
                        }
                        this.m_hoverGeoRend.material.SetColor("_RimColor", new Color(0.3f, 0.3f, 1f, 1f));
                    }
                    else if ((this.HeldObject as FVRPhysicalObject).m_isHardnessed)
                    {
                        if (!this.HoverGeo.activeSelf)
                        {
                            this.HoverGeo.SetActive(true);
                        }
                        this.m_hoverGeoRend.material.SetColor("_RimColor", new Color(0.3f, 1f, 0.3f, 1f));
                    }
                    else
                    {
                        if (this.HoverGeo.activeSelf != this.IsHovered)
                        {
                            this.HoverGeo.SetActive(this.IsHovered);
                        }
                        this.m_hoverGeoRend.material.SetColor("_RimColor", hoverColor);
                    }
                }
                else
                {
                    if (this.HoverGeo.activeSelf != this.IsHovered)
                    {
                        this.HoverGeo.SetActive(this.IsHovered);
                    }
                    this.m_hoverGeoRend.material.SetColor("_RimColor", hoverColor);
                }

                //if (!switchingObject && currentSelectedObject != null)
                //{
                //    FVRFireArmMagazine mag = currentSelectedObject.GetComponent<FVRFireArmMagazine>();
                //    if (mag.IsHeld)
                //    {
                //        storedGameObjects.RemoveAt(selectedMag);
                //        Debug.Log("RemovedMagFromStorage");
                //        audioSource.PlayClip(extractSound, this.transform.position);
                //    }
                //    else currentSelectedObject.SetActive(false);
                //}

                if (storesMagazines && CurObject != null && CurObject is FVRFireArmMagazine && storedGameObjects.Count < maxItems)
                {
                    StoreCurObject();
                }
                else if (storesClips && CurObject != null && CurObject is FVRFireArmClip && storedGameObjects.Count < maxItems)
                {
                    StoreCurObject();
                }
                else if (storesSpeedloaders && CurObject != null && CurObject is Speedloader && storedGameObjects.Count < maxItems)
                {
                    StoreCurObject();
                }
                else if (CurObject != null)
                {
                    EjectCurObject();
                }

                if (storedGameObjects.Count > 0)
                {
                    //if (!switchingObject) StartCoroutine(SelectObjectSlow());
                    //if (!switchingObject) StartCoroutine(SelectObjectInstantly());
                    timeWaited += Time.deltaTime;
                    if (!switchingObject)
                    {
                        timeWaited = 0f;
                        SelectObject();
                    }
                    else if (timeWaited > timeBetweenMagSwitch)
                    {
                        switchingObject = false;
                    }

                    int removalIndex = -1;

                    for (int i = 0; i < storedGameObjects.Count; i++)
                    {
                        storedGameObjects[i].SetActive(i == selectedObjectIndex);

                        if (i != selectedObjectIndex)
                        {
                            if (QuickbeltRoot != null)
                            {
                                storedGameObjects[i].transform.position = this.QuickbeltRoot.position;
                                storedGameObjects[i].transform.rotation = this.QuickbeltRoot.rotation;
                            }
                            else if (PoseOverride != null)
                            {
                                storedGameObjects[i].transform.position = this.PoseOverride.position;
                                storedGameObjects[i].transform.rotation = this.PoseOverride.rotation;
                            }
                            else
                            {
                                storedGameObjects[i].transform.position = this.transform.position;
                                storedGameObjects[i].transform.rotation = this.transform.rotation;
                            }
                        }

                        FVRPhysicalObject objectComponent = storedGameObjects[i].GetComponent<FVRPhysicalObject>();
                        if (objectComponent.IsHeld)
                        {
                            removalIndex = i;
                        }
                        if (objectComponent.m_isSpawnLock)
                        {
                            objectComponent.m_isSpawnLock = false;
                        }
                    }

                    if (removalIndex >= 0)
                    {
                        storedGameObjects[removalIndex].SetActive(true);
                        storedGameObjects.RemoveAt(removalIndex);
                        SM.PlayGenericSound(extractSound, this.transform.position);
                        switchingObject = false;
                    }

                    if (canvas != null && numberOfItemsDisplay != null)
                    {
                        if (textTurnsOffOnNoItemsStored) canvas.SetActive(true);
                        numberOfItemsDisplay.text = textPrefix + storedGameObjects.Count.ToString();
                    }
                }
                else if (canvas != null && numberOfItemsDisplay != null)
                {
                    if (textTurnsOffOnNoItemsStored) canvas.SetActive(false);
                    numberOfItemsDisplay.text = textPrefix + storedGameObjects.Count.ToString();
                }

            }
            else orig(self);
        }
#endif
        void StoreCurObject()
        {
            if (!storedGameObjects.Contains(CurObject.gameObject))
            {
                if (onlyStoresEmpty)
                {
                    if (!CheckEmpty()) return;
                }
                storedGameObjects.Add(CurObject.gameObject);
                CurObject.gameObject.SetActive(false);

                CurObject = null;
                HeldObject = null;

                SM.PlayGenericSound(insertSound, this.transform.position);
            }
        }

        bool EjectCurObject()
        {
            CurObject.SetQuickBeltSlot(null);
            CurObject = null;
            HeldObject = null;
            SM.PlayGenericSound(failureSound, this.transform.position);
            return false;
        }

        bool CheckEmpty()
        {
            FVRFireArmMagazine mag = CurObject.gameObject.GetComponent<FVRFireArmMagazine>();
            FVRFireArmClip clip = CurObject.gameObject.GetComponent<FVRFireArmClip>();
            Speedloader speedloader = CurObject.gameObject.GetComponent<Speedloader>();

            if (mag != null && mag.m_numRounds > 0) return EjectCurObject();
            if (clip != null && clip.m_numRounds > 0) return EjectCurObject();
            if (speedloader != null)
            {
                bool notEmpty = false;
                foreach (var chamber in speedloader.Chambers)
                {
                    notEmpty = chamber.IsLoaded;
                }
                if (notEmpty) return EjectCurObject();
            }
            return true;
        }

        Vector3 GetPointInside()
        {
            switch (Shape)
            {
                case QuickbeltSlotShape.Sphere:
                    return UnityEngine.Random.insideUnitSphere * HoverGeo.transform.localScale.x;
                case QuickbeltSlotShape.Rectalinear:
                    return new Vector3(UnityEngine.Random.Range(-RectBounds.localScale.x, RectBounds.localScale.x), UnityEngine.Random.Range(-RectBounds.localScale.y, RectBounds.localScale.y), UnityEngine.Random.Range(-RectBounds.localScale.z, RectBounds.localScale.z)); ;
                default:
                    return new Vector3();
            }
        }


        void SelectObject()
        {
            switchingObject = true;
            selectedObjectIndex = UnityEngine.Random.Range(0, storedGameObjects.Count);
            currentSelectedObject = storedGameObjects[selectedObjectIndex];
            currentSelectedObject.SetActive(true);
        }

        IEnumerator SelectObjectSlow()
        {
            Debug.Log("SelectingObjectSlow");

            switchingObject = true;
            selectedObjectIndex = UnityEngine.Random.Range(0, storedGameObjects.Count - 1);

            Debug.Log("SelectedMag: " + selectedObjectIndex);
            currentSelectedObject = storedGameObjects[selectedObjectIndex];
            Debug.Log("SelectedMagObject: " + currentSelectedObject);
            currentSelectedObject.SetActive(true);
            Debug.Log("SelectedMagObject Active: " + currentSelectedObject.activeSelf);
            yield return new WaitForSeconds(0.25f);
            Debug.Log("SelectedMagObject: " + currentSelectedObject);
            Debug.Log("Checking hand");
            Debug.Log("StoredObjects: " + storedGameObjects);
            Debug.Log("StoredObjects: " + storedGameObjects.Count);
            Debug.Log("SelectedMag: " + selectedObjectIndex);
            Debug.Log("SelectedMagObject: " + storedGameObjects[selectedObjectIndex]);
            currentSelectedObject = storedGameObjects[selectedObjectIndex];
            Debug.Log("SelectedMagObject: " + currentSelectedObject);
            FVRFireArmMagazine mag = currentSelectedObject.GetComponent<FVRFireArmMagazine>();
            Debug.Log("SelectedMagMag: " + mag);
            if (mag.IsHeld)
            {
                Debug.Log("isHeld");
                storedGameObjects.RemoveAt(selectedObjectIndex);
                Debug.Log("RemovedMagFromStorage");
                SM.PlayGenericSound(extractSound, this.transform.position);
            }
            else
            {
                Debug.Log("!isHeld");
                currentSelectedObject.SetActive(false);
            }
            Debug.Log("SelectingObjectSlow End");
            switchingObject = false;
        }

        IEnumerator SelectObjectInstantly()
        {
            switchingObject = true;

            selectedObjectIndex = UnityEngine.Random.Range(0, storedGameObjects.Count - 1);
            currentSelectedObject = storedGameObjects[selectedObjectIndex];

            currentSelectedObject.SetActive(true);

            yield return new WaitForEndOfFrame();
            switchingObject = false;
        }
#endif
    }
    }
