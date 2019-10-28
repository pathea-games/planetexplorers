Shader "Wave/LineWave" 
 {
	Properties 
	{
		_Frequency ("Frequency", Float) = 300
		_Strength ("Strength", Float) = 0.05
		_Speed ("Speed", Float) = 1
		_Distance ("Distance", Range(0, 1)) = 1
		_TimeFactor ("Time", Float) = 0
		_DeltaTime ("Delta Time", Float) = 1
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
			#pragma target 3.0
	
			float _Frequency;
			float _Strength;
			float _Speed;
			float _TimeFactor;
			float _DeltaTime;
			float _Distance;
			float4 _FadeFactor;
	
			struct v2f 
			{
				float4 pos : POSITION0;
				float2 texc : TEXCOORD0;
			};
			
			float strengthCurve (float time)
			{
//				return 1;
				return 1/sqrt(_FadeFactor.x*max(time,0.1));
			}
			
			float amplitudeCurve (float x)
			{
//				return 1;
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
				float2 p = (i.texc - float2(0.25, 0.5))*2 ;
				float2 pA = float2(0, 0);
				float2 pB = float2(_Distance, 0);
				
//				float2 vBA = pB - pA;
				
				float vt = _Speed * _DeltaTime;
//				float2 pC = float2(pA.x, pA.y + vt);
//				float2 pD = float2(pA.x, pA.y - vt);
				
				float2 center = 0;
				float len = 0;
				

				center.x = clamp (p.x - vt*abs(p.y), 0, pB.x);
				len = distance(p, center) + center.x * vt / sqrt(vt*vt+pB.x);
				
//				// cal relative pos
				float2 lpos = p - center;
//				float len = length(lpos);
				float2 dir = lpos*20;
				if (length(lpos*20) > 1)
					dir = normalize(dir);
				float x = max(_TimeFactor * _Speed - sqrt(len), 0);
				float wave = sin(_Frequency * x) * _Strength * strengthCurve(_TimeFactor) * amplitudeCurve(_Frequency * x) * saturate(1 - saturate((length(i.texc - 0.5) - 0.05)) * 2); //* saturate(0.5 - (abs(p.y) - 0.5)) ;//* max(0.1, _Distance); //* saturate(len*5);
//				float _x = center.x - (1 -_Distance);
//				wave *= (1-saturate(abs(center.x - 0.5) - 0.1)*2);
//				wave *= (0.5 - clamp(abs(i.texc.x - 0.5) + 0.1, 0, 0.5));//(1-saturate(abs(center.x - 0.5))*2);
//				wave *= (1- (center.x - 0.5)
//				if (center.x - 0.5 < 0)
//					wave *= (1-saturate(-center.x + 0.5 - 0.05)*2);
//				else 
//					wave *= (1-saturate(center.x - 0.5 - 0.1)*2);
//				wave *= (1-saturate(abs(center.x - 0.5*_Distance )-max(0.05, 0.2*(1 - _Distance)))*2/_Distance);
				dir *= wave;
				
				return half4(dir, 0, 1);
			}
			ENDCG 
		}
	} 
}
