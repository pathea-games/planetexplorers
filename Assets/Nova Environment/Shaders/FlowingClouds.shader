Shader "NovaEnv/Flowing Clouds" 
{
	Properties 
	{
		_NoiseTexture("Noise Texture", 2D) = "white" {}
		_CloudTile("Cloud Tile", float) = 15
		_CloudThreshold("Cloud Threshold", float) = 1.5
		_CloudDensity("Cloud Density", float) = 3
		_CloudColor1("Cloud Color 1", Color) = (1,1,1)
		_CloudColor2("Cloud Color 2", Color) = (1,1,1)
		_CloudColor3("Cloud Color 3", Color) = (1,1,1)
		_CloudColor4("Cloud Color 4", Color) = (1,1,1)
		_SunDirection("Sun Direction", Vector) = (0,0,0)
		_Overcast("Overcast", float) = 1
		_VisibleDist("Visible Distance", float) = 0.5
		_CloudOffset("Cloud Offset", Vector) = (0,0,0)
		_CloudFactor1("Cloud Factor 1",Vector) = (3,10,30,100)
		_CloudFactor2("Cloud Factor 2",Vector) = (3,10,30,40)
	}

	Subshader 
	{
		Tags
		{
			"RenderType"="Geometry"
			"Queue" = "Geometry+300"
		}
		Fog { Mode Off }
	  	Pass
	  	{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off
	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
			
			#define  ONEPX     1.0/256.0
			#define  HALFPX    0.5/256.0
	
			sampler2D _NoiseTexture;
			float _CloudTile;
			float _CloudThreshold;
			float _CloudDensity;
			float4 _CloudColor1;
			float4 _CloudColor2;
			float4 _CloudColor3;
			float4 _CloudColor4;
			float4 _SunDirection;
			float _Overcast;
			float _VisibleDist;
			float3 _CloudOffset;
			float4 _CloudFactor1;
			float4 _CloudFactor2;

			float4 randNum (float2 coord)
			{
				return tex2D(_NoiseTexture, coord).rgba;
			}
			
			float fade(float t)
			{
				return t;
			}
			
			float Noise3D(float3 P)
			{
				float3 Pi = ONEPX * floor(P) + HALFPX;
				float3 Pf = frac(P);

				float perm00 = randNum(Pi.xy).a ;
				float3  grad000 = randNum(float2(perm00, Pi.z)).rgb * 4.0 - 1.0;
				float n000 = dot(grad000, Pf);
				float3  grad001 = randNum(float2(perm00, Pi.z + ONEPX)).rgb * 4.0 - 1.0;
				float n001 = dot(grad001, Pf - float3(0.0, 0.0, 1.0));
	
				float perm01 = randNum(Pi.xy + float2(0.0, ONEPX)).a;
				float3 grad010 = randNum(float2(perm01, Pi.z)).rgb * 4.0 - 1.0;
				float n010 = dot(grad010, Pf - float3(0.0, 1.0, 0.0));
				float3  grad011 = randNum(float2(perm01, Pi.z + ONEPX)).rgb * 4.0 - 1.0;
				float n011 = dot(grad011, Pf - float3(0.0, 1.0, 1.0));
	
				float perm10 = randNum(Pi.xy + float2(ONEPX, 0.0)).a ;
				float3  grad100 = randNum(float2(perm10, Pi.z)).rgb * 4.0 - 1.0;
				float n100 = dot(grad100, Pf - float3(1.0, 0.0, 0.0));
				float3  grad101 = randNum(float2(perm10, Pi.z + ONEPX)).rgb * 4.0 - 1.0;
				float n101 = dot(grad101, Pf - float3(1.0, 0.0, 1.0));
	
				float perm11 = randNum(Pi.xy + float2(ONEPX, ONEPX)).a ;
				float3  grad110 = randNum(float2(perm11, Pi.z)).rgb * 4.0 - 1.0;
				float n110 = dot(grad110, Pf - float3(1.0, 1.0, 0.0));
				float3  grad111 = randNum(float2(perm11, Pi.z + ONEPX)).rgb * 4.0 - 1.0;
				float n111 = dot(grad111, Pf - float3(1.0, 1.0, 1.0));
	
				float4 n_x = lerp(float4(n000, n001, n010, n011),
				float4(n100, n101, n110, n111), fade(Pf.x));
				float2 n_xy = lerp(n_x.xy, n_x.zw, fade(Pf.y));
				float n_xyz = lerp(n_xy.x, n_xy.y, fade(Pf.z));
	
				return n_xyz;
			}
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
	
			struct v2f
			{
				float3 texc : TEXCOORD0;
				float3 wpos : TEXCOORD1;
				float4 pos : POSITION0;
			};
						
			v2f vert (appdata_v v)
			{
				v2f o;
	
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.wpos = mul(_Object2World, v.vertex).xyz;
				o.texc.xy = v.texcoord.xy;
				o.texc.z = 0;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				// calc cloud graph
				float3 noise3dcoord = i.texc.xyz * _CloudTile + _CloudOffset;
				//noise3dcoord += randNum(noise3dcoord.xy*10).xyz*0.01;
				float2 ofs = float2(Noise3D(noise3dcoord*0.1), Noise3D(noise3dcoord.yxz*0.1));
				noise3dcoord.xy += ofs*1;
				//return half4(ofs, 0, 1);
				
				float clouds = 1.0f;
				float noise = 0;
				float grad = 0;
				
				noise = Noise3D( noise3dcoord + (clouds*0.2f) );
				grad += noise*0.05;
				clouds += noise * 1.1f;
				
				noise = abs( Noise3D(( noise3dcoord + (clouds*0.19f) ) * 3.3f) );
				grad += noise*0.25;
				clouds += noise / 3.3f ;
				
				noise = abs( Noise3D(( noise3dcoord + (clouds*0.21f) ) * _CloudFactor1.x) );
				grad += noise*0.2;
				clouds += noise / _CloudFactor2.x;
				
				
				noise = abs( Noise3D(( noise3dcoord + (clouds*0.23f) ) * _CloudFactor1.y) );
				grad += noise*0.3;
				clouds += noise / _CloudFactor2.y;
				
				noise = abs( Noise3D(( noise3dcoord + (clouds*0.25f) ) * _CloudFactor1.z) );
				grad += noise*0.2;
				clouds += noise / _CloudFactor2.z;
				
				//clouds += abs( Noise3D(( noise3dcoord + (clouds*0.27f) ) * _CloudFactor1.w) ) / _CloudFactor2.w ;
				// coloring cloud
				float4 retclr = (1,1,1,1);
				
				// alpha
				float thickness = clouds - _CloudThreshold;
				retclr.a = thickness;
				
				// color
				float vdot = dot(normalize(i.wpos - _WorldSpaceCameraPos) * 1.1, normalize(_SunDirection.xyz)) * 0.5 + 0.5;
				vdot = pow(vdot, 4);
				
				float bright_thickness = /*max((1 - vdot)*1.2,0)+*/  0.2;
				float lightmap = max(pow(saturate(1 - abs(thickness - bright_thickness)),2), 0);
				retclr.rgb = lerp(_CloudColor4, _CloudColor2, saturate(vdot)) + lerp(_CloudColor3, _CloudColor1, saturate(vdot)) * lightmap;
				
				// Overcast
				float gray = retclr.r * 0.299 + retclr.g * 0.587 + retclr.b * 0.114;
				retclr.rgb = lerp(float3(gray, gray, gray), retclr.rgb, pow(_Overcast, 1.5));
				retclr.rgb = lerp(float3(0,0,0), retclr.rgb, _Overcast);
				
				// visibility
				retclr.a *= _CloudDensity;
				retclr.a *= saturate(1 - length(i.texc.xy-0.5)/_VisibleDist);
				retclr.a = saturate(retclr.a);
				
				return retclr;
			}
	
			ENDCG
		}
	}
} 