Shader "Custom/FootprintDecal" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Cutoff ("Alpha cutoff", Range(0,1)) = 0.05
	}
	
	
	SubShader {
		Tags { "RenderType"="Opaque"  "Queue" = "Transparent-300" }
		//LOD 200
		
		Blend DstColor Zero
		//Blend SrcAlpha OneMinusSrcAlpha     
		CGPROGRAM
		#pragma surface surf Lambert noforwardadd alphatest:_Cutoff

		float4 _Color;
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = (c.rgb)*(2 -_Color.a);//*1.3;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
