using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FistVR;
namespace Cityrobo
{
    public class ScopeShaderZoom : MonoBehaviour
    {
        public FVRInteractiveObject AttachmentInterface;
        public MeshRenderer scopeLens;
        public Camera camera;
        public int currentZoomIndex = 0;

        public List<float> ZoomFactor;
        public bool doesZoomAffectReticle = false;

        [Header("If you want a Screen above the scope that shows the current Settings, use these:")]
        public GameObject canvas;
        public Text text;
        public Text zeroText;
        public Text elevationText;
        public Text windageText;
        [Tooltip("The existence of this text enables the reticle change functionality")]
        public Text reticleText;

        public GameObject textFrame;

        public string zoomPrefix = "Zoom: ";
        public string zeroPrefix = "Zero Distance: ";
        public string elevationPrefix = "Elevation: ";
        public string windagePrefix = "Windage: ";
        public string reticlePrefix = "Reticle: ";

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

        public float elevationIncreasePerClick = 0.5f;
        public float windageIncreasePerClick = 0.5f;
        [Header("Optimization Setting. Set to false when done testing for vanilla scope like behavior of showing a black picture when not attached to gun.")]
        public bool activeWithoutMount = true;

        [Header("Rotating Scope Bit")]
        public bool hasRotatingBit = false;
        public Transform rotatingBit;
        public enum Axis
        {
            x,
            y,
            z
        }

        [SearchableEnum]
        public Axis axis;

        [Tooltip("Needs to be same length as zoom levels or it will break!")]
        public float[] rotationAngles;

        [Header("Integrated Scope Settings")]
        public bool isIntegrated = false;
        public FVRFireArm firearm = null;
        [Header("Reticle Change Settings")]
        [Tooltip("All reticle textures. Default reticle is first entry.")]
        public List<Texture2D> reticles;
        [Tooltip("Colors of all reticles. Default reticle name is first entry.")]
        public List<Color> reticleColors;
        [ColorUsage(true, true, float.MaxValue, float.MaxValue, 0f, 0f)]
        [Tooltip("Names of all reticles. Default reticle name is first entry.")]
        public string[] reticleName;
        
        public int currentReticle = 0;

        [Tooltip("This enables the very specialized reticle change system.")]
        public bool doesEachZoomFactorHaveOwnReticle = false;
        [Tooltip("Starts with default reticle, than all default reticle variants for the following zoom levels. Next entries are additional reticles and their according zoom levels, all ordered by zoom level and grouped by reticle type.")]
        public List<Texture2D> additionalReticlesPerZoomLevel;

        private List<float> _correspondingCameraFOV;

        private bool _hasZoomText;
        private RenderTexture _renderTexture;

        private FVRFireArmAttachment _attachment;

        private float _elevationStep;
        private float _windageStep;

        private int _currentMenu;
        private bool _initialZero = false;

        private float _baseReticleSize;

        public void Start()
        {
            _correspondingCameraFOV = new List<float>();
            if (canvas != null) _hasZoomText = true;
            else _hasZoomText = false;

            for (int i = 0; i < ZoomFactor.Count; i++)
            {
                //CorrespondingCameraFOV.Add(53.7f * Mathf.Pow(ZoomFactor[i], -0.9284f) - 0.5035f);
                //CorrespondingCameraFOV.Add(54.3f * Mathf.Pow(ZoomFactor[i], -0.9613f) - 0.1378f);
                float zoomValue = 53.6f * Mathf.Pow(ZoomFactor[i], -0.9364f) - 0.3666f;
                _correspondingCameraFOV.Add(zoomValue);
            }

            _renderTexture = camera.targetTexture;
            _renderTexture = Instantiate(_renderTexture);
            _renderTexture.width = OpenScripts_BepInEx.ScopeResolution.Value;
            _renderTexture.height = OpenScripts_BepInEx.ScopeResolution.Value;

            camera.targetTexture = _renderTexture;
            scopeLens.material.mainTexture = _renderTexture;

            if (doesZoomAffectReticle)
            {
                _baseReticleSize = scopeLens.material.GetFloat("_ReticleScale");
            }

            SetZoom();

            FVRFireArmAttachmentInterface attachmentInterface = AttachmentInterface as FVRFireArmAttachmentInterface;

            if (!isIntegrated && attachmentInterface != null)
            {
                _attachment = attachmentInterface.Attachment;
            }
            else if (!isIntegrated)
            {
                _attachment = this.gameObject.GetComponent<FVRFireArmAttachment>();
            }

            if (!isIntegrated && _attachment == null) Debug.LogWarning("Attachment not found. Scope zeroing disabled!");

            UpdateMenu();

            ScopeEnabled(activeWithoutMount);

            //camera.gameObject.SetActive(activeWithoutMount);
#if !DEBUG
            if (isIntegrated) Zero();
#endif
            if (text == null) 
            { 
                _currentMenu++;
                if (zeroText == null)
                {
                    _currentMenu++;
                    if (elevationText == null)
                    {
                        _currentMenu++;
                        if (windageText == null)
                        {
                            _currentMenu = 0;
                            _hasZoomText = false;
                        }
                    }
                }
            }

            if ((reticleText != null || doesEachZoomFactorHaveOwnReticle))
            {
                /*
                if (reticles.Count != reticleName.Length)
                {
                    reticles.Insert(0,scopeLens.material.GetTexture("_ReticleTex") as Texture2D);
                }
                if (reticleColors.Count != reticleName.Length)
                {
                    reticleColors.Insert(0, scopeLens.material.GetColor("_ReticleColor"));
                }

                if (doesEachZoomFactorHaveOwnReticle)
                {
                    for (int i = 0; i < reticles.Count; i++)
                    {
                        if (additionalReticlesPerZoomLevel[ZoomFactor.Count * i] != reticles[i])
                        {
                            additionalReticlesPerZoomLevel.Insert(ZoomFactor.Count * i, reticles[i]);
                        }
                    }
                }
                */
                if (currentReticle >= reticles.Count && reticles.Count != 0) currentReticle = reticles.Count - 1;
                ChangeReticle();
            }
        }
        public void OnDestroy()
        {
            Destroy(_renderTexture);
        }
#if !DEBUG
        public void Update()
        {
            FVRViveHand hand = AttachmentInterface.m_hand;
            if (_hasZoomText && hand != null)
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f) NextMenu();
                else if (_currentMenu == 0 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) PreviousZoom();
                else if (_currentMenu == 0 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) NextZoom();
                else if (_currentMenu == 1 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) PreviousZero();
                else if (_currentMenu == 1 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) NextZero();
                else if (_currentMenu == 2 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) DecreaseElevationAdjustment();
                else if (_currentMenu == 2 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) IncreaseElevationAdjustment();
                else if (_currentMenu == 3 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) DecreaseWindageAdjustment();
                else if (_currentMenu == 3 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) IncreaseWindageAdjustment();
                else if (_currentMenu == 4 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) PreviousReticle();
                else if (_currentMenu == 4 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) NextReticle();

                canvas.gameObject.SetActive(true);
            }
            else if (_hasZoomText) canvas.gameObject.SetActive(false);

            if (!activeWithoutMount)
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
            else if (!_initialZero && isIntegrated)
            {
                Zero();
                _initialZero = true;
            }
        }
#endif
        public void NextZoom()
        {
            if (currentZoomIndex >= ZoomFactor.Count - 1) return;
            currentZoomIndex++;
            SetZoom();
            if (doesEachZoomFactorHaveOwnReticle) ChangeReticle();
            UpdateMenu();
        }

        public void PreviousZoom()
        {
            if (currentZoomIndex <= 0) return;
            currentZoomIndex--;
            SetZoom();
            if (doesEachZoomFactorHaveOwnReticle) ChangeReticle();
            UpdateMenu();
        }

        public void SetZoom()
        {
            camera.fieldOfView = _correspondingCameraFOV[currentZoomIndex];

            if (doesZoomAffectReticle)
            {
                scopeLens.material.SetFloat("_ReticleScale", _baseReticleSize * ZoomFactor[currentZoomIndex]/ZoomFactor[0]);
            }

            if (hasRotatingBit)
            {
                Vector3 origRot = rotatingBit.localEulerAngles;

                switch (axis)
                {
                    case Axis.x:
                        rotatingBit.localEulerAngles = new Vector3(rotationAngles[currentZoomIndex], origRot.y, origRot.z);
                        break;
                    case Axis.y:
                        rotatingBit.localEulerAngles = new Vector3(origRot.x, rotationAngles[currentZoomIndex], origRot.z);
                        break;
                    case Axis.z:
                        rotatingBit.localEulerAngles = new Vector3(origRot.x, origRot.y, rotationAngles[currentZoomIndex]);
                        break;
                    default:
                        break;
                }
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
            _elevationStep += elevationIncreasePerClick;
            Zero();
            UpdateMenu();
        }
        public void DecreaseElevationAdjustment()
        {
            _elevationStep -= elevationIncreasePerClick;
            Zero();
            UpdateMenu();
        }

        public void IncreaseWindageAdjustment()
        {
            _windageStep += windageIncreasePerClick;
            Zero();
            UpdateMenu();
        }
        public void DecreaseWindageAdjustment()
        {
            _windageStep -= windageIncreasePerClick;
            Zero();
            UpdateMenu();
        }

        public void NextReticle()
        {
            currentReticle++;
            if (currentReticle >= reticles.Count) currentReticle = 0;
            ChangeReticle();
            UpdateMenu();
        }

        public void PreviousReticle()
        {
            currentReticle--;
            if (currentReticle <= 0) currentReticle = reticles.Count - 1;
            ChangeReticle();
            UpdateMenu();
        }
        public void NextMenu()
        {
            if (text == null && zeroText == null && elevationText == null && windageText == null)
                return;
            _currentMenu++;

            if (_currentMenu >= 5) _currentMenu = 0;

            switch (_currentMenu)
            {
                case 0:
                    if (text == null)
                    {
                        NextMenu();
                        return;
                    }
                    break;
                case 1:
                    if (zeroText == null)
                    {
                        NextMenu();
                        return;
                    }
                    break;
                case 2:
                    if (elevationText == null)
                    {
                        NextMenu();
                        return;
                    }
                    break;
                case 3:
                    if (windageText == null)
                    {
                        NextMenu();
                        return;
                    }
                    break;
                case 4:
                    if (reticleText == null)
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
            if (textFrame != null)
                switch (_currentMenu)
                {
                    case 0:
                        if (text == null) break;
                        textFrame.transform.position = text.transform.position;
                        break;
                    case 1:
                        if (zeroText == null) break;
                        textFrame.transform.position = zeroText.transform.position;
                        break;
                    case 2:
                        if (elevationText == null) break;
                        textFrame.transform.position = elevationText.transform.position;
                        break;
                    case 3:
                        if (windageText == null) break;
                        textFrame.transform.position = windageText.transform.position;
                        break;
                    case 4:
                        if (reticleText == null) break;
                        textFrame.transform.position = reticleText.transform.position;
                        break;
                    default:
                        break;
                }

            if (text != null) text.text = zoomPrefix + ZoomFactor[currentZoomIndex] + "x";
            if (zeroText != null) zeroText.text = zeroPrefix + ZeroDistances[ZeroDistanceIndex] + "m";
            if (elevationText != null) elevationText.text = elevationPrefix + _elevationStep + " MOA";
            if (windageText != null) windageText.text = windagePrefix + _windageStep + " MOA";
            if (reticleText != null) reticleText.text = reticlePrefix + reticleName[currentReticle];
        }
        public void Zero()
        {
#if !DEBUG
            if (isIntegrated || (this._attachment != null && this._attachment.curMount != null && this._attachment.curMount.Parent != null && this._attachment.curMount.Parent is FVRFireArm))
            {
                if (!isIntegrated) firearm = this._attachment.curMount.Parent as FVRFireArm;

                if (isIntegrated && firearm == null) Debug.LogError("ScopeShaderZoom: FireArm not set on integrated Scope! Can't zero sight!");

                FireArmRoundType roundType = firearm.RoundType;
                float zeroDistance = this.ZeroDistances[this.ZeroDistanceIndex];
                float num = 0.0f;
                if (AM.SRoundDisplayDataDic.ContainsKey(roundType))
                    num = AM.SRoundDisplayDataDic[roundType].BulletDropCurve.Evaluate(zeroDistance * (1f / 1000f));
                Vector3 p = firearm.MuzzlePos.position + firearm.GetMuzzle().forward * zeroDistance + firearm.GetMuzzle().up * num;
                Vector3 vector3_1 = Vector3.ProjectOnPlane(p - this.transform.forward, this.transform.right);
                Vector3 vector3_2 = Quaternion.AngleAxis(this._elevationStep /60f, this.transform.right) * vector3_1;
                Vector3 forward = Quaternion.AngleAxis(this._windageStep / 60f, this.transform.up) * vector3_2;


                Vector3 projected_p = Vector3.ProjectOnPlane(p, this.transform.right) + Vector3.Dot(this.transform.position, this.transform.right) * this.transform.right;

                //this.TargetAimer.rotation = Quaternion.LookRotation(forward, this.transform.up);
                //this.camera.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(forward - camera.transform.position, camera.transform.right), camera.transform.up);// PointTowards(p);
                this.camera.transform.LookAt(projected_p, this.transform.up);
                this.camera.transform.localEulerAngles += new Vector3(-this._elevationStep / 60f, this._windageStep / 60f, 0);
                //this.camera.transform.Rotate(new Vector3(-this.ElevationStep / 60f, this.WindageStep / 60f, 0));

                //this.camera.transform.LookAt(forward);

                //this.ScopeCam.ScopeCamera.transform.rotation = Quaternion.LookRotation(forward, this.transform.up);
            }
            else this.camera.transform.localRotation = Quaternion.identity;
#endif
        }

        public void ScopeEnabled(bool state)
        {
            if (state)
            {
                camera.gameObject.SetActive(true);
            }
            else
            {
                camera.gameObject.SetActive(false);
                RenderTexture.active = _renderTexture;
                GL.Clear(false, true, Color.black);
                RenderTexture.active = (RenderTexture) null;
            }
        }

        private void ChangeReticle()
        {
            if (!doesEachZoomFactorHaveOwnReticle)
            {
                scopeLens.material.SetColor("_ReticleColor", reticleColors[currentReticle]);
                scopeLens.material.SetTexture("_ReticleTex", reticles[currentReticle]);
            }
            else
            {
                scopeLens.material.SetColor("_ReticleColor", reticleColors[currentReticle]);
                scopeLens.material.SetTexture("_ReticleTex", additionalReticlesPerZoomLevel[currentZoomIndex + currentReticle * ZoomFactor.Count]);
            }
        }
    }
}

