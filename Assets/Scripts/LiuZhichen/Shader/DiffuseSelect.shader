

Shader "MyShader/DiffuseSelect_??" {

		Properties 
		{
			_TexU("Select U", float) = 0
			_TexV("Select V", float) = 0
			_Color ("Main Color", Color) = (1,1,1,1)
			_MainTex ("Texture", 2D) = "white" { }
			_AlphaCutoff("Alpha cutoff", Range(0,0.9999)) = 0.5
		}

    SubShader {
      Tags { "RenderType" = "Opaque" }
      CGPROGRAM
      #pragma surface surf Lambert
      struct Input {
          float2 uv_MainTex;
      };
      sampler2D _MainTex;
	  float4 _Color;
	
	  float _TexU;
	  float _TexV;
      void surf (Input IN, inout SurfaceOutput o) {
      
			half4 texcol = tex2D( _MainTex, IN.uv_MainTex );
			if(IN.uv_MainTex.x<_TexU && IN.uv_MainTex.y<_TexV)
			{
				texcol *=_Color;
			}
          o.Albedo = texcol;
      }
      ENDCG
    }
		Fallback "VertexLit"
}