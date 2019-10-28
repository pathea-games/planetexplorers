Shader "SpecialItem/Holographic" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Lightn ("Lightness", 2D) = "white" {}
		_Dist ("DistanceMultipler", float) = 150
		_Flick ("FlickerMultipler", float) = 25
		_MinL ("FlickerLight", Range(0,1)) = 0.3
		_Ribbon ("RibbonMultipler", float) = 3
		_RibL ("RibbonLight", float) = 0.05
		_Scale ("Scale", float) = 1
		_Direct ("Direction", float) = 1
	}
	SubShader {
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent+1"
		}
	
			Blend ONE ONE			
			ZWrite Off
			
			CGPROGRAM
			#pragma surface surf Lambert
			#pragma vertex vert
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			sampler2D _Lightn;
			sampler2D _CameraDepthTexture;
			half _Dist;
			half _Flick;
			half _MinL;
			half _Ribbon;
			half _RibL;
			half _Scale;
			half _Direct;
			
			half3 basec;
			half alp;
			half flicker;
			half append;
			struct Input
			{
				float2 uv_MainTex;
				float3 world_pos;
				float4 screenPos;
			};
			void vert (inout appdata_base v, out Input o)
			{
				o.uv_MainTex = v.texcoord.xy;
		    	o.world_pos = v.vertex;
		    	o.screenPos = mul(UNITY_MATRIX_MVP, v.vertex);
			}
			float GetLED ( sampler2D depthtex, float4 screen_pos )
			{
				return LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(depthtex, UNITY_PROJ_COORD(screen_pos))));
			}
			void surf (Input IN, inout SurfaceOutput o)
			{
				basec = tex2D(_MainTex, half2(IN.screenPos.x / IN.screenPos.w, IN.screenPos.y / IN.screenPos.w)).rgb;
				alp = tex2D(_Lightn, half2(0, (IN.world_pos.y * _Direct + IN.world_pos.x * (1 - _Direct)) * _Dist * _Scale)).r;
				flicker = frac(_Time.y * _Flick) * _MinL + 1 - _MinL;
				append = frac((IN.world_pos.y * _Direct + IN.world_pos.x * (1 - _Direct) + _Time.y) * _Ribbon) * _RibL;
				o.Emission = basec * alp * min(1, flicker + append) * 2 * saturate((GetLED(_CameraDepthTexture, IN.screenPos) - IN.screenPos.w)*3);				
			}
			ENDCG
	}
	FallBack "Diffuse"
}
