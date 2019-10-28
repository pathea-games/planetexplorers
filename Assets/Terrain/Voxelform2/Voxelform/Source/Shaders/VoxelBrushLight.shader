/*  Author: Mark Davis
 *  
 *  This shader provides triplanar texturing.
 *
 *  I highly recommend experimenting with some of the textures from this site:
 *  http://www.filterforge.com/filters/category46-page1.html
 */

Shader "Voxelform/Voxel Brush Light" {

    Properties {
    
		 _Color ("Main Color", Color) = (1, 0, 0, 1)
    }

    SubShader {
		
		ZWrite Off
		Blend One One

		Tags { "RenderType" = "Opaque" }
		
		CGPROGRAM
		#pragma surface surf Lambert
		
		struct Input {
			float2 uv_Texture;
			float3 worldPos;
			float3 worldNormal;
		};
		
		float4 _Color;
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			o.Albedo = _Color;
			o.Alpha = 1.0;

		}

      ENDCG

    }

    Fallback "Diffuse"

}

