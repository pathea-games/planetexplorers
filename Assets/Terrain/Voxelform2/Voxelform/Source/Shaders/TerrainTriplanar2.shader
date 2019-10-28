/*  Author: Mark Davis
 *  
 *  This shader provides triplanar texturing.
 *
 *  I highly recommend experimenting with some of the textures from this site:
 *  http://www.filterforge.com/filters/category46-page1.html
 */

Shader "Voxelform/Triplanar 2/Diffuse Normal Specular" {

    Properties {
		
		_WallDiffuse ("Wall Diffuse", 2D) = "white" {}
		_WallNormal ("Wall Normal", 2D) = "white" {}
		_WallSpecular ("Wall Specular", 2D) = "white" {}
		
		_FloorDiffuse ("Floor Diffuse", 2D) = "white" {}		
		_FloorNormal ("Floor Normal", 2D) = "white" {}
		_FloorSpecular ("Floor Specular", 2D) = "white" {}
				
		_SpecularPower ("Specular Power", Float) = 1
		_TriplanarFrequency ("Triplanar Frequency", Float) = .2

    }

    SubShader {
		
		// Prevents chunk seam shimmer
		// Leaving this off by default.  If you see seams, then turn it back on.
		//Cull Off
		
		Tags { "RenderType" = "Opaque" }
		
		CGPROGRAM
		#pragma target 3.0
		#include "UnityCG.cginc"
		#pragma surface surf SimpleLambert

		float _NormalPower;
		float _SpecularPower;
		float _TriplanarFrequency;

		float4 _Rotation;

		sampler2D _WallDiffuse;
		sampler2D _FloorDiffuse;

		sampler2D _WallNormal;
		sampler2D _FloorNormal;
		
		sampler2D _WallSpecular;
		sampler2D _FloorSpecular;
				
		struct CustomSurfaceOutput
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Alpha;
			half3 BumpNormal;
			half Specular;
		};

		half4 LightingSimpleLambert (CustomSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			half NdotL = dot(normalize(s.BumpNormal), normalize(lightDir));
			
			half3 h = normalize (lightDir + normalize(viewDir));
			
			half nh = max (0, dot (s.BumpNormal, h));
			half spec = smoothstep(0, 1.0, pow(nh, 32.0 * s.Specular)) * _SpecularPower;
			
			half4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * NdotL + _LightColor0.rgb * spec) * atten;
			c.a = s.Alpha;

			return c;
		}

		struct Input
		{
			float3 uv_Texture;
			float3 worldPos;
			float3 worldNormal;
		};
		
		void surf (Input IN, inout CustomSurfaceOutput o)
		{
			float3 blend_weights = abs( normalize(IN.worldNormal) );   // Tighten up the blending zone: 
			blend_weights = (blend_weights - 0.2) * 7; 
			blend_weights = max(blend_weights, 0);      // Force weights to sum to 1.0 (very important!) 
			blend_weights /= (blend_weights.x + blend_weights.y + blend_weights.z ).xxx; 

			float4 blended_color;
			float3 blended_bump_vec;
			float blended_spec;
			
			float tex_scale = _TriplanarFrequency;

			float2 coord1 = IN.worldPos.yz * -tex_scale;
			float2 coord2 = IN.worldPos.zx * tex_scale;
			float2 coord3 = IN.worldPos.xy * -tex_scale;
			
			float4 col1 = tex2D(_WallDiffuse, coord1);
			float4 col2 = tex2D(_FloorDiffuse, coord2);
			float4 col3 = tex2D(_WallDiffuse, coord3);

			// Sample bump maps too, and generate bump vectors. 
			float2 bumpFetch1 = tex2D(_WallNormal, coord1).xy - 0.5; 
			float2 bumpFetch2 = tex2D(_FloorNormal, coord2).xy - 0.5; 
			float2 bumpFetch3 = tex2D(_WallNormal, coord3).xy - 0.5; 

			// (Note: this uses an oversimplified tangent basis.) 
			float3 bump1 = float3(0, bumpFetch1.x, bumpFetch1.y); 
			float3 bump2 = float3(bumpFetch2.y, 0, bumpFetch2.x); 
			float3 bump3 = float3(bumpFetch3.x, bumpFetch3.y, 0);

			// Sample bump maps too, and generate bump vectors. 
			float spec1 = tex2D(_WallSpecular, coord1).x; 
			float spec2 = tex2D(_FloorSpecular, coord2).x; 
			float spec3 = tex2D(_WallSpecular, coord3).x;

			blended_spec = 1.0 - (spec1 * blend_weights.x
				+ spec2 * blend_weights.y
				+ spec3 * blend_weights.z);

			// Finally, blend the results of the 3 planar projections. 
			blended_color = col1.xyzw * blend_weights.xxxx + 
			col2.xyzw * blend_weights.yyyy + 
			col3.xyzw * blend_weights.zzzz;

			blended_bump_vec = bump1.xyz * blend_weights.xxx + 
			bump2.xyz * blend_weights.yyy + 
			bump3.xyz * blend_weights.zzz;
			
			float4 n = float4(blended_bump_vec.x, blended_bump_vec.y, -blended_bump_vec.z, 1);
			float4 camVec = normalize(n);
			
			float3 N_for_lighting = normalize(IN.worldNormal + (camVec) * -_NormalPower);
			
			o.BumpNormal = N_for_lighting;
			o.Specular = blended_spec;
			o.Albedo = blended_color;
			o.Alpha = 1.0;

		}

      ENDCG

    }

    Fallback "Diffuse"

}

