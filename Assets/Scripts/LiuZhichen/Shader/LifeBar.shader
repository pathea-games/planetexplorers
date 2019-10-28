Shader "MyShader/LifeBar" {

		Properties 
		{
			_Color ("Main Color", Color) = (1,1,1,1)
			_MainTex ("Texture", 2D) = "white" { }
			_AlphaTex ("Texture", 2D) = "white" { }
			_LifeValue("_LifeValue", Range(0,1)) = 0
			_AlphaCutoff("Alpha cutoff", Range(0.0,1.0)) = 0.0001
		}

		SubShader {
			Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
			Pass {
				//Blend SrcAlpha  OneMinusSrcAlpha
				AlphaTest Greater [_AlphaCutoff]
				Cull back

				Lighting off
				
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_fog_exp2
				#include "UnityCG.cginc"
				
				float4 _Color;
				float _LifeValue;
				sampler2D _MainTex;
				sampler2D _AlphaTex;
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
					half4 texAlpha  = tex2D( _AlphaTex, i.uv );
					half4 col = texcol * _Color;
					clip(_LifeValue - texAlpha.r);
					return col;
				}
				ENDCG
			}
		}
		Fallback "VertexLit"
}