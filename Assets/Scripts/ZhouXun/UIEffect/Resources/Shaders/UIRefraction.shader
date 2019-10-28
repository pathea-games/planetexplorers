Shader "Zhouxun/UIRefraction"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "white" {}
		_DistortionTex ("Distortion Mask", 2D) = "bump" {}
		_RandomTex ("Noise", 2D) = "bump" {}
		_SizeSettings ("Sizes (w, h, sw, sh)", Float) = 100
		_Border ("Border (left, top, right, bottom)", Vector) = (0,0,0,0)
		_Intensity ("Intensity", Float) = 1.8
		_Randomness ("Randomness", Float) = 0.005
	}
	SubShader
	{
		LOD 100
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		GrabPass
        {
        	//"_ScreenTex"
        }
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			ColorMask RGB
			AlphaTest Greater .01
			//Blend One One
			
			CGPROGRAM
			#pragma exclude_renderers gles
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
	
			sampler2D _MainTex;
			sampler2D _DistortionTex;
			sampler2D _RandomTex;
			float4 _SizeSettings;
			float4 _Border;
			float _Intensity;
			float _Randomness;
	
			sampler2D _GrabTexture : register(s0);
			float4 _GrabTexture_TexelSize;
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
			struct v2f 
			{
				float2 texc : TEXCOORD0;
				float4 pos : POSITION0;
				float4 projpos : TEXCOORD1;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord;
				o.projpos = o.pos;
				return o;
			}
			
			float trans_coord (float x, float left, float right, float w, float sw)
			{
				float px = x*w;
				if (px < left)
					return px/sw;
				else if (w - px < right)
					return 1 - (w-px)/sw;
				else
					return (left + (px-left)/(w-left-right)*(sw-left-right))/sw;
			}
			
			float2 random (float2 coord)
			{
				return tex2D(_RandomTex, coord*_SizeSettings.xy/64).rg;
			}
			
			float4 GetGrabColor ( sampler2D grabtex, float4 screen_pos )
			{
				return tex2Dproj(grabtex, UNITY_PROJ_COORD(screen_pos));
			}
			
			half4 frag (v2f i) : COLOR
			{
				float2 btexc = i.texc.xy;
				btexc.x = trans_coord(btexc.x, _Border.x, _Border.z, _SizeSettings.x, _SizeSettings.z);
				btexc.y = trans_coord(btexc.y, _Border.w, _Border.y, _SizeSettings.y, _SizeSettings.w);
				half4 c = (tex2D (_DistortionTex, btexc) + (tex2D(_RandomTex, i.texc.xy*100)*2-1) * _Randomness) * 2 - 1;

				fixed4 grab = GetGrabColor(_GrabTexture, ComputeGrabScreenPos(i.projpos + c*_Intensity));
				grab.a = c.a;
				return grab;
			}
			ENDCG 
		}
	} 
	FallBack "Diffuse"
}
