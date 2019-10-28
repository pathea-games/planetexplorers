Shader "NovaEnv/FogSphere"
{
	Properties
	{
		_FogColor ("Fog Color", Color) = (1,1,1,1)
		_FogHeight ("Fog Height", Float) = 0.5
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Background+32"
		}
		Pass
	  	{
	  		Fog {Mode Off}
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0

	    	#include "UnityCG.cginc"

			float4 _FogColor;
			float _FogHeight;
			
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4 pos : SV_POSITION;
				float3 vertPos : TEXCOORD0;
			};

			v2f vert (appdata_v v)
			{
				v2f o;
				o.vertPos.xyz = v.vertex.xyz;	
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 retval = 1;
				retval.rgb = _FogColor;
				float height = i.vertPos.y;
				retval.a = pow((1 - saturate(height/_FogHeight)),1.5);

				return retval;
			}
			ENDCG 
		}
	}
}
