Shader "Voxel Creation/Decal AlphaBlend"
{
	Properties
	{
		_Texture ("Texture", 2D) = "white" {}
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_Center ("Center", Vector) = (0,0,0,1)
		_Forward ("Forward", Vector) = (0,0,1,0)
		_Right ("Right", Vector) = (1,0,0,0)
		_Up ("Up", Vector) = (0,1,0,0)
		_Size ("Size", Float) = 1
		_Depth ("Depth", Float) = 0.01
	}
	SubShader
	{
		Tags{ "RenderType"="Opaque" "Queue"="Geometry+451" }

		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		ZWrite Off
		
		CGPROGRAM
        //#pragma target 3.0
		#pragma surface surf VCLambert vertex:vert
		//#pragma glsl

		sampler2D _Texture;
		float4 _TintColor;
		float4 _Center;
		float4 _Forward;
		float4 _Right;
		float4 _Up;
		float _Size;
		float _Depth;

		struct Input
		{
            float3 wNorm;
			float3 wPos;
            float4 vCol;
		};
		
        struct VCSurfaceOutput
        {
            float3 Albedo;
            float3 Normal;
            float3 Normal_;
            float3 Emission;
            float3 SpecularColor;
            float  Specular;
            float  SpecularStrength;
            float  Alpha;
        };
        
        void vert (inout appdata_full v, out Input o)
        {  
 			o.wPos = mul(_Object2World, v.vertex).xyz;
            o.wNorm = normalize(mul(_Object2World, float4(normalize(v.normal), 0)).xyz);
            o.vCol = v.color; 
		}

		void surf (Input i, inout VCSurfaceOutput o)
		{
			float3 ofs = i.wPos.xyz - _Center.xyz;
			float3 lpos = 0;
			lpos.x = dot(ofs, _Right) / _Size;
			lpos.y = dot(ofs, _Up) / _Size;
			lpos.z = dot(ofs, _Forward);
			if ( abs(lpos.x) > 0.999 )
				discard;
			if ( abs(lpos.y) > 0.999 )
				discard;
			if ( abs(lpos.z) > _Depth - 0.001 )
				discard;
			
			float4 diff = tex2D(_Texture, (lpos.xy+1)*0.5);
			
			//o.Albedo = ((diff.rgb * i.vCol.rgb) * (1-i.vCol.a) + i.vCol.rgb * i.vCol.a) * _TintColor.rgb;
			o.Albedo = diff.rgb * _TintColor.rgb;
			o.Alpha = diff.a * _TintColor.a;
			o.Normal_ = i.wNorm;
			o.Emission = 0;
			o.SpecularColor = 1;
			o.Specular = 1;
			o.SpecularStrength = 0;
		}
		
        // Lighting 
        half4 LightingVCLambert (VCSurfaceOutput s, half3 normLightDir, half atten)
        {
            // Lighting model
            half NdotL = clamp( dot(s.Normal_, normLightDir ), 0, 1 );
			
            half4 c = (0,0,0,0);
            c.rgb = (s.Albedo * _LightColor0.rgb * NdotL + UNITY_LIGHTMODEL_AMBIENT.rgb*s.Albedo) * atten;
            c.a = s.Alpha;
            
            return c;
        }
		ENDCG
	}
	Fallback "Voxel Creation/Decal Emission"
}
