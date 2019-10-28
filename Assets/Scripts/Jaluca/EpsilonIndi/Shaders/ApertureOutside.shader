Shader "SpecialItem/ApertureOutside"
{
	Properties 
	{				
		_Radius("Radius", float) = 0.3
		_SunAngle("Sun Angle", float) = 0
		_SunDirect("Sun Direct", Vector) = (0,0,0,0)
		_Color1 ("RefractColor", Color) = (1,1,1,1)
		_Thick1Min ("RefractColor Thick Min", float) = 0.05
		_Thick1Max ("RefractColor Thick Max", float) = 0.2
		_Intensity1Min ("RefractColor Intensity Min", range(0, 1)) = 0.2
		_Intensity1Add ("Sunset RefractColor Intensity Add", float) = 0.3
		_Color2 ("SunSetColor", Color) = (1,1,1,1)
		_Thick2 ("SunSetColor Thick", float) = 0.25
		_SunSetAngle ("SunSet Angle", range(0, 90)) = 45
	}

	Subshader 
	{
		Tags
		{		
			"RenderType"="Transparent"
			"Queue" = "Transparent-10"
		}
		Fog { Mode Off }
	  	Pass
	  	{
			Blend ONE ONE
			ZWrite Off			
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
						
			half _Radius;
			half _SunAngle;
			half4 _SunDirect;
			half4 _Color1;
			half _Thick1Min;
			half _Thick1Max;
			half _Intensity1Min;
			half _Intensity1Add;
			half4 _Color2;
			half _Thick2;
			half _SunSetAngle;
						
			struct appdata_v
			{
				float4 vertex : POSITION;				
				float2 texcoord : TEXCOORD0;				
			};
	
			struct v2f
			{
				float2 texCoord : TEXCOORD0;
				float4 pos : POSITION0;
				
			};
						
	
			v2f vert (appdata_v v)
			{
				v2f o;
	 
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texCoord = v.texcoord;	
				return o;
			}
			
			float fade(float t)
			{
				return t * t * (3 - t - t);
			}
			
			half4 frag (v2f i) : COLOR
			{
				//_SunAngle  0 dark  180 bright
				float2 lxy = (i.texCoord.xy - 0.5) / _Radius * 0.5 + 0.5;
				
				half r = length(lxy - 0.5) * 2;
				half apr = abs(r - 1);
				half scale = r * r * r;
				half2 sunxy = normalize(_SunDirect.xy);
				half2 texxy = normalize(i.texCoord.xy - 0.5);
				//anglexy  0:sunpos  180:antisunpos
				half anglexy = acos(sunxy.x * texxy.x - sunxy.y * texxy.y) * 57.32484;
				
				//blue light				
				half t11 = max(0, 1 - (anglexy / 120));							//0~1
				half t12 = max(0, 1 - abs(_SunSetAngle - _SunAngle) / 25);		//0~1
				half light1 = max(_Intensity1Min, (fade(t11) + _Intensity1Add) * t12);
				half thick1 = lerp(_Thick1Min, _Thick1Max, t12 * t12 * (1.5 - 0.5 * t12));				
				half4 ap1 = lerp(half4(0,0,0,0), _Color1, pow(saturate(1 - apr / thick1), 3) * scale) * light1;
				
				//golden light
				half t21 = max(0, 1 - (anglexy / 150));							//0~1
				half t22 = max(0, 1 - abs(_SunSetAngle - _SunAngle) / 15);		//0~1
				half light2 = pow(fade(t21) * 1.1, 3) * t22;
				half thick2 = (t22 * t22 * (2.5 - 1.5 * t22)) * _Thick2;
				half4 ap2 = lerp(half4(0,0,0,0), _Color2, pow(saturate(1 - apr / thick2), 3)) * light2;
				
				float4 retclr = lerp(ap1, ap2, pow(saturate((r - 0.8) / 0.2), 2) * t21 * t21 * (2 - t21) * min(1, t22 * t22 * 4));

				return retclr;
			}	
			ENDCG
		}
	}
} 