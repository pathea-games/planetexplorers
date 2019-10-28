Shader "Wave/PokeWave" 
{
	Properties
	{
		_Strength ("Strength", Float) = 10
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent+10000" }
		Pass
		{
			Fog { Mode Off } 
//			Blend SrcAlpha OneMinusSrcAlpha
			Blend one one
			Cull Off
			//ZWrite Off 
			
			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			float _Strength;
			
			struct v2f 
			{
				float3 texc : TEXCOORD0;
				float4 pos : POSITION0;
			};
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord;
				return o; 
			}
			
			half4 frag (v2f i) : COLOR
			{
				float2 v = (i.texc.xy - float2(0.5, 0.5))/0.5;
				float l = length(v);
				
				
				return half4(saturate(1 -l) * v.x, saturate(1- l) * v.y, 0, 0) * _Strength;
			}
			ENDCG 
		}
	} 
}
