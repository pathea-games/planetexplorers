Shader "SpecialItem/ApertureInside"
{
	Properties 
	{				
		_Radius("Radius", float) = 0.3
		_SunAngle("Sun Angle", float) = 0
		_SunDirect("Sun Direct", Vector) = (0,0,0,0)
		_Color3 ("AtmosphericColor", Color) = (1,1,1,1)
		_Thick3 ("AtmosphericColor Thick", float) = 0.1
		_ThickPow3 ("AtmosphericColor Offset", float) = 3
		_Intensity3Min ("Atmospheric Intensity Min", float) = 0.1
		_Intensity3MinAp ("Atmospheric Intensity Min Pow", range(0,1)) = 0.7
	}

	Subshader 
	{
		Tags
		{		
			"RenderType"="Transparent"
			"Queue" = "Transparent-11"
		}
		Fog { Mode Off }
	  	Pass
	  	{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off			
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
						
			half _Radius;
			half _SunAngle;
			half4 _SunDirect;
			half4 _Color3;
			half _Thick3;
			half _ThickPow3;
			half _Intensity3Min;
			half _Intensity3MinAp;
						
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
			
			half4 frag (v2f i) : COLOR
			{
				//_SunAngle  0 dark  180 bright
				float2 lxy = (i.texCoord.xy - 0.5) / _Radius * 0.5 + 0.5;
				
				half r = length(lxy - 0.5) * 2;
				half apr = abs(r - 1);
				half2 sunxy = normalize(_SunDirect.xy);
				half2 texxy = normalize(i.texCoord.xy - 0.5);
				//anglexy  0:sunpos  180:antisunpos
				half anglexy = acos(sunxy.x * texxy.x - sunxy.y * texxy.y) * 57.32484;
				
				//inside light
				half t32 = max(0, _SunAngle * (0.0287 - 0.00011 * _SunAngle) - 0.875);	// 0~35: 0(night)	145: 1(midday)
				half t31 = min(1, anglexy / (120 + 90 * t32 * t32));						//0~1
				t31 = 1 - t31 * t31 * t31 * t31 * t31;
				half thick3 = _Thick3;				
				half ap3fade = pow(saturate(1 - apr / thick3), 3) / pow(r, _ThickPow3);
				half light3dark = ap3fade * pow(_Intensity3Min, _Intensity3MinAp);
				half light3 = max((r > 1 ? 0 : _Intensity3Min), light3dark + (1 - _Intensity3Min) * t32 * t31 * ap3fade);
				half4 ap3 = lerp(half4(0,0,0,0), _Color3, light3);
				
				return ap3;
			}	
			ENDCG
		}
	}
} 