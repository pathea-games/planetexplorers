Shader "Voxel Creation/PolarNet"
{
	Properties
	{
		_MajorGird("Major Grid", Float) = 4
		_MajorTintColor("Major Tint Color", Color) = (1,1,1,1)
		_MinorGrid("Minor Grid", Float) = 1
		_MinorTintColor("Minor Tint Color", Color) = (1,1,1,1)
		_CellSize("Cell Size", Float) = 0.25
		_CellTintColor("Cell Tint Color", Color) = (1,1,1,1)
		_GridThickness("Grid Thickness", Float) = 0.1
		_NoiseMap("Noise map", 2D) = "black"
	}
   
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent+100"}
		
		Pass
		{
			Fog {Mode Off}
			Blend One One
			Zwrite Off
			Cull Off 
			Lighting Off 
			
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members pos_nm)
#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
			#pragma glsl 
			
			float _MajorGird;
			float4 _MajorTintColor;
			float _MinorGrid;
			float4 _MinorTintColor;
			float _CellSize;
			float4 _CellTintColor;
			float _GridThickness;

            sampler2D _NoiseMap;
			
			struct v2f 
		 	{
				float4 pos : POSITION;
				float3 pos_nm : TEXCOORD0;
			};

			float BrightnessMapping(float coord, float thickness)
			{
				float delta = clamp(abs(coord-0.5) * (2 + thickness), 0, 1);
				return pow(delta,0.5);
			}
			
			float Min3(float3 vec3)
			{
				return min(min(vec3.x,vec3.y),vec3.z);
			}
			float Max3(float3 vec3)
			{
				return max(max(vec3.x,vec3.y),vec3.z);
			}
			#define TEXEL 0.015625
			float RandBrightness(float2 pos)
			{
				pos.y += _Time.x*5;
				return tex2D(_NoiseMap, pos).r;
			}

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.pos_nm = normalize(v.vertex.xyz);
				return o; 
			}
			
		 	half4 frag(v2f v) : COLOR0
         	{
         		float3 wPos = 0;
				wPos.z = 0;
				wPos.x = atan2(v.pos_nm.x, v.pos_nm.z) / 3.1415927 * 180;
				float latitude = asin(v.pos_nm.y);
				float coslatitude = cos(latitude);
				wPos.y = latitude / 3.1415927 * 180;
         		
         		float3 time = _Time.xyz;

                float bpos = frac(-time.y*0.5f) - 0.25f;
         		
         		float thickness = _GridThickness / coslatitude;
         		
         		float3 cell = frac(wPos/_CellSize);
         		float brightness_cell = max(BrightnessMapping(cell.x, thickness) , BrightnessMapping(cell.y, thickness));

         		float3 minor = frac(wPos/_MinorGrid);
         		float br_minorx = BrightnessMapping(minor.x, thickness);
         		float br_minory = BrightnessMapping(minor.y, thickness);
         		float brightness_minor = max(br_minorx , br_minory);
         		                                         
         		float3 major = frac(wPos/_MajorGird);
         		float brightness_major = max(BrightnessMapping(major.x, thickness) , BrightnessMapping(major.y, thickness));
         		
         		float3 br_rand_longi = tex2D(_NoiseMap, float2(0,floor(wPos/_MinorGrid-0.5f).y*TEXEL)).rgb;
         		float br_longi = (time.y*(br_rand_longi.g-0.5) + br_rand_longi.r * 6.28)*10;
         		float br_cos = cos(br_longi);
         		float br_sin = sin(br_longi);
         		float2 br_coord = float2(br_cos,br_sin);
         		float dist = length(br_coord.xy-normalize(v.pos_nm.xz));
         		br_longi = clamp((0.4-dist) * 3, 0,1) * clamp((br_minory - 0.98)*50, 0,1) * coslatitude;
         		br_longi *= br_longi;
         		
         		float3 br_rand_lati = tex2D(_NoiseMap, float2(0,floor(wPos/_MinorGrid-0.5f).x*TEXEL)).rgb;
         		float br_lati = (time.y*(br_rand_lati.r-0.5) + br_rand_lati.g * 6.28)*10;
         		float br_y = sin(br_lati);
         		dist = abs(br_y-v.pos_nm.y);
         		br_lati = clamp((0.4-dist) * 3, 0,1) * clamp((br_minorx - 0.98)*50, 0,1) * coslatitude;
         		br_lati *= br_lati;
         		
         		float rand_bright = RandBrightness(float2(wPos.x/10, wPos.y/180));
				float4 c = 0;
				c.rgb = (pow(brightness_cell,50) * _CellTintColor * coslatitude + (pow(brightness_minor,30)*(1+br_longi*4 + br_lati*4)) * _MinorTintColor + pow(brightness_major,10) * _MajorTintColor) * (1+rand_bright*0.2);
				c.a = 1;
				
				return c;
         	}
         	ENDCG
      	} 
	}
}
