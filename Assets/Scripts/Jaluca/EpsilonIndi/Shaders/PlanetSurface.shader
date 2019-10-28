Shader "SpecialItem/PlanetSurface" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("BumpMap", 2D) = "white" {}
		_SpecularMap ("SpecularMap", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Planet
		#pragma vertex vert

		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _SpecularMap;
		float3 _SunDirect;

		struct Input {
			float2 uv_MainTex;
			float3 world_pos;
			float3 world_normal;
			//float3 viewDir;
		};
		void vert (inout appdata_base v, out Input o)
		{
			o.uv_MainTex = v.texcoord.xy;
		    o.world_pos = v.vertex;
		    o.world_normal = v.normal;
		    //o.viewDir = ObjSpaceViewDir(v.vertex);
		}
		void surf (Input IN, inout SurfaceOutput o) {			
			o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			o.Specular = tex2D(_SpecularMap, IN.uv_MainTex);
		}
		half4 LightingPlanet(SurfaceOutput s, half3 lightDir, half3 viewDir)
		{
			half sunDot = max(0, dot(s.Normal, lightDir));
			half4 aaa = float4(s.Albedo, 1);
			return aaa;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
