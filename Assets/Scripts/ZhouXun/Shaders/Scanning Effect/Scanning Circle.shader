Shader "zhouxun/Scanning Circle"
{
	Properties 
	{
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_Brightness ("Brightness", Float) = 1
		_CircleBrightness ("Circle Brightness", Float) = 1
		_Thickness ("Thickness", Float) = 0.5
		_CircleThickness ("Circle Thickness", Float) = 0.5
		_Radius ("Radius", Float) = 5
	}
	
	CGINCLUDE
	#include "UnityCG.cginc"
	
	struct appdata_myvert
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	sampler2D _CameraDepthTexture;
	
	float4 _TintColor;
	float _Brightness;
	float _CircleBrightness;
	float _Thickness;
	float _CircleThickness;
	float _Radius;
	
	ENDCG
	
	SubShader 
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent"}
		Lod 200
		Pass 
		{
			Zwrite Off
			Cull Off
			Blend One One
			
			CGPROGRAM
			#pragma target 3.0 
			#pragma vertex vert
			#pragma fragment frag
			
			struct v2f 
			{
				float4 pos 			: SV_POSITION;
				float4 screenPos    : TEXCOORD0;
				float4 vert_pos    : TEXCOORD1;
			};
			
			v2f vert(appdata_myvert v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.screenPos = ComputeScreenPos(o.pos);
				o.vert_pos = v.vertex;
				return o;
			}
						
			half4 frag( v2f i ) : COLOR
			{
				float depth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)))) - i.screenPos.w;
				float brt0 = pow(clamp((_Thickness-depth)/_Thickness+0.2,0,1.2),5);
				float brt1 = pow(depth/_Thickness*10,2);
				half4 scan = _TintColor * min(brt0,brt1) * _Brightness;
				half4 circle = _TintColor * saturate((_CircleThickness-abs(length(i.vert_pos.xyz) - _Radius))/_CircleThickness) * _CircleBrightness * _Brightness;
				return scan + circle;
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
