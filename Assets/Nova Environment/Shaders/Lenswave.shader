Shader "NovaEnv/Lenswave"
{
	Properties
	{
		_LensMap ("Lens map", 2D) = "white" {}
		_NoiseMap ("Noise map", 2D) = "white" {}
		_TintColor ("Tint color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Blend One One

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			sampler2D _LensMap;
			sampler2D _NoiseMap;
			float4 _TintColor;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float3 wpos : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			// 按法线折射坐标
			float4 Refraction (float4 grab_pos, float2 norm, float strength, float aspect)
			{
				norm.y *= aspect;
				grab_pos.xy /= grab_pos.ww;
				grab_pos.xy += (norm * strength / grab_pos.ww);
				grab_pos.xy *= grab_pos.ww;
				return grab_pos;
			}

			// 1x1x1 随机颜色块
			#define  ONEPX     1.0/64.0
			#define  JUMPPX    61.0/64.0
			
			float3 RandNum (float3 coord)
			{
				float nz = floor(coord.z);
				coord *= ONEPX;
				float2 _coord = coord.xy;
				_coord.y += JUMPPX * nz;
				return tex2D(_NoiseMap, _coord).rgb;
			}
			
			// Perlin Noise 插值函数
			float Fade (float t) { return t * t * (3 - t - t); }
			
			// 一个简单的Perlin Noise 输出3个分量
			float3 Noise3D (float3 P)
			{
				float3 n = floor(P.xyz);
				float3 t = frac(P.xyz);
				
				float3 h000 = RandNum(n+float3(0,0,0)).rgb;
				float3 h100 = RandNum(n+float3(1,0,0)).rgb;
				float3 h010 = RandNum(n+float3(0,1,0)).rgb;
				float3 h110 = RandNum(n+float3(1,1,0)).rgb;
				float3 h001 = RandNum(n+float3(0,0,1)).rgb;
				float3 h101 = RandNum(n+float3(1,0,1)).rgb;
				float3 h011 = RandNum(n+float3(0,1,1)).rgb;
				float3 h111 = RandNum(n+float3(1,1,1)).rgb;
				
				return saturate(lerp(
				lerp(lerp(h000, h100, Fade(t.x)),lerp(h010, h110, Fade(t.x)),Fade(t.y)),
				lerp(lerp(h001, h101, Fade(t.x)),lerp(h011, h111, Fade(t.x)),Fade(t.y)),
				Fade(t.z)));
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.wpos = mul(_Object2World, v.vertex).xyz;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 pos = i.wpos + float3(0, _Time.y * 1, 0);
				float3 _pos = pos;
				_pos.x = 0.7 * pos.y + 0.3 * pos.z;
				_pos.y = 0.7 * pos.z + 0.3 * pos.x;
				_pos.z = 0.7 * pos.x + 0.3 * pos.y;
				// sample the texture
				float2 lc = Noise3D(_pos*3).xy + Noise3D(_pos*4).xy * 0.5 + Noise3D(_pos*5).xy * 0.25;
				fixed4 col = tex2D(_LensMap, lc) * _TintColor * 2;
				col = pow(col, 2);
				col *= saturate(97-i.wpos.y);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
