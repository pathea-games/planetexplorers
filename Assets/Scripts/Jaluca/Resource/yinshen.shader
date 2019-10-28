Shader "Ajaluca/testyinshen" 
{
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Disturb ("Disturb", 2D) = "black" {}
		_Honeycomb ("Honeycomb", 2D) = "white"{}		
		_Intensity ("Disturb Intensity", Range(0,0.2)) = 0.05
		_Self("Self Disturb", Range(0,1)) = 0.2
		_Trans("Transparency", Range(0,1)) = 0
		_SideLen("SideLength", float) = 0.7
		_HidTime("FadeTime", float) = 2
		_AprTime("AppearTime", float) = 2
		_FlsTime("FlashTime", float) = 0.3
		_Tail("TailLength", Range(0,0.5)) = 0.1
		_Tall("Height", float) = 1.8		
		_Dt1("StartTime", float) = 0
		_Dt2("StartTime2", float) = 0
	}
	SubShader {
		Tags{"Queue"="Transparent"}
		GrabPass{}
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert vertex:vert
		#pragma glsl
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _Disturb;
		sampler2D _Honeycomb;
		half _Intensity;
		half _Self;
		half _Trans;
		half _SideLen;
		half _HidTime;
		half _AprTime;
		half _FlsTime;
		half _Tail;
		half _Tall;
		float _Dt1;
		float _Dt2;
		sampler2D _GrabTexture;		

		struct Input {
			float2 uv_MainTex;
			float3 world_pos;
			float3 world_normal;
			float4 screenPos;
		};
		void vert (inout appdata_full v, out Input o)
		{
		    o.world_pos = v.vertex;
		    o.world_normal = normalize(v.normal);
		}		
		void surf (Input IN, inout SurfaceOutput o) {
			float3 org = tex2D(_MainTex, IN.uv_MainTex).rgb;
			float3 org2 = org * _Trans;
			float scw = IN.screenPos.w;
			float2 uvbase = float2(IN.screenPos.x / scw, 1 - IN.screenPos.y / scw);
			float3 grabbase = (tex2D(_GrabTexture, uvbase + (tex2D(_Disturb, IN.screenPos.xy / scw + _Time.xx * _Self).rg -0) * _Intensity)) * (1 - _Trans);			
			float coory = 0.4985 - IN.world_pos.x / _Tall + _Dt1 / _HidTime;
			float3 hexagoncolor = float3(0.5,1,1) * tex2D(_Honeycomb, float2(IN.world_pos.z, IN.world_pos.x * 1.010363) / _SideLen).r + float3(0.75,1.5,1.5) * (1 - abs(IN.world_normal.y));
			float3 tailcolor = grabbase + hexagoncolor * (1 + _Tail - coory) / _Tail;
			float3 alb = org2;
			float3 emis;
			if(coory < 1 - 0.0015)
			{
				alb = org;
				emis = half3(0,0,0);
			}
			else if(coory < 1 + 0.0015)					
				emis = float3(0.7,1,1);			
			else if(coory < 1 + _Tail)
				emis = tailcolor;
			else
				emis = grabbase;							
			o.Emission = emis;
			o.Albedo = alb;

			float wx = IN.world_pos.x / _SideLen * 1.010363;
			float wy = IN.world_pos.z / _SideLen;
			float lx = frac(wx * 12);
			float ly = frac(wy * 14);
			float centerindex;
			
			if((abs(ly - 0.5) - 0.16666) * 1.5 > abs(abs(lx - 0.5) - 0.25))
				centerindex = 25 + int(lx * 2) * 50;
			else
				centerindex = 150 - int(abs(lx - 0.5) * 4) * 50;
			float2 centerpos = float2(frac(centerindex / 100), int(centerindex / 100) * 0.5);			
			lx -= centerpos.x;
			if(lx > 0.5)
				lx = lx - 1;
			ly -= centerpos.y;
			if(ly > 0.5)
				ly = ly - 1;
			half lr = sqrt(lx * lx + ly * ly);		// 0~0.33333			
			half la = atan(ly / lx) + 1.5708;		// -pi/2~ pi/2			
			half startt = tex2D(_Honeycomb, float2(wx, wy)).g * _AprTime;
			half cellproc = (_Dt2 - startt) / _FlsTime;
			if (_Dt2 * _Dt2 > startt * _Dt2)
				if(1.33333 - cellproc < lr * 3 + fmod(la / 3.14159, 0.33333))
					o.Emission = org;
				else if(1.33333 - cellproc - lr * 3 - fmod(la / 3.14159, 0.33333) < 0.1)
					o.Emission = float3(0.7,1,1);
				else
					o.Emission = grabbase;
		}
	
		ENDCG
	} 
	FallBack "Diffuse"	
}