Shader "Voxel Creation/Science Cube"
{
	Properties
	{
		_NoiseMap("Noise map", 2D) = "black" {}
		_TintColor("Tint Color", Color) = (1,1,1,1)
		_Layer1 ("Layer 1 (Intensity, Thickness, Speed, Tile)", Vector) = (1,2,2,0.5)
		_Layer2 ("Layer 2 (Intensity, Thickness, Speed, Tile)", Vector) = (0.75,1,1,1)
		_Layer3 ("Layer 3 (Intensity, Thickness, Speed, Tile)", Vector) = (0.5,0.5,1,2)
		_Layer4 ("Layer 4 (Intensity, Thickness, Speed, Tile)", Vector) = (0.5,0.5,1,4)
		_Speed ("Speed", Float) = 0.1
	}
   
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent+100"}
		
		Pass
		{
			Fog { Mode Off }
			Blend One One
			Zwrite Off
			Cull Off 
			Lighting Off 
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
			#pragma glsl 
			
			sampler2D _NoiseMap;
			float4 _TintColor;
			float4 _Layer1; 
			float4 _Layer2; 
			float4 _Layer3; 
			float4 _Layer4;
			float _Speed;
			
			struct v2f 
		 	{
				float4 pos : POSITION;
				float3 pos_loc : TEXCOORD0;
			};

			float BrightnessMapping(float4 layer_setting, float3 coord)
			{
				float2 tilecoord = (coord.xz) / layer_setting.w + 0.5;
				float2 tileidx = floor(tilecoord);
				float2 tileofs = frac(tilecoord);
				float4 rc = tex2D(_NoiseMap, tileidx.xy/64).rgba;
				float2 br_pos = abs(tileofs - 0.5);
				float2 dist = float2((rc.r*0.2+0.2) - br_pos.x, (rc.g*0.2+0.2) - br_pos.y);
				float mindist = min(dist.x, dist.y);
				float br1 = max((mindist), 0) * 1;
				float br2 = max((0.05*layer_setting.y-mindist), 0) * 20/layer_setting.y;
				return (pow(br2,9)*1000+(0.5-br1))*br1 * layer_setting.x;
			}
			
			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.pos_loc = v.vertex.xyz;
				return o; 
			}
			
		 	half4 frag(v2f v) : COLOR0
         	{
				return	BrightnessMapping(_Layer1, v.pos_loc + _Time.xyz*_Speed*_Layer1.z)*_TintColor + 
						BrightnessMapping(_Layer2, v.pos_loc + _Time.yzw*_Speed*_Layer2.z)*_TintColor +
						BrightnessMapping(_Layer3, v.pos_loc + _Time.zwx*_Speed*_Layer3.z)*_TintColor +
						BrightnessMapping(_Layer4, v.pos_loc + _Time.wxy*_Speed*_Layer4.z)*_TintColor;
         	}
         	ENDCG
      	} 
	}
}
