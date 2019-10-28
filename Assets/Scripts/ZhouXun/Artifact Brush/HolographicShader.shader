Shader "zhouxun/Holographic"
{
	Properties
	{
		_Color ("Main Color",Color) = (0.08,0.4,0,0.2)
		_NotVisibleColor ("X-ray Color", Color) = (0,0.03,0.075,0.2)
		_HoloColor ("Holo Color", Color) = (0.3,0.7,0.25,0.2)
		_NoiseTex ("Noise Texture", 2D) = "black" {}
		_Tile ("Tile", Float) = 0.2
		_Speed ("Speed", Float) = 2
		_BodyIntensity ("Body Intensity", Float) = 0.27
		_HoloIntensity ("Holo Intensity", Float) = 0.8
		_NoiseIntensity ("Noise Intensity", Float) = 0.15
		_TimeFactor ("Time Factor", Float) = 0
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent+2"
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
	 
	 		float4 _Color;
	 		float4 _HoloColor;
			sampler2D _NoiseTex;
			float _Tile;
			float _Speed;
			float _BodyIntensity;
			float _HoloIntensity;
			float _NoiseIntensity;
			float _TimeFactor;
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 vertPos : COLOR0;	
			};
	
			struct v2f 
			{
				float3 vertPos : COLOR0;
				float4 pos : POSITION0;
			};
	
			v2f vert (appdata_v v)
			{
				v2f o;
				o.vertPos.rgb = v.vertex.xyz;	
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
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
				_TimeFactor = _Time.y;
				float xintensity = pow(randIntensity(i.vertPos.x + _TimeFactor*_Speed).r, 50);
				float yintensity = pow(randIntensity(i.vertPos.y + _TimeFactor*_Speed).g, 50);
				float zintensity = pow(randIntensity(i.vertPos.z + _TimeFactor*_Speed).b, 50);
				float inc = (randIntensity(i.vertPos.x + _TimeFactor*_Speed*0.3).r + randIntensity(i.vertPos.y + _TimeFactor*_Speed*0.3).g + randIntensity(i.vertPos.z + _TimeFactor*_Speed*0.3).b) * _NoiseIntensity;
				float coef = (randIntensity(_TimeFactor).r * 0.2 + 0.9) * _HoloIntensity;
				return _Color*_BodyIntensity + _HoloColor*(xintensity+yintensity+zintensity+inc*inc*inc)*coef;
			}
			ENDCG
	    }
		Pass
		{
        	ZTest Greater
	        Material
	        {
		 		Diffuse [_NotVisibleColor]
	      		Ambient [_NotVisibleColor]
	        }
	        Lighting On
        }
	} 
	FallBack "Diffuse"
}
