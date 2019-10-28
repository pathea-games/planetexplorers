/*  Author: Mark Davis
 *  
 *  Some of the following code is based on a public sample provided by Ken Perlin.
 *  Source: http://mrl.nyu.edu/~perlin/noise/
 *  
 *  This shader provides true procedural volumetric texturing blended with triplanar texturing,
 *  to provide a mixed mode with some of the best of both worlds.
 *  Terrain elevation controls where the palette texture is sampled vertically (v).
 *  The horizontal direction (u), acts like a color palette (aka ramp). 
 */

Shader "Voxelform/Triplanar 1/Palette Diffuse" {

    Properties {
    
    	_PaletteTexture ("Palette", 2D) = "white" {}
    	_WallTexture ("Wall", 2D) = "white" {}
    	
    	_PaletteFrequency ("Palette Frequency", Float) = 4.0
    	_TriplanarFrequency ("Triplanar Frequency", Float) = .2
    	
		_ScaleX ("Scale X", Float) = 1.0
		_ScaleY ("Scale Y", Float) = 1.0
		
		_NoiseVal1 ("Noise Value 1", Float) = 1257787.0
		_NoiseVal2 ("Noise Value 2", Float) = 233.0

    }

    SubShader {
    
    	// Prevents chunk seam shimmer.
    	// Leaving this off by default.  If you see seams, turn it back on.
		Cull Off

		Tags { "RenderType" = "Opaque" }
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert

		struct Input {
			float2 uv_Texture;
			float3 worldPos;
			float3 worldNormal;
		};
		
		sampler2D _PaletteTexture;
		sampler2D _WallTexture;
		
		float _PaletteFrequency;
		float _TriplanarFrequency;
		
		float _SolidWallWeight;
		
		float _ScaleX;
		float _ScaleY;

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
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			float3 p = IN.worldPos * _PaletteFrequency;
			
			p.x -= 1.0;
			p.z -= 1.0;
				
			float noise = inoise(p);
			
			p *= .25;
			noise += 4.0 * inoise(p);
			
			p *= .125;
			noise += 16.0 * inoise(p);
			
			p = normalize(p);
			
			// For fun, try playing with IN.color values.
			// Out of the box, the vertex colors contain voxel type information.
			
			// Currently p.y (terrain elevation) controls where the palette texture is sampled vertically (v).
			// The horizontal direction (u), acts like a color palette (aka ramp).
			// Note: There's nothing special about the math here.  Change it however you like.
			float2 uv = float2((lerp(p.z, p.x, noise) + lerp(p.y, p.x, noise)) * .01, p.y * 3.0);//  IN.color.r);
			
			uv.x *= _ScaleX;
			uv.y *= _ScaleY;

			float4 detail = tex2D(_PaletteTexture, uv) * .25;
			
			// Setup and adjust normals for better blending.
			p = IN.worldPos * _TriplanarFrequency;
			float3 wn = IN.worldNormal;
			float3 n = normalize(abs(wn));
			
			n = (n - 0.275) * 7.0;
			n = max(n, 0.0);
			n /= float3(n.x + n.y + n.z);
			
			// Calculate weighting for palette texture based on normals.
			float solidWeight = (wn.y < 0.0)
				? n.y
				: n.y;
			
			solidWeight += (n.x + n.z);
			
			solidWeight = clamp(solidWeight, 0.0, 1.0);

			// Calculate triplanar texturing.
			float3 cy = (wn.y < 0.0)
				? tex2D(_WallTexture, p.zx) * (n.y)
				: tex2D(_WallTexture, p.zx) * (n.y);
			
			float3 cz = tex2D(_WallTexture, p.xy) * n.z;
			float3 cx = tex2D(_WallTexture, p.yz) * n.x;
			
			// Blend Mode: Multiply
			float3 c = (cy + cz + cx);
			o.Albedo = (c * (detail * solidWeight));
			o.Alpha = 1.0;
			
		}

      ENDCG

    }

    Fallback "Diffuse"

}

