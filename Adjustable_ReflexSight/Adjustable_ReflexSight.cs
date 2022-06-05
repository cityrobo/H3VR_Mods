using FistVR;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Serialization;

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
        [Tooltip("Index of the Array below, not the actual value. Starts at 0.")]
        public int currentTexture = 0;

        [Header("If you want a Screen above the scope that shows stuff, use this:")]
        public Transform textFrame;
        public Text reticleTextScreen;
        public Text zeroTextScreen;
        public Text BrightnessTextScreen;

        public string reticleTestPrefix = "Reticle: ";
        public string zeroTextPrefix = "Zero Distance: ";
        public string BrightnessTextPrefix = "Brightness: ";
        [Tooltip("Index of the Array below, not the actual value. Starts at 0.")]
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

        [Header("Brightness Settings")]
        [Tooltip("Index of the Array below, not the actual value. Starts at 0.")]
        public int currentBrightnessIndex = 3;
        public float[] HDRBrightnessLevels = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 2.5f, 3f };
        public float[] BrightnessAlphaLevels = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };

        private FVRViveHand m_hand;
        private bool _attached = false;
        private int _currentMenu = 0;

        private bool _zeroOnlyMode = false;
        // Shader variable name constants
        private const string s_nameOfTextureVariable = "_RedDotTex";
        private const string s_nameOfColorVariable = "_DotColor";
        private const string s_nameOfDistanceVariable = "_RedDotDist";
        private const string s_nameOfXOffsetVariable = "_MuzzleOffsetX";
        private const string s_nameOfYOffsetVariable = "_MuzzleOffsetY";

        // Occlusion Variables
        private List<Collider> _scopeColliders;
        private Transform _muzzlePos;
        private Vector3 _leftEye;
        private Vector3 _rightEye;

        public void Start()
        {
            if (currentTexture >= textures.Length || currentTexture < 0) currentTexture = 0;
            if (currentZeroDistance >= zeroDistances.Length || currentZeroDistance < 0) currentZeroDistance = 0;
            if (currentBrightnessIndex >= HDRBrightnessLevels.Length || currentBrightnessIndex < 0) currentBrightnessIndex = 0;

            if (HDRBrightnessLevels.Length != BrightnessAlphaLevels.Length) Debug.LogError($"AdjustableReflexsight {gameObject.name}: HDRBrightnessLevels.Length != BrightnessAlphaLevels.Length!");
            if (textures.Length != 0) reticle.material.SetTexture(s_nameOfTextureVariable, textures[currentTexture]);
            reticle.material.SetFloat(s_nameOfDistanceVariable, zeroDistances[currentZeroDistance]);

            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];

            if (reflexSightInterface.IsSimpleInteract) Hook();

            if (textures.Length <= 1) 
            { 
                _zeroOnlyMode = true;
                _currentMenu = 1;
            }

            if (isStandalone)
            {
                _muzzlePos = fireArm.MuzzlePos;
                Vector3 muzzleOffset = _muzzlePos.InverseTransformPoint(reticle.transform.position);

                reticle.material.SetFloat(s_nameOfXOffsetVariable, -muzzleOffset.x);
                reticle.material.SetFloat(s_nameOfYOffsetVariable, -muzzleOffset.y);
            }

            StartScreen();
#if !DEBUG
            _leftEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * -0.032f;
            _rightEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * +0.032f;

            _scopeColliders = new List<Collider>(attachment.m_colliders);
#endif
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
                m_hand = reflexSightInterface.m_hand;
                if (m_hand != null)
                {
                    if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.left) < 45f && _currentMenu == 0) UsePreviousTexture();
                    else if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.right) < 45f && _currentMenu == 0) UseNextTexture();
                    if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.left) < 45f && _currentMenu == 1) UsePreviousZeroDistance();
                    else if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.right) < 45f && _currentMenu == 1) UseNextZeroDistance();
                    if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.left) < 45f && _currentMenu == 2) UsePreviousBrightness();
                    else if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.right) < 45f && _currentMenu == 2) UseNextBrightness();
                    else if ((m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.up) < 45f) && !_zeroOnlyMode) ShowNextMenu();
                }
            }
            if (!isStandalone && attachment.curMount != null && !_attached)
            {
                _attached = true;
                fireArm = attachment.curMount.GetRootMount().MyObject as FVRFireArm;
                if (fireArm != null)
                {
                    _muzzlePos = fireArm.CurrentMuzzle;

                    Vector3 muzzleOffset = _muzzlePos.InverseTransformPoint(reticle.transform.position);

                    reticle.material.SetFloat(s_nameOfXOffsetVariable, -muzzleOffset.x);
                    reticle.material.SetFloat(s_nameOfYOffsetVariable, -muzzleOffset.y);
                }
            }
            else if (!isStandalone && attachment.curMount == null && _attached)
            {
                _attached = false;
                reticle.material.SetFloat(s_nameOfXOffsetVariable, 0f);
                reticle.material.SetFloat(s_nameOfYOffsetVariable, 0f);
                fireArm = null;
                _muzzlePos = null;
            }

            _leftEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * -0.032f;
            _rightEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * +0.032f;

            if (!disableOcclusionCulling && (isStandalone || _attached)) CheckReticleVisibility();
        }
#endif
        public void UseNextTexture()
        {
            currentTexture = (currentTexture + 1) % textures.Length;

            reticle.material.SetTexture(s_nameOfTextureVariable, textures[currentTexture]);
            if (reticleColors != null && reticleColors.Length == textures.Length) reticle.material.SetColor(s_nameOfColorVariable, reticleColors[currentTexture]);
            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];

            UpdateBrightness();
            UpdateScreen();
        }

        public void UsePreviousTexture()
        {
            currentTexture = (currentTexture + textures.Length - 1) % textures.Length;

            reticle.material.SetTexture(s_nameOfTextureVariable, textures[currentTexture]);
            if (reticleColors != null && reticleColors.Length == textures.Length) reticle.material.SetColor(s_nameOfColorVariable, reticleColors[currentTexture]);
            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];

            UpdateBrightness();
            UpdateScreen();
        }

        private void ShowNextMenu() 
        {
            if (reticleTextScreen == null && zeroTextScreen == null) return;
            _currentMenu++;

            if (_currentMenu > 3) _currentMenu = 0;

            switch (_currentMenu)
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
                case 2:
                    if (BrightnessTextScreen == null)
                    {
                        ShowNextMenu();
                        return;
                    }
                    break;
                default:
                    _currentMenu = 0;
                    break;
            }
            UpdateScreen();
        }

        private void UpdateScreen()
        {
            if (reticleTextScreen != null && _currentMenu == 0)
            {
                if (textFrame != null) textFrame.localPosition = reticleTextScreen.transform.localPosition;
                reticleTextScreen.text = reticleTestPrefix + reticleText[currentTexture];
            }
            else if (reticleTextScreen == null)
            {
                _currentMenu = 1;
            }

            if (zeroTextScreen != null && _currentMenu == 1)
            {
                if (textFrame != null) textFrame.localPosition = zeroTextScreen.transform.localPosition;
                zeroTextScreen.text = zeroTextPrefix + zeroDistances[currentZeroDistance] + "m";
            }
            else if (zeroTextScreen == null)
            {
                _currentMenu = 2;
            }
            
            if (BrightnessTextScreen != null && _currentMenu == 2)
            {
                if (textFrame != null) textFrame.localPosition = BrightnessTextScreen.transform.localPosition;
                BrightnessTextScreen.text = BrightnessTextPrefix + HDRBrightnessLevels[currentBrightnessIndex];
            }
            else if (BrightnessTextScreen == null)
            {
                _currentMenu = 0;
            }
        }

        private void StartScreen()
        {
            if (reticleTextScreen != null) reticleTextScreen.text = reticleTestPrefix + reticleText[currentTexture];
            if (zeroTextScreen != null) zeroTextScreen.text = zeroTextPrefix + zeroDistances[currentZeroDistance] + "m";
            if (BrightnessTextScreen != null) BrightnessTextScreen.text = BrightnessTextPrefix + HDRBrightnessLevels[currentBrightnessIndex];
        }
        public void UseNextZeroDistance()
        {
            if (currentZeroDistance < zeroDistances.Length - 1) currentZeroDistance++;
            reticle.material.SetFloat(s_nameOfDistanceVariable, zeroDistances[currentZeroDistance]);

            UpdateScreen();
        }
        public void UsePreviousZeroDistance()
        {
            if (currentZeroDistance > 0) currentZeroDistance--;
            reticle.material.SetFloat(s_nameOfDistanceVariable, zeroDistances[currentZeroDistance]);

            UpdateScreen();
        }
        public void UseNextBrightness()
        {
            if (currentBrightnessIndex < HDRBrightnessLevels.Length - 1) currentBrightnessIndex++;

            UpdateBrightness();
            UpdateScreen();
        }
        public void UsePreviousBrightness()
        {
            if (currentBrightnessIndex > 0) currentBrightnessIndex--;

            UpdateBrightness();
            UpdateScreen();
        }
        public void UpdateBrightness()
        {
            float factor = Mathf.Pow(2, HDRBrightnessLevels[currentBrightnessIndex] - 1f);
            Color currentReticleColor = reticleColors[currentTexture];
            Color color = new Color(currentReticleColor.r * factor, currentReticleColor.g * factor, currentReticleColor.b * factor, currentReticleColor.a);
            color.a *= BrightnessAlphaLevels[currentBrightnessIndex];

            reticle.material.SetColor(s_nameOfColorVariable, color);
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
            if (lensCollider == null && _scopeColliders.Count > 0)
            {
                RaycastHit[] raycastHits;
                float distance = Vector3.Distance(this.gameObject.transform.position, GM.CurrentPlayerBody.Head.position) + 0.2f;
                Vector3 direction = _muzzlePos.position + this.transform.forward * zeroDistances[currentZeroDistance] - _rightEye;
                bool angleGood = false;
                angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
                //float distance = 1f;

                //Right Eye check
                if (angleGood)
                {
                    raycastHits = Physics.RaycastAll(_rightEye, direction, distance, LayerMask.NameToLayer("Environment"), QueryTriggerInteraction.Ignore);
                    if (raycastHits.Length != 0)
                        foreach (var hit in raycastHits)
                        {
                            if (_scopeColliders.Contains(hit.collider))
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
                    direction = _muzzlePos.position + this.transform.forward * zeroDistances[currentZeroDistance] - _leftEye;
                    angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
                    if (angleGood)
                    {
                        raycastHits = Physics.RaycastAll(_leftEye, direction, distance, LayerMask.NameToLayer("Environment"), QueryTriggerInteraction.Ignore);
                        if (raycastHits.Length != 0)
                            foreach (var hit in raycastHits)
                            {
                                if (_scopeColliders.Contains(hit.collider))
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
                Vector3 direction = _muzzlePos.position + this.transform.forward * zeroDistances[currentZeroDistance] - _rightEye;
                bool angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
                if (angleGood)
                {
                    Ray ray = new Ray(_rightEye,direction);
                    RaycastHit hit;
                    if (lensCollider.Raycast(ray, out hit, distance))
                    {
                        reticle.gameObject.SetActive(true);
                        scopeHit = true;
                    }
                }

                if (!scopeHit)
                {
                    direction = _muzzlePos.position + this.transform.forward * zeroDistances[currentZeroDistance] - _leftEye;
                    angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
                    if (angleGood)
                    {
                        Ray ray = new Ray(_leftEye, direction);
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