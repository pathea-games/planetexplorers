/*  Author: Mark Davis
 *  
 *  This shader provides triplanar texturing.
 *
 */

Shader "Voxelform/Triplanar 1/Diffuse" {

    Properties {
		
		_Diffuse ("Diffuse", 2D) = "white" {}
		_TriplanarFrequency ("Triplanar Frequency", Float) = .2
    }

    SubShader {
		
		// Prevents chunk seam shimmer
		// Leaving this off by default.  If you see seams, then turn it back on.
		//Cull Off
		
		Tags { "RenderType" = "Opaque" }
		
		CGPROGRAM

		#pragma target 2.0
		#pragma surface surf Lambert

		struct Input
		{
			float2 uv_Texture;
			float3 worldPos;
			float3 worldNormal;
		};
		
		sampler2D _Diffuse;
		
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
			
			float3 cy = tex2D(_Diffuse, p.zx) * n.y;
			float3 cz = tex2D(_Diffuse, p.xy) * n.z;
			float3 cx = tex2D(_Diffuse, p.yz) * n.x;
			
			o.Albedo = (cy + cz + cx) * .33;
			o.Alpha = 1.0;

		}
		
		ENDCG

    }

    Fallback "Diffuse"

}

