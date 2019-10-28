/*  Author: Mark Davis
 *  
 *  This shader provides triplanar texturing.
 *
 *  I highly recommend experimenting with some of the textures from this site:
 *  http://www.filterforge.com/filters/category46-page1.html
 */

Shader "Voxelform/Triplanar 2/Noise Blend/Diffuse Bump" {

    Properties {
		
		_FloorDiffuse ("Floor Diffuse", 2D) = "white" {}		
		_FloorNormal ("Floor Normal", 2D) = "white" {}
		
		_WallDiffuse ("Wall Diffuse", 2D) = "white" {}
		_WallNormal ("Wall Normal", 2D) = "white" {}
		
		_NormalPower ("Normal Power", Float) = 1
		_SpecularPower ("Specular Power", Float) = 1
		_TriplanarFrequency ("Triplanar Frequency", Float) = .2

		_NoiseScale ("Noise Scale", Float) = 1.0
		_NoiseVal1 ("Noise Value 1", Float) = 125777.0
		_NoiseVal2 ("Noise Value 2", Float) = 233.0

    }

    SubShader {
		
		// Prevents chunk seam shimmer.
    	// Leaving this off by default.  If you see seams, turn it back on.
		//Cull Off
		
		ZWrite On
		//Blend One One

		//Tags { "RenderType" = "Opaque" }
		
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
		
		float _NoiseScale;
		float _NoiseVal1;
		float _NoiseVal2;

		float perm(float t)
		{
			float p = frac(_NoiseVal1 / t) - frac(_NoiseVal2 * t);
			return p;
		}

		float3 fade(float3 t)
		{
			return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
		}
		
		float grad(float x, float3 p)
		{
			float3 gv = (x / float3(.2020202011112101, .2200111120202020, .1111220022001210));
			return (dot(gv, p));
		}

		float inoise(float3 p)
		{
			float X = floor(p.x);
			float Y = floor(p.y);
			float Z = floor(p.z);
											
			p -= floor(p);
			
			float3 f = fade(p);
			
			// Hash coordinates for 6 of the 8 cube corners.

			float A = 	perm(X)		+ Y;
			float AA = 	perm(A)		+ Z;
			float AB = 	perm(A + 1) + Z;
			float B = 	perm(X + 1) + Y;
			float BA = 	perm(B) 	+ Z;
			float BB = 	perm(B + 1) + Z;
			
			float ret = lerp(
				    lerp(lerp(grad(perm(AA), 	p),
				              grad(perm(BA), 	p + float3(-1,	 0,	 0)), f.x),
				         lerp(grad(perm(AB), 	p + float3( 0,	-1,	 0)),
				              grad(perm(BB), 	p + float3(-1,	-1,	 0)), f.x), f.y),
				    lerp(lerp(grad(perm(AA + 1),p + float3( 0,	 0,	-1)),
				              grad(perm(BA + 1),p + float3(-1,	 0,	-1)), f.x),
				         lerp(grad(perm(AB + 1),p + float3( 0,	-1,	-1)),
				              grad(perm(BB + 1),p + float3(-1,	-1,	-1)), f.x), f.y),
				    f.z);
			
			return ret;
		}
				
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
			float3 p = IN.worldPos * _NoiseScale;
			
			p += float3(1023, 1230, 2300);
			
			float noise = inoise(p);
			
			p /= 32.0;
			noise += 16 * inoise(p);
			
//			noise += 17.0;
//			noise /= 34.0;

			noise = clamp(noise, 0.0, 1.0);

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
			
			float4 col1 = lerp(tex2D(_WallDiffuse, coord1), tex2D(_FloorDiffuse, coord1), noise);
			float4 col2 = lerp(tex2D(_WallDiffuse, coord2), tex2D(_FloorDiffuse, coord2), noise);
			float4 col3 = lerp(tex2D(_WallDiffuse, coord3), tex2D(_FloorDiffuse, coord3), noise); 

			// Sample bump maps too, and generate bump vectors. 
			float2 bumpFetch1 = lerp(tex2D(_WallNormal, coord1).xy - 0.5, tex2D(_FloorNormal, coord1).xy - 0.5, noise);
			float2 bumpFetch2 = lerp(tex2D(_WallNormal, coord2).xy - 0.5, tex2D(_FloorNormal, coord2).xy - 0.5, noise); 
			float2 bumpFetch3 = lerp(tex2D(_WallNormal, coord3).xy - 0.5, tex2D(_FloorNormal, coord3).xy - 0.5, noise); 

			// (Note: this uses an oversimplified tangent basis.) 
			float3 bump1 = float3(0, bumpFetch1.x, bumpFetch1.y); 
			float3 bump2 = float3(bumpFetch2.y, 0, bumpFetch2.x); 
			float3 bump3 = float3(bumpFetch3.x, bumpFetch3.y, 0);

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
			o.Albedo = blended_color;
			o.Alpha = 1.0;
			o.Specular = _SpecularPower;
			
		}

      ENDCG

    }
	
}

