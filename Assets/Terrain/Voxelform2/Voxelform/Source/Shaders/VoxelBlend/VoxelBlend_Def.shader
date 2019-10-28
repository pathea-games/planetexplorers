// Upgrade NOTE: commented out 'float4 unity_ShadowFadeCenterAndType', a built-in variable

Shader "VoxelMultiMaterial_Def" {
Properties {
	_NormSpecPowerTex ("Normal Specular Power Texture", 2D) = "grey"{}
	_DiffuseTex_0 ("Diffuse Texture(0)", 2D) = "white" { } 
    _NormalMap_0 ("Normal Map(0)", 2D) = "bump" { } 
	_DiffuseTex_1 ("Diffuse Texture(1)", 2D) = "white" { } 
    _NormalMap_1 ("Normal Map(1)", 2D) = "bump" { } 
	_DiffuseTex_2 ("Diffuse Texture(2)", 2D) = "white" { } 
    _NormalMap_2 ("Normal Map(2)", 2D) = "bump" { } 
	_DiffuseTex_3 ("Diffuse Texture(3)", 2D) = "white" { } 
    _NormalMap_3 ("Normal Map(3)", 2D) = "bump" { } 
	_DiffuseTex_4 ("Diffuse Texture(4)", 2D) = "white" { } 
    _NormalMap_4 ("Normal Map(4)", 2D) = "bump" { } 
    _DiffuseTex_5 ("Diffuse Texture(5)", 2D) = "white" { } 
    _NormalMap_5 ("Normal Map(5)", 2D) = "bump" { } 

	_TileSize("Tile Size", Float)= 0.25
	_TileSize_1("Tile Size 1", Float)= 0.5
	_Repetition("Repetition Size", Float)= 3
	_Repetition_1("Repetition Size 1", Float)= 10
	_Indent("_Indent", Float)= 0
	_LogCoefficient("LogCoefficient", Float)= 0.82
	_DotCoefficient("DotCoefficient", Float)= 0.71
	_AmbientThres("AmbientThreshold", Color) = (0.12,0.12,0.12, 1)
    }

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200		

	// ---- forward rendering base pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }

		CGPROGRAM		
		#pragma vertex vert_surf
		#pragma fragment frag_surf
		#pragma exclude_renderers xbox360 gles
		#pragma target 3.0			
		#pragma glsl
		#pragma multi_compile_fog
		#pragma multi_compile_fwdbase
		#include "HLSLSupport.cginc"
		#include "UnityShaderVariables.cginc"

		#define UNITY_PASS_FORWARDBASE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"

		#define INTERNAL_DATA
		#define WorldReflectionVector(data,normal) data.worldRefl
		#define WorldNormalVector(data,normal) normal

			#include "VoxelBlend.cginc"
			struct v2f_surf {
			  float4 pos : SV_POSITION;
			  half3 worldNormal : TEXCOORD0;
			  float3 worldPos : TEXCOORD1;
			  float4 custompack0 : TEXCOORD2; // weights
			  float4 custompack1 : TEXCOORD3; // blendMask
			  fixed3 vlight : TEXCOORD4; // ambient/SH/vertexlights
			  SHADOW_COORDS(5)
			  UNITY_FOG_COORDS(6)
			};

			// vertex shader
			v2f_surf vert_surf (appdata_full v) {
			  v2f_surf o;
			  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
			  Input customInputData;
			  vert (v, customInputData);
			  o.custompack0.xyzw = customInputData.weights;
			  o.custompack1.xyzw = customInputData.blendMask;
			  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			  float3 worldPos = mul(_Object2World, v.vertex).xyz;
			  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
			  o.worldPos = worldPos;
			  o.worldNormal = worldNormal;

			  // SH/ambient and vertex lights
			  float3 shlight = ShadeSH9 (float4(worldNormal,1.0));
			  o.vlight = shlight;
			  
			  TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
			  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
			  return o;
			}

			// fragment shader
			fixed4 frag_surf (v2f_surf IN) : SV_Target {
			  // prepare and unpack data
			  Input surfIN;
			  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
			  surfIN.weights = IN.custompack0.xyzw;
			  surfIN.blendMask = IN.custompack1.xyzw;
			  float3 worldPos = IN.worldPos;
			  fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
		      fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
			  surfIN.worldNormal = IN.worldNormal;
			  surfIN.worldPos = worldPos;
			  CustomSurfaceOutput o = (CustomSurfaceOutput)0;
			  o.Albedo = 0.0;
			  o.Emission = 0.0;
			  o.Specular = 0.0;
			  o.Alpha = 0.0;
			  fixed3 normalWorldVertex = fixed3(0,0,1);
			  o.Normal = IN.worldNormal;
			  normalWorldVertex = IN.worldNormal;

			  // call surface function
			  surf (surfIN, o);

			  // compute lighting & shadowing factor
			  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
			  fixed4 c = 0;
			  c.rgb += o.Albedo * IN.vlight;
			  
			  // realtime lighting: call lighting function
			  c += LightingSimpleLambert (o, lightDir, worldViewDir, atten);

			  UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
			  UNITY_OPAQUE_ALPHA(c.a);
			  return c;
			}

		ENDCG
	}

	// ---- forward rendering additive lights pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardAdd" }
		ZWrite Off Blend One One

		CGPROGRAM
		#pragma vertex vert_surf
		#pragma fragment frag_surf
		#pragma exclude_renderers xbox360 gles
		#pragma target 3.0			
		#pragma glsl
		#pragma multi_compile_fog
		#pragma multi_compile_fwdadd_fullshadows
		#include "HLSLSupport.cginc"
		#include "UnityShaderVariables.cginc"

		#define UNITY_PASS_FORWARDADD
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"

		#define INTERNAL_DATA
		#define WorldReflectionVector(data,normal) data.worldRefl
		#define WorldNormalVector(data,normal) normal

			#include "VoxelBlend.cginc"
			struct v2f_surf {
			  float4 pos : SV_POSITION;
			  half3 worldNormal : TEXCOORD0;
			  float3 worldPos : TEXCOORD1;
			  float4 custompack0 : TEXCOORD2; // weights
			  float4 custompack1 : TEXCOORD3; // blendMask
			  SHADOW_COORDS(4)
			  UNITY_FOG_COORDS(5)
			};

			// vertex shader
			v2f_surf vert_surf (appdata_full v) {
			  v2f_surf o;
			  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
			  Input customInputData;
			  vert (v, customInputData);
			  o.custompack0.xyzw = customInputData.weights;
			  o.custompack1.xyzw = customInputData.blendMask;
			  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			  float3 worldPos = mul(_Object2World, v.vertex).xyz;
			  fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);
			  o.worldPos = worldPos;
			  o.worldNormal = worldNormal;

			  TRANSFER_SHADOW(o); // pass shadow coordinates to pixel shader
			  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
			  return o;
			}

			// fragment shader
			fixed4 frag_surf (v2f_surf IN) : SV_Target {
			  // prepare and unpack data
			  Input surfIN;
			  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
			  surfIN.weights = IN.custompack0.xyzw;
			  surfIN.blendMask = IN.custompack1.xyzw;
			  float3 worldPos = IN.worldPos;
			  fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
			  fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
			  surfIN.worldNormal = IN.worldNormal;
			  surfIN.worldPos = worldPos;
			  CustomSurfaceOutput o = (CustomSurfaceOutput)0;
			  o.Albedo = 0.0;
			  o.Emission = 0.0;
			  o.Specular = 0.0;
			  o.Alpha = 0.0;
			  fixed3 normalWorldVertex = fixed3(0,0,1);
			  o.Normal = IN.worldNormal;
			  normalWorldVertex = IN.worldNormal;

			  // call surface function
			  surf (surfIN, o);
			  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
			  fixed4 c = 0;
			  c += LightingSimpleLambert (o, lightDir, worldViewDir, atten);
			  c.a = 0.0;
			  UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
			  UNITY_OPAQUE_ALPHA(c.a);
			  return c;
			}

		ENDCG
	}

	// ---- deferred lighting base geometry pass:
	Pass {
		Name "PREPASS"
		Tags { "LightMode" = "PrePassBase" }

		CGPROGRAM
		#pragma vertex vert_surf
		#pragma fragment frag_surf
		#pragma exclude_renderers xbox360 gles
		#pragma target 3.0			
		#pragma glsl

		#include "HLSLSupport.cginc"
		#include "UnityShaderVariables.cginc"

		#define UNITY_PASS_PREPASSBASE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"

		#define INTERNAL_DATA
		#define WorldReflectionVector(data,normal) data.worldRefl
		#define WorldNormalVector(data,normal) normal

			#include "VoxelBlend.cginc"
			struct v2f_surf {
			  float4 pos : SV_POSITION;
			  half3 worldNormal : TEXCOORD0;
			  float3 worldPos : TEXCOORD1;
			  float4 custompack0 : TEXCOORD2; // weights
			  float4 custompack1 : TEXCOORD3; // blendMask
			};

			// vertex shader
			v2f_surf vert_surf (appdata_full v) {
			  v2f_surf o;
			  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
			  Input customInputData;
			  vert (v, customInputData);
			  o.custompack0.xyzw = customInputData.weights;
			  o.custompack1.xyzw = customInputData.blendMask;
			  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			  o.worldPos = mul(_Object2World, v.vertex).xyz;
			  o.worldNormal = UnityObjectToWorldNormal(v.normal);
			  return o;
			}

			// fragment shader
			fixed4 frag_surf (v2f_surf IN) : SV_Target {
			  // prepare and unpack data
			  Input surfIN;
			  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
			  surfIN.weights = IN.custompack0.xyzw;
			  surfIN.blendMask = IN.custompack1.xyzw;
			  surfIN.worldPos = IN.worldPos;
			  surfIN.worldNormal = IN.worldNormal;
			  CustomSurfaceOutput o = (CustomSurfaceOutput)0;
			  o.Albedo = 0.0;
			  o.Emission = 0.0;
			  o.Specular = 0.0;
			  o.Alpha = 0.0;
			  fixed3 normalWorldVertex = fixed3(0,0,1);
			  //o.Normal = IN.worldNormal;
			  normalWorldVertex = IN.worldNormal;

			  // call surface function
			  surf_pre (surfIN, o);

			  // output normal and specular
			  fixed4 res;
			  res.rgb = o.Normal * 0.5 + 0.5;
			  res.a = o.Specular;
			  return res;
			}

		ENDCG

	}

	// ---- deferred lighting final pass:
	Pass {
		Name "PREPASS"
		Tags { "LightMode" = "PrePassFinal" }
		ZWrite Off

		CGPROGRAM
		#pragma vertex vert_surf
		#pragma fragment frag_surf
		#pragma exclude_renderers xbox360 gles
		#pragma target 3.0			
		#pragma glsl
		#pragma multi_compile_fog
		#pragma multi_compile_prepassfinal
		#include "HLSLSupport.cginc"
		#include "UnityShaderVariables.cginc"

		#define UNITY_PASS_PREPASSFINAL
		#include "UnityCG.cginc"
		#include "Lighting.cginc"

		#define INTERNAL_DATA
		#define WorldReflectionVector(data,normal) data.worldRefl
		#define WorldNormalVector(data,normal) normal

			#include "VoxelBlend.cginc"
			struct v2f_surf {
			  float4 pos : SV_POSITION;
			  half3 worldNormal : TEXCOORD0;
			  float3 worldPos : TEXCOORD1;
			  float4 custompack0 : TEXCOORD2; // weights
			  float4 custompack1 : TEXCOORD3; // blendMask
			  float4 screen : TEXCOORD4;
			  float4 lmap : TEXCOORD5;
			  float3 vlight : TEXCOORD6;
			  UNITY_FOG_COORDS(7)
			};

			// vertex shader
			v2f_surf vert_surf (appdata_full v) {
			  v2f_surf o;
			  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
			  Input customInputData;
			  vert (v, customInputData);
			  o.custompack0.xyzw = customInputData.weights;
			  o.custompack1.xyzw = customInputData.blendMask;
			  o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			  o.worldPos = mul(_Object2World, v.vertex).xyz;
			  o.worldNormal = UnityObjectToWorldNormal(v.normal);
			  o.screen = ComputeScreenPos (o.pos);
			  o.lmap.zw = 0;
			  o.lmap.xy = 0;
			  o.vlight = ShadeSH9 (float4(o.worldNormal,1.0));
			  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
			  return o;
			}
			sampler2D _LightBuffer;
			#if defined (SHADER_API_XBOX360) && defined (UNITY_HDR_ON)
			sampler2D _LightSpecBuffer;
			#endif
			fixed4 unity_Ambient;

			// fragment shader
			fixed4 frag_surf (v2f_surf IN) : SV_Target {
			  // prepare and unpack data
			  Input surfIN;
			  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
			  surfIN.weights = IN.custompack0.xyzw;
			  surfIN.blendMask = IN.custompack1.xyzw;
			  surfIN.worldNormal = IN.worldNormal;
			  surfIN.worldPos = IN.worldPos;
			  fixed3 lightDir = normalize(UnityWorldSpaceLightDir(IN.worldPos));
			  CustomSurfaceOutput o = (CustomSurfaceOutput)0;
			  o.Albedo = 0.0;
			  o.Emission = 0.0;
			  o.Specular = 0.0;
			  o.Alpha = 0.0;
			  fixed3 normalWorldVertex = fixed3(0,0,1);
			  //o.Normal = IN.worldNormal;
			  normalWorldVertex = IN.worldNormal;

			  // call surface function
			  surf_final (surfIN, o);
			  half4 light = tex2Dproj (_LightBuffer, UNITY_PROJ_COORD(IN.screen));
			#if defined (SHADER_API_MOBILE)
			  light = max(light, half4(0.001));
			#endif
			#ifndef UNITY_HDR_ON
			  light = -log2(light);
			#endif
			#if defined (SHADER_API_XBOX360) && defined (UNITY_HDR_ON)
			  light.w = tex2Dproj (_LightSpecBuffer, UNITY_PROJ_COORD(IN.screen)).r;
			#endif
			  light.rgb += IN.vlight;
			  
			  half4 c = LightingSimpleLambert_PrePass (o, light);
			  UNITY_APPLY_FOG(IN.fogCoord, c); // apply fog
			  UNITY_OPAQUE_ALPHA(c.a);
			  return c;
			}

		ENDCG

	}
}

    SubShader 
    {
		Tags { "RenderType" = "Opaque" }
		Fog { Mode Off }
		Cull Off
		
		CGPROGRAM
		// Upgrade NOTE: excluded shader from Xbox360 and OpenGL ES 2.0 because it uses unsized arrays
		#pragma exclude_renderers xbox360 gles
		#pragma target 3.0
		#pragma glsl
		#include "VoxelBlendInc_Fallback.cginc"
		#pragma surface surf SimpleLambert vertex:vert
		
		ENDCG
    }    
    Fallback "Diffuse"
}
	