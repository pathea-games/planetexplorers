Shader "Zhouxun/PlanetHalo Sun"
{
	Properties 
	{
		_PlanetPos("Planet Pos(xyz) & Radius (w)", Vector) = (0,0,0,1)
		_SunRiseColor("AA Border Color", Color) = (0.7,0.3,0.1,1)
		_SunRiseThickness("AA Border Thickness", Float) = 0.2
		_Intensity("Global Intensity", Float) = 1
		_SunlightDir("Sunlight Direction", Vector) = (1,1,0,0)
	}

	Subshader 
	{
		Tags
		{		
			"RenderType"="Transparent"
			"Queue" = "Transparent+1"
		}
		Fog { Mode Off }
	  	Pass
	  	{
	  		Blend One One
			Zwrite Off
			Cull Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
						
			float4 _PlanetPos;
			float4 _SunRiseColor;
			float _SunRiseThickness;
			float _Intensity;
			half4 _SunlightDir;
						
			struct appdata_v
			{
				float4 vertex : POSITION;
			};
	
			struct v2f
			{
				float3 wpos : TEXCOORD0;
				float3 wnorm : TEXCOORD1;
				float4 pos : POSITION0;
			};
		
	
			v2f vert (appdata_v v)
			{
				v2f o;
	 
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.wpos = mul(_Object2World, v.vertex).xyz;
				o.wnorm = mul(_Object2World, float4(normalize(v.vertex.xyz),0)).xyz;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float radius = _PlanetPos.w;
				float3 pixelpos = i.wpos;
				float3 directrix = _PlanetPos - _WorldSpaceCameraPos;
				float3 viewvec = pixelpos - _WorldSpaceCameraPos;
				float cosp = dot(normalize(directrix), normalize(viewvec));
				float dist = length(directrix);
				float alt = sqrt(1 - cosp * cosp) * dist / radius;
				
				float coss = saturate((1 - dot(normalize(directrix), normalize(_SunlightDir.xyz)))*1000);
				
				float diff = (alt > 1 ? (alt - 1) * 3 : (1 - alt));
				float sdotd = dot(normalize(viewvec), normalize(_SunlightDir.xyz));
				if (sdotd < 0)
					discard;
				float sun_intens1 = pow(sdotd,100) * coss;
				
				float thick = _SunRiseThickness ;
				float u = saturate((thick - diff) / thick);
				float4 retval = _SunRiseColor * sun_intens1 * pow(u,3) * _Intensity;

				return retval;
			}	
			ENDCG
		}
	}
}
