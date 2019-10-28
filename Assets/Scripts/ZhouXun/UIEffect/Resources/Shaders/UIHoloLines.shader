Shader "Zhouxun/UIHoloLines"
{
	Properties
	{
		_Color ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Specular Texture", 2D) = "white" {}
		_TexH ("Texture H", 2D) = "black" {}
		_TexV ("Texture V", 2D) = "black" {}
		_Intensity ("Intensity", Float) = 1.8
		_TileSpeed ("Tile & Speed", Vector) = (48,7,1,1)
	}
	SubShader
	{
		LOD 100
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			ColorMask RGB
			//AlphaTest Greater .01
			Blend One One
			
			CGPROGRAM
			#pragma exclude_renderers gles
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
	
			float4 _Color;
			sampler2D _MainTex;
			sampler2D _TexH;
			sampler2D _TexV;

			float _Intensity;
			float4 _TileSpeed;
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
			struct v2f 
			{
				float2 texc : TEXCOORD0;
				float4 pos : POSITION0;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 mask = tex2D(_MainTex, i.texc.xy);
				half4 hc = tex2D(_TexH, i.texc.xy * _TileSpeed.xy + float2(0, _Time.y * _TileSpeed.z));
				half4 vc = tex2D(_TexV, i.texc.xy * _TileSpeed.xy + float2(_Time.y * _TileSpeed.w, 0));
				return mask * (hc + vc) * _Intensity * _Color;
			}
			ENDCG 
		}
	} 
	FallBack "Diffuse"
}
