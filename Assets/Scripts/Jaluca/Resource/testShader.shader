Shader "AJaluca/Test" 
{
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}		
	}
	SubShader {
		Tags
		{
		"RenderType" = "Transparent"
		"Queue"="Transparent"
		}
		BLEND SrcAlpha OneMinusSrcAlpha		
		
		CGPROGRAM
		#pragma surface surf Lambert
		#pragma vertex vert

		sampler2D _MainTex;		

		struct Input {
			float2 uv_MainTex;			
		};
		void vert (inout appdata_base v, out Input o)
		{
			o.uv_MainTex = v.texcoord.xy;		    
		}		
		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = tex2D(_MainTex, IN.uv_MainTex);			 
			o.Alpha = 0.5;
		}	
		ENDCG
	}  
	FallBack "Diffuse"	
}