Shader "wuyiqiu/Outline/StencilZ"
{
	
	SubShader
	{
		Pass
		{
			ZWrite On
			ZTest LEqual
			Lighting Off
			Fog { Mode Off }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			uniform fixed4 _Outline;

			struct appdata_vert
			{
				float4 vertex : POSITION;
			};

			float4 vert(appdata_vert v) : POSITION
			{
				return mul(UNITY_MATRIX_MVP, v.vertex);
			}

			fixed4 frag() : COLOR
			{
				return _Outline;
			}
			ENDCG
		}
	}
	
	Fallback Off
}
