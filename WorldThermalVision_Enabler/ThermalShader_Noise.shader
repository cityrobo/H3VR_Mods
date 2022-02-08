// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Thermal/ThermalShader_Noise"
{
	Properties
	{
		_ThermalColorLUT ("Thermal Color Look-up texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { 
		"Thermal" = "Cold"  
		"Queue" = "Geometry"
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "ThermalShaderCore_Noise.cginc"

			float _ThermalPowExponent;
			float _ThermalMax;
			float _ThermalMin;
			float _NoiseScale;
			
			fixed4 frag (v2f i) : SV_Target
			{
				_NoiseScale = 0.2;
				return thermal_frag(i,_ThermalPowExponent,_ThermalMax,_ThermalMin,_NoiseScale);
			}
			ENDCG
		}
	}

	SubShader {
		Tags { 
		"Thermal" = "Hot"
		"Queue" = "Geometry"
		 }
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "ThermalShaderCore_Noise.cginc"

			float _ThermalPowExponent;
			float _ThermalMax;
			float _ThermalMin;
			float _NoiseScale;

			
			
			fixed4 frag (v2f i) : SV_Target
			{
				_NoiseScale = 0.2;
				return thermal_frag(i,_ThermalPowExponent,_ThermalMax,_ThermalMin,_NoiseScale);
			}
			ENDCG
		}
	}
}
