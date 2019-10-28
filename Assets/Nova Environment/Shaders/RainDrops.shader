Shader "NovaEnv/RainDrops"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_DistortTex ("Distort Texture", 2D) = "white" {}
		_Saturate ("Lighting Saturate", Float) = 0.5
		_Intensity ("Intensity", Float) = 1
		_BaseIntensity ("Base Intensity", Float) = 0.1
	}
	
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent+449"
		}
		Fog { Mode Off }

		GrabPass 
		{							
			"_GrabTex_Origin"
		}
		Pass
	  	{
			Blend One One
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
	    	#include "UnityCG.cginc"
	    	
	    	fixed4 _TintColor;
	    	sampler2D _MainTex;
	    	sampler2D _DistortTex;
	    	float _Saturate;
	    	float _Intensity;
	    	float _BaseIntensity;
			sampler2D _GrabTex_Origin : register(s0);
			float4 _GrabTex_Origin_TexelSize;
	    	
			struct v2f 
			{
				float3 texc : TEXCOORD0;
				float4 pos : POSITION0;
				float4 projpos : TEXCOORD1;
				float4 color : COLOR;
			};

			v2f vert (appdata_full v)
			{
				v2f o;
				o.texc.xyz = v.texcoord.xyz;	
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.projpos = o.pos;
				o.color = v.color;
				return o;
			}

			float4 GetGrabColor ( sampler2D grabtex, float4 screen_pos )
			{
				return tex2Dproj(grabtex, UNITY_PROJ_COORD(screen_pos));
			}
			
			float4 RefractionProjectionPos ( float4 projpos, float3 norm, float strength, float2 texel_size )
			{
				half2 bump = norm.xy;
				float2 ofs = bump * strength * texel_size;
				projpos.xy = projpos.xy + ofs;
				return projpos;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 c = tex2D(_MainTex, i.texc.xy);
				c = c * c.a * i.color * i.color.a * _TintColor * _TintColor.a;
				
				float3 worldNormal = tex2D(_DistortTex, i.projpos.xz*0.5 + _Time.xy * 5) * 2 - 1;
				float4 refract_projpos = RefractionProjectionPos(i.projpos, worldNormal, 400, _GrabTex_Origin_TexelSize.xy*8); 
				float4 refract_grabpos = ComputeGrabScreenPos(refract_projpos);
				
				half4 grabcolor = GetGrabColor(_GrabTex_Origin, refract_grabpos).rgba;
				float grabgray = grabcolor.r * 0.299 + grabcolor.g * 0.587 + grabcolor.b * 0.114;
				grabcolor = saturate(lerp(half4(grabgray.xxxx), grabcolor, _Saturate));
				return (pow(grabcolor, 0.5) * _Intensity + _BaseIntensity) * (c.r + c.g + c.b);
			}
			ENDCG 
		}
	}
	
	//
	// Fall-back
	//
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent+449"
		}
		Fog { Mode Off }
		Pass
	  	{
			Blend One One
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
	    	#include "UnityCG.cginc"
	    	
	    	fixed4 _TintColor;
	    	sampler2D _MainTex;
	    	float _Saturate;
	    	float _Intensity;
	    	float _BaseIntensity;
	    	
			struct v2f 
			{
				float3 texc : TEXCOORD0;
				float4 pos : POSITION0;
				float4 color : COLOR;
			};

			v2f vert (appdata_full v)
			{
				v2f o;
				o.texc.xyz = v.texcoord.xyz;	
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 c = tex2D(_MainTex, i.texc.xy);
				c = c * c.a * i.color * i.color.a * _TintColor * _TintColor.a;
				return c;
			}
			ENDCG 
		}
	}
	FallBack "Particles/Additive"
}

