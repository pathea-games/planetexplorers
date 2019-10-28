Shader "wuyiqiu/PEWater/Depth_DEBUG" 
{
	Properties 
	{
		_NormMap ("Normal Map", 2D) = "bump" {}
		_ReflectionTex ("Reflection Tex", 2D) = "black" {}
		_WaveMap ("wave Map", 2D) = "bump" {}
		
		_DepthColor ("Depth Color", Color) = (1,1,1,1)
		_FoamColor ("Foam Color", Color) = (0,0,0,0)
		_ReflectionColor ("Reflection Color", Color) = (1,1,1,1)
		_FresnelColor ("Fresnel Color", Color) = (1,1,1,1)
		
		_Tile ("Main Tile", Range(0.005, 0.05)) = 0.05
		_ReflectionBlend ("Reflection Blend", Range(0.0, 1.0)) = 0.8
		_RefractionAmt  ("Refraction Amount", range (0,1000)) = 200.0
		
		_DensityParams ("(Density, Base Density, Underwater, )", Vector) = (0.02 ,0.1, 0, 0)
		_FoamTex ("Foam texture ", 2D) = "white" {}	
		_FoamParams("Foam (Threshold, tile, Speed, brightness)", Vector) = (12, 0.25, 0.25, 1.5)
		
		_WorldLightDir ("light direction(Only one)", Vector) = (0.0, 0.1, -0.5, 0.0)
		_Shininess ("Shininess", Range (2.0, 1200.0)) = 1200.0	
		_Specular ("Specular", Color) = (1,1,1,1)
		
		_PLEdgeAtten ("Point light edge attenuation", float) = 0.5
		_PLPos1 ("Point light position 1", Vector) = (0.0, 0.1, -0.5, 0.0)
		_PLParam1 ("Point light param 1 (range, Intensity, ,)", vector) = (10, 0, 0.0, 0.0)
		_PLColor1 ("Point light 1 color", Color) = (1,1,1,1)
		
		_PLPos2 ("Point light position 2", Vector) = (0.0, 0.1, -0.5, 0.0)
		_PLParam2 ("Point light param 2 (range, Intensity, ,)", vector) = (10, 0, 0.0, 0.0)
		_PLColor2 ("Point light 2 color", Color) = (1,1,1,1)
	}
	
	CGINCLUDE
	#include "PEWaterInc_DEBUG.cginc"
	
	struct appdata_water
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	sampler2D _GrabTex_Origin : register(s0);
	sampler2D _CameraDepthTexture; 
	float4 _GrabTex_Origin_TexelSize;
	
	sampler2D _NormMap;
	sampler2D _ReflectionTex;
	sampler2D _FoamTex;
	sampler2D _WaveMap;
	
	uniform float _Tile;
	uniform float4 _DepthColor;
	uniform float4 _FoamColor;
	uniform float4 _ReflectionColor;
	uniform float4 _FresnelColor;
	uniform float  _ReflectionBlend;
	uniform float  _RefractionAmt;
	uniform float4 _DensityParams;
	uniform float4 _FoamParams;
	
	uniform float4 _WorldLightDir;
	uniform half3 _Specular;
	uniform float _Shininess;
	
	uniform float _PLEdgeAtten;
	uniform float4 _PLPos1;
	uniform float4 _PLParam1;
	uniform half4 _PLColor1;
	
	uniform float4 _PLPos2;
	uniform float4 _PLParam2;
	uniform half4 _PLColor2;
	ENDCG
	
	SubShader 
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent-109"}
		GrabPass 
		{							
			"_GrabTex_Origin"
		}
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
				
				float2 vel = normalize(float2(i.worldPos.z, -i.worldPos.x)) * 0;
				half3 waveNorm = tex2Dproj(_WaveMap, screenPos).xzy - half3(0.5, 1, 0.5);
				half3 worldNormal = FinalWaterNormal(_NormMap, _Tile*2, i.worldPos.xz, vel, 10) + waveNorm;

				// Calc depth
				float depth = screenPos.w;
				
				// Refraction
				float sink = CalcSink(depth, _DensityParams.x*0.5);
				float4 refract_projpos = RefractionProjectionPos(i.projpos, worldNormal, sink, _RefractionAmt, _GrabTex_Origin_TexelSize.xy*8); 
				float4 refract_grabpos = ComputeGrabScreenPos(refract_projpos);
				float4 viewVector = float4(i.worldPos.xyz - _WorldSpaceCameraPos.xyz, 1);
				float3 viewDir = normalize(viewVector.xyz);
				half vdotn = pow(saturate(dot(viewDir, float3(0,1,0))), _DensityParams.z);
				half vdotl = pow(saturate(dot(viewDir, -_WorldLightDir.xyz)), _DensityParams.z * 4);
				half3 grabcolor = GetGrabColor(_GrabTex_Origin, refract_grabpos).rgb * (1+vdotl*_DensityParams.z+vdotn);
				//grabcolor = clamp(grabcolor, 0, 5);
				half3 refraction = lerp(grabcolor, _DepthColor.rgb, saturate(sink+ _DensityParams.y)).rgb; 
				refraction = lerp(refraction, _DepthColor.rgb, saturate(1-vdotn)).rgb; 
				half4 retval = half4(refraction, saturate(depth*2));
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
				float4 uvgrab = ComputeGrabScreenPos(i.projpos);
				float4 screenPos = ComputeScreenPos(i.projpos);
				float4 viewVector = float4(i.worldPos.xyz - _WorldSpaceCameraPos.xyz, 1);
				float3 viewDir = normalize(viewVector.xyz);
				float viewDist = length(viewVector.xyz);

				float2 vel = normalize(float2(i.worldPos.z, -i.worldPos.x)) * 0;
				half3 waveNorm = tex2Dproj(_WaveMap, screenPos).xzy - half3(0.5, 1, 0.5);
				half3 worldNormal = FinalWaterNormal(_NormMap, _Tile, i.worldPos.xz, vel, 10) + waveNorm;
				worldNormal = normalize(worldNormal);
				
				// Calc depth
				float depth = GetLED(_CameraDepthTexture, screenPos) - screenPos.w;
				return half4(saturate(depth * 0.01).xxx, 1);
				
				// Refraction
				float sink = CalcSink(depth, _DensityParams.x);
				float4 refract_projpos = RefractionProjectionPos(i.projpos, worldNormal, sink, _RefractionAmt, _GrabTex_Origin_TexelSize.xy); 
				float4 refract_grabpos = ComputeGrabScreenPos(refract_projpos);
				float4 refract_screenpos = ComputeScreenPos(refract_projpos);
				half depthr = GetLED(_CameraDepthTexture, refract_screenpos) - screenPos.w;
				float sinkr = CalcSink(depthr, _DensityParams.x);
				half3 grabcolor = GetGrabColor(_GrabTex_Origin, sinkr > sink*0.7 ? refract_grabpos : uvgrab);
				float final_sink = sinkr > sink*0.7 ? sinkr : sink;
				half3 refraction = lerp(grabcolor, _DepthColor.rgb , saturate(final_sink+ _DensityParams.y));
				
				// Dot product for fresnel effect
				half fresnel = pow( 1 - saturate(dot(-viewDir, worldNormal)), 2 );
				half fresnel_nobump = pow( 1 - saturate(dot(-viewDir, half3(0,1,0))), 4 );
				
				// Reflection
				float4 rPos = screenPos.xyzw + float4(worldNormal.xz * 8 * saturate(viewDist*0.01), 0, 0);
				half3 reflection = lerp(tex2Dproj(_ReflectionTex, rPos).rgb, lerp(_ReflectionColor, _FresnelColor, fresnel), _ReflectionBlend).rgb * saturate(_FresnelColor.b * 6 + 0.2);
				
				// foam
				half foam_intens = FoamIntensity(i.worldPos.xz, depth + worldNormal.x, _FoamParams.x, _FoamParams.z);
				half foam_tex = (tex2D(_FoamTex, float2(i.worldPos.x+TIME_FACTOR, i.worldPos.z)*_FoamParams.y).r
				               + tex2D(_FoamTex, float2(-i.worldPos.x+TIME_FACTOR, i.worldPos.z)*_FoamParams.y).r)*0.5;
				refraction = lerp(refraction, _FoamColor.rgb, saturate(foam_tex*foam_intens*_FoamParams.w*max(1-viewDist*0.01, 0.6))); 
				
				// final color
				half refl_rate = saturate(lerp(fresnel_nobump * pow(final_sink, 2), 1, 0.2)) * saturate(viewDist*0.1);
				half3 final_col = lerp(refraction, reflection, refl_rate);
				
				// Directional Light
				half3 nNormal = normalize(worldNormal);
				half3 lightDir = normalize(_WorldLightDir);
				half reflectiveFactor = max(0.0, dot(-viewDir, reflect(lightDir, nNormal))); 
				half3 dl_spec_color = _Specular.rgb * pow(reflectiveFactor, _Shininess)*3*saturate((-lightDir.y+0.05)*20); 
				
				// Point light
				half3 pl_spec_color = (_PLParam1.y * GetSpecularFactorPL(i.worldPos - _PLPos1, nNormal, viewDir, _PLParam1.x, _PLEdgeAtten, _Shininess) * _PLColor1
									+ _PLParam2.y * GetSpecularFactorPL(i.worldPos - _PLPos2, nNormal, viewDir, _PLParam2.x, _PLEdgeAtten, _Shininess) * _PLColor2).rgb;
				
				half4 retval = half4(final_col + dl_spec_color + pl_spec_color, saturate(depth*2));
				UNITY_APPLY_FOG(i.fogCoord, retval);
				return retval; 
			}
			ENDCG
		}
	} 
	FallBack "wuyiqiu/PEWater/Medium"
}
