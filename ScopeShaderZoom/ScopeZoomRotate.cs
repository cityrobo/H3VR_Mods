using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FistVR;

namespace Cityrobo.AndrewFTW
{
    public class ScopeZoomRotate : MonoBehaviour
    {
        public FVRInteractiveObject AttachmentInterface;
        public MeshRenderer scopeLens;
        public Camera camera;
        public List<float> ZoomFactor;

        public int currentZoomIndex;

        [Header("Rotation deatails")]
        public List<float> Rotation;
        public Axis axis;

        public GameObject ObjectToRotate;

        public enum Axis
        {
            x,
            y,
            z
        }

        [Header("If you want a Screen above the scope that shows the current Magninification, use these two:")]
        public GameObject canvas;
        public Text text;

        private List<float> CorrespondingCameraFOV;

        private bool hasZoomText;

        private RenderTexture renderTexture;
        public void Start()
        {
            CorrespondingCameraFOV = new List<float>();
            if (text != null && canvas != null) hasZoomText = true;
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

            SetZoom();
        }
        public void OnDestroy()
        {
            Destroy(renderTexture);
        }
#if !DEBUG
        public void Update()
        {
            FVRViveHand hand = AttachmentInterface.m_hand;
            if (hand != null)
            {
                if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.left) < 45f) PreviousZoom();
                else if (hand.Input.TouchpadDown && Vector2.Angle(hand.Input.TouchpadAxes, Vector2.right) < 45f) NextZoom();

                if (hasZoomText)
                {
                    canvas.gameObject.SetActive(true);
                    text.text = string.Format("Zoom: {0}x", ZoomFactor[currentZoomIndex].ToString());
                }
            }
            else if (hasZoomText) canvas.gameObject.SetActive(false);
        }
#endif
        public void NextZoom()
        {
            if (currentZoomIndex == ZoomFactor.Count - 1) return;
            currentZoomIndex++;
            SetZoom();
        }

        public void PreviousZoom()
        {
            if (currentZoomIndex == 0) return;
            currentZoomIndex--;
            SetZoom();
        }

        public void SetZoom()
        {
            Vector3 rotation;
            switch (axis)
            {
                case Axis.x:
                    rotation = new Vector3(Rotation[currentZoomIndex],0,0);
                    break;
                case Axis.y:
                    rotation = new Vector3(0, Rotation[currentZoomIndex], 0);
                    break;
                case Axis.z:
                    rotation = new Vector3(0, 0, Rotation[currentZoomIndex]);
                    break;
                default:
                    rotation = new Vector3();
                    break;
            }
            ObjectToRotate.transform.localEulerAngles = rotation;
            camera.fieldOfView = CorrespondingCameraFOV[currentZoomIndex];
        }

        public void ChangeElevation()
        {

        }
    }
}

