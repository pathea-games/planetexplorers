Shader "SpecialItem/CloudFragTemp"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_CloudCut ("CloudAlphaCut", float) = 1	//0~1
		_CloudFade ("CloudAlphaFade", float) = 1
		_EdgeCut ("EdgeAlphaCut", float) = 7
		_CloudDarkness ("CloudDarkness", range(0, 1)) = 0
		_SunDirect ("SunDirect", Vector) = (0,0,0,0)
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue"="Transparent-12"
		}
		pass
		{
			BLEND SrcAlpha OneMinusSrcAlpha
			ZWRITE OFF			
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct Input members uv_MainTex)
#pragma exclude_renderers d3d11 xbox360
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag		
			//#pragma glsl
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			half _CloudCut;
			half _CloudFade;
			float _EdgeCut;
			float4 _SunDirect;
			half _CloudDarkness;

			struct appdata_v
			{
				float4 vertex : POSITION;				
				float2 texcoord : TEXCOORD0;
				float3 normal : NORMAL;			
			};
			
			struct Input
			{
				float2 texcoord : TEXCOORD0;
				float4 world_pos : POSITION0;
				float3 world_normal : NORMAL;
				float3 viewDir : NORMAL1;
			};
			
			Input vert (appdata_v v)
			{
				Input o;	
							
			    o.world_pos = mul(UNITY_MATRIX_MVP, v.vertex);
			    o.texcoord = v.texcoord;
			    o.world_normal = normalize(mul(_Object2World, float4(v.normal.xyz,0)).xyz);
			    o.viewDir = normalize(mul(_Object2World, ObjSpaceViewDir(v.vertex)).xyz);
			    return o;
			}
			half4 frag (Input IN) : COLOR
			{
				float3 mainColor = tex2D(_MainTex, IN.texcoord);
				
				//cut clouds at planet edge
				half edgeAlpha = min(1.0f, dot(IN.viewDir, IN.world_normal) * _EdgeCut);
				edgeAlpha = pow(edgeAlpha, 5);
				
				//darkness back of sun
				half dark = dot(_SunDirect, -IN.world_normal);
				dark = max(0, 0.75 * dark * dark * dark - 2.25 * dark * dark + 2.35 * dark + 0.15);
				dark = lerp(_CloudDarkness, 1, dark);				
				
				//cloud alpha
				half alp = mainColor.x;
				alp = alp > _CloudCut ? 1 : 1 - min(1, (_CloudCut - alp) * _CloudFade);
				
				//end
				half alpha = alp * edgeAlpha;
				half3 albedo = sqrt(mainColor * alpha) * dark;
								
				return half4(albedo.xyz, alpha);
			}
			ENDCG
		}
	}  
	FallBack "Diffuse"	
}