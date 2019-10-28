Shader "zhouxun/Bullet/Bullet Trail"
{
	Properties
	{
		_Width ("Width", Float) = 10
		_MaxTexWidth ("Max Width in texcoord", Float) = 0.1
		_Intensity ("Intensity", Float) = 8
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
			
			float _Width;
			float _MaxTexWidth;
			float _Intensity;
			
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
				float3 wpos : TEXCOORD1;
				float4 color : COLOR;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord.xy;
				o.wpos = mul(_Object2World, v.vertex).xyz;
				o.color.rgba = v.color.rgba;	
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float dist = length(i.wpos.xyz - _WorldSpaceCameraPos.xyz);
				float width = min(dist * 0.005 * _Width, _MaxTexWidth);
				float width_intens = saturate((width - abs(i.texc.y - 0.5f))/width);
				width_intens = pow(width_intens, 2);
				return width_intens * i.color * _Intensity * saturate(2 - log10(dist));
			}
			ENDCG
		}
	}
}
