Shader "UI/UIGridEffect"
{
	Properties
	{
		_GlowTex ("Base (RGB)", 2D) = "black" {}
		_NoiseTex ("Noise", 2D) = "black" {}
		_FadeTime ("Fade Time", Float) = 0
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		LOD 200
		Fog { Mode off }
		Blend One One
		Cull off
		Zwrite off
		
		Pass
		{
			CGPROGRAM
			// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members tc)
			#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
		    #pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _GlowTex;
			sampler2D _NoiseTex;
			float _FadeTime;
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
	
			struct v2f 
			{
				float2 tc : TEXCOORD0;
				float3 lpos : TEXCOORD1;
				float4 pos : POSITION0;
			};
	
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.tc.xy = v.texcoord.xy;
				o.lpos.xyz = v.vertex.xyz;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float2 texc = i.tc.xy;
				float3 lpos = i.lpos;
				
				float base_intens = saturate(-20 * abs(lpos.x) + 10.5);
				half4 ret_color = tex2D(_GlowTex, float2(texc.x , texc.y) + float2(0, (_FadeTime*2-1))) * base_intens;
				float t = clamp(pow((1 - abs(0.6-_FadeTime) * 1)*0.96, 4), 0.2, 1.3) * 0.4;
				return ret_color * t;
			}
			ENDCG
	    }

	} 
	FallBack "Diffuse"
}
