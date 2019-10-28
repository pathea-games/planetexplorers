Shader "Demo/PaintProjector" 
{
		Properties 
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_TintColor  ("Tint Color", Color) = (1,1,1,1)
		_AlphaCeof ("_AlphaCeof", float) = 1
		_Brightness ("_Brightness", Range(0.2, 2)) = 1
		_Depth ("_Depth", float) = 30
		_CenterAndSize ("center pos and size", Vector) = (0, 0, 0, 0)
		//_Direction ("center pos and size", Vector) = (0, -1, 0, 0)asdasdas

	}
	SubShader 
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent" 
		}
		
		LOD 200
		Fog { Mode off }
//		Blend One One
		Blend SrcAlpha OneMinusSrcAlpha
		Cull off
		Zwrite off
		
		Pass
		{
		
			CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members v_pos)
//			#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			  #pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D 	_MainTex;
			half4 		_TintColor;
			float4 		_CenterAndSize;
			float 		_AlphaCoef;
			float 		_Brightness;
			float4		_Direction;
			float 		_Depth;
			
			float4x4 _Projector;

			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
			
			struct v2f 
			{
				float3 v_pos : TEXCOORD0;
				float4 pos : POSITION0;
				float4 uv : TEXCOORD1;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.v_pos.xyz = mul(_Object2World, v.vertex);	
				o.uv = mul (_Projector, v.vertex); 
				return o;
			}
			

			half4 frag (v2f i) : COLOR
			{
				float3 wPos = i.v_pos.xyz;
				float3 center_to_dest = wPos.xyz - _CenterAndSize.xyz;
				float3 nd = normalize(_Direction.xyz);
				float dot_v = dot(center_to_dest, nd);
				float3 center_ = _CenterAndSize.xyz + nd * dot_v;
				
				float dist = length(wPos - center_);
				
				float f =  step (dist, _CenterAndSize.w);
				
				float half_depth = _Depth * 0.5;
				float a = 1 - clamp(abs(dot_v) - _Depth, 0,  half_depth) / half_depth;
				
				half4 col = tex2Dproj (_MainTex, UNITY_PROJ_COORD(i.uv)) * f;
//				return 1;
				return half4 (_TintColor.xyz * _Brightness, col.a * _AlphaCoef * pow(a, 2) );  
				
//				return 1;

//				float3 wPos = i.v_pos.xyz;
////				float dist = max ( abs (wPos.x - _CenterAndSize.x), abs (wPos.z - _CenterAndSize.z));
//				float dist = length(wpos)
//				float f =  step (dist, _CenterAndSize.w);
//			
//				float double_size = 2 * _CenterAndSize.w;
////				half4 color = tex2D (_MainTex, (wPos.xz - _CenterAndSize.xz + _CenterAndSize.ww)/ double_size ) * f  * _TintColor * _Brightness;
//				half4 col = tex2D (_MainTex, (wPos.xz - _CenterAndSize.xz + _CenterAndSize.ww)/ double_size ) * f;
//				return half4 (_TintColor.xyz * _Brightness, col.a * _AlphaCoef);
			}
				
			ENDCG
		}   
	} 
	FallBack "Diffuse"
}