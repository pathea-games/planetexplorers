Shader "zhouxun/Volume Fog" 
{
	Properties 
	{
		_DepthColor ("Depth Color", Color) = (1,1,1,1)
		_NoiseMap ("Cloud", 2D) = "white" {}		
	}
	
	CGINCLUDE
	#include "PEWaterInc.cginc"
	
	struct appdata_water
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	sampler2D _CameraDepthTexture; 
	sampler2D _NoiseMap;
	
	uniform float4 _DepthColor;
	ENDCG
	
	SubShader 
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent-109"}
		Lod 200
		
		Pass 
		{
			Name "Back Face"
			ZWrite On
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma target 3.0 
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma fragmentoption ARB_fog_exp2
			
			struct v2f 
			{
				float4 pos 			: SV_POSITION;
				float4 projpos      : TEXCOORD1;
				float3 worldPos		: TEXCOORD2;
			};
			
			v2f vert(appdata_water v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.projpos = o.pos;
				o.worldPos = mul(_Object2World,(v.vertex)).xyz;
				return o;
			}
						
			half4 frag( v2f i ) : COLOR
			{
				float4 screenPos = ComputeScreenPos(i.projpos);
				// Calc depth
				float depth = screenPos.w;
				return half4(_DepthColor.rgb, 0); 
			}
			ENDCG
		}
		Pass 
		{
			Name "Front Face"
			Zwrite Off
			Cull Back
			Blend One One
			
			CGPROGRAM
			#pragma target 3.0 
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest 
			#pragma fragmentoption ARB_fog_exp2
			
			struct v2f 
			{
				float4 pos 			: SV_POSITION;
				float4 projpos      : TEXCOORD1;
				float3 worldPos		: TEXCOORD2; 
			};
			
			v2f vert(appdata_water v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.projpos = o.pos;
				o.worldPos = mul(_Object2World,(v.vertex)).xyz;
				return o;
			}
						
			half4 frag( v2f i ) : COLOR
			{
				float4 screenPos = ComputeScreenPos(i.projpos);
				// Calc depth
				float depth = GetLED(_CameraDepthTexture, screenPos) - screenPos.w;
				return half4(_DepthColor.rgb*(pow(depth,1.5)/1000)*(tex2D(_NoiseMap, i.worldPos.xy*0.01+float2(TIME_FACTOR*0.01,TIME_FACTOR*0.01)).r + 
				                                                 tex2D(_NoiseMap, i.worldPos.xy*0.01+float2(-TIME_FACTOR*0.01,TIME_FACTOR*0.01)).r), 1); 
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
