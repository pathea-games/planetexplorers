Shader "zhouxun/ShieldNet"
{
	Properties
	{
		_TintColor1 ("Tint Color 1", Color) = (0,0,0,0.1) 
		_TintColor2 ("Tint Color 2", Color) = (0,0,0,0.1) 
		_DistThreshold ("_DistThreshold", Float) = 0.99 
		_TimeFactor ("Time factor", Float) = 0
	}
	SubShader
	{
		Tags
		{
			"Queue"="Transparent+1" "IgnoreProjector"="True" "RenderType"="Transparent"
		}
		Fog { Mode Off }
		Pass
	  	{ 
	  		Cull Off 
	  		Zwrite off
			Blend One One
		 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		    #pragma target 3.0
			#include "UnityCG.cginc"
	 
	 		float4 _TintColor1;
	 		float4 _TintColor2;
	 		float _DistThreshold;
	 		float _TimeFactor;
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 vertPos : COLOR0;	
			};
	
			struct v2f 
			{
				float3 vertPos : COLOR0;
				float4 pos : POSITION0;
			};
	
			v2f vert (appdata_v v)
			{
				v2f o;
				o.vertPos.rgb = v.vertex.xyz;	
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			} 
			
			half4 frag (v2f i) : COLOR
			{
	            float ver = length(i.vertPos.rgb) - _DistThreshold;
	            float intensity = clamp(pow(ver * 100,20), 0, 3)/3; 
//	            float circle1 = pow((1 - abs(frac((i.vertPos.x + i.vertPos.y + i.vertPos.z)*3) - frac(_TimeFactor)) ) , 50)*2+0.2;
//	            float circle2 = pow((1 - abs(frac((i.vertPos.x + i.vertPos.y - i.vertPos.z)*3) - frac(_TimeFactor)) ) , 50)*2+0.2;
//	            float circle3 = pow((1 - abs(frac((-i.vertPos.x + i.vertPos.y - i.vertPos.z)*3) - frac(_TimeFactor)) ) , 50)*2+0.2;
//	            float circle4 = pow((1 - abs(frac((-i.vertPos.x + i.vertPos.y + i.vertPos.z)*3) - frac(_TimeFactor)) ) , 50)*2+0.2;
				float circle1 = pow((0.5-abs(0.5 - frac(((i.vertPos.x + i.vertPos.y + i.vertPos.z)*10 - _TimeFactor*.5)*2)))*2 , 2)*1+0.1;
	            float circle2 = pow((0.5-abs(0.5 - frac(((i.vertPos.x + i.vertPos.y - i.vertPos.z)*10 - _TimeFactor*.5)*2)))*2 , 2)*1+0.1;
	            float circle3 = pow((0.5-abs(0.5 - frac(((-i.vertPos.x + i.vertPos.y - i.vertPos.z)*10 - _TimeFactor*.5)*2)))*2 , 2)*1+0.1;
	            float circle4 = pow((0.5-abs(0.5 - frac(((-i.vertPos.x + i.vertPos.y + i.vertPos.z)*10 - _TimeFactor*.5)*2)))*2 , 2)*1+0.1;
	            
				return lerp(_TintColor2, _TintColor1,clamp(intensity,0,1)) * intensity * (circle1 + circle2 + circle3 + circle4);
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
