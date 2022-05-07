using FistVR;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace Cityrobo
{
    public class GrenadeLauncherRedDot : MonoBehaviour
    {
        [Header("Grenade Launcher Red Dot Config")]
        public FVRFireArmAttachment Attachment;
        public FVRInteractiveObject AttachmentInterface;
        public Transform TiltingOpticPart;
        public MeshRenderer ReticleMeshRenderer;

        [Header("Reticles")]
        [Tooltip("Index of the Array below, not the actual value. Starts at 0.")]
        public int CurrentSelectedTextureIndex = 0;
        public Texture2D[] ReticleTextures;

        [Tooltip("Switch that moves with the selected texture (optional).")]
        public Transform ButtonSwitch;
        public Vector3[] SwitchPositions;

        [Header("If you want a Screen above the scope that shows stuff, use this:")]
        public Transform TextFrame;
        public Text ReticleTextScreen;
        public Text ZeroTextScreen;

        public string ReticleTextPrefix = "Reticle: ";
        public string[] ReticleTextField;

        public string ZeroTextPrefix = "Zero Distance: ";
        [Tooltip("Index of the Array below, not the actual value. Starts at 0.")]
        public int CurrentZeroDistanceIndex = 0;
        [Tooltip("In meters. Miss me with that imperial shit!")]
        public float[] ZeroDistances = new float[7] { 50f, 100f, 150f, 200f, 300f, 500f, 1000f };
        //public float dotSizeAt100mDistance = 2;
        [Header("Intergrated Sight configuration")]
        public bool IsIntegrated = false;
        public FVRFireArm FireArm;

        [Header("Reticle Occlusion culling")]
        [Tooltip("Use this for extra performant reticle occlusion culling")]
        public Collider LensCollider;
        public bool DisableOcclusionCulling = false;

        private FVRViveHand m_hand;
        private int _currentMenu = 0;

        private bool _zeroOnlyMode = false;
        private string _nameOfTexture = "_RedDotTex";
        private string _nameOfDistanceVariable = "_RedDotDist";
        private string _nameOfXOffset = "_MuzzleOffsetX";
        private string _nameOfYOffset = "_MuzzleOffsetY";
        private List<Collider> _scopeColliders;
        //private string nameOfDotSizeVariable = "_RedDotSize";

        private Transform _muzzlePos;

        private bool _attached = false;

        private Transform _head;
        private Vector3 _leftEye;
        private Vector3 _rightEye;
        public void Start()
        {
            if (CurrentSelectedTextureIndex >= ReticleTextures.Length) CurrentSelectedTextureIndex = 0;
            if (CurrentZeroDistanceIndex >= ZeroDistances.Length) CurrentZeroDistanceIndex = 0;
            if (ReticleTextures.Length != 0) ReticleMeshRenderer.material.SetTexture(_nameOfTexture, ReticleTextures[CurrentSelectedTextureIndex]);
            ReticleMeshRenderer.material.SetFloat(_nameOfDistanceVariable, ZeroDistances[CurrentZeroDistanceIndex]);
            //lens.material.SetFloat(nameOfDotSizeVariable, dotSizeAt100mDistance * (zeroDistances[currentZeroDistance] / 100) );

            if (ButtonSwitch != null) ButtonSwitch.localPosition = SwitchPositions[CurrentSelectedTextureIndex];

            if (ReticleTextures.Length <= 1) 
            { 
                _zeroOnlyMode = true;
                _currentMenu = 1;
            }

            _scopeColliders = new List<Collider>(Attachment.m_colliders);
            /*
            if (IsIntegrated)
            {
                _muzzlePos = FireArm.MuzzlePos;
                Vector3 muzzleOffset = _muzzlePos.InverseTransformPoint(ReticleMeshRenderer.transform.position);

                ReticleMeshRenderer.material.SetFloat(_nameOfXOffset, -muzzleOffset.x);
                ReticleMeshRenderer.material.SetFloat(_nameOfYOffset, -muzzleOffset.y);
            }*/

            StartScreen();
            _head = GM.CurrentPlayerBody.Head;
            _leftEye = _head.position + _head.right * -0.032f;
            _rightEye = _head.position + _head.right * +0.032f;
            Zero();
        }
#if !DEBUG

        public void Update()
        {
            m_hand = AttachmentInterface.m_hand;
            if (m_hand != null)
            {
                if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.left) < 45f && _currentMenu == 0) UsePreviousTexture();
                else if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.right) < 45f && _currentMenu == 0) UseNextTexture();
                if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.left) < 45f && _currentMenu == 1) UsePreviousZeroDistance();
                else if (m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.right) < 45f && _currentMenu == 1) UseNextZeroDistance();
                else if ((m_hand.Input.TouchpadDown && Vector2.Angle(m_hand.Input.TouchpadAxes, Vector2.up) < 45f) && !_zeroOnlyMode) ShowNextMenu();
            }
            /*
            if (!IsIntegrated && Attachment.curMount != null && !_attached)
            {
                _attached = true;
                FireArm = Attachment.curMount.GetRootMount().MyObject as FVRFireArm;
                if (FireArm != null)
                {
                    _muzzlePos = FireArm.CurrentMuzzle;

                    Vector3 muzzleOffset = _muzzlePos.InverseTransformPoint(ReticleMeshRenderer.transform.position);

                    ReticleMeshRenderer.material.SetFloat(_nameOfXOffset, -muzzleOffset.x);
                    ReticleMeshRenderer.material.SetFloat(_nameOfYOffset, -muzzleOffset.y);
                }
            }
            else if (!IsIntegrated && Attachment.curMount == null && _attached)
            {
                _attached = false;
                ReticleMeshRenderer.material.SetFloat(_nameOfXOffset, 0f);
                ReticleMeshRenderer.material.SetFloat(_nameOfYOffset, 0f);
                FireArm = null;
                _muzzlePos = null;
            }*/
            _head = GM.CurrentPlayerBody.Head;
            _leftEye = _head.position + _head.right * -0.032f;
            _rightEye = _head.position + _head.right * +0.032f;

            if (!DisableOcclusionCulling && (IsIntegrated || _attached)) CheckReticleVisibility();
        }
#endif
        public void UseNextTexture()
        {
            CurrentSelectedTextureIndex = (CurrentSelectedTextureIndex + 1) % ReticleTextures.Length;

            ReticleMeshRenderer.material.SetTexture(_nameOfTexture, ReticleTextures[CurrentSelectedTextureIndex]);
            if (ButtonSwitch != null) ButtonSwitch.localPosition = SwitchPositions[CurrentSelectedTextureIndex];
            UpdateScreen();
        }

        public void UsePreviousTexture()
        {
            CurrentSelectedTextureIndex = (CurrentSelectedTextureIndex + ReticleTextures.Length - 1) % ReticleTextures.Length;

            ReticleMeshRenderer.material.SetTexture(_nameOfTexture, ReticleTextures[CurrentSelectedTextureIndex]);
            if (ButtonSwitch != null) ButtonSwitch.localPosition = SwitchPositions[CurrentSelectedTextureIndex];
            UpdateScreen();
        }

        private void ShowNextMenu() 
        {
            //currentMenu = (currentMenu + 1) % 2;
            if (ReticleTextScreen == null && ZeroTextScreen == null) return;
            _currentMenu++;

            if (_currentMenu > 2) _currentMenu = 0;

            switch (_currentMenu)
            {
                case 0:
                    if (ReticleTextScreen == null)
                    {
                        ShowNextMenu();
                        return;
                    }
                    break;
                case 1:
                    if (ZeroTextScreen == null)
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
            if (ReticleTextScreen != null && _currentMenu == 0)
            {
                if (TextFrame != null) TextFrame.localPosition = ReticleTextScreen.transform.localPosition;
                ReticleTextScreen.text = ReticleTextPrefix + ReticleTextField[CurrentSelectedTextureIndex];
            }
            else if (ReticleTextScreen == null)
            {
                _currentMenu = 1;
            }

            if (ZeroTextScreen != null && _currentMenu == 1)
            {
                if (TextFrame != null) TextFrame.localPosition = ZeroTextScreen.transform.localPosition;
                ZeroTextScreen.text = ZeroTextPrefix + ZeroDistances[CurrentZeroDistanceIndex] + "m";
            }
            
        }

        private void StartScreen()
        {
            if (ReticleTextScreen != null) ReticleTextScreen.text = ReticleTextPrefix + ReticleTextField[CurrentSelectedTextureIndex];
            if (ZeroTextScreen != null) ZeroTextScreen.text = ZeroTextPrefix + ZeroDistances[CurrentZeroDistanceIndex] + "m";
        }
        public void UseNextZeroDistance()
        {
            if (CurrentZeroDistanceIndex < ZeroDistances.Length - 1) CurrentZeroDistanceIndex++;
            ReticleMeshRenderer.material.SetFloat(_nameOfDistanceVariable, ZeroDistances[CurrentZeroDistanceIndex]);
            UpdateScreen();
            Zero();
        }

        public void UsePreviousZeroDistance()
        {
            if (CurrentZeroDistanceIndex > 0) CurrentZeroDistanceIndex--;
            ReticleMeshRenderer.material.SetFloat(_nameOfDistanceVariable, ZeroDistances[CurrentZeroDistanceIndex]);
            UpdateScreen();
            Zero();
        }

        private void CheckReticleVisibility()
        {
            bool scopeHit = false;

            if (LensCollider == null && _scopeColliders.Count > 0)
            {
                RaycastHit[] raycastHits;
                float distance = Vector3.Distance(this.gameObject.transform.position, GM.CurrentPlayerBody.Head.position) + 0.2f;
                Vector3 direction = _muzzlePos.position + this.transform.forward * ZeroDistances[CurrentZeroDistanceIndex] - _rightEye;
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
                                ReticleMeshRenderer.gameObject.SetActive(true);
                                scopeHit = true;
                            }
                        }
                }
                //Left Eye check
                if (!scopeHit)
                {
                    angleGood = false;
                    direction = _muzzlePos.position + this.transform.forward * ZeroDistances[CurrentZeroDistanceIndex] - _leftEye;
                    angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
                    if (angleGood)
                    {
                        raycastHits = Physics.RaycastAll(_leftEye, direction, distance, LayerMask.NameToLayer("Environment"), QueryTriggerInteraction.Ignore);
                        if (raycastHits.Length != 0)
                            foreach (var hit in raycastHits)
                            {
                                if (_scopeColliders.Contains(hit.collider))
                                {
                                    ReticleMeshRenderer.gameObject.SetActive(true);
                                    scopeHit = true;
                                }
                            }
                    }
                }

                if (!scopeHit) ReticleMeshRenderer.gameObject.SetActive(false);
            }
            else if (LensCollider != null)
            {
                float distance = Vector3.Distance(this.gameObject.transform.position, GM.CurrentPlayerBody.Head.position) + 0.2f;
                Vector3 direction = _muzzlePos.position + this.transform.forward * ZeroDistances[CurrentZeroDistanceIndex] - _rightEye;
                bool angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
                if (angleGood)
                {
                    Ray ray = new Ray(_rightEye,direction);
                    RaycastHit hit;
                    if (LensCollider.Raycast(ray, out hit, distance))
                    {
                        ReticleMeshRenderer.gameObject.SetActive(true);
                        scopeHit = true;
                    }
                }

                if (!scopeHit)
                {
                    direction = _muzzlePos.position + this.transform.forward * ZeroDistances[CurrentZeroDistanceIndex] - _leftEye;
                    angleGood = Vector3.Angle(GM.CurrentPlayerBody.Head.forward, this.transform.forward) < 45f;
                    if (angleGood)
                    {
                        Ray ray = new Ray(_leftEye, direction);
                        RaycastHit hit;
                        if (LensCollider.Raycast(ray, out hit, distance))
                        {
                            ReticleMeshRenderer.gameObject.SetActive(true);
                            scopeHit = true;
                        }
                    }
                }

                if (!scopeHit) ReticleMeshRenderer.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning("No usable colliders for reticle occlusion found! If you are a modmaker, please add colliders or a lens collider, or disable occlusion culling with the checkbox!\n Disabling Occlusion culling now!");
                DisableOcclusionCulling = true;
            }
        }

        public void Zero()
        {
#if !Debug
            if (IsIntegrated || (this.Attachment != null && this.Attachment.curMount != null && this.Attachment.curMount.Parent != null && this.Attachment.curMount.Parent is FVRFireArm))
            {
                if (!IsIntegrated) FireArm = this.Attachment.curMount.Parent as FVRFireArm;

                if (IsIntegrated && FireArm == null) Debug.LogError("ScopeShaderZoom: FireArm not set on integrated Scope! Can't zero sight!");

                FireArmRoundType roundType = FireArm.RoundType;
                float zeroDistance = ZeroDistances[CurrentZeroDistanceIndex];
                float num = 0.0f;
                if (AM.SRoundDisplayDataDic.ContainsKey(roundType))
                    num = AM.SRoundDisplayDataDic[roundType].BulletDropCurve.Evaluate(zeroDistance * (1f / 1000f));
                Vector3 p = FireArm.MuzzlePos.position + FireArm.GetMuzzle().forward * zeroDistance + FireArm.GetMuzzle().up * num;
                Vector3 vector3_1 = Vector3.ProjectOnPlane(p - this.transform.forward, this.transform.right);
                //Vector3 vector3_2 = Quaternion.AngleAxis(this.ElevationStep / 60f, this.transform.right) * vector3_1;
                //Vector3 forward = Quaternion.AngleAxis(this.WindageStep / 60f, this.transform.up) * vector3_2;


                Vector3 projected_p = Vector3.ProjectOnPlane(p, this.transform.right) + Vector3.Dot(this.transform.position, this.transform.right) * this.transform.right;

                TiltingOpticPart.LookAt(projected_p, this.transform.up);
                //this.camera.transform.localEulerAngles += new Vector3(-this.ElevationStep / 60f, this.WindageStep / 60f, 0);

            }
            else TiltingOpticPart.localRotation = Quaternion.identity;
#endif
        }
    }
}
