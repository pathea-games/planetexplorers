Shader "zhouxun/Particle Add Ex"
{
	Properties
	{
		_MainTex ("Main tex", 2D) = "black" {}
		_Color ("Tint Color", Color) = (1,1,1,1)
		_Intensity ("Intensity", Float) = 1
		_Power ("Power", Float) = 1
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
			
			sampler2D _MainTex;
			float4 _Color;
			float _Intensity;
			float _Power;
			
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};
			
			struct v2f 
			{
				float4 pos : POSITION0;
				float2 texc : TEXCOORD0;
				float4 color : COLOR;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord.xy;
				o.color.rgba = v.color.rgba;	
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 tex = tex2D(_MainTex, i.texc.xy).rgba;

				return pow(tex, _Power) * _Color * _Intensity;
			}
			ENDCG
		}
	}
}
