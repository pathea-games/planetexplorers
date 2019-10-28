Shader "VoxelMultiMaterial" {
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

			CGPROGRAM
			
			// Upgrade NOTE: excluded shader from Xbox360 and OpenGL ES 2.0 because it uses unsized arrays
			#pragma exclude_renderers xbox360 gles
			#pragma target 3.0			
			#pragma glsl
			#include "VoxelBlend.cginc"
			
			#pragma surface surf SimpleLambert vertex:vert fullforwardshadows
					
			ENDCG
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
