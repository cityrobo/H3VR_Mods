using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiguelFerreira 
{
//    [ExecuteInEditMode]
    public class ThermalVisionMode : MonoBehaviour
    {
		public Shader thermalShader;
		public Texture2D thermalLookUpTexture;
		public bool useOwnRenderTexture;
		public RenderTexture referenceRenderTexture;
		public MeshRenderer screen;

		RenderingPath mainCameraOriginalRenderingPath;
		bool mainCameraOriginalOcclusionCulling;
		private RenderTexture renderTexture;

		Camera _mainCamera;
		Camera MainCamera 
		{
			get 
			{ 
				if (_mainCamera == null) 
				{
					_mainCamera = GetComponent<Camera> ();
				}

				return _mainCamera;
			}
		}

		public void OnEnable() 
		{
			Shader.SetGlobalTexture ("_ThermalColorLUT",thermalLookUpTexture);

			mainCameraOriginalOcclusionCulling = MainCamera.useOcclusionCulling;
			mainCameraOriginalRenderingPath = MainCamera.renderingPath;
			MainCamera.useOcclusionCulling = false;
			MainCamera.renderingPath = RenderingPath.Forward;
			MainCamera.SetReplacementShader (thermalShader, "Thermal");

			if (useOwnRenderTexture)
			{
				renderTexture = RenderTexture.Instantiate(referenceRenderTexture);
				MainCamera.targetTexture = renderTexture;

				screen.material.mainTexture = renderTexture;
			}
		}

		public void OnDisable() 
		{
			MainCamera.useOcclusionCulling = mainCameraOriginalOcclusionCulling;
			MainCamera.renderingPath = mainCameraOriginalRenderingPath;
			MainCamera.ResetReplacementShader ();
		}

		private RenderTexture CopyRenderTexture(RenderTexture reference)
        {
			RenderTexture renderTexture = new RenderTexture(reference.width,reference.height,reference.depth,reference.format,RenderTextureReadWrite.sRGB);
			renderTexture.antiAliasing = reference.antiAliasing;
			renderTexture.useMipMap = reference.useMipMap;
			renderTexture.autoGenerateMips = reference.autoGenerateMips;
			renderTexture.wrapMode = reference.wrapMode;
			renderTexture.filterMode = reference.filterMode;
			renderTexture.anisoLevel = reference.anisoLevel;
			
			return renderTexture;
        }
	}
}