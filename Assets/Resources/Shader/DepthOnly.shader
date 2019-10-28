Shader "Custom/DepthOnly" 
{
	Properties
	{
		_MainTex("Albedo", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"			
			"Queue" = "Transparent-100"
		}
		Fog { Mode Off }
		Pass
	  	{
			Blend One One
			Cull Off
			ZWrite On
			
			CGPROGRAM
			#pragma shader_feature _ALPHATEST_ON 
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			
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
			
			sampler2D _MainTex;
			half _Cutoff;
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord;				
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half aphla = tex2D(_MainTex, i.texcoord.xy).a;

				#if defined(_ALPHATEST_ON)
				clip(aphla - _Cutoff);
				#endif
				
//				clip(aphla - _Cutoff);
				
				return 0;
			}
			ENDCG
		}
	}
}