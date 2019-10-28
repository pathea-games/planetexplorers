Shader "Custom/b45" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_BumpMap ("BumpMap", 2D) = "bump" {}
//		_SpecularMap ("SpecularMap", 2D) = "specular" {}
		_SpecularPower ("SpecularPower", Float) = 1
		_UVScale("UV Scale", Float)= 2
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		CGPROGRAM
		#pragma target 3.0
		
		//#define USE_TMPVAR_TO_SKIP_VLIGHT
		//#pragma surface surf SimpleSpecular vertex:vert // noforwardadd
		#pragma surface surf Lambert vertex:vert
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _BumpMap;
//		sampler2D _SpecularMap;
		float  _SpecularPower;
		float  _UVScale;
		
		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 world_pos;
			float3 world_normal;
		};
		void vert (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
		    o.world_pos = mul (_Object2World, v.vertex);		    
		    o.world_normal = v.normal.xyz;
		}

		float cos_YP(float3 vec){
			return dot(float3(0,1,0), vec);
		}
		float cos_YN(float3 vec){
			return dot(float3(0,-1,0), vec);
		}
		float cos_XP(float3 vec){
			float3 vecHorz = float3(vec);
			vecHorz.x = vec.x;
			vecHorz.y = 0;
			vecHorz.z = vec.z;
			normalize(vecHorz);
			return max(dot(vec, float3(1,0,0)), dot(vec, float3(-1,0,0)));
		}
#ifdef USE_TMPVAR_TO_SKIP_VLIGHT
		struct CustomSurfaceOutput
		{
			half3 Albedo;
			half3 Albedo_;
			half3 Normal;
			half3 Emission;
			half Specular;
			half Alpha;
		};
		void surf (Input IN, inout CustomSurfaceOutput o) {
			float uv_scale = 3;
			float b45_scale = 0.5f;
			
			float xc = (IN.world_pos.x / b45_scale / _UVScale);
			float yc = (IN.world_pos.y / b45_scale / _UVScale);
			float zc = (IN.world_pos.z / b45_scale / _UVScale); 
			
			float2 uv = IN.uv_MainTex;
			 
			float cos45 = 0.5f;
			if(cos_YP(IN.world_normal) > cos45 || cos_YN(IN.world_normal) > cos45)
			{
				uv.x = xc;
				uv.y = zc;
				
			}
			// else the normal is pointing horizontally, now subdivide this case into x-pointing or z-pointing
			else if(cos_XP(IN.world_normal) > cos45)
			{
				uv.x = zc;
				uv.y = yc;
				
			}else
			{
				uv.x = xc;
				uv.y = yc;
			}
			float4 color4 = tex2D (_MainTex, uv);

			//using o.Emission to store Albedo to work around c.rgb += o.Albedo * IN.vlight;
			o.Albedo = color4.rgb*0.5f;
			o.Albedo_ = color4.rgb*0.5f;
        	o.Normal = UnpackNormal (tex2D (_BumpMap, uv));
        	o.Specular = _SpecularPower;        	
		}
		half4 LightingSimpleSpecular (CustomSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
			half3 h = normalize (lightDir + viewDir);
			
			half diff = max (0, dot (s.Normal, lightDir));
			
			float nh = max (0, dot (s.Normal, h));
			float spec = pow (nh, 250) * s.Specular;
			
			half4 c;
			c.rgb = (s.Albedo_ * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten*2);
			c.a = s.Alpha;
			return c;
     	}
#else
		void surf (Input IN, inout SurfaceOutput o) {
			float uv_scale = 3;
			float b45_scale = 0.5f;
			
			float xc = (IN.world_pos.x / b45_scale / _UVScale);
			float yc = (IN.world_pos.y / b45_scale / _UVScale);
			float zc = (IN.world_pos.z / b45_scale / _UVScale); 
			
			float2 uv = IN.uv_MainTex;
			 
			float cos45 = 0.5f;
			if(cos_YP(IN.world_normal) > cos45 || cos_YN(IN.world_normal) > cos45)
			{
				uv.x = xc;
				uv.y = zc;
				
			}
			// else the normal is pointing horizontally, now subdivide this case into x-pointing or z-pointing
			else if(cos_XP(IN.world_normal) > cos45)
			{
				uv.x = zc;
				uv.y = yc;
				
			}else
			{
				uv.x = xc;
				uv.y = yc;
			}
			float4 color4 = tex2D (_MainTex, uv);

			//using o.Emission to store Albedo to work around c.rgb += o.Albedo * IN.vlight;
			o.Albedo = color4.rgb*0.5f;
        	o.Normal = UnpackNormal (tex2D (_BumpMap, uv));
        	o.Specular = _SpecularPower;        	
		}
#endif
		ENDCG
	} 
	FallBack "Diffuse"
}
