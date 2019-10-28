Shader "NovaEnv/MoonBody"
{
	Properties
	{
		_TintColor("Tint Color", Color) = (1,1,1)
		_MainTexture ("Moon Texture", 2D) = "white" {}
		_NormalTexture ("Moon Normal Texture", 2D) = "bump" {}
		_MoonRect ("Moon Rect", Vector) = (0,0,1,1)
		_SunDir ("Sun Direction", Vector) = (0,0,-1,0)
		_CurrSkyColor("Current Sky Color", Color) = (0,0,0)
		_Overcast("Overcast", Float) = 1
	}
   
	SubShader
	{
		Tags
		{
			"RenderType"="Transparent"
			"Queue" = "Background+2"
		}
		Cull Back 
		Lighting Off 
		Fog { Mode Off }
		ZWrite Off
		
      	// Moon Body
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma multi_compile_builtin
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
			
			float3 _TintColor;
			sampler2D _MainTexture;
			sampler2D _NormalTexture;
			float4 _MoonRect;
			float4 _SunDir;
			float3 _CurrSkyColor;
			float _Overcast;
			
			struct appdata_moon
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
			
			struct v2f 
		 	{
				float4 pos : POSITION;
				float2 texCoord : TEXCOORD0;
			};

			v2f vert (appdata_moon v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texCoord = v.texcoord;
				return o;
			}
			
		 	half4 frag(v2f v) : COLOR0
         	{
				float4 retcolor = tex2D(_MainTexture, v.texCoord);
				float gray = (retcolor.r * 0.3 + retcolor.g * 0.6 + retcolor.b * 0.1) * retcolor.a;
				retcolor = lerp(retcolor, gray.xxxx, 0.5);
				retcolor.rgb *= _TintColor * 3;
				float2 moon_center = (_MoonRect.xy + _MoonRect.zw) * 0.5;
				float2 moon_size = _MoonRect.zw - _MoonRect.xy;
				float2 coord = (v.texCoord.xy - moon_center) / moon_size * 2.0;
				float3 sphere_coord = 0;
				sphere_coord.x = coord.x;
				sphere_coord.y = coord.y;
				sphere_coord.z = 1 - sphere_coord.x*sphere_coord.x - sphere_coord.y*sphere_coord.y;
				
				float3 _normal_bias = tex2D(_NormalTexture, v.texCoord).rgb - 0.5;
				_normal_bias.z = 0;
				sphere_coord.z = sqrt(sphere_coord.z);
				sphere_coord = normalize(sphere_coord-_normal_bias*1);
				float3 sun_dir = _SunDir;
				sun_dir.z = -sun_dir.z;
				float sundot = dot(sun_dir, sphere_coord);

				float alpha = saturate(sundot > 0 ? (sundot*5 + 0.05) : (sundot*0.05 + 0.05));
				float bright = clamp(sundot, 0, 1);
				retcolor.rgb *= (0.75+bright*2);
				float fade = 1.18 - (_CurrSkyColor.r * 0.3 + _CurrSkyColor.g * 0.6 + _CurrSkyColor.b * 0.1);
				float skyfade = clamp((pow(fade, 3) - 0.25f), 0, 1);
				retcolor.a *= alpha*skyfade;
				retcolor.a *= _Overcast;
				return retcolor;
         	}
         	ENDCG
      	} 
      	
	}
}
