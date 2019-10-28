Shader "Wave/SpotWave" 
{
	Properties
	{
		_Frequency ("Frequency", Float) = 300
		_Strength ("Strength", Float) = 0.05
		_Speed ("Speed", Float) = 1
		_TimeFactor ("Time", Float) = 0
		_FadeFactor ("Fade", Vector) = (3, 3, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent+10000" }
		Pass
		{
			Fog { Mode Off } 
			Blend One One
			Cull Off
			ZWrite Off 
			
			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
	
			float _Frequency;
			float _Strength;
			float _Speed;
			float _TimeFactor;
			float4 _FadeFactor;
	
			struct v2f 
			{
				float4 pos : POSITION0;
				float2 texc : TEXCOORD0;
			};
			
			float strengthCurve (float time)
			{
				return 1/sqrt(_FadeFactor.x*max(time,0.1));
			}
			
			float amplitudeCurve (float x)
			{
				x /= 2;
				x -= 3.76;
				float y = x*x + 2; 
				return 1/(_FadeFactor.y*y);
			}
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord.xy;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				// cal relative pos
				float2 lpos = i.texc - 0.5;
				float len = length(lpos);
				float2 dir = normalize(lpos);
				float _len = sqrt(len);
				float x = max(_TimeFactor * _Speed - _len, 0);
				float wave = sin(_Frequency * x) * _Strength * strengthCurve(_TimeFactor) * amplitudeCurve(_Frequency * x) * saturate(0.5-len) * saturate(len*5);
				dir *= wave;
				
				return half4(dir.xy, 0, 1);
			}
			ENDCG 
		}
	}
}
