Shader "Zhouxun/PlanetSurface"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
   		_BumpMap ("Normalmap", 2D) = "white" {}
   		_SpecMap ("Specularmap", 2D) = "black" {}
   		_DetailMap ("Detailmap", 2D) = "white" {}
   		_DetailBumpMap ("DetailBumpmap", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf SpecLambert
		#pragma target 3.0
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _BumpMap; 
		sampler2D _SpecMap; 
		sampler2D _DetailMap; 
		sampler2D _DetailBumpMap; 

		struct Input
		{
			float2 uv_MainTex;
		};

		half4 LightingSpecLambert (SurfaceOutput s, half3 normLightDir, half3 normViewDir, half atten)
		{
			// Lighting model
			half NdotL = clamp( dot(s.Normal, normLightDir), 0, 1 );
			half3 h = normalize (normLightDir + normViewDir);
			half nh = max (0, dot (s.Normal, h));
			half spec = smoothstep(0, 1.0, pow(nh, 50.0 * s.Specular)) * s.Specular * 0.35f * NdotL + saturate(s.Specular - 0.92f);
			
			_SpecColor = 1;
			half4 c = (0,0,0,0);
			c.rgb = (s.Albedo * _LightColor0.rgb * NdotL + _LightColor0.rgb * _SpecColor * spec) * atten * 2; 
			c.a = 1;
			return c;
		}
		
		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 cmain = tex2D (_MainTex, IN.uv_MainTex);
			half3 nmain = normalize(UnpackNormal(tex2D (_BumpMap, IN.uv_MainTex)).xyz);
			nmain.y = -nmain.y;
			nmain.xy *= 2;
			half spec = tex2D (_SpecMap, IN.uv_MainTex).r;
			half t = saturate(spec * 1.5f);
			half detail1 = lerp(saturate(tex2D (_DetailMap, IN.uv_MainTex * 4).r * 1.2), 1, t);
			half detail2 = lerp(saturate(tex2D (_DetailMap, IN.uv_MainTex * 8).r * 1.2), 1, t);
			half detail3 = lerp(saturate(tex2D (_DetailMap, IN.uv_MainTex * 16).r * 1.2), 1, t);
			half detail4 = lerp(saturate(tex2D (_DetailMap, IN.uv_MainTex * 32).r * 1.2), 1, t);
			
			half3 detailb1 = UnpackNormal(tex2D (_DetailBumpMap, IN.uv_MainTex * 4)).rgb;
			half3 detailb2 = UnpackNormal(tex2D (_DetailBumpMap, IN.uv_MainTex * 8)).rgb;
			half3 detailb3 = UnpackNormal(tex2D (_DetailBumpMap, IN.uv_MainTex * 16)).rgb;
			half3 detailb4 = UnpackNormal(tex2D (_DetailBumpMap, IN.uv_MainTex * 32)).rgb;
			
			o.Albedo = cmain.rgb * detail1 * detail2 * detail3;
			o.Alpha = cmain.a;
    		o.Normal = normalize(lerp(nmain + (detailb1 + detailb2 + detailb3 + detailb4)*0.6, half3(0,0,1), t));
			o.Gloss = 5;
    		o.Specular = spec;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
