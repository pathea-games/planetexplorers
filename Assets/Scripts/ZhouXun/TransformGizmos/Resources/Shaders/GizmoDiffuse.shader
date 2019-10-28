Shader "zhouxun/GizmoDiffuse"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
      	_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
      	_RimPower ("Rim Power", Range(0.1,8.0)) = 3.0
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_IntensityAdjust ("Intensity Adjust", Float) = 0.4
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Fog {Mode Off}
		
		CGPROGRAM
		#pragma surface surf MyLambert noambient

		sampler2D _MainTex;
		float4 _Color;
      	float4 _RimColor;
      	float _RimPower;
      	float _IntensityAdjust;

      	struct Input {
          	float2 uv_MainTex;
          	float3 viewDir;
      	};

		void surf (Input IN, inout SurfaceOutput o)
		{
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb * _Color.rgb;
			o.Alpha = c.a * _Color.a;
			o.Emission = _RimColor.rgb * pow (rim, _RimPower);
		}
		
		// Lighting 
        half4 LightingMyLambert (SurfaceOutput s, half3 normLightDir, half3 normViewDir, half atten)
        {
        	normLightDir = normalize(normViewDir + half3(0.2,0.2,0.2));
        	half3 lc = half3(0.5,0.5,0.5);
        	
            // Lighting model
            half NdotL = saturate(dot(s.Normal, normLightDir) + 0.2);
			
            half4 c = (0,0,0,0);
            c.rgb = (s.Albedo * lc * pow(NdotL,1.5)) * atten * 2;
            c.a = s.Alpha;
            return c * _IntensityAdjust;
        }

		ENDCG
	} 
	FallBack "Diffuse"
}
