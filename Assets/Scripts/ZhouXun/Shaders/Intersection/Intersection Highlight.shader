Shader "zhouxun/Intersection Highlight"
{
	Properties 
	{
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_Brightness ("Brightness", Float) = 1
		_Thickness ("Thickness", Float) = 1
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
	float _Thickness;
	
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
			};
			
			v2f vert(appdata_myvert v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.screenPos = ComputeScreenPos(o.pos);
				return o;
			}
						
			half4 frag( v2f i ) : COLOR
			{
				float depth = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)))) - i.screenPos.w;
				float brt0 = pow(clamp((_Thickness-depth)/_Thickness+0.2,0,1.2),5);
				float brt1 = pow(depth/_Thickness*10,2);
				return _TintColor * min(brt0,brt1) * _Brightness; 
			}
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
