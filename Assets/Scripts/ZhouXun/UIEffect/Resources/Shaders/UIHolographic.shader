Shader "Zhouxun/UIHolographic"
{
	Properties
	{
		_Color ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Specular Texture", 2D) = "white" {}
		_RandomTex ("Noise", 2D) = "bump" {}
		_RandomTex2 ("Noise2", 2D) = "bump" {}
		_AlphabetTex ("Alphabet Texture", 2D) = "white" {}
		_Intensity ("Intensity", Float) = 1.8
		_Speed ("Intensity", Float) = 1
		_Height ("Height", Float) = 64
		_Tile ("Tile", Vector) = (48,7,0,0)
		_Twinkle("Twinkle",Float) = 0.7
	}
	SubShader
	{
		LOD 100
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		Pass
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			ColorMask RGB
			//AlphaTest Greater .01
			Blend One One
			
			CGPROGRAM
			#pragma exclude_renderers gles
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
	
			float4 _Color;
			sampler2D _MainTex;
			sampler2D _RandomTex;
			sampler2D _RandomTex2;
			sampler2D _AlphabetTex;
			float _Intensity;
			float _Speed;
			float _Height;
			float4 _Tile;
			float _Twinkle;
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
			struct v2f 
			{
				float2 texc : TEXCOORD0;
				float4 pos : POSITION0;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord;
				return o;
			}
			
			float random (float2 coord)
			{
				float3 c = tex2D(_RandomTex2, float2(_Tile.x * coord.x,0)/64).rgb;
				coord.y += _Time.y*(c.g+0.3) * _Speed;
				float2 final_coord = coord.xy*_Tile.xy/64;
				c = tex2D(_RandomTex, final_coord).rgb;
				return c.r;
			}
			
			float random2 (float2 coord)
			{
				float3 c = tex2D(_RandomTex2, float2(_Tile.x * coord.x,0)/64).rgb;
				coord.y += _Time.y*(c.g+0.3) * _Speed;
				coord.x += int(_Time.y*4*64)/64;
				float2 final_coord = coord.xy*_Tile.xy/64;
				c = tex2D(_RandomTex2, final_coord).rgb;
				return int(c.r*10)*0.1;
			}
			
			float2 number (float n, float2 coord)
			{
				float2 acoord_base = float2(n, 0);
				float3 c = tex2D(_RandomTex2, float2(_Tile.x * coord.x,0)/64).rgb;
				coord.y += _Time.y*(c.g+0.3) * _Speed;
				return coord.xy*float2(_Tile.x/10,_Tile.y) + acoord_base;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 mask = tex2D(_MainTex, i.texc.xy);
				half random1 = pow(random(i.texc.xy),2);
				half4 c = tex2Dlod(_AlphabetTex, float4(number(random2(i.texc.xy), i.texc.xy),0,0));
				half scr = sin(i.texc.y*_Height*3.14159);
				return (mask * (random1 * c + scr*0.2)) * _Intensity * _Color * (scr*0.3+1) * (random2(_Time.xy*10) * _Twinkle+1);
			}
			ENDCG 
		}
	} 
	FallBack "Diffuse"
}
