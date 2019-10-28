Shader "zhouxun/EmptyFragment"
{
	Properties
	{
	
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
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord;				
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				return 1;
			}
			ENDCG
		}
	}
}
