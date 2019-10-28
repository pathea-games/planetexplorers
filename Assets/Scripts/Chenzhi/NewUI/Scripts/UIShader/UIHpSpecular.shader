Shader "UI/UIHpSpecular" 
{
	Properties
	{
		_MainTex ("Specular Texture", 2D) = "white" {}
		_SpecularMask ("Specular Mask", 2D) = "white" {}
		_RandomTex ("Noise", 2D) = "bump" {}
		_SizeSettings ("Sizes (w, h, sw, sh)", Float) = 100
		_FadeTime ("Fade Time", Float) = 0
		_Color0 ("Color0", Color) = (0,0,0,0)
		_Color1 ("Color1", Color) = (0,0,0,0)
		_Color2 ("Color2", Color) = (0,0,0,0)
		_Color3 ("Color3", Color) = (0,0,0,0)
		_Intensity ("Intensity", Float) = 1.8
		_OtherSettings ("(Randomness, wave threshold, wave length, wave speed", Vector) = (0.05,0.28,20,4)
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
	
			sampler2D _MainTex;
			sampler2D _SpecularMask;
			sampler2D _RandomTex;
			float4 _SizeSettings;
			float _FadeTime;
			float4 _Color0;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float _Intensity;
			float4 _OtherSettings;
	
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
			
			float2 random (float2 coord)
			{
				return tex2D(_RandomTex, coord*_SizeSettings.xy/64).rg;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float2 btexc = i.texc.xy;
				half4 c = tex2D (_SpecularMask, btexc);
				float2 coord = i.texc.xy*1 + random(i.texc.xy + _FadeTime)*_OtherSettings.x;
				coord.x -= _FadeTime*0.05;
				coord.y += _FadeTime*0.08;
				coord.x = abs(frac(coord.x*0.5)*2-1);
				coord.y = abs(frac(coord.y*0.5)*2-1);
				half4 mainc = lerp(lerp(_Color0, _Color1, coord.x), lerp(_Color2, _Color3, coord.x), coord.y) * tex2D(_MainTex, coord);
				float intens = sin((i.texc.x + i.texc.y + _Time.x * _OtherSettings.w)*_OtherSettings.z) + 2;
				intens = lerp(1, intens, saturate(c.r - _OtherSettings.y)*4);
				mainc *= c.r * _Intensity * intens;
				return  mainc;
			}
			ENDCG 
		}
	} 
	FallBack "Diffuse"
}