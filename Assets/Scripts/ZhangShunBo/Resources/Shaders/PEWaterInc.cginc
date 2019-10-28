#ifndef WATER_CG_INCLUDED
#define WATER_CG_INCLUDED

#include "UnityCG.cginc"

#define  TIME_FACTOR   _Time.x*20
#define  PI            3.1415927
#define  DOUBLEPI      3.1415927*2

inline half3 WaterNormal2(sampler2D bumpMap, half2 wpos, half tiling, half speed, half strength) 
{
	half2 coords = wpos*tiling;
	half ofs = speed*TIME_FACTOR*0.1;
	half4 bump = 0;
	bump += tex2D(bumpMap, half2(coords.x+0.5, coords.y) + half2(1,0)*ofs);
	bump += tex2D(bumpMap, half2(-coords.x, coords.y) + half2(1,0)*ofs);
	bump.xy = (bump.xy - half2(1.0, 1.0))*strength;
	bump.z = 1;
	return normalize(bump.xyz);
} 

inline half3 WaterNormal4(sampler2D bumpMap, half2 wpos, half tiling, half speed, half strength) 
{
	half2 coords = wpos*tiling;
	half ofs = speed*TIME_FACTOR*0.1;
	half4 bump = 0;
	bump += tex2D(bumpMap, half2(coords.x+0.5, coords.y) + half2(1,0)*ofs);
	bump += tex2D(bumpMap, half2(-coords.x, coords.y) + half2(1,0)*ofs);
	bump += tex2D(bumpMap, half2(coords.x, coords.y+0.5) + half2(0,1)*ofs);
	bump += tex2D(bumpMap, half2(coords.x, -coords.y) + half2(0,1)*ofs);
	bump.xy = (bump.xy * 0.5 - half2(1.0, 1.0))*strength;
	bump.z = 1;
	return normalize(bump.xyz);
}

inline float2 FlowCoord(float2 wpos, half2 vel, float T, float tofs)
{
	float invT = 1.0/T;
	float phase = TIME_FACTOR*invT;
	float t = frac(phase);
	
    float2 timeofs = vel*(t+tofs)*T*(-5);
    return wpos + timeofs;
}

inline half3 FinalWaterNormal(sampler2D bumpMap, float tile, float2 wpos, half2 vel, float T)
{
	float2 flowcoord = 0;
	flowcoord = FlowCoord(wpos, vel, T, 0);
	half3 n0 = WaterNormal4(bumpMap, flowcoord, tile, 0.5, 2).xzy;
	flowcoord = FlowCoord(wpos, vel, T, 1);
	half3 n1 = WaterNormal4(bumpMap, flowcoord, tile, 0.5, 2).xzy;
	
	return normalize(lerp(n1, n0, frac(TIME_FACTOR/T)));
}
			
float GetLED ( sampler2D depthtex, float4 screen_pos )
{
	return LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(depthtex, UNITY_PROJ_COORD(screen_pos))));
}

float CalcSink ( float delta_depth, float dens )
{
	return saturate(pow(max((delta_depth), 0)*dens, 0.5));
}

float4 RefractionProjectionPos ( float4 projpos, float3 norm, float sink, float strength, float2 texel_size )
{
	half2 bump = norm.xz;
	float2 ofs = bump * strength * texel_size * saturate(sink) * pow(projpos.z, 0.8);
	projpos.xy = projpos.xy + ofs;
	return projpos;
}

float4 GetGrabColor ( sampler2D grabtex, float4 screen_pos )
{
	return tex2Dproj(grabtex, UNITY_PROJ_COORD(screen_pos));
}

// Point light calc
float GetSpecularFactorPL(half3 lightDir, half3 nNormal, half3 nViewDir, half radius, half param, half power)
{
	half dist = length(lightDir);
	half3 nLightDir = normalize(lightDir);
	half k =  (1 - param*0.9) / (param * radius * radius);
	half atten = 1/(k*dist*dist + 0.9);
	half reflectiveFactor = max(0.0, dot(-nViewDir, reflect(nLightDir, nNormal)));
	return pow(reflectiveFactor, power) * atten;
}

half FoamIntensity(float2 wpos, half depth, half threshold, half speed)
{
	half t_depth = depth / threshold;
	half main_intens = pow(saturate(1-t_depth), 0.3);
	
	float wave_t_depth = (t_depth + TIME_FACTOR*speed + (wpos.x + wpos.y)*0.005)*3;
	wave_t_depth = floor(wave_t_depth) + pow(frac(wave_t_depth), 0.3);
	half wave_intens = pow((sin(wave_t_depth*PI)-0.3)*1.5,3);
	return clamp(main_intens*wave_intens, 0, 1);
}

#endif