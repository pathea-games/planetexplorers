Shader "zhouxun/HolographicForScan"
{
	Properties
	{
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		LOD 200
		Fog { Mode off }
		Blend One One
		Cull off
		Zwrite off
            
		Pass
		{
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members v_pos)
#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
		    #pragma target 3.0
			#include "UnityCG.cginc"
	 
	 		float4 _Color_4Scan;
	 		float4 _HoloColor_4Scan;
			sampler2D _NoiseTex_4Scan;
			float _Tile_4Scan;
			float _Speed_4Scan;
			float _BodyIntensity_4Scan;
			float _HoloIntensity_4Scan;
			float _NoiseIntensity_4Scan;
			float4 _CameraPos_4Scan;
			float4 _CameraForward_4Scan;
			float _TimeFactor_4Scan;
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
	
			struct v2f 
			{
				float4 pos : POSITION;
				float3 v_pos : TEXCOORD0;
			};
	
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.v_pos.xyz = mul(_Object2World, v.vertex);	
				return o;
			}
			
			#define ONEPX 0.015625
			#define HALFPX 0.0078125
			
			float3 randIntensity (float coord)
			{
				coord *= _Tile_4Scan;
				float2 coord_0 = float2(coord, coord*ONEPX);
				
				float3 randnum = tex2D(_NoiseTex_4Scan, coord_0).rgb;
				return randnum;
			}
			
			float BrightnessMapping(float coord, float thickness)
			{
				float delta = clamp(abs(coord-0.5) * (2 + thickness), 0, 1);
				return pow(delta,0.5);
			}
			
			half4 frag (v2f i) : COLOR
			{
				float3 wPos = i.v_pos.xyz;
         		float3 cell = frac(wPos/25);
         		float brightness_cell = max(BrightnessMapping(cell.x, .05) , BrightnessMapping(cell.z, .05));

         		float3 minor = frac(wPos/100);
         		float brightness_minor = max(BrightnessMapping(minor.x, .05) , BrightnessMapping(minor.z, .05));
         		                                         
         		float3 major = frac(wPos/200);
         		float brightness_major = max(BrightnessMapping(major.x, .02) , BrightnessMapping(major.z, .02));
         		
				float dist_intense = clamp(1.5 - length(i.v_pos.xyz - _CameraPos_4Scan.xyz) / _CameraPos_4Scan.w, 0,1);
				float player_mark = clamp((1 - abs(length(i.v_pos.xz - _CameraPos_4Scan.xz) - 5))*1.5,0,1);
				float player_forward_mark = clamp((1 - abs(length(i.v_pos.xz - (_CameraPos_4Scan.xz + _CameraForward_4Scan.xz*5)))),0,1);
//				_TimeFactor_4Scan = _Time.y;
//				float xintensity = pow(randIntensity(i.v_pos.x + _TimeFactor_4Scan*_Speed_4Scan).r, 50);
//				float yintensity = pow(randIntensity(i.v_pos.y + _TimeFactor_4Scan*_Speed_4Scan).g, 50);
//				float zintensity = pow(randIntensity(i.v_pos.z + _TimeFactor_4Scan*_Speed_4Scan).b, 50);
//				float inc = (randIntensity(i.v_pos.x + _TimeFactor_4Scan*_Speed_4Scan*0.3).r + randIntensity(i.v_pos.y + _TimeFactor_4Scan*_Speed_4Scan*0.3).g + randIntensity(i.v_pos.z + _TimeFactor_4Scan*_Speed_4Scan*0.3).b) * _NoiseIntensity_4Scan;
//				float coef = (randIntensity(_TimeFactor_4Scan).r * 0.2 + 0.9) * _HoloIntensity_4Scan;
//				return dist_intense*(_Color_4Scan*_BodyIntensity_4Scan + _HoloColor_4Scan*(xintensity+yintensity+zintensity+inc*inc*inc)*coef);
				return dist_intense*(_Color_4Scan*(_BodyIntensity_4Scan) + _HoloColor_4Scan*_HoloIntensity_4Scan*(pow(brightness_cell,200) + pow(brightness_minor,200) + pow(brightness_major,200) + pow(player_mark+player_forward_mark, 5)));
			}
			ENDCG
	    }
//		Pass
//		{
//        	ZTest Greater
//	        Material
//	        {
//		 		Diffuse [_NotVisibleColor_4Scan]
//	      		Ambient [_NotVisibleColor_4Scan]
//	        }
//	        Lighting On
//        }
	} 
	FallBack "Diffuse"
}
