/* This shader requires Unity Pro - if you are receiving an error
that "No subshaders can run on this card", or if this shader is pink and
doesn't present slots for the textures and properties, you will need to use a different shader
that is compatible with your version of Unity 
*/

Shader "River Shader Depth" 
{
	Properties 
	{
		_BumpAmt  ("Distortion", range (0,128)) = 10.0
		_BumpMap ("Bumpmap (RGB)", 2D) = "bump" {}
		_Tint("Tint Color", Color) = (1.0,1.0,1.0,1.0) 
		_InvFadeParemeter ("Auto blend parameter (Edge, Shore, Distance scale)", Vector) = (0.21 ,0.43, 0.5, 1.0)
	 	_Foam ("Foam (intensity, cutoff)", Vector) = (0.23, 0.07, 0.0, 0.0)
		_ShoreTex ("Shore & Foam texture ", 2D) = "black" {}	
	}
	
	SubShader 
	{
		Tags {"RenderType" = "RiverWater" "Queue" = "Transparent-109" }
		Lod 200
		GrabPass 
		{							
			Name "BASE"
			Tags { "LightMode" = "Always" }
		}
		
		Pass 
		{
			Name "BASE"
			Tags { "LightMode" = "Always" }
			//Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Back
			
		CGPROGRAM
		#pragma exclude_renderers gles 
		#pragma vertex vert
		#pragma fragment frag
		#pragma fragmentoption ARB_precision_hint_fastest 
		#pragma fragmentoption ARB_fog_exp2
		#include "UnityCG.cginc"

		
		uniform float _BumpAmt;
		uniform float4 _Tint; 
		uniform float4 _InvFadeParemeter;
		uniform float4 _Foam;
		float4 _GrabTexture_TexelSize;
		sampler2D _GrabTexture : register(s0);
		sampler2D _BumpMap : register(s1);
		sampler2D _ShoreTex;
		sampler2D _CameraDepthTexture;

		struct v2f 
		{
			float4 uvgrab : TEXCOORD0;
			float2 uvbump : TEXCOORD1;
			float4 pos : SV_POSITION;
			float4 screenPos : TEXCOORD3;	
		};

		v2f vert(appdata_full v)
		{ 
			v2f o;
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
#else
			float scale = 1.0f;
#endif	
			o.screenPos = ComputeScreenPos(o.pos); 
			o.uvbump = TRANSFORM_UV(1);
			o.uvgrab.xy = ( float2( o.pos.x, o.pos.y*scale ) + o.pos.w ) * 0.5;
			o.uvgrab.zw = o.pos.zw;
			return o;
		}

		half4 frag( v2f i ) : COLOR
		{
			half2 bump = tex2D( _BumpMap, i.uvbump + _Time * 0.5f).rg * 2 - 1;
			float2 offset = bump * _BumpAmt;

			offset *= _GrabTexture_TexelSize.xy;
 
			i.uvgrab.xy = offset * i.uvgrab.z + i.uvgrab.xy; 
//			if (LinearEyeDepth(refrFix) < i.screenPos.z) 
//				rtRefractions = rtRefractionsNoDistort;	

			half4 edgeBlendFactors = half4(1.0, 0.0, 0.0, 0.0);
			half depth = UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenPos)));
			depth = LinearEyeDepth(depth); 
			edgeBlendFactors = saturate(_InvFadeParemeter * (depth-i.screenPos.w));		
			//edgeBlendFactors.y = 1.0-edgeBlendFactors.y; //   In water shader, y to control foam

			half4 col = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uvgrab)); 
			// handle foam 
			//half4 foam = (tex2D(_ShoreTex, i.uvgrab.xy*0.0) * tex2D(_ShoreTex,i.uvgrab.yx*0.2)) - 0.125; 
			half4 foam = (tex2D(_ShoreTex,i.uvgrab.yx*0.2)) - 0.125;
			col.rgb += foam.rgb * _Foam.x * saturate(edgeBlendFactors.y - _Foam.y);  
			col = col*(1-edgeBlendFactors.x) + edgeBlendFactors.x*_Tint;
			clip(800 - depth);
			return col;
		}

		ENDCG
			SetTexture [_GrabTexture] {}	// Texture we grabbed in the pass above
			SetTexture [_BumpMap] {}		// Perturbation bumpmap
			SetTexture [_ShoreTex] {}		// _ShoreTex
		}
	}
	SubShader 
	{
		Tags {"RenderType" = "RiverWater" "Queue" = "Transparent-109" }
	
		Lod 100
		ColorMask RGB
		
		Pass {
			Fog {Mode Off}
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest LEqual
			ZWrite Off
			Color (0.098,0.2196,0.3647,0.8863)
		}
	}	
}
