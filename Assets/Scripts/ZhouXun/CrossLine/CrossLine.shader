Shader "zhouxun/CrossLine"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_RefractionTex ("Texture", 2D) = "white" {}
		_NoiseMap("Noise map", 2D) = "black"
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent+100" "IgnoreProjector"="True" }
		Pass
		{
			Fog { Mode Off } 
			Blend One One
			Cull Off
			ZWrite Off 
			
			CGPROGRAM 
			#pragma target 3.0
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma glsl
			#include "CameraDepthInc.cginc"

			struct appdata_line 
			{
				float4 vertex : POSITION;
				float3 texcoord : TEXCOORD0;
			};
			
			float4 _Color;
			sampler2D _CameraDepthTexture; 
			sampler2D _NoiseMap;
	
			struct v2f 
			{
				float4 pos 			: SV_POSITION;
				float3 texc 		: TEXCOORD0;
				float4 projpos      : TEXCOORD1;
			};
			
			#define TEXEL 0.015625
			float RandBrightness(float3 pos)
			{
				pos += (_Time.xyz);
				return tex2D(_NoiseMap, float2(pos.xy)) 
				     + tex2D(_NoiseMap, float2(pos.yz)) 
				     + tex2D(_NoiseMap, float2(pos.zx));
			}
			
			v2f vert (appdata_line v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord;
				o.projpos = o.pos;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float4 screenPos = ComputeScreenPos(i.projpos);
				float depth = screenPos.w;
				float linearDepth = GetLED(_CameraDepthTexture, screenPos);
				float delta_depth = linearDepth - depth;
				
				float atten = pow(1 - abs(i.texc.y),5);
				float atten2 = sin(i.texc.x + _Time.y * 3) * 0.5 + 0.5; 
				float atten3 = pow(1 - abs(abs(i.texc.y) - 0.25 + 0.1 * sin(i.texc.x*3 + _Time.y))*1.01,100);
				float atten4 = sin(i.texc.x * 2 + _Time.y * 10) * 0.5; 
				float atten_noise = RandBrightness(i.texc.yxz * 2);
				float depth_atten = pow(saturate(delta_depth*0.5) * saturate(depth*0.5-0.5),1);
				
				half4 c = _Color;
				c *= depth_atten * (atten + max(0, atten4*atten3*0.3)) * (atten2 + atten_noise * 0.2);
				c *= saturate((800-depth)*0.00125);
				return c;
			}
			ENDCG 
		}
	} 
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent+100" "IgnoreProjector"="True" }
		Pass
		{
			Fog { Mode Off } 
			Blend One One
			Cull Off
			ZWrite Off 
			
			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
	
			float4 _Color;
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			
			struct v2f 
			{
				float3 texc : TEXCOORD0;
				float4 pos : POSITION0;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord;
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 c = pow(saturate(1 - abs(i.texc.y)),5) * _Color;
				return c;
			}
			ENDCG 
		}
	} 
}
