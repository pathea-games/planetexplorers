Shader "MyShader/Diffuse??" {

		Properties 
		{
			//_Color ("Main Color", Color) = (1,1,1,0.5)
			_MainTex ("Texture", 2D) = "white" { }
		}
		SubShader {
			Pass {
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_fog_exp2
				#include "UnityCG.cginc"
				
				float4 _Color;
				sampler2D _MainTex;
				
				struct v2f 
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};
				
				v2f vert (appdata_base v)
				{
					v2f o;
					o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
					o.uv = TRANSFORM_UV(0);
					return o;
				}
				
				half4 frag (v2f i) : COLOR
				{
					half4 texcol = tex2D( _MainTex, i.uv );					
					texcol = texcol.a > 0 ? texcol : texcol * _Color;
					return texcol;
				}
				ENDCG
			}
		}
		Fallback "VertexLit"
}