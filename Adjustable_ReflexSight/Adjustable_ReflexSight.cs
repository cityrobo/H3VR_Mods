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
        public float[] BrightnessAlphaLevels = new float[] { 0.25f, 0.5f, 0.75f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        public string[] BrightnessTexts = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", };

        private FVRViveHand m_hand;
        private bool _attached = false;
        private int _currentMenu = 0;

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

        private MaterialPropertyBlock _materialPropertyBlock;

        public void Start()
        {
            _materialPropertyBlock = new MaterialPropertyBlock();

            if (currentTexture >= textures.Length || currentTexture < 0) currentTexture = 0;
            if (currentZeroDistance >= zeroDistances.Length || currentZeroDistance < 0) currentZeroDistance = 0;
            if (currentBrightnessIndex >= HDRBrightnessLevels.Length || currentBrightnessIndex < 0) currentBrightnessIndex = 0;

            if (HDRBrightnessLevels.Length != BrightnessAlphaLevels.Length) Debug.LogError($"AdjustableReflexsight {gameObject.name}: HDRBrightnessLevels.Length != BrightnessAlphaLevels.Length!");
            if (textures.Length != 0) _materialPropertyBlock.SetTexture(s_nameOfTextureVariable, textures[currentTexture]);

            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];

            if (reflexSightInterface.IsSimpleInteract) Hook();

            if (textures.Length <= 1) 
            { 
                _currentMenu = 1;
            }

            StartScreen();
#if !DEBUG
            _leftEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * -0.032f;
            _rightEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * +0.032f;

            if (!isStandalone) _scopeColliders = new List<Collider>(attachment.m_colliders);
#endif
            Zero();

            reticle.SetPropertyBlock(_materialPropertyBlock);
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
                    if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.left) < 45f)
                    {
                        switch (_currentMenu)
                        {
                            case 0:
                                UsePreviousTexture();
                                break;
                            case 1:
                                UsePreviousZeroDistance();
                                break;
                            case 2:
                                UsePreviousBrightness();
                                break;
                        }
                    }
                    else if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.right) < 45f)
                    {
                        switch (_currentMenu)
                        {
                            case 0:
                                UseNextTexture();
                                break;
                            case 1:
                                UseNextZeroDistance();
                                break;
                            case 2:
                                UseNextBrightness();
                                break;
                        }
                    }
                    else if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.up) < 45f) ShowNextMenu();
                }
            }
            if (!isStandalone && attachment.curMount != null && !_attached)
            {
                _attached = true;
                fireArm = attachment.curMount.GetRootMount().MyObject as FVRFireArm;
                Zero();
            }
            else if (!isStandalone && attachment.curMount == null && _attached)
            {
                _attached = false;
                Zero(true);
            }

            _leftEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * -0.032f;
            _rightEye = GM.CurrentPlayerBody.Head.position + GM.CurrentPlayerBody.Head.right * +0.032f;

            if (!disableOcclusionCulling && (isStandalone || _attached)) CheckReticleVisibility();

            reticle.SetPropertyBlock(_materialPropertyBlock);
        }
#endif

        private void ShowNextMenu() 
        {
            if (reticleTextScreen == null && zeroTextScreen == null && BrightnessTextScreen == null) return;
            _currentMenu++;

            if (_currentMenu > 2) _currentMenu = 0;

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
            }
            UpdateScreen();
        }

        private void StartScreen()
        {
            if (reticleTextScreen != null) reticleTextScreen.text = reticleTestPrefix + reticleText[currentTexture];
            if (zeroTextScreen != null) zeroTextScreen.text = zeroTextPrefix + zeroDistances[currentZeroDistance] + "m";
            if (BrightnessTextScreen != null) BrightnessTextScreen.text = BrightnessTextPrefix + BrightnessTexts[currentBrightnessIndex];
        }

        private void UpdateScreen()
        {
            if (reticleTextScreen != null && _currentMenu == 0)
            {
                if (textFrame != null) textFrame.localPosition = reticleTextScreen.transform.localPosition;
                reticleTextScreen.text = reticleTestPrefix + reticleText[currentTexture];
            }
            else if (zeroTextScreen != null && _currentMenu == 1)
            {
                if (textFrame != null) textFrame.localPosition = zeroTextScreen.transform.localPosition;
                zeroTextScreen.text = zeroTextPrefix + zeroDistances[currentZeroDistance] + "m";
            }
            else if (BrightnessTextScreen != null && _currentMenu == 2)
            {
                if (textFrame != null) textFrame.localPosition = BrightnessTextScreen.transform.localPosition;
                BrightnessTextScreen.text = BrightnessTextPrefix + BrightnessTexts[currentBrightnessIndex];
            }
        }
        public void UseNextTexture()
        {
            currentTexture = (currentTexture + 1) % textures.Length;

            _materialPropertyBlock.SetTexture(s_nameOfTextureVariable, textures[currentTexture]);
            if (reticleColors != null && reticleColors.Length == textures.Length) _materialPropertyBlock.SetColor(s_nameOfColorVariable, reticleColors[currentTexture]);
            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];

            if(BrightnessTextScreen != null) UpdateBrightness();
            UpdateScreen();
        }

        public void UsePreviousTexture()
        {
            currentTexture = (currentTexture + textures.Length - 1) % textures.Length;

            _materialPropertyBlock.SetTexture(s_nameOfTextureVariable, textures[currentTexture]);
            if (reticleColors != null && reticleColors.Length == textures.Length) _materialPropertyBlock.SetColor(s_nameOfColorVariable, reticleColors[currentTexture]);
            if (buttonSwitch != null) buttonSwitch.localPosition = switchPositions[currentTexture];

            if (BrightnessTextScreen != null) UpdateBrightness();
            UpdateScreen();
        }

        public void UseNextZeroDistance()
        {
            if (currentZeroDistance < zeroDistances.Length - 1) currentZeroDistance++;
            Zero();
            UpdateScreen();
        }
        public void UsePreviousZeroDistance()
        {
            if (currentZeroDistance > 0) currentZeroDistance--;
            Zero();
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

            if (reticleColors == null || reticleColors.Length == 0) 
            { 
                Debug.LogError("Trying to change brightness but reference color array is empty!");
                return;
            }
            Color currentReticleColor;
            try
            {
                currentReticleColor = reticleColors[currentTexture];
            }
            catch (System.Exception)
            {
                Debug.LogError("Trying to change brightness but reference color array is empty at selected texture index!");
                return;
            }
            Color color = new Color(currentReticleColor.r * factor, currentReticleColor.g * factor, currentReticleColor.b * factor, currentReticleColor.a);
            color.a *= BrightnessAlphaLevels[currentBrightnessIndex];

            _materialPropertyBlock.SetColor(s_nameOfColorVariable, color);
        }
        private void CheckReticleVisibility()
        {
            bool scopeHit = false;
            Vector3 muzzleOffset = _muzzlePos.InverseTransformPoint(reticle.transform.position);
            if (lensCollider == null && _scopeColliders != null && _scopeColliders.Count > 0)
            {
                RaycastHit[] raycastHits;
                float distance = Vector3.Distance(this.transform.position, GM.CurrentPlayerBody.Head.position) + 0.2f;
                Vector3 direction = this.transform.TransformPoint(-muzzleOffset) + this.transform.forward * zeroDistances[currentZeroDistance] - _rightEye;
                bool angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
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
                    direction = this.transform.TransformPoint(-muzzleOffset) + this.transform.forward * zeroDistances[currentZeroDistance] - _leftEye;
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
                float distance = Vector3.Distance(this.transform.position, GM.CurrentPlayerBody.Head.position) + 0.2f;
                Vector3 direction = this.transform.TransformPoint(-muzzleOffset) + this.transform.forward * zeroDistances[currentZeroDistance] - _rightEye;
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
                    direction = this.transform.TransformPoint(-muzzleOffset) + this.transform.forward * zeroDistances[currentZeroDistance] - _leftEye;
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

        private void Zero(bool reset = false)
        {
            if (!reset)
            {
                if (isStandalone)
                {
                    _muzzlePos = fireArm.MuzzlePos;
                    Vector3 muzzleOffset = _muzzlePos.position - reticle.transform.position;

                    _materialPropertyBlock.SetFloat(s_nameOfXOffsetVariable, muzzleOffset.x);
                    _materialPropertyBlock.SetFloat(s_nameOfYOffsetVariable, muzzleOffset.y);

                    reticle.transform.rotation = Quaternion.LookRotation(_muzzlePos.forward);
                    _materialPropertyBlock.SetFloat(s_nameOfDistanceVariable, zeroDistances[currentZeroDistance]);
                }
                else
                {
                    if (fireArm != null)
                    {
                        _muzzlePos = fireArm.CurrentMuzzle;

                        Vector3 muzzleOffset = _muzzlePos.position - reticle.transform.position;

                        _materialPropertyBlock.SetFloat(s_nameOfXOffsetVariable, muzzleOffset.x);
                        _materialPropertyBlock.SetFloat(s_nameOfYOffsetVariable, muzzleOffset.y);

                        reticle.transform.rotation = Quaternion.LookRotation(_muzzlePos.forward);
                        _materialPropertyBlock.SetFloat(s_nameOfDistanceVariable, zeroDistances[currentZeroDistance]);
                    }
                }
            }
            else
            {
                _materialPropertyBlock.SetFloat(s_nameOfXOffsetVariable, 0f);
                _materialPropertyBlock.SetFloat(s_nameOfYOffsetVariable, 0f);

                reticle.transform.localRotation = Quaternion.identity;
                fireArm = null;
                _muzzlePos = null;
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