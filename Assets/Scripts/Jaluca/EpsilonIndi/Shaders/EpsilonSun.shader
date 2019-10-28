Shader "SpecialItem/EpsilonSun" 
{
	Properties {
		_SunColor("SunColor", Color) = (1,1,1)
		_SunSize("SunSize", range(0,1)) = 0.3
		_SunBdSize("SunBoarderSize", range(0,1)) = 0.32
		_SunApSize("SunApertureSize", range(0,1)) = 1
	}
	SubShader {
		Tags
		{
			"RenderType" = "Transparent"			
			"Queue" = "Geometry +1"
		}
		Fog { Mode Off }
		Pass
	  	{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	//#pragma target 3.0
			#include "UnityCG.cginc"
			
			float3 _SunColor;
			half _SunSize;
			half _SunBdSize;
			half _SunApSize;
			
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
				//r = [0,1] useful,		(1,1.414] useless				
				half r = length(i.texcoord - 0.5) * 2;
				
				//color from size to board size
				half t1 = saturate((r - _SunSize) / (_SunBdSize - _SunSize));
				half3 sunColor = lerp(float3(8,8,8), _SunColor, pow(t1, 3));
				
				//alpha from board size to aperture size
				half t2 = saturate((r - _SunBdSize) / (_SunApSize - _SunBdSize));
				half sunAlpha = r < _SunBdSize ? 1 : pow((1 - t2),5);
				return half4(sunColor, sunAlpha);
			}
			ENDCG
		}
	}
}