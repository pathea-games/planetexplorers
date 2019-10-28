Shader "zhouxun/ScreenMask"
{
	Properties
	{
		_MainTex ("Screen Mask", 2D) = "white" {}
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
			Blend Zero SrcColor
			Cull Off
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
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
						
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord;				
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float4 mask0 = tex2D(_MainTex, i.texcoord.xy).xyzw;
				
				float4 retval = 0;
				retval.rgb = lerp(1, mask0.rgb, mask0.a);
				retval.a = 1;
				retval.rgb = lerp(1, retval.rgb, _Intensity);
				return retval;
			}
			ENDCG
		}
	}
}
