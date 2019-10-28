Shader "Unlit/Revival"
{
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_SubTex ("Background (RGB), Alpha (A)", 2D) = "white" {}
		_AlphaThreshold ("Alpha threshold", Float) = 0.5
		_Percent ("percent", Float) = 0.5
		_Start ("start", Float) = 0.05
		_End ("end", Float) = 0.9
	}

	SubShader
	{
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Offset -1, -1
			Fog { Mode Off }
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _SubTex;
			float _AlphaThreshold;
			float _Percent;
			float _Start;
			float _End;

			float4 _MainTex_ST;

			struct appdata_t
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				o.texcoord = v.texcoord;
				return o;
			}

			half4 frag (v2f IN) : COLOR
			{
				half4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				half4 bc = tex2D(_SubTex, IN.texcoord) * IN.color;
				half increasement = 0;
				increasement = clamp(-(c.a - _AlphaThreshold) * 10, 0 ,1);
				float pos = _Start + (_End-_Start) * _Percent;
				half increasement2 = 0;
				increasement2 = clamp(-(IN.texcoord.y - pos) * 50, 0 ,1);				
				return c+increasement*increasement2*bc;
			}
			ENDCG
		}
	}
	
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		
		LOD 100
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		ColorMask RGB
		AlphaTest Greater .01
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass
		{
			ColorMaterial AmbientAndDiffuse
			
			SetTexture [_MainTex]
			{
				Combine Texture * Primary
			}
		}
	}
}