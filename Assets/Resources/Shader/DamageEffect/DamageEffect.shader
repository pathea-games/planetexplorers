Shader "zhouxun/DamageEffect___"
{
	Properties
	{
		_IntensityMask ("Intensity Mask", 2D) = "white" {}
		_Color3 ("Color Main", Color) = (1,1,1,1)
		_Color1 ("Color 1", Color) = (1,1,1,1)
		_Color2 ("Color 2", Color) = (1,1,1,1)
		_Falloff ("Falloff", Float) = 1
		_Intensity ("Intensity", Float) = 1
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		Fog { Mode Off }
		Pass
	  	{
			Blend One One
			Cull Off
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			sampler2D _IntensityMask;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float _Falloff;
			float _Intensity;
			
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
			
			struct v2f 
			{
				float4 pos : POSITION0;
				float2 texcoord : TEXCOORD0;
			};
			
			#define  ONEPX     1.0/32.0
			#define  HALFPX    0.5/32.0
						
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord;				
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float mask0 = tex2D(_IntensityMask, float2(i.texcoord.x, i.texcoord.y)).r;
				float mask1 = tex2D(_IntensityMask, float2(1-i.texcoord.x, i.texcoord.y)).r;
				float mask2 = tex2D(_IntensityMask, float2(1-i.texcoord.x, 1-i.texcoord.y)).r;
				float mask3 = tex2D(_IntensityMask, float2(i.texcoord.x, 1-i.texcoord.y)).r;
				
				float weight0 = sin(_Time.y*1.5)*0.5 + 0.5;
				float weight1 = cos(_Time.y*1.5)*0.5 + 0.5;
				float weight2 = -sin(_Time.y*1.5)*0.5 + 0.5;
				float weight3 = -cos(_Time.y*1.5)*0.5 + 0.5;

				float intens0 = (mask0*weight0 + mask1*weight1) / (weight0+weight1);
				float intens1 = (mask2*weight2 + mask3*weight3) / (weight2+weight3);
				float fallen_intens0 = pow(intens0, _Falloff);
				float fallen_intens1 = pow(intens1, _Falloff);
				
				return (fallen_intens0*_Color1 + fallen_intens1*_Color2)*_Color3 * _Intensity;
			}
			ENDCG
		}
	}
}
