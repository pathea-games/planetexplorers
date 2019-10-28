/*  Author: Mark Davis
 *  
 *  This shader provides triplanar texturing.
 *
 *  I highly recommend experimenting with some of the textures from this site:
 *  http://www.filterforge.com/filters/category46-page1.html
 */

Shader "Voxelform/Triplanar 3/Diffuse" {

    Properties {
    
		_CeilingTexture ("Ceiling", 2D) = "white" {}
		_FloorTexture ("Floor", 2D) = "white" {}
		_WallTexture ("Wall", 2D) = "white" {}
		_TriplanarFrequency ("Triplanar Frequency", Float) = .2
    }

    SubShader {
		
		// Prevents chunk seam shimmer
		// Leaving this off by default.  If you see seams, then turn it back on.
		//Cull Off
		
		Tags { "RenderType" = "Opaque" }
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert
		
		struct Input {
			float2 uv_Texture;
			float3 worldPos;
			float3 worldNormal;
			float4 color: COLOR;
		};
		
		sampler2D _CeilingTexture;
		sampler2D _FloorTexture;
		sampler2D _WallTexture;
				
		float _TriplanarFrequency;
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			
			float3 p = IN.worldPos * _TriplanarFrequency;
			float3 wn = IN.worldNormal;
			float3 n = wn;
			n = normalize(abs(n));
			
			n = (n - 0.275) * 7.0;
			n = max(n, 0.0);
			n /= float3(n.x + n.y + n.z);
						
			float3 cy = (wn.y < 0.0)
				? tex2D(_CeilingTexture, p.zx) * (n.y)
				: tex2D(_FloorTexture, p.zx) * (n.y);
			
			float3 cz = tex2D(_WallTexture, p.xy) * n.z;
			float3 cx = tex2D(_WallTexture, p.yz) * n.x;
			
			o.Albedo = (cy + cz + cx) * .33;
			o.Alpha = 1.0;
			
		}

      ENDCG

    }

    Fallback "Diffuse"

}

