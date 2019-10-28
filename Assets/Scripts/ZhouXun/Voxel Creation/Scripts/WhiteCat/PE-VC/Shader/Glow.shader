Shader "White Cat/Glow"
{
	Properties
	{
		_Intensity("Intensity", Float) = 100
		_Color("Color", Color) = (1,1,1,1)
	}

		SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		Fog{ Mode Off }

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float _Intensity;
			float4 _Color;

			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : POSITION0;
				float2 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};

			v2f vert(appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texcoord = v.texcoord;
				o.color.rgba = v.color.rgba;
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				i.color.rgb = i.color.rgb * _Intensity * _Color.rgb;
				return i.color;
			}

			ENDCG
		}
	}
}