Shader "Zhouxun/PlanetHalo Standard"
{
	Properties 
	{
		_PlanetPos("Planet Pos(xyz) & Radius (w)", Vector) = (0,0,0,1)
		_AABorderColor("AA Border Color", Color) = (0.1,0.1,0.2,1)
		_AABorderThickness("AA Border Thickness", Float) = 0.01
		_HaloOuterParams("Outer (Thickness1, Power1, Thickness2, Power2)", Vector) = (0.1,2,0.3,5)
		_HaloOuterColor1("Halo Outer Color 1", Color) = (0,0.5,1,1)
		_HaloOuterColor2("Halo Outer Color 2", Color) = (0,0.5,1,1)
		_HaloInnerParams("Inner (Thickness1, Power1, Thickness2, Power2)", Vector) = (0.1,3,0.6,2)
		_HaloInnerColor1("Halo Inner Color 1", Color) = (0,0.5,1,1)
		_HaloInnerColor2("Halo Inner Color 2", Color) = (0,0.5,1,1)
		_HaloInnerColor3("Halo Inner Color 3", Color) = (0,0.1,0.2,1)
		_Intensity("Global Intensity", Float) = 1
		_SunlightDir("Sunlight Direction", Vector) = (1,1,0,0)
	}

	Subshader 
	{
		Tags
		{		
			"RenderType"="Transparent"
			"Queue" = "Transparent"
		}
		Fog { Mode Off }
	  	Pass
	  	{
			Blend One One
			ZWrite Off
			Cull Back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
						
			float4 _PlanetPos;
			float4 _HaloOuterParams;
			half4 _HaloOuterColor1;
			half4 _HaloOuterColor2;
			float4 _HaloInnerParams;
			half4 _HaloInnerColor1;
			half4 _HaloInnerColor2;
			half4 _HaloInnerColor3;
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
				float diff = abs(alt - 1);
				
				float ndotl = dot(normalize(i.wnorm.xyz), normalize(_SunlightDir.xyz));
				float sun_intens = pow(ndotl + 2,1.3);
				
				if (alt > 1)
				{
					float thick1 = _HaloOuterParams.x;
					float power1 = _HaloOuterParams.y;
					float thick2 = _HaloOuterParams.z;
					float power2 = _HaloOuterParams.w;
					float u1 = saturate((thick1 - diff) / thick1);
					float u2 = saturate((thick2 - diff) / thick2);
					return (_HaloOuterColor1 * pow(u1, power1) + _HaloOuterColor2 * pow(u2, power2)) * _Intensity * sun_intens;
				}
				else
				{
					float thick1 = _HaloInnerParams.x;
					float power1 = _HaloInnerParams.y;
					float thick2 = _HaloInnerParams.z;
					float power2 = _HaloInnerParams.w;
					float u1 = saturate((thick1 - diff) / thick1);
					float u2 = saturate((thick2 - diff) / thick2);
					return (_HaloInnerColor1 * pow(u1, power1) + _HaloInnerColor2 * pow(u2, power2) + _HaloInnerColor3) * _Intensity * sun_intens;
				}
			}	
			ENDCG
		}
	  	Pass
	  	{
	  		Blend SrcAlpha OneMinusSrcAlpha
			Zwrite Off
			Cull Back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
						
			float4 _PlanetPos;
			float4 _AABorderColor;
			float _AABorderThickness;
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
				float diff = abs(alt - 1);
				float ndotl = dot(normalize(i.wnorm.xyz), normalize(_SunlightDir.xyz));
				float sun_intens = pow(ndotl + 2,1.3);
				
				float thick = _AABorderThickness*sun_intens;
				float u = saturate((thick - diff) / thick);
				float4 retval = _AABorderColor * sun_intens * 0.5;
				
				retval.a = pow(u,3);
				return retval;
			}	
			ENDCG
		}
	}
}
