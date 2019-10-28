Shader "SpecialItem/TaskItem" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		//_BumpMap ("BumpMap", 2D) = "bump" {}
		_Angle("Insident Angle", Range(0, 3.1416)) = 0
		_Xmin("Xcoormin", float) = -0.25
		_Xmax("Xcoormax", float) = 0.25
		_Zmin("Zcoormin", float) = -0.16475
		_Zmax("Zcoormax", float) = 0.6494
		_Cycle("CycleMultiple", float) = 3
		_Speed("Speed", float) = 5
		_Wide("BandWide", float) = 0.25
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert vertex:vert
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		//sampler2D _BumpMap;
		float _Angle;
		float _Xmin;
		float _Xmax;		
		float _Zmin;
		float _Zmax;		
		float _Cycle;
		float _Speed;
		float _Wide;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 world_pos;
			float3 world_normal;
		};
		void vert (inout appdata_full v, out Input o)
		{
		    o.world_pos = v.vertex;		    
		    o.world_normal = v.normal.xyz;
		}
		
		float bandcolor (float coorX, float coorZ)
		{	
			float dist = max(_Xmax - _Xmin, _Zmax - _Zmin) * 1.15 + _Wide;
			float onecycle = dist / _Speed;
			float cooling = onecycle * _Cycle;
			float pct = _Wide / dist;
			
			float coorT = clamp(fmod(_Time.y, cooling), 0, onecycle) / onecycle;
			float lx = coorX - _Xmin;
			float lz = _Zmax - coorZ;
			float coorXnew = (lx / cos(_Angle) + (lz - lx * tan(_Angle)) * sin(_Angle)) / dist;
			float coorV = clamp(coorXnew, coorT - pct, coorT);
			coorV = (coorV - coorT + pct) / pct;			
			
			return 1 - pow((coorV + coorV - 1), 2);
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			
			float4 color4 = tex2D (_MainTex, IN.uv_MainTex);			
			o.Albedo = color4.rgb * (1 + bandcolor(IN.world_pos.x, IN.world_pos.z));
		
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
