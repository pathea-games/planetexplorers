Shader "WuYiqiu/LocateCubeEffect" 
{
	Properties
	{
		_TintColor ("Tint color", Color) = (1,1,1,1)
		_Brightness ("Brightness", Range(0.2, 10)) = 1
		_Length ("Length ", Float)	= 0.5
		_Height ("Height ", Float)  = 0.5
		_MainTex("Main Texture", 2D) = "white" {}
		_CenterWorldPos ("Center world position", Vector) = (0,0,0)
		_NoiseTexture("Noise Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		LOD 200
		Fog { Mode Off }
		Pass
	  	{
			Blend One One
			Cull Off
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"

			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;	
			};

			struct v2f 
			{
				float3 vertPos : TEXCOORD0;
				float4 pos : POSITION0;
	 			float4 worldPos : TEXCOORD2;
				float2 texPos :  TEXCOORD1;
			};

			half4 _TintColor;
			float _Brightness;
			float _Height;
			float _Length;
			float4 _CenterWorldPos;
			sampler2D _NoiseTexture;
			sampler2D _MainTex;
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.vertPos.xyz = v.vertex.xyz;	
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.worldPos = mul(_Object2World, v.vertex);
				//_object2world
				o.texPos = v.texcoord;
				return o;
			}
			
			float randBrightness (float2 coord)
			{
				float3 col = tex2D(_NoiseTexture, coord).xyz;
				return col.r*0.299 + col.g*0.587 + col.b*0.114;
			}

			
			half4 frag (v2f i) : COLOR
			{
				// cal relative pos
				float4 localPos = i.worldPos - _CenterWorldPos;
				
				half4 main_color = tex2D(_MainTex, i.texPos);
				
				float br_rand = (randBrightness(_Time.yy * 0.2) -0.5) * 0.5 + 1;
				
				float factor = abs( step(_Height, - localPos.y) - 1);
				float br_rand_2 = (randBrightness(localPos.xz + _Time.yy * 0.15) - 0.5) *0.5*factor + 1;
				
				float br_y  = pow( clamp( abs(localPos.y - _Height), 0,  _Height*2) * 0.5 / _Height, 4) ;
				float br_xz = pow( saturate(max( abs(localPos.z), abs(localPos.x)) - _Length *0.5) / (_Length * 0.5),  2 );
				
				float br = br_y * br_xz * br_rand * br_rand_2;
             	return  main_color* _TintColor * br * _Brightness;
			}
			ENDCG 
		}
	}
	
	FallBack "Diffuse"
}
