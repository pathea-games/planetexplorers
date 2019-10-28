/*  Author: Mark Davis
 *  
 *  This shader provides psuedo-volumetric texturing, using a 2D texture as a source.
 *  Please experiment with different source textures.  The 3D texturing will look different
 *  than the 2D source, but some elements will still be seen.  Results will vary due to
 *  the number of cycles and unique features of the 2D source.
 *
 *  Note: Fixed mip-mapping "black mirage" issue in v1.1
 *  
 *  I highly recommend experimenting with some of the textures from this site:
 *  http://www.filterforge.com/filters/category46-page1.html
 */

Shader "Voxelform/Solid Textured/Diffuse Bump" {

    Properties {
    
      _Diffuse ("Diffuse", 2D) = "white" {}
	  _Normal ("Normal", 2D) = "white" {}
      _Color ("Main Color", Color) = (1, .8, .1, 1)
      _Tilt ("Tilt", Float) = .15
      _BandsShift ("Bands Shift", Float) = 1.614
      _BandsIntensity ("Bands Intensity", Float) = .75
	  _NormalPower ("Normal Power", Float) = 1
      _SpecularPower ("Specular Power", Float) = 1

    }

    SubShader {
		
		// Prevents chunk seam shimmer
		//Cull Off
		
		Tags { "RenderType" = "Opaque" }
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf SimpleLambert

		sampler2D _Diffuse;
		sampler2D _Normal;

		float _Tilt;
		float _BandsIntensity;
		float _BandsShift;
		float _NormalPower;
		float _SpecularPower;
		

		float3 _Hue;
		float4 _Color;

		struct Input {
			float2 uv_Texture;
			float3 worldPos;
			float3 worldNormal;
			float4 color: COLOR;
		};
				
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

		void surf (Input IN, inout CustomSurfaceOutput o)
		{
			
			float3 p;
			float c = 0;
			float cycleNum = 0;
			float featureCorrection = (_Color.r + _Color.g + _Color.b) * .33;
			
			// The number of cycles isn't important.  Each cycle just adds detail.
			// Remove a few cycles and you'll see a performance boost.
			// To my knowledge, it's not possible to make this dynamic.
			
			float3 blendedBumpVec;

			for (int i = 1; i < 6; ++i)
			{
				cycleNum += 1.0;
				p = IN.worldPos * (9 / (cycleNum * 6));
				
				// Here, we're creating a psuedo-volumetric texture from any source
				// texture you'd like to use.  I think it works well enough, but please
				// experiment and make sure it works for your situation.
				
				float cx = tex2D(_Diffuse, float2(p.x + p.y * _Tilt, p.z)).r;
				float cy = tex2D(_Diffuse, float2(p.y + p.z * _Tilt, p.x)).r;
				float cz = tex2D(_Diffuse, float2(p.z + p.x * _Tilt, p.y)).r;
				
				float2 coord1 = float2(p.x + p.y * _Tilt, p.z);
				float2 coord2 = float2(p.y + p.z * _Tilt, p.x);
				float2 coord3 = float2(p.z + p.x * _Tilt, p.y);

				// Sample bump maps too, and generate bump vectors. 
				float2 bumpFetch1 = tex2D(_Normal, coord1).xy - 0.5;
				float2 bumpFetch2 = tex2D(_Normal, coord2).xy - 0.5; 
				float2 bumpFetch3 = tex2D(_Normal, coord3).xy - 0.5; 

				// (Note: this uses an oversimplified tangent basis.) 
				float3 bump1 = float3(0, bumpFetch1.x, bumpFetch1.y); 
				float3 bump2 = float3(bumpFetch2.y, 0, bumpFetch2.x); 
				float3 bump3 = float3(bumpFetch3.x, bumpFetch3.y, 0);

				blendedBumpVec += bump1.xyz + bump2.xyz + bump3.xyz;

				// As might be expected, lerp provides linear blending... the smooth gradient detail.
				// Smoothstep however, provides the features that really make this look natural.
				// It may be worthwhile to try disabling one or the other, so that you can see the difference.
				
				float features = (lerp(cx, cy, cz) + smoothstep(cx, cy, cz)) * .25;
				if (features < 0) features = featureCorrection;
				c += (features) * .2;
			}
			
			// The bands may provide a natural stratification look.
			// Currently, they are sampled from _PlasmaTex.  If this isn't working for a specific texture,
			// try adding another texture that's more to your liking and then sample the bands from that instead.
			
			// Note: Normals are being used to prevent banding on flat ground.
			
			float yshift = tex2D(_Diffuse, float2((p.x + p.z) * .001, 0)).r * _BandsShift;
			float bandsIntensity = clamp(abs(IN.worldNormal.x) + abs(IN.worldNormal.z), 0, 1) * _BandsIntensity;
			float bands = tex2D(_Diffuse, float2(0, (p.y * .0001) + yshift)).r * bandsIntensity + (1 - bandsIntensity);
			
			blendedBumpVec /= 5.0;

			float4 n = float4(blendedBumpVec.x, blendedBumpVec.y, -blendedBumpVec.z, 1);
			float4 camVec = normalize(n);
			
			float3 N_for_lighting = normalize(IN.worldNormal + (camVec) * -_NormalPower);
			
			o.BumpNormal = N_for_lighting;

			o.Albedo = c * bands * _Color.rgb * 2.0;
			o.Alpha = 1.0;
			o.Specular = _SpecularPower;

		}

      ENDCG

    }

    Fallback "Diffuse"

}

