Shader "zhouxun/DigSign"
{
	Properties
	{
		_DecalTex ("Decal Texture", 2D) = "white" {}
		_NoiseTex ("Noise Texture", 2D) = "black" {}
		_CenterAndRadius ("Center(xyz) and Radius(w)", Vector) = (0,0,0,10)
		_TintColor ("Main Color",Color) = (1,1,1,1)
		_StreamColor ("Stream Color", Color) = (1,1,1,1)
		_Tile ("Stream Tile", Float) = 0.2
		_Speed ("Stream Speed", Float) = 2
		_BodyIntensity ("Body Intensity", Float) = 0.27
		_StreamIntensity ("Stream Intensity", Float) = 0.8
		_NoiseIntensity ("Noise Intensity", Float) = 0.15
		_ExhaustEffect ("Exhausted Effect", Float) = 0
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		LOD 200
		Fog { Mode off }
		Blend One One
		Cull off
		Zwrite off
            
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		    #pragma target 3.0
			#include "UnityCG.cginc"
	 
	 		float4 _CenterAndRadius;
	 		float4 _TintColor;
	 		float4 _StreamColor;
			sampler2D _NoiseTex;
			float _Tile;
			float _Speed;
			float _BodyIntensity;
			float _StreamIntensity;
			float _NoiseIntensity;
			float _ExhaustEffect;
			sampler2D _DecalTex;
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 wPos : COLOR0;	
			};
	
			struct v2f 
			{
				float3 wPos : TEXCOORD0;
				float4 pos : POSITION0;
			};
	
			v2f vert (appdata_v v)
			{
				v2f o; 
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.wPos = mul(_Object2World, v.vertex).xyz;
				return o;
			}
			
			#define ONEPX 0.015625
			#define HALFPX 0.0078125
			
			float3 randIntensity (float coord)
			{
				coord *= _Tile;
				float2 coord_0 = float2(coord, coord*ONEPX);
				
				float3 randnum = tex2D(_NoiseTex, coord_0).rgb;
				return randnum;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float dist = length(i.wPos.xyz - _CenterAndRadius.xyz);
				float brightness_body = clamp((_CenterAndRadius.w - dist) / _CenterAndRadius.w * 5, 0,1);
//				float brightness_border = clamp(0.005-abs((_CenterAndRadius.w - dist)/_CenterAndRadius.w - 0.005),0,1)*50;
				float3 texc = tex2D(_DecalTex, (i.wPos.xz-_CenterAndRadius.xz+_CenterAndRadius.ww*0.5)/(_CenterAndRadius.w)).rgb;
//				float _TimeFactor = _Time.y * _Speed;
//				float dist_u = (1-dist / _CenterAndRadius.w);
//				float xintensity = pow(randIntensity(dist_u + _TimeFactor).r, 50) * ((clamp(texc.b,0.65,1) - 0.65) * 10) * dist_u;
//				float yintensity = pow(randIntensity(dist_u*0.687 + _TimeFactor).g, 50) * ((clamp(texc.b,0.65,1) - 0.65) * 10) * dist_u;
//				float zintensity = pow(randIntensity(dist_u*1.472 + _TimeFactor).b, 50) * ((clamp(texc.b,0.65,1) - 0.65) * 10) * dist_u;
//				float exhaust_factor = (randIntensity(_TimeFactor * 100).r - 0.5) * 2;
//				float inc = (randIntensity(i.wPos.x + _TimeFactor*0.3).r + randIntensity(i.wPos.y + _TimeFactor*0.3).g + randIntensity(i.wPos.z + _TimeFactor*0.3).b) * _NoiseIntensity;
//				float coef = (randIntensity(_TimeFactor).r * 0.2 + 0.9) * _StreamIntensity;
				float4 retc = (0,0,0,0);
				retc.rgb = texc.rgb * _TintColor*_BodyIntensity*brightness_body;
				retc.a = 1;
				return retc;
			}
			ENDCG
	    }
	} 
	FallBack "Diffuse"
}
