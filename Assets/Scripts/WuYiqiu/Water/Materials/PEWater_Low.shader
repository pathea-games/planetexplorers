Shader "wuyiqiu/PEWater/Low" 
{
	Properties 
	{
		_NormMap ("Normal Map", 2D) = "bump" {}
		_ReflectionTex ("Reflection Tex", 2D) = "black" {}//for unity5 { TexGen ObjectLinear }
		
		_DepthColor ("Base Color", Color) = (1,1,1,1)
		_FoamColor ("Reserved Parameter (Useless)", Color) = (0,0,0,0)
		_ReflectionColor ("Reflection Color", Color) = (1,1,1,1)
		_FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
		
		_Tile ("Main Tile", Range(0.005, 0.025)) = 0.25
		_ReflectionBlend ("Reflection Blend", Range(0.0, 1.0)) = 0.8
		_RefractionAmt ("Reserved Parameter (Useless)", range (0,1000)) = 200.0
		
		_DensityParams ("(, Base Density, , )", Vector) = (0.02 ,0.1, 0, 0)
		_FoamTex ("Reserved Texture (Useless)", 2D) = "white" {}	
		_FoamParams("Reserved Parameter (Useless)", Vector) = (12, 0.25, 0.25, 1.5)
		
		_WorldLightDir ("light direction(Only one)", Vector) = (0.0, 0.1, -0.5, 0.0)
		_Shininess ("Shininess", Range (2.0, 1200.0)) = 1200.0	
		_Specular ("Specular", Color) = (1,1,1,1)
	}
	
	CGINCLUDE
	#include "PEWaterInc.cginc"
	
	struct appdata_water
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	sampler2D _NormMap;
	sampler2D _ReflectionTex;
	
	uniform float _Tile;
	uniform float4 _DepthColor;
	uniform float4 _ReflectionColor;
	uniform float4 _FresnelColor;
	uniform float  _ReflectionBlend;
	uniform float4 _DensityParams;
	
	uniform float4 _WorldLightDir;
	uniform half3 _Specular;
	uniform float _Shininess;
	ENDCG
	
	SubShader 
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent-109"}
		Lod 200
		Pass 
		{
			Name "Underwater"
			ZWrite On
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma target 3.0 
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			struct v2f 
			{
				float4 pos : SV_POSITION;
				UNITY_FOG_COORDS(3)
			};
			
			v2f vert(appdata_water v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
						
			half4 frag( v2f i ) : COLOR
			{
				half4 retval = half4(_DepthColor.rgb, _DensityParams.y * 8);
				UNITY_APPLY_FOG(i.fogCoord, retval);
				return retval; 
			}
			ENDCG
		}
		Pass 
		{
			Name "Water"
			Zwrite Off
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma target 3.0 
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			struct v2f 
			{
				float4 pos 			: SV_POSITION;
				float4 projpos      : TEXCOORD1;
				float3 worldPos		: TEXCOORD2; 
				UNITY_FOG_COORDS(3)
			};
			
			v2f vert(appdata_water v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.projpos = o.pos;
				o.worldPos = mul(_Object2World,(v.vertex)).xyz;
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
						
			half4 frag( v2f i ) : COLOR
			{
				float4 screenPos = ComputeScreenPos(i.projpos);
				float4 viewVector = float4(i.worldPos.xyz - _WorldSpaceCameraPos.xyz, 1);
				float3 viewDir = normalize(viewVector.xyz);
				float viewDist = length(viewVector.xyz);

				float2 vel = normalize(float2(i.worldPos.z, -i.worldPos.x)) * 0;
				half3 worldNormal = FinalWaterNormal(_NormMap, _Tile, i.worldPos.xz, vel, 10);
				
				half3 dcol = _DepthColor.rgb;
				
				// Dot product for fresnel effect
				half fresnel = pow( 1 - saturate(dot(-viewDir, worldNormal)), 2 );
				half fresnel_nobump = pow( 1 - saturate(dot(-viewDir, half3(0,1,0))), 4 );
				
				// Reflection
				float4 rPos = screenPos.xyzw + float4(worldNormal.xz * 8 * saturate(viewDist*0.01), 0, 0);
				half3 reflection = lerp(tex2Dproj(_ReflectionTex, rPos).rgb, lerp(_ReflectionColor, _FresnelColor, fresnel), _ReflectionBlend).rgb * (_FresnelColor.b * 1.5 + 0.2);
				
				// final color
				half refl_rate = saturate(lerp(fresnel_nobump, 1, 0.2)) * saturate(viewDist*0.1);
				half3 final_col = lerp(dcol, reflection, refl_rate);
				
				// Directional Light
				half3 nNormal = normalize(worldNormal);
				half3 lightDir = normalize(_WorldLightDir);
				half reflectiveFactor = max(0.0, dot(-viewDir, reflect(lightDir, nNormal))); 
				half3 dl_spec_color = _Specular.rgb * pow(reflectiveFactor, _Shininess); 
				
				half4 retval = half4(final_col + dl_spec_color, _DensityParams.y * 8);
				UNITY_APPLY_FOG(i.fogCoord, retval);
				return retval; 
			}
			ENDCG
		}
	} 
	FallBack "Transparent/Specular"
}
