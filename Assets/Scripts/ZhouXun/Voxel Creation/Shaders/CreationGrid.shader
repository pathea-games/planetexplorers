Shader "Voxel Creation/Grid"
{
	Properties
	{
		_FocusCenter("Focus Center",Vector) = (6,0,6,0)
		_MaxDist("Max Distance", float) = 8
		_BoundingBox("Bounding Box", Vector) = (32,16,32,0)
		_MajorGrid("Major Grid", Float) = 4
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
		Tags { "RenderType"="Opaque" "Queue"="Transparent+21"}
		
		Pass
		{
			Blend One One 
			Zwrite Off
			Cull Off 
			Lighting Off 
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
			
			float4 _FocusCenter;
			float _MaxDist;
			float4 _BoundingBox;
			float _MajorGrid;
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
				float4 texCoord : TEXCOORD0;
			};

			float BrightnessMapping(float coord, float thickness)
			{
				float delta = clamp(1-abs(coord-0.5) * (2 + thickness), 0, 1);
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
			float RandBrightness(float3 pos)
			{
				pos += (_Time.xyz);
				return tex2D(_NoiseMap, float2(pos.xy)) 
				     + tex2D(_NoiseMap, float2(pos.yz)) 
				     + tex2D(_NoiseMap, float2(pos.zx));
			}

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texCoord = mul(_Object2World, v.vertex);
				return o;
			}
			
		 	half4 frag(v2f v) : COLOR0
         	{
         		float3 wPos = v.texCoord.xyz;
         		
         		float3 time = _Time.xyz;

                float bpos = frac(-time.y*0.5f) - 0.25f;
         		
         		
         		float3 cell = frac(wPos/_CellSize);
         		float brightness_cell = 1 - Max3(float3(min(BrightnessMapping(cell.x, _GridThickness) , BrightnessMapping(cell.y, _GridThickness)), 
         		                                        min(BrightnessMapping(cell.y, _GridThickness) , BrightnessMapping(cell.z, _GridThickness)), 
         		                                        min(BrightnessMapping(cell.z, _GridThickness) , BrightnessMapping(cell.x, _GridThickness))));

         		float3 minor = frac(wPos/_MinorGrid);
         		float3 minor2 = frac(wPos/(_MinorGrid*2));
         		float3 c_minor = abs(minor2 - 0.5);
         		float pos = Min3(c_minor);
         		float pb_minor = clamp(((0.1 - abs(bpos-pos))*10), 0,1) * (pos + 0.1);
         		
         		float brightness_minor = 1 - Max3(float3(min(BrightnessMapping(minor.x, _GridThickness) , BrightnessMapping(minor.y, _GridThickness)), 
         		                                         min(BrightnessMapping(minor.y, _GridThickness) , BrightnessMapping(minor.z, _GridThickness)), 
         		                                         min(BrightnessMapping(minor.z, _GridThickness) , BrightnessMapping(minor.x, _GridThickness))));
         		float coef = pow(clamp( (brightness_cell - 0.5) * 2, 0,1 ), 2);
         		brightness_minor *= (1+pb_minor*coef * 0.9f);
         		                                         
         		float3 major = frac(wPos/_MajorGrid);
         		float brightness_major = 1 - Max3(float3(min(BrightnessMapping(major.x, _GridThickness) , BrightnessMapping(major.y, _GridThickness)), 
         		                                         min(BrightnessMapping(major.y, _GridThickness) , BrightnessMapping(major.z, _GridThickness)), 
         		                                         min(BrightnessMapping(major.z, _GridThickness) , BrightnessMapping(major.x, _GridThickness))));
         		
         		float rbright = RandBrightness(wPos);
				float4 c = 0;
				c = (pow(brightness_cell,5) * _CellTintColor + pow(brightness_minor,5) * _MinorTintColor + pow(brightness_major,5) * _MajorTintColor) * (1+rbright*0.1);
				c.a = 1;

				return c;
         	}
         	ENDCG
      	} 
	}
}
