using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FistVR;
namespace Cityrobo
{
    public class ScopeShaderLinearZoom : MonoBehaviour
    {
        [Header("Linear Zoom Scope Config")]
        [Tooltip("This can be an Attachment interface or an InteractiveObject for integrated scopes.")]
        public FVRInteractiveObject ScopeInterface;
        public MeshRenderer ScopeLens;
        public Camera ScopeCamera;
        
        [Header("Zoom Settings")]
        public float MinZoom = 1f;
        public float MaxZoom = 6f;

        [Tooltip("Between 0f and 1f. 0f is MinZoom while 1f is MaxZoom.")]
        public float ZoomLerp = 0f;
        //public List<float> ZoomFactor;
        [Tooltip("If true, Reticle will scale with increasing zoom level.")]
        public bool ZoomIncreasesReticleMagnification = false;

        [Header("Text Screen Settings")]
        public GameObject CanvasRoot;
        public Text ZoomText;
        public Text ZeroText;
        public Text ElevationText;
        public Text WindageText;
        [Tooltip("The existence of this text enables the reticle change functionality.")]
        public Text ReticleText;
        public GameObject TextFrame;

        public string ZoomPrefix = "Zoom: ";
        public string ZeroPrefix = "Zero Distance: ";
        public string ElevationPrefix = "Elevation: ";
        public string WindagePrefix = "Windage: ";

        [Header("Zeroing System Settings")]
        public int ZeroDistanceIndex = 0;
        public List<float> ZeroDistances = new List<float>()
        {
          100f,
          150f,
          200f,
          250f,
          300f,
          350f,
          400f,
          450f,
          500f,
          600f,
          700f,
          800f,
          900f,
          1000f
        };
        public float ElevationIncreasePerClick = 0.25f;
        public float WindageIncreasePerClick = 0.25f;

        [Header("Reticle Change System Settings")]

        public string ReticlePrefix = "Reticle: ";
        [Tooltip("All reticle textures. Default reticle is first entry.")]
        public List<Texture2D> Reticles;
        [Tooltip("Colors of all reticles. Default reticle name is first entry.")]
        [ColorUsage(true, true, float.MaxValue, float.MaxValue, 0f, 0f)]
        public List<Color> ReticleColors;
        [Tooltip("Names of all reticles. Default reticle name is first entry.")]
        public string[] ReticleNames;

        public int CurrentReticle = 0;

        [Header("Rotating Scope Bit Monitoring Settings")]
        [Tooltip("Will Monitor this part and adjust the Zoom Lerp accordingly.")]
        public Transform RotatingBit;
        public enum Axis
        {
            X = 0,
            Y = 1,
            Z = 2
        }

        [SearchableEnum]
        public Axis axis;

        public float MinRotation;
        public float MaxRotation;

        [Header("Integrated Scope Settings")]
        public bool IsIntegrated = false;
        public FVRFireArm FireArm = null;

        [Header("Optimization Setting. Set to false when done testing for vanilla scope like behavior of showing a black picture when not attached to gun.")]
        public bool ActiveWithoutMount = true;

        private bool _hasZoomText;
        private RenderTexture _renderTexture;

        private FVRFireArmAttachment _attachment;

        private float _elevationStep;
        private float _windageStep;

        private int _currentMenu;
        private bool _initialZero = false;

        private float _baseReticleSize;

        private float _zoomFactor;
        private float _lastZoomLerp;

        public void Start()
        {
            if (CanvasRoot != null) _hasZoomText = true;
            else _hasZoomText = false;


            _zoomFactor = Mathf.Lerp(MinZoom, MaxZoom, ZoomLerp);

            _renderTexture = ScopeCamera.targetTexture;
            _renderTexture = RenderTexture.Instantiate(_renderTexture);
            ScopeCamera.targetTexture = _renderTexture;
            ScopeLens.material.mainTexture = _renderTexture;

            if (ZoomIncreasesReticleMagnification)
            {
                _baseReticleSize = ScopeLens.material.GetFloat("_ReticleScale");
            }

            SetZoom();

            FVRFireArmAttachmentInterface attachmentInterface = ScopeInterface as FVRFireArmAttachmentInterface;

            if (!IsIntegrated && attachmentInterface != null)
            {
                _attachment = attachmentInterface.Attachment;
            }
            else if (!IsIntegrated)
            {
                _attachment = this.gameObject.GetComponent<FVRFireArmAttachment>();
            }

            if (!IsIntegrated && _attachment == null) Debug.LogWarning("Attachment not found. Scope zeroing disabled!");

            UpdateMenu();

            ScopeEnabled(ActiveWithoutMount);

            if (IsIntegrated) Zero();

            if (ZoomText == null) 
            { 
                _currentMenu++;
                if (ZeroText == null)
                {
                    _currentMenu++;
                    if (ElevationText == null)
                    {
                        _currentMenu++;
                        if (WindageText == null)
                        {
                            _currentMenu = 0;
                            _hasZoomText = false;
                        }
                    }
                }
            }

            if ((ReticleText != null))
            {
                if (CurrentReticle >= Reticles.Count) CurrentReticle = Reticles.Count - 1;
                ChangeReticle();
            }

            _lastZoomLerp = ZoomLerp;
        }
        public void OnDestroy()
        {
            Destroy(_renderTexture);
        }
#if !DEBUG
        public void Update()
        {
            FVRViveHand hand = ScopeInterface.m_hand;
            if (_hasZoomText && hand != null)
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f) NextMenu();
                else if (_currentMenu == 0 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) PreviousZero();
                else if (_currentMenu == 0 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) NextZero();
                else if (_currentMenu == 1 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) DecreaseElevationAdjustment();
                else if (_currentMenu == 1 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) IncreaseElevationAdjustment();
                else if (_currentMenu == 2 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) DecreaseWindageAdjustment();
                else if (_currentMenu == 2 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) IncreaseWindageAdjustment();
                else if (_currentMenu == 3 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) PreviousReticle();
                else if (_currentMenu == 3 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) NextReticle();

                CanvasRoot.gameObject.SetActive(true);
            }
            else if (_hasZoomText) CanvasRoot.gameObject.SetActive(false);

            if (!ActiveWithoutMount)
            {
                if (_attachment != null && _attachment.curMount != null) ScopeEnabled(true);
                else if (_attachment != null && _attachment.curMount == null) ScopeEnabled(false);
                else ScopeEnabled(true);
            }

            if (!_initialZero && _attachment != null && _attachment.curMount != null)
            {
                Zero();
                _initialZero = true;
            }
            else if (_initialZero && _attachment != null && _attachment.curMount == null)
            {
                Zero();
                _initialZero = false;
            }
            else if (!_initialZero && IsIntegrated)
            {
                Zero();
                _initialZero = true;
            }

            if (_lastZoomLerp != ZoomLerp)
            {
                if (RotatingBit != null) SetZoomLerp();
                SetZoom();
                _lastZoomLerp = ZoomLerp;
            }
        }
#endif
        public void SetZoomLerp()
        {
            Vector3 currentRotatingBitEulers = RotatingBit.localEulerAngles;
            float currentRotatingBitRot = currentRotatingBitEulers[(int)axis];

            ZoomLerp = Mathf.InverseLerp(MinRotation, MaxRotation, currentRotatingBitRot);
        }

        public void SetZoom()
        {
            _zoomFactor = Mathf.Lerp(MinZoom, MaxZoom, ZoomLerp);

            float cameraFOV = 53.6f * Mathf.Pow(_zoomFactor, -0.9364f) - 0.3666f;

            ScopeCamera.fieldOfView = cameraFOV;

            if (ZoomIncreasesReticleMagnification)
            {
                ScopeLens.material.SetFloat("_ReticleScale", _baseReticleSize * _zoomFactor/MinZoom);
            }
        }

        public void NextZero()
        {
            if (ZeroDistanceIndex >= ZeroDistances.Count - 1) return;
            ZeroDistanceIndex++;
            Zero();
            UpdateMenu();
        }

        public void PreviousZero()
        {
            if (ZeroDistanceIndex <= 0) return;
            ZeroDistanceIndex--;
            Zero();
            UpdateMenu();
        }

        public void IncreaseElevationAdjustment()
        {
            _elevationStep += ElevationIncreasePerClick;
            Zero();
            UpdateMenu();
        }
        public void DecreaseElevationAdjustment()
        {
            _elevationStep -= ElevationIncreasePerClick;
            Zero();
            UpdateMenu();
        }

        public void IncreaseWindageAdjustment()
        {
            _windageStep += WindageIncreasePerClick;
            Zero();
            UpdateMenu();
        }
        public void DecreaseWindageAdjustment()
        {
            _windageStep -= WindageIncreasePerClick;
            Zero();
            UpdateMenu();
        }

        public void NextReticle()
        {
            CurrentReticle++;
            if (CurrentReticle >= Reticles.Count) CurrentReticle = 0;
            ChangeReticle();
            UpdateMenu();
        }

        public void PreviousReticle()
        {
            CurrentReticle--;
            if (CurrentReticle <= 0) CurrentReticle = Reticles.Count - 1;
            ChangeReticle();
            UpdateMenu();
        }
        public void NextMenu()
        {
            if (ZoomText == null && ZeroText == null && ElevationText == null && WindageText == null)
                return;
            _currentMenu++;

            if (_currentMenu >= 4) _currentMenu = 0;

            switch (_currentMenu)
            {
                case 0:
                    if (ZeroText == null)
                    {
                        NextMenu();
                        return;
                    }
                    break;
                case 1:
                    if (ElevationText == null)
                    {
                        NextMenu();
                        return;
                    }
                    break;
                case 2:
                    if (WindageText == null)
                    {
                        NextMenu();
                        return;
                    }
                    break;
                case 3:
                    if (ReticleText == null)
                    {
                        NextMenu();
                        return;
                    }
                    break;
                default:
                    break;
            }

            UpdateMenu();
        }

        public void UpdateMenu()
        {
            if (TextFrame != null)
                switch (_currentMenu)
                {
                    case 0:
                        if (ZoomText == null) break;
                        TextFrame.transform.position = ZoomText.transform.position;
                        break;
                    case 1:
                        if (ZeroText == null) break;
                        TextFrame.transform.position = ZeroText.transform.position;
                        break;
                    case 2:
                        if (ElevationText == null) break;
                        TextFrame.transform.position = ElevationText.transform.position;
                        break;
                    case 3:
                        if (WindageText == null) break;
                        TextFrame.transform.position = WindageText.transform.position;
                        break;
                    case 4:
                        if (ReticleText == null) break;
                        TextFrame.transform.position = ReticleText.transform.position;
                        break;
                    default:
                        break;
                }

            if (ZoomText != null) ZoomText.text = ZoomPrefix + _zoomFactor + "x";
            if (ZeroText != null) ZeroText.text = ZeroPrefix + ZeroDistances[ZeroDistanceIndex] + "m";
            if (ElevationText != null) ElevationText.text = ElevationPrefix + _elevationStep + " MOA";
            if (WindageText != null) WindageText.text = WindagePrefix + _windageStep + " MOA";
            if (ReticleText != null) ReticleText.text = ReticlePrefix + ReticleNames[CurrentReticle];
        }
        public void Zero()
        {
#if!Debug
            if (IsIntegrated || (this._attachment != null && this._attachment.curMount != null && this._attachment.curMount.Parent != null && this._attachment.curMount.Parent is FVRFireArm))
            {
                if (!IsIntegrated) FireArm = this._attachment.curMount.Parent as FVRFireArm;

                if (IsIntegrated && FireArm == null) Debug.LogError("ScopeShaderZoom: FireArm not set on integrated Scope! Can't zero sight!");

                FireArmRoundType roundType = FireArm.RoundType;
                float zeroDistance = this.ZeroDistances[this.ZeroDistanceIndex];
                float num = 0.0f;
                if (AM.SRoundDisplayDataDic.ContainsKey(roundType))
                    num = AM.SRoundDisplayDataDic[roundType].BulletDropCurve.Evaluate(zeroDistance * (1f / 1000f));
                Vector3 p = FireArm.MuzzlePos.position + FireArm.GetMuzzle().forward * zeroDistance + FireArm.GetMuzzle().up * num;
                Vector3 vector3_1 = Vector3.ProjectOnPlane(p - this.transform.forward, this.transform.right);
                Vector3 vector3_2 = Quaternion.AngleAxis(this._elevationStep /60f, this.transform.right) * vector3_1;
                Vector3 forward = Quaternion.AngleAxis(this._windageStep / 60f, this.transform.up) * vector3_2;


                Vector3 projected_p = Vector3.ProjectOnPlane(p, this.transform.right) + Vector3.Dot(this.transform.position, this.transform.right) * this.transform.right;

                this.ScopeCamera.transform.LookAt(projected_p, this.transform.up);
                this.ScopeCamera.transform.localEulerAngles += new Vector3(-this._elevationStep / 60f, this._windageStep / 60f, 0);
            }
            else this.ScopeCamera.transform.localRotation = Quaternion.identity;
#endif
        }

        public void ScopeEnabled(bool state)
        {
            if (state)
            {
                ScopeCamera.gameObject.SetActive(true);
            }
            else
            {
                ScopeCamera.gameObject.SetActive(false);
                RenderTexture.active = _renderTexture;
                GL.Clear(false, true, Color.black);
                RenderTexture.active = (RenderTexture) null;
            }
        }

        private void ChangeReticle()
        {
            ScopeLens.material.SetColor("_ReticleColor", ReticleColors[CurrentReticle]);
            ScopeLens.material.SetTexture("_ReticleTex", Reticles[CurrentReticle]);
        }
    }
}

