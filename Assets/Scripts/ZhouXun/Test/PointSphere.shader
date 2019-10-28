Shader "Custom/SphereShader" {
	SubShader {
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

//			float4 _ProjectionParams :
//				x is 1.0 or -1.0, negative if currently rendering with a flipped projection matrix
//				y is camera's near plane
//				z is camera's far plane
//				w is 1/FarPlane. 
									
//			float4 _ScreenParams :
//				x is current render target width in pixels
//				y is current render target height in pixels
//				z is 1.0 + 1.0/width
//				w is 1.0 + 1.0/height 
			
			struct VertInput
			{
				float3 center: POSITION;
				float4 color: COLOR;
				float radius: TEXCOORD0;
			};
			
			struct VertOutput
			{
				float4 position: SV_POSITION;
				float4 color: COLOR;
				float psize: PSIZE;
				float4x4 MT_IT: TEXCOORD0;
			};
			
			void vert(VertInput input, out VertOutput output)
			{
				const float4 diag = { 1, 1, 1, -1 };
			
				output.color = input.color;
				
				float4 hpos = float4(input.center.xyz, 1.0);  // Homogeneous vertex position
			    float4 mvp_r1 = UNITY_MATRIX_MVP[0];
			    float4 mvp_r2 = UNITY_MATRIX_MVP[1];
			    float4 mvp_r4 = UNITY_MATRIX_MVP[3];
			    
			    // Optimized multiplication of the modelview-projection matrix with the variance matrix
			    float4 r1, r2, r4;
			    r1.xyz = mvp_r1.xyz * input.radius;
			    r1.w = dot(hpos, mvp_r1);
			    r2.xyz = mvp_r2.xyz * input.radius;
			    r2.w = dot(hpos, mvp_r2);
			    r4.xyz = mvp_r4.xyz * input.radius;
			    r4.w = dot(hpos, mvp_r4);
			    
			    float4 r1D = r1 * diag;
			    float4 r2D = r2 * diag;
			    float4 r4D = r4 * diag;
			
			    // Solve the quadratic equation
			    float a = dot(r4D, r4);
			
			    float2 b, c, bbox;
			    
			    b.x = dot(r1D, r4);     // -b_x / 2    
			    b.y = dot(r2D, r4);     // -b_y / 2
			    c.x = dot(r1D, r1);
			    c.y = dot(r2D, r2);
			
			    float4 eqn = float4(b, c) / a;    // (-b / 2a), (c / a)
			
			    output.position = float4(eqn.x, eqn.y, 0, 1);    // bbox center = -b / 2a    
			    
			    bbox = sqrt(eqn.xy*eqn.xy - eqn.zw) * _ScreenParams.xy;
			    output.psize = max(bbox.x, bbox.y);
			
			    // Calculate inverse transpose of ModelView * Variance
			    float inv_radius = 1.0 / input.radius;  
			    for (int i = 0; i < 3; i++)
			        output.MT_IT[i] = UNITY_MATRIX_IT_MV[i] * inv_radius;
			
			    output.MT_IT[3].xyz = (UNITY_MATRIX_IT_MV[3].xyz - input.center) * inv_radius;
			    output.MT_IT[3].w = 1;
			}
			
			struct FragInput
			{
			    float4 color: COLOR;
			    float2 wpos: WPOS;
			    float4x4 MT_IT: TEXCOORD0;
			};
			
			struct FragOutput
			{
				float4 color: COLOR;
				//float depth: DEPTH;
			};
			
			void frag(FragInput input, out FragOutput output)
			{
				output.color = input.color;
			}
			ENDCG
		}
	}
}

