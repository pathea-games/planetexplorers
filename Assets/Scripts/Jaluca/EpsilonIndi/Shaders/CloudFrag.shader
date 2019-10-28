Shader "SpecialItem/CloudFrag" 
{
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CloudCut ("CloudAlphaCut", float) = 1	//0~1
		_CloudFade ("CloudAlphaFade", float) = 1
		_EdgeCut ("EdgeAlphaCut", float) = 7
		_CloudDarkness ("CloudDarkness", range(0, 1)) = 0
		_SunDirect ("SunDirect", Vector) = (0,0,0,0)
	}
	SubShader {
		Tags
		{
		"RenderType" = "Transparent"
		"Queue"="Transparent+101"
		}
		BLEND SrcAlpha OneMinusSrcAlpha
		ZWRITE OFF
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert
		#pragma vertex vert
		#pragma glsl
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		half _CloudCut;
		half _CloudFade;
		float _EdgeCut;
		float4 _SunDirect;
		half _CloudDarkness;

		struct Input {
			float2 uv_MainTex;
			float3 world_pos;
			float3 world_normal;
			float3 viewDir;			
		};
		void vert (inout appdata_base v, out Input o)
		{
			o.uv_MainTex = v.texcoord.xy;
		    o.world_pos = v.vertex;
		    o.world_normal = normalize(mul(_Object2World, float4(v.normal.xyz,0)).xyz);
		    o.viewDir = ObjSpaceViewDir(v.vertex);
		}		
		void surf (Input IN, inout SurfaceOutput o) {
			float3 mainColor = tex2D(_MainTex, IN.uv_MainTex);
			
			//cut clouds at planet edge			
			half edgeAlpha = min(1.0f, dot(normalize(IN.viewDir), IN.world_normal) * _EdgeCut);
			edgeAlpha = pow(edgeAlpha, 5);
			
			//darkness back of sun
			half dark = dot(_SunDirect, -IN.world_normal);
			dark = max(0, 0.75 * dark * dark * dark - 2.25 * dark * dark + 2.35 * dark + 0.15);
			dark = lerp(_CloudDarkness, 1, dark);
			dark = 1;
			
			//cloud alpha
			half alp = mainColor.x;
			alp = alp > _CloudCut ? 1 : 1 - min(1, (_CloudCut - alp) * _CloudFade);
			
			//end			
			o.Alpha = alp * edgeAlpha;					
			o.Albedo = sqrt(mainColor * o.Alpha) * dark;
			o.Alpha = 0;
		}
	
		ENDCG
	}  
	FallBack "Diffuse"	
}