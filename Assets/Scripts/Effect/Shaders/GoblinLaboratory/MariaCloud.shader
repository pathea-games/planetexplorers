Shader "SpecialItem/MariaCloud" {
	Properties 
	{
		_NoiseTexture("Noise Texture", 2D) = "white" {}
		_TimeFactor("Time Factor", float) = 0
		_CloudTile("Cloud Tile", float) = 10
		_CloudThreshold("Cloud Threshold", float) = 2
		_CloudDensity("Cloud Density", float) = 1.5 
		_CloudColor1("Cloud Color 1", Color) = (1,1,1)
		_CloudColor2("Cloud Color 2", Color) = (1,1,1)
		_CloudColor3("Cloud Color 3", Color) = (1,1,1)
		_CloudColor4("Cloud Color 4", Color) = (1,1,1)
		_SunDirection("Sun Direction", Vector) = (0,0,0)
		_Precipitation("Precipitation", float) = 1
		_VisibleDist("Visible Distance", float) = 8000
		_CloudSpeed("Cloud Speed", Vector) = (1,1,1)
		_CloudFactor1("Cloud Factor 1",Vector) = (3,10,30,100)
		_CloudFactor2("Cloud Factor 2",Vector) = (3,10,30,40)
		_MeshRotateX("Mesh Rotate", float) = 0
	}

	Subshader 
	{
		Tags
		{
			"RenderType"="Geometry"
			"Queue" = "Geometry +55"
		}
		Fog { Mode Off }
	  	Pass
	  	{
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off
	
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members normal)
#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
			
			#define  ONEPX     1.0/256.0
			#define  HALFPX    0.5/256.0
	
			sampler2D _NoiseTexture;			
			float _TimeFactor;
			float _CloudTile;
			float _CloudThreshold;
			float _CloudDensity;
			float4 _CloudColor1;
			float4 _CloudColor2;
			float4 _CloudColor3;
			float4 _CloudColor4;
			float4 _SunDirection;
			float _Precipitation;
			float _VisibleDist;
			float3 _CloudSpeed;
			float4 _CloudFactor1;
			float4 _CloudFactor2;
			float _MeshRotateX;

			struct appdata_v
			{
				float4 vertex : POSITION;
				float4 normal : NORMAL;				
				float2 texcoord : TEXCOORD0;
				float3 vertPos : COLOR0;				
			};
	
			struct v2f
			{
				float2 texCoord : TEXCOORD0;
				float3 vertPos : TEXCOORD1;
				float3 normal;
				float2 dist : COLOR1;
				float4 pos : POSITION0;
				
			};
						
			float4 randNum (float2 coord)
			{
				return tex2D(_NoiseTexture, coord).rgba;
			}
			
			float fade(float t)
			{
				return t*t*t*(t*(t*6.0-15.0)+10.0);
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
	
			v2f vert (appdata_v v)
			{
				v2f o;
	 
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texCoord = v.texcoord;
				o.vertPos.xy= o.texCoord;
				o.vertPos.z = 0;
				o.normal = normalize(mul(_Object2World, float4(v.normal.xyz, 0)).xyz);
				o.dist = length(v.vertex);				
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
			
//				// calc cloud graph
				float3 noise3dcoord = i.vertPos.xyz * _CloudTile + _TimeFactor * _CloudSpeed.xyz + float3(_MeshRotateX, 0, 0);
//				
				float clouds = 1.0f;
				clouds += Noise3D(noise3dcoord) * 1.1f;
				clouds += abs(Noise3D((noise3dcoord + clouds * 0.19f) * 3.3f)) / 5f;
				clouds += abs(Noise3D((noise3dcoord + clouds * 0.21f) * _CloudFactor1.x) ) / _CloudFactor2.x;
				clouds += abs(Noise3D((noise3dcoord + clouds * 0.23f) * _CloudFactor1.y) ) / _CloudFactor2.y;
				clouds += abs(Noise3D((noise3dcoord + clouds * 0.25f) * _CloudFactor1.z) ) / _CloudFactor2.z;

				// coloring cloud
				float4 retclr = (1,1,1,1);
				
				// alpha
				float alp = clouds - _CloudThreshold;
				//retclr.a = clouds - _CloudThreshold;
				
				// color
				float vdots = dot(_SunDirection.xyz, i.normal);
				float t = 0f;
				retclr.rgb = lerp( lerp( _CloudColor1, _CloudColor2, t ), lerp( _CloudColor3, _CloudColor4, t ), max(0.1f, -0.7f * vdots * vdots - 1.4f * vdots + 0.3f));
//				float var_scale = 4.0f;
//				float vdots = dot( float3(_SunDirection.x * 0.5f, 0, _SunDirection.z * 0.5f) - float3(0.5f - i.vertPos.x, 0, i.vertPos.y - 0.5f) * var_scale , _SunDirection );
//				float angle = atan(vdots);
//				float t = pow(clamp( (clouds - _CloudThreshold)*0.4f+0.3f, 0, 1 ), 1.5f);
//				retclr.rgb = lerp( lerp( _CloudColor1, _CloudColor2, t ), lerp( _CloudColor3, _CloudColor4, t ), angle );
				
				// precipitation
				retclr.rgb = lerp(float3(0,0,0), retclr.rgb, _Precipitation);		
				// visibility
				//alp = sqrt(alp);
				alp *= _CloudDensity;
				//alp = (alp > 0) ? alp * (1 - length(i.dist)/_VisibleDist) : 0;
				alp = saturate(alp);
				retclr.a = alp * (alp * (alp * 0.8 - 2.4) + 2.6);
				//if(abs(frac(noise3dcoord).x-0.5f) > 0.45f && abs(frac(noise3dcoord).y-0.5f) > 0.45f && abs(frac(noise3dcoord).z-0.5f) > 0.45f)
				//retclr = float4(1,0,0,1);
				//retclr.rgb = 1-angle;
				//retclr.a = 1;
				
				return retclr;
			}
	
			ENDCG
		}
	}
} 