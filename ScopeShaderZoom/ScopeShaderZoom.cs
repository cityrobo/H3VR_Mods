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
        public List<float> ZoomFactor;

        [Header("If you want a Screen above the scope that shows the current Magninification, use these two:")]
        public GameObject canvas;
        public Text text;

        private List<float> CorrespondingCameraFOV;


        private int currentZoomIndex;
        private bool hasZoomText;
        public void Start()
        {
            CorrespondingCameraFOV = new List<float>();
            Debug.Log(camera.ToString());

            currentZoomIndex = 0;
            if (text != null && canvas != null) hasZoomText = true;
            else hasZoomText = false;

            for (int i = 0; i < ZoomFactor.Count; i++)
            {
                CorrespondingCameraFOV.Add(53.7f * Mathf.Pow(ZoomFactor[i], -0.9284f) - 0.5035f);
            }
            Debug.Log(CorrespondingCameraFOV.ToString());

            RenderTexture renderTexture = camera.targetTexture;
            renderTexture = RenderTexture.Instantiate(renderTexture);
            camera.targetTexture = renderTexture;
            scopeLens.material.mainTexture = renderTexture;
        }
#if !UNITY_EDITOR
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
            camera.fieldOfView = CorrespondingCameraFOV[currentZoomIndex];
        }

        public void ChangeElevation()
        {
            
        }
    }
}

