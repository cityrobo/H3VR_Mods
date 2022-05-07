using FistVR;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace Cityrobo

{
    public class Adjustable_ReflexSight : MonoBehaviour
    {
        public FVRFireArmAttachment attachment;
        [Tooltip("This can be an AttachmentInterface or a standalone FVRInteractiveObject set to \" is simple interact \" ")]
        public FVRInteractiveObject reflexSightInterface;
        public MeshRenderer reticle;



        [Header("Reticle Settings")]
        [Tooltip("All reticle textures. Default reticle is first entry.")]
        public Texture2D[] textures;
        [Tooltip("Colors of all reticles. Default reticle name is first entry.")]
        [ColorUsage(true, true, float.MaxValue, float.MaxValue, 0f, 0f)]
        public Color[] reticleColors;
        [Tooltip("Names of all reticles. Default reticle name is first entry.")]
        public string[] reticleText;

        public int currentTexture = 0;

        [Header("If you want a Screen above the scope that shows stuff, use this:")]
        public Transform textFrame;
        public Text reticleTextScreen;
        public Text zeroTextScreen;

        public string reticleTestPrefix = "Reticle: ";


        public string zeroTextPrefix = "Zero Distance: ";
        [Tooltip("Index of the Array below, not the actual value")]
        public int currentZeroDistance = 3;
        [Tooltip("In meters. Miss me with that imperial shit!")]
        public float[] zeroDistances = new float[7] { 2, 5, 10, 15, 25, 50, 100 };
        //public float dotSizeAt100mDistance = 2;
        [Header("Intergrated Sight configuration")]
        [Tooltip("Check this box if integrated. (sorry, bad naming.)")]
        public bool isStandalone = false;
        public FVRFireArm fireArm;

        [Header("Reticle Occlusion culling")]
        [Tooltip("Use this for extra performant reticle occlusion culling")]
        public Collider lensCollider;
        public bool disableOcclusionCulling = false;

        [Header("Moving Switch Settings")]
        [Tooltip("Switch that moves with the selected texture")]
        public Transform buttonSwitch;
        public Vector3[] switchPositions;

        private FVRViveHand hand;
        private int currentMenu = 0;

        private bool zeroOnlyMode = false;
        private string nameOfTexture = "_RedDotTex";
        private string nameOfDistanceVariable = "_RedDotDist";
        private string nameOfXOffset = "_MuzzleOffsetX";
        private string nameOfYOffset = "_MuzzleOffsetY";
        private List<Collider> scopeColliders;
        //private string nameOfDotSizeVariable = "_RedDotSize";

        private Transform muzzlePos;

        private bool attached = false;

        private Vector3 leftEye;
        private Vector3 rightEye;
        public void Start()
        {
            if (currentTexture >= textures.Length || currentTexture < 0) currentTexture = 0;
            if (currentZeroDistance >= zeroDistances.Length || currentZeroDistance < 0) currentZeroDistance = 0;
            if (textures.Length != 0) reticle.material.SetTexture(nameOfTexture, textures[currentTexture]);
            reticle.material.SetFloat(nameOfDistanceVariable, zeroDistances[currentZeroDistance]);
            //lens.material.SetFloat(nameOfDotSizeVariable, dotSizeAt100mDistance * (zeroDistances[currentZeroDistance] / 100) );

            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];

            if (reflexSightInterface.IsSimpleInteract) Hook();

            if (textures.Length <= 1) 
            { 
                zeroOnlyMode = true;
                currentMenu = 1;
            }

            scopeColliders = new List<Collider>(attachment.m_colliders);

            if (isStandalone)
            {
                muzzlePos = fireArm.MuzzlePos;
                Vector3 muzzleOffset = muzzlePos.InverseTransformPoint(reticle.transform.position);

                reticle.material.SetFloat(nameOfXOffset, -muzzleOffset.x);
                reticle.material.SetFloat(nameOfYOffset, -muzzleOffset.y);
            }

            StartScreen();

            leftEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * -0.032f;
            rightEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * +0.032f;
        }
#if !DEBUG
        public void OnDestroy()
        {
            Unhook();
        }

        public void Update()
        {
            if (!reflexSightInterface.IsSimpleInteract)
            {
                hand = reflexSightInterface.m_hand;
                if (hand != null)
                {
                    if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f && currentMenu == 0) UsePreviousTexture();
                    else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f && currentMenu == 0) UseNextTexture();
                    if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f && currentMenu == 1) UsePreviousZeroDistance();
                    else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f && currentMenu == 1) UseNextZeroDistance();
                    else if ((hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f) && !zeroOnlyMode) ShowNextMenu();
                }
            }
            if (!isStandalone && attachment.curMount != null && !attached)
            {
                attached = true;
                fireArm = attachment.curMount.GetRootMount().MyObject as FVRFireArm;
                if (fireArm != null)
                {
                    muzzlePos = fireArm.CurrentMuzzle;

                    Vector3 muzzleOffset = muzzlePos.InverseTransformPoint(reticle.transform.position);
                    /*
                    Debug.Log(muzzleOffset.x);
                    Debug.Log(muzzleOffset.y);
                    Debug.Log(muzzleOffset.z);
                    */

                    reticle.material.SetFloat(nameOfXOffset, -muzzleOffset.x);
                    reticle.material.SetFloat(nameOfYOffset, -muzzleOffset.y);
                }
            }
            else if (!isStandalone && attachment.curMount == null && attached)
            {
                attached = false;
                reticle.material.SetFloat(nameOfXOffset, 0f);
                reticle.material.SetFloat(nameOfYOffset, 0f);
                fireArm = null;
                muzzlePos = null;
            }

            leftEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * -0.032f;
            rightEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * +0.032f;

            if (!disableOcclusionCulling && (isStandalone || attached)) CheckReticleVisibility();
        }
#endif
        public void UseNextTexture()
        {
            currentTexture = (currentTexture + 1) % textures.Length;

            reticle.material.SetTexture(nameOfTexture, textures[currentTexture]);
            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];
            UpdateScreen();
        }

        public void UsePreviousTexture()
        {
            currentTexture = (currentTexture + textures.Length - 1) % textures.Length;

            reticle.material.SetTexture(nameOfTexture, textures[currentTexture]);
            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];
            UpdateScreen();
        }

        private void ShowNextMenu() 
        {
            //currentMenu = (currentMenu + 1) % 2;
            if (reticleTextScreen == null && zeroTextScreen == null) return;
            currentMenu++;

            if (currentMenu > 2) currentMenu = 0;

            switch (currentMenu)
            {
                case 0:
                    if (reticleTextScreen == null)
                    {
                        ShowNextMenu();
                        return;
                    }
                    break;
                case 1:
                    if (zeroTextScreen == null)
                    {
                        ShowNextMenu();
                        return;
                    }
                    break;
                default:
                    currentMenu = 0;
                    break;
            }
            UpdateScreen();
        }

        private void UpdateScreen()
        {
            if (reticleTextScreen != null && currentMenu == 0)
            {
                if (textFrame != null) textFrame.localPosition = reticleTextScreen.transform.localPosition;
                reticleTextScreen.text = reticleTestPrefix + reticleText[currentTexture];
            }
            else if (reticleTextScreen == null)
            {
                currentMenu = 1;
            }

            if (zeroTextScreen != null && currentMenu == 1)
            {
                if (textFrame != null) textFrame.localPosition = zeroTextScreen.transform.localPosition;
                zeroTextScreen.text = zeroTextPrefix + zeroDistances[currentZeroDistance] + "m";
            }
            
        }

        private void StartScreen()
        {
            if (reticleTextScreen != null) reticleTextScreen.text = reticleTestPrefix + reticleText[currentTexture];
            if (zeroTextScreen != null) zeroTextScreen.text = zeroTextPrefix + zeroDistances[currentZeroDistance] + "m";
        }
        public void UseNextZeroDistance()
        {
            if (currentZeroDistance < zeroDistances.Length - 1) currentZeroDistance++;
            reticle.material.SetFloat(nameOfDistanceVariable, zeroDistances[currentZeroDistance]);
            //lens.material.SetFloat(nameOfDotSizeVariable, dotSizeAt100mDistance * (zeroDistances[currentZeroDistance] / 100));
            UpdateScreen();
        }

        public void UsePreviousZeroDistance()
        {
            if (currentZeroDistance > 0) currentZeroDistance--;
            reticle.material.SetFloat(nameOfDistanceVariable, zeroDistances[currentZeroDistance]);
            //lens.material.SetFloat(nameOfDotSizeVariable, dotSizeAt100mDistance * (zeroDistances[currentZeroDistance] / 100));
            UpdateScreen();
        }

        private void CheckReticleVisibility()
        {
            bool scopeHit = false;
            /*
            RaycastHit raycastHit;
            if (Physics.Linecast(GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * 0.032f, muzzlePos.position + this.transform.forward * zeroDistances[currentZeroDistance] , out raycastHit, LayerMask.NameToLayer("Environment")))
            {
                if (scopeColliders.Contains(raycastHit.collider))
                {
                    reticle.gameObject.SetActive(true);
                    scopeHit = true;
                }
            }
            if (Physics.Linecast(GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * -0.032f, muzzlePos.position + this.transform.forward * zeroDistances[currentZeroDistance], out raycastHit, LayerMask.NameToLayer("Environment")))
            {
                if (scopeColliders.Contains(raycastHit.collider))
                {
                    reticle.gameObject.SetActive(true);
                    scopeHit = true;
                }
            }
            if (!scopeHit) reticle.gameObject.SetActive(false);

            */
            if (lensCollider == null && scopeColliders.Count > 0)
            {
                RaycastHit[] raycastHits;
                float distance = Vector3.Distance(this.gameObject.transform.position, GM.CurrentPlayerBody.Head.position) + 0.2f;
                Vector3 direction = muzzlePos.position + this.transform.forward * zeroDistances[currentZeroDistance] - rightEye;
                bool angleGood = false;
                angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
                //float distance = 1f;

                //Right Eye check
                if (angleGood)
                {
                    raycastHits = Physics.RaycastAll(rightEye, direction, distance, LayerMask.NameToLayer("Environment"), QueryTriggerInteraction.Ignore);
                    if (raycastHits.Length != 0)
                        foreach (var hit in raycastHits)
                        {
                            if (scopeColliders.Contains(hit.collider))
                            {
                                reticle.gameObject.SetActive(true);
                                scopeHit = true;
                            }
                        }
                }
                //Left Eye check
                if (!scopeHit)
                {
                    angleGood = false;
                    direction = muzzlePos.position + this.transform.forward * zeroDistances[currentZeroDistance] - leftEye;
                    angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
                    if (angleGood)
                    {
                        raycastHits = Physics.RaycastAll(leftEye, direction, distance, LayerMask.NameToLayer("Environment"), QueryTriggerInteraction.Ignore);
                        if (raycastHits.Length != 0)
                            foreach (var hit in raycastHits)
                            {
                                if (scopeColliders.Contains(hit.collider))
                                {
                                    reticle.gameObject.SetActive(true);
                                    scopeHit = true;
                                }
                            }
                    }
                }

                if (!scopeHit) reticle.gameObject.SetActive(false);
            }
            else if (lensCollider != null)
            {
                float distance = Vector3.Distance(this.gameObject.transform.position, GM.CurrentPlayerBody.Head.position) + 0.2f;
                Vector3 direction = muzzlePos.position + this.transform.forward * zeroDistances[currentZeroDistance] - rightEye;
                bool angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
                if (angleGood)
                {
                    Ray ray = new Ray(rightEye,direction);
                    RaycastHit hit;
                    if (lensCollider.Raycast(ray, out hit, distance))
                    {
                        reticle.gameObject.SetActive(true);
                        scopeHit = true;
                    }
                }

                if (!scopeHit)
                {
                    direction = muzzlePos.position + this.transform.forward * zeroDistances[currentZeroDistance] - leftEye;
                    angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
                    if (angleGood)
                    {
                        Ray ray = new Ray(leftEye, direction);
                        RaycastHit hit;
                        if (lensCollider.Raycast(ray, out hit, distance))
                        {
                            reticle.gameObject.SetActive(true);
                            scopeHit = true;
                        }
                    }
                }

                if (!scopeHit) reticle.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("No usable colliders for reticle occlusion found! If you are a modmaker, please add colliders or a lens collider, or disable occlusion culling with the checkbox!\n Disabling Occlusion culling now!");
                disableOcclusionCulling = true;
            }
        }

        private void Unhook()
        {
#if !(DEBUG || MEATKIT)
            On.FistVR.FVRInteractiveObject.SimpleInteraction -= FVRInteractiveObject_SimpleInteraction;
#endif
        }

        private void Hook()
        {
#if !(DEBUG || MEATKIT)
            On.FistVR.FVRInteractiveObject.SimpleInteraction += FVRInteractiveObject_SimpleInteraction;
#endif
        }


#if !(DEBUG || MEATKIT)
        private void FVRInteractiveObject_SimpleInteraction(On.FistVR.FVRInteractiveObject.orig_SimpleInteraction orig, FVRInteractiveObject self, FVRViveHand hand)
        {
            orig(self, hand);
            if (self == reflexSightInterface) UseNextTexture();
        }
#endif
    }
}
