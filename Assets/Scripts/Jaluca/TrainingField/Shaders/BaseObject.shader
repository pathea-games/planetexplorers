Shader "SpecialItem/TrainingRoom/BaseObject"
{
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
		_Tolerance ("Tolerance", Range(0,0.5)) = 0.2
		_TexH ("TexH", int) = 512
		_TexW ("TexW", int) = 512
	}
	SubShader {
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent+1"
		}
		Pass
		{
			Blend ONE ONE			
			ZWrite Off
			
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"			
			
			sampler2D _MainTex;
			sampler2D _Lightn;			
			half _Dist;
			half _Flick;
			half _MinL;
			half _Ribbon;
			half _RibL;
			half _Scale;
			half _Direct;
			half _Tolerance;
			int _TexH;
			int _TexW;
			
			half4 basec;
			half alp;
			half flicker;
			half append;			
			
			struct appdata_v
			{
				float2 texcoord : TEXCOORD0;
				float4 vertex : POSITION;				
			};			
			struct Input
			{
				float2 uv : TEXCOORD0;
				float4 world_pos : POSITION;
				float2 pos : TEXCOORD1;
			};
			Input vert (appdata_v v)
			{
				Input i;
				i.uv = v.texcoord;
				i.world_pos = mul(UNITY_MATRIX_MVP, v.vertex);
				i.pos = i.world_pos.xy;		    	
		    	return i;
			}
			half LineLightness(half px, half py, half4 center, float2 uv)
			{
				half4 c0 = abs(tex2D(_MainTex, uv + half2(-px, -py)) - center);
				half4 c1 = abs(tex2D(_MainTex, uv + half2(-px, py)) - center) + c0;
				half4 c2 = abs(tex2D(_MainTex, uv + half2(px, -py)) - center) + c1;
				half4 c3 = abs(tex2D(_MainTex, uv + half2(px, py)) - center) + c2;
				half4 c4 = abs(tex2D(_MainTex, uv + half2(0, -py)) - center) + c3;
				half4 c5 = abs(tex2D(_MainTex, uv + half2(0, py)) - center) + c4;
				half4 c6 = abs(tex2D(_MainTex, uv + half2(-px, 0)) - center) + c5;
				half4 c7 = abs(tex2D(_MainTex, uv + half2(px, 0)) - center) + c6;				
				return saturate(length(c7.xyz / 8) / _Tolerance) * 1.5f;
			}
			half4 frag (Input i) : COLOR
			{				
				basec = tex2D(_MainTex, i.uv);
				half ll = LineLightness(1.0f / _TexW, 1.0f / _TexH, basec, i.uv);
				alp = tex2D(_Lightn, half2(0, (i.pos.y * _Direct + i.pos.x * (1 - _Direct)) * _Dist * _Scale)).r;
				flicker = frac(_Time.y * _Flick) * _MinL + 1 - _MinL;
				append = frac((i.pos.y * _Direct + i.pos.x * (1 - _Direct) + _Time.y) * _Ribbon) * _RibL;
				half4 ret = half4(1,1,1,1);				
				ret.rgb = half3(0.55,0.8235,1) * ll * alp * min(1, flicker + append) * 2 * basec.a;
				return ret;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}