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
        public GameObject textFrame;

        public string zoomPrefix = "Zoom: ";
        public string zeroPrefix = "Zero Distance: ";
        public string elevationPrefix = "Elevation: ";
        public string windagePrefix = "Windage: ";

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
        [Tooltip("The existence of this text enables the reticle change functionality")]
        public Text reticleText;
        public string reticlePrefix = "Reticle: ";

        [Tooltip("Names of additional reticles first. Last reticle name is the name of the default reticle (on the shader itself)")]
        public string[] reticleName;
        [Tooltip("Additional reticle colors")]
        public List<Color> reticleColors;
        [Tooltip("Additional reticles")]
        public List<Texture2D> reticles;
        public int currentReticle = 0;

        public bool doesEachZoomFactorHaveOwnReticle = false;
        public List<Texture2D> additionalReticlesPerZoomLevel;

        private List<float> CorrespondingCameraFOV;

        private bool hasZoomText;
        private RenderTexture renderTexture;

        private FVRFireArmAttachment Attachment;

        private float ElevationStep;
        private float WindageStep;

        private int currentMenu;
        private bool initialZero = false;

        private float baseReticleSize;

        

        public void Start()
        {
            CorrespondingCameraFOV = new List<float>();
            if (canvas != null) hasZoomText = true;
            else hasZoomText = false;

            for (int i = 0; i < ZoomFactor.Count; i++)
            {
                //CorrespondingCameraFOV.Add(53.7f * Mathf.Pow(ZoomFactor[i], -0.9284f) - 0.5035f);
                //CorrespondingCameraFOV.Add(54.3f * Mathf.Pow(ZoomFactor[i], -0.9613f) - 0.1378f);
                float zoomValue = 53.6f * Mathf.Pow(ZoomFactor[i], -0.9364f) - 0.3666f;
                CorrespondingCameraFOV.Add(zoomValue);
            }


            renderTexture = camera.targetTexture;
            renderTexture = RenderTexture.Instantiate(renderTexture);
            camera.targetTexture = renderTexture;
            scopeLens.material.mainTexture = renderTexture;

            if (doesZoomAffectReticle)
            {
                baseReticleSize = scopeLens.material.GetFloat("_ReticleScale");
            }

            SetZoom();

            FVRFireArmAttachmentInterface attachmentInterface = AttachmentInterface as FVRFireArmAttachmentInterface;

            if (!isIntegrated && attachmentInterface != null)
            {
                Attachment = attachmentInterface.Attachment;
            }
            else if (!isIntegrated)
            {
                Attachment = this.gameObject.GetComponent<FVRFireArmAttachment>();
            }

            if (!isIntegrated && Attachment == null) Debug.LogWarning("Attachment not found. Scope zeroing disabled!");

            UpdateMenu();

            ScopeEnabled(activeWithoutMount);

            //camera.gameObject.SetActive(activeWithoutMount);

            if (isIntegrated) Zero();

            if (text == null) 
            { 
                currentMenu++;
                if (zeroText == null)
                {
                    currentMenu++;
                    if (elevationText == null)
                    {
                        currentMenu++;
                        if (windageText == null)
                        {
                            currentMenu = 0;
                            hasZoomText = false;
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
                if (currentReticle >= reticles.Count) currentReticle = reticles.Count - 1;
                ChangeReticle();
            }
        }
        public void OnDestroy()
        {
            Destroy(renderTexture);
        }
#if !DEBUG
        public void Update()
        {
            FVRViveHand hand = AttachmentInterface.m_hand;
            if (hasZoomText && hand != null)
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.up) < 45f) NextMenu();
                else if (currentMenu == 0 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) PreviousZoom();
                else if (currentMenu == 0 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) NextZoom();
                else if (currentMenu == 1 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) PreviousZero();
                else if (currentMenu == 1 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) NextZero();
                else if (currentMenu == 2 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) DecreaseElevationAdjustment();
                else if (currentMenu == 2 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) IncreaseElevationAdjustment();
                else if (currentMenu == 3 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) DecreaseWindageAdjustment();
                else if (currentMenu == 3 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) IncreaseWindageAdjustment();
                else if (currentMenu == 4 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) PreviousReticle();
                else if (currentMenu == 4 && hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) NextReticle();

                canvas.gameObject.SetActive(true);
            }
            else if (hasZoomText) canvas.gameObject.SetActive(false);

            if (!activeWithoutMount)
            {
                if (Attachment != null && Attachment.curMount != null) ScopeEnabled(true);
                else if (Attachment != null && Attachment.curMount == null) ScopeEnabled(false);
                else ScopeEnabled(true);
            }

            if (!initialZero && Attachment != null && Attachment.curMount != null)
            {
                Zero();
                initialZero = true;
            }
            else if (initialZero && Attachment != null && Attachment.curMount == null)
            {
                Zero();
                initialZero = false;
            }
            else if (!initialZero && isIntegrated)
            {
                Zero();
                initialZero = true;
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
            camera.fieldOfView = CorrespondingCameraFOV[currentZoomIndex];

            if (doesZoomAffectReticle)
            {
                scopeLens.material.SetFloat("_ReticleScale", baseReticleSize * ZoomFactor[currentZoomIndex]/ZoomFactor[0]);
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
            ElevationStep += elevationIncreasePerClick;
            Zero();
            UpdateMenu();
        }
        public void DecreaseElevationAdjustment()
        {
            ElevationStep -= elevationIncreasePerClick;
            Zero();
            UpdateMenu();
        }

        public void IncreaseWindageAdjustment()
        {
            WindageStep += windageIncreasePerClick;
            Zero();
            UpdateMenu();
        }
        public void DecreaseWindageAdjustment()
        {
            WindageStep -= windageIncreasePerClick;
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
            currentMenu++;

            if (currentMenu >= 5) currentMenu = 0;

            switch (currentMenu)
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
                switch (currentMenu)
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
            if (elevationText != null) elevationText.text = elevationPrefix + ElevationStep + " MOA";
            if (windageText != null) windageText.text = windagePrefix + WindageStep + " MOA";
            if (reticleText != null) reticleText.text = reticlePrefix + reticleName[currentReticle];
        }
        public void Zero()
        {
#if!Debug
            if (isIntegrated || (this.Attachment != null && this.Attachment.curMount != null && this.Attachment.curMount.Parent != null && this.Attachment.curMount.Parent is FVRFireArm))
            {
                if (!isIntegrated) firearm = this.Attachment.curMount.Parent as FVRFireArm;

                if (isIntegrated && firearm == null) Debug.LogError("ScopeShaderZoom: FireArm not set on integrated Scope! Can't zero sight!");

                FireArmRoundType roundType = firearm.RoundType;
                float zeroDistance = this.ZeroDistances[this.ZeroDistanceIndex];
                float num = 0.0f;
                if (AM.SRoundDisplayDataDic.ContainsKey(roundType))
                    num = AM.SRoundDisplayDataDic[roundType].BulletDropCurve.Evaluate(zeroDistance * (1f / 1000f));
                Vector3 p = firearm.MuzzlePos.position + firearm.GetMuzzle().forward * zeroDistance + firearm.GetMuzzle().up * num;
                Vector3 vector3_1 = Vector3.ProjectOnPlane(p - this.transform.forward, this.transform.right);
                Vector3 vector3_2 = Quaternion.AngleAxis(this.ElevationStep /60f, this.transform.right) * vector3_1;
                Vector3 forward = Quaternion.AngleAxis(this.WindageStep / 60f, this.transform.up) * vector3_2;


                Vector3 projected_p = Vector3.ProjectOnPlane(p, this.transform.right) + Vector3.Dot(this.transform.position, this.transform.right) * this.transform.right;

                //this.TargetAimer.rotation = Quaternion.LookRotation(forward, this.transform.up);
                //this.camera.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(forward - camera.transform.position, camera.transform.right), camera.transform.up);// PointTowards(p);
                this.camera.transform.LookAt(projected_p, this.transform.up);
                this.camera.transform.localEulerAngles += new Vector3(-this.ElevationStep / 60f, this.WindageStep / 60f, 0);
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
                RenderTexture.active = renderTexture;
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

