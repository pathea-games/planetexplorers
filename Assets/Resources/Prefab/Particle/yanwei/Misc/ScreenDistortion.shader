Shader "zhouxun/Screen Distortion/General"
{
	Properties
	{
		_DistortionTex ("Distortion Texture", 2D) = "bump" {}
		_DistortionStrength ("Distortion Strength", Float) = 1
		_DistortionTile ("Distortion Tile", Float) = 1
	}
	
	CGINCLUDE
	struct appdata_distort
	{
		float4 vertex : POSITION;
		float3 texcoord : TEXCOORD0;
		float4 color : COLOR;
	};

	sampler2D _ScreenTex : register(s0);
	float4 _ScreenTex_TexelSize;
	float _DistortionStrength;
	float _DistortionTile;
	
	sampler2D _DistortionTex;
	ENDCG
	
	SubShader
	{
		Tags {"RenderType"="Transparent" "Queue"= "Transparent+4000" "IgnoreProjector"="True"}
        GrabPass
        {
        	"_ScreenTex"
        }
        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
            Name "Screen Distortion"
            ZWrite Off
            Fog {Mode Off}

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
			struct v2f 
			{
				float4 pos 			: SV_POSITION;
				float4 color        : COLOR;
				float3 texc         : TEXCOORD0;
				float4 projpos      : TEXCOORD1;
				float3 worldPos		: TEXCOORD2;
			};

			v2f vert(appdata_distort v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord;
				o.projpos = o.pos;
				o.worldPos = mul(_Object2World,(v.vertex)).xyz;
				o.color = v.color;
				return o;
			}
                
			float GetLED ( sampler2D depthtex, float4 screen_pos )
			{
				return LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(depthtex, UNITY_PROJ_COORD(screen_pos))));
			}

			float CalcSink ( float delta_depth, float dens )
			{
				return saturate(pow(max((delta_depth), 0)*dens, 0.5));
			}

			float4 RefractionProjectionPos ( float4 projpos, float3 norm, float strength, float2 texel_size )
			{
				half2 bump = norm.xz;
				float2 ofs = bump * strength * texel_size * pow(projpos.z, 0.8);
				projpos.xy = projpos.xy + ofs;
				return projpos;
			}

			float4 GetGrabColor ( sampler2D grabtex, float4 screen_pos )
			{
				return tex2Dproj(grabtex, UNITY_PROJ_COORD(screen_pos));
			}

			half4 frag( v2f i ) : COLOR
			{
				float4 screenPos = ComputeScreenPos(i.projpos);
				
				half3 norm = tex2D(_DistortionTex, i.texc.xy*_DistortionTile).xyz * 2 - 1;

				float4 refract_projpos = RefractionProjectionPos(i.projpos, norm, _DistortionStrength, _ScreenTex_TexelSize.xy*8); 
				float4 refract_grabpos = ComputeGrabScreenPos(refract_projpos);
				half3 grabcolor = GetGrabColor(_ScreenTex, refract_grabpos).rgb;

				return half4(grabcolor*i.color, i.color.a); 
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
