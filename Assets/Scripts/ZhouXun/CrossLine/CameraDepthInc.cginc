#ifndef CAMDEPTH_CG_INCLUDED
#define CAMDEPTH_CG_INCLUDED

#include "UnityCG.cginc"

#define  TIME_FACTOR   _Time.x*20
#define  PI            3.1415927
#define  DOUBLEPI      3.1415927*2

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
#endif