Shader "NovaEnv/Rain Distortion"
{
	Properties
	{
		_Color ("Tint Color", Color) = (1,1,1,1)
		_DistortionTex ("Distortion Texture", 2D) = "bump" {}
		_DistortionStrength ("Distortion Strength", Float) = 1
		_FlareThreshold ("Flare Threshold", Float) = 0.8
	}
	
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Overlay"
			"IgnoreProjector" = "True"
		}
		
        GrabPass { "_ScreenTex" }
        
        Pass
        {
            Name "Screen Distortion"
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZTest Off
            ZWrite Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
			struct appdata_distort
			{
				float4 vertex : POSITION;
				float3 texcoord : TEXCOORD0;
				float4 color : COLOR;
			};

			float4 _Color;
			sampler2D _DistortionTex;
			float _DistortionStrength;
			float _FlareThreshold;
			
			sampler2D _ScreenTex : register(s0);
			float4 _ScreenTex_TexelSize;
	
			struct v2f 
			{
				float4 pos 			: SV_POSITION;
				float4 color        : COLOR;
				float3 texc         : TEXCOORD0;
				float4 projpos      : TEXCOORD1;
			};

			v2f vert(appdata_distort v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord;
				o.projpos = o.pos;
				o.color = v.color;
				return o;
			}
                
			float4 Refraction (float4 grab_pos, float2 norm, float strength, float aspect)
			{
				norm.y *= aspect;
				grab_pos.xy /= grab_pos.ww;
				grab_pos.xy += (norm * strength / grab_pos.ww);
				grab_pos.xy *= grab_pos.ww;
				return grab_pos;
			}

			float4 GetGrabColor ( sampler2D grabtex, float4 grab_pos )
			{
				return tex2Dproj(grabtex, UNITY_PROJ_COORD(grab_pos));
			}

			half4 frag( v2f i ) : COLOR
			{
				half3 norm = tex2D(_DistortionTex, i.texc.xy).xyz * 2 - 0.996078;
				norm = normalize(norm);
				if (norm.z > 0.999f)
					discard;

				float4 grab_pos = ComputeGrabScreenPos(i.projpos);
				float aspect = _ScreenTex_TexelSize.z / _ScreenTex_TexelSize.w;
				grab_pos = Refraction(grab_pos, norm.xy, _DistortionStrength, aspect); 
				half3 grabcolor = GetGrabColor(_ScreenTex, grab_pos).rgb;

				float ref = clamp(pow(abs(norm.z), 3), 0.1, 1);
				float gray = grabcolor.r * 0.3 + grabcolor.g * 0.6 + grabcolor.b * 0.1;
				float bloom = saturate(gray - _FlareThreshold) * 20 * (1 - ref) * saturate(abs(_DistortionStrength)*5) + 1;
				return half4(grabcolor*bloom*i.color, i.color.a) * lerp(_Color, half4(1,1,1,1), ref); 
			}
			ENDCG
		}
	}
	
	// Not support , fallback
	SubShader
	{
		Tags {"RenderType"="Transparent" "Queue"= "Transparent" "IgnoreProjector"="True"}
        Pass
        {
            Name "Not support"
			Blend One One
			Cull Off
            ZWrite Off
            Fog {Mode Off}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
			struct v2f 
			{
				float4 pos 			: SV_POSITION;
				float3 texc         : TEXCOORD0;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord;
				return o;
			}


			half4 frag( v2f i ) : COLOR
			{
				return 0;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
