Shader "Custom/PointShader" {
	Properties
	{
		_MainTexture("Main Texture", 2D) = "white" {}
	}
 	SubShader {
 		Pass {
 			Blend SrcAlpha OneMinusSrcAlpha
 			Cull Off
 			CGPROGRAM
 			#pragma vertex vert
 			#pragma fragment frag
 			#include "UnityCG.cginc"
 			
			sampler2D _MainTexture;

 			struct VertInput
 			{
 				float4 center: POSITION;
 				float4 normal: NORMAL;
 				float4 color: COLOR;
 				float4 texCoord : TEXCOORD0;
 			};

 			struct VertOutput
 			{
 				float4 position: SV_POSITION;
 				float4 color: COLOR;
 				float psize: PSIZE;
 				//float4 pos;
 			};

 			void vert(VertInput input, out VertOutput output)
 			{
 				// output.pos = input.texCoord;//mul(_Object2World, input.center);
 				output.position = mul(UNITY_MATRIX_MVP, input.center);
				output.color = input.color;
                output.psize = 2;
 			}

 			float4 frag(VertOutput input) : COLOR0 { return input.color; }
 			ENDCG
 		}

 	}

 }

