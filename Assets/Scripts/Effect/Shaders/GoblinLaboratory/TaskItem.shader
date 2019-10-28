Shader "SpecialItem/TaskItem" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		//_BumpMap ("BumpMap", 2D) = "bump" {}
		_Angle("Insident Angle", Range(0, 3.1416)) = 0
		_Xmin("Xcoormin", float) = -0.25
		_Xmax("Xcoormax", float) = 0.25
		_Zmin("Zcoormin", float) = -0.16475
		_Zmax("Zcoormax", float) = 0.6494
		_Speed("Speed", float) = 5
		_Wide("BandWide", float) = 0.25
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert
		#pragma vertex vert
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		//sampler2D _BumpMap;
		float _Angle;
		float _Xmin;
		float _Xmax;		
		float _Zmin;
		float _Zmax;
		float _Speed;
		float _Wide;
		float _Dt;

		struct Input {
			float2 uv_MainTex;
			//float2 uv_BumpMap;
			float4 world_pos;
		};
		void vert (inout appdata_base v, out Input o)
		{			
			o.uv_MainTex = v.texcoord.xy;
		    o.world_pos = v.vertex;  
		}
		float bandcolor (float coorX, float coorZ)
		{	
			float dist = max(_Xmax - _Xmin, _Zmax - _Zmin) * 1.15 + _Wide;
			float onecycle = dist / _Speed;
			float pct = _Wide / dist;
			
			float coorT = clamp(_Dt, 0, onecycle) / onecycle;
			float lx = coorX - _Xmin;
			float lz = _Zmax - coorZ;
			float coorXnew = (lx / cos(_Angle) + (lz - lx * tan(_Angle)) * sin(_Angle)) / dist;
			float coorV = clamp(coorXnew, coorT - pct, coorT);
			coorV = (coorV - coorT + pct) / pct;			
			
			return 1 - pow((coorV + coorV - 1), 2);
		}
		
		void surf (Input IN, inout SurfaceOutput o)
		{		
			half3 orgColor = tex2D(_MainTex, IN.uv_MainTex).rgb;
			o.Albedo = orgColor;
			o.Emission = orgColor.rgb * bandcolor(IN.world_pos.x, IN.world_pos.z);		
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
