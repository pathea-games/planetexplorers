Shader "zhouxun/EnergySheild"
{
	Properties
	{
		_Color ("Main Color",Color) = (0.03,0.1,0.15,0.2)
		_ImpactColor ("Holo Color", Color) = (0.2,0.5,0.7,0.2)
		_NoiseTex ("Noise Texture", 2D) = "black" {}
		_NoiseTile ("Tile", Float) = 0.2
		_SpreadSpeed ("Spread Speed", Float) = 0.2
		_BodyIntensity ("Body Intensity", Float) = 0.27
		_HoloIntensity ("Holo Intensity", Float) = 0.1
		_Impact1Point ("Impact 1 Position", Vector) = (0,0,0,0)
		_Impact1Dist ("Impact 1 Wave Dist", float) = -1000
		_Impact2Point ("Impact 2 Position", Vector) = (0,0,0,0)
		_Impact2Dist ("Impact 2 Wave Dist", float) = -1000
		_Impact3Point ("Impact 3 Position", Vector) = (0,0,0,0)
		_Impact3Dist ("Impact 3 Wave Dist", float) = -1000
		_WaveLength ("Wave Length", float) = 0.2
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
	 		float4 _ImpactColor;
			sampler2D _NoiseTex;
			float _NoiseTile;
			float _SpreadSpeed;
			float _BodyIntensity;
			float _HoloIntensity;
			float4 _Impact1Point;
			float _Impact1Dist;
			float4 _Impact2Point;
			float _Impact2Dist;
			float4 _Impact3Point;
			float _Impact3Dist;
			float _WaveLength;
	
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
			
			float3 randIntensity (float2 coord)
			{
				coord *= _NoiseTile;
				
				float3 randnum = tex2D(_NoiseTex, coord).rgb;
				return randnum;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float3 vert = i.vertPos;
				float xintensity = pow(randIntensity(vert.yz + _Time.xy*_SpreadSpeed).r, 5)*1;
				float yintensity = pow(randIntensity(vert.zx + _Time.xy*_SpreadSpeed).g, 5)*1;
				float zintensity = pow(randIntensity(vert.xy + _Time.xy*_SpreadSpeed).b, 5)*1;
				float xintensity2 = pow(randIntensity(vert.yz*2 + _Time.xy*_SpreadSpeed * 0.5).r, 3);
				float yintensity2 = pow(randIntensity(vert.zx*2 + _Time.xy*_SpreadSpeed * 0.5).g, 3);
				float zintensity2 = pow(randIntensity(vert.xy*2 + _Time.xy*_SpreadSpeed * 0.5).b, 3);
				float xintensity4 = pow(randIntensity(vert.yz*4 + _Time.xy*_SpreadSpeed * 0.25).r, 2);
				float yintensity4 = pow(randIntensity(vert.zx*4 + _Time.xy*_SpreadSpeed * 0.25).g, 2);
				float zintensity4 = pow(randIntensity(vert.xy*4 + _Time.xy*_SpreadSpeed * 0.25).b, 2);
				float inc1 = xintensity + yintensity + zintensity;
				float inc2 = xintensity2 + yintensity2 + zintensity2;
				float inc4 = xintensity4 + yintensity4 + zintensity4;
				float waveintensity = 0;
				float dist1 = length(vert - _Impact1Point);
				float dist2 = length(vert - _Impact2Point);
				float dist3 = length(vert - _Impact3Point);
				float lamda1 = (dist1 / (_WaveLength*10) + 0.1);
				float lamda2 = (dist2 / (_WaveLength*10) + 0.1);
				float lamda3 = (dist3 / (_WaveLength*10) + 0.1);
				float strength1 = pow( 0.5, abs(dist1 - _Impact1Dist) / _WaveLength ) / (lamda1 * lamda1);
				float strength2 = pow( 0.5, abs(dist2 - _Impact2Dist) / _WaveLength ) / (lamda2 * lamda2);
				float strength3 = pow( 0.5, abs(dist3 - _Impact3Dist) / _WaveLength ) / (lamda3 * lamda3);
				waveintensity = (strength1 + strength2 + strength3) * 3;
				return _Color*_BodyIntensity + _ImpactColor*(inc1*_HoloIntensity + inc2*_HoloIntensity*0.8f + inc4*_HoloIntensity*0.5f + waveintensity*inc1);
			}
			ENDCG
	    }
	} 
	FallBack "Diffuse"
}
