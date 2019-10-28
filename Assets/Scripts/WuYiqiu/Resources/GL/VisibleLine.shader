Shader "wuyiqiu/Visible Line"
{
	Properties
	{
		_NotVisibleAlpha ("Not Visible Alpha", Float) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent-100" "IgnoreProjector"="True" }
		Pass
		{
			ZTest Less
			Fog { Mode Off } 
			Blend SrcAlpha OneMinusSrcAlpha 
			ZWrite Off 
			Cull off
			
			CGPROGRAM 
			#pragma vertex vert
			#pragma fragment frag
	
			float _NotVisibleAlpha;
	
			struct appdata_l
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};
			struct v2f 
			{
				float4 pos : POSITION0;
				float4 color : COLOR;
			};
			
			v2f vert (appdata_l v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				return o; 
			}
			half4 frag (v2f i) : COLOR
			{
				return i.color;
			}
			ENDCG 
		}
		Pass
		{
        	ZTest Greater
			Fog { Mode Off } 
			Blend SrcAlpha OneMinusSrcAlpha 
			ZWrite Off
			Cull off
			
			CGPROGRAM 
			#pragma vertex vert
			#pragma fragment frag
	
			float _NotVisibleAlpha;
	
			struct appdata_l
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};
			struct v2f 
			{
				float4 pos : POSITION0;
				float4 color : COLOR;
			};
			
			v2f vert (appdata_l v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				return o; 
			}
			half4 frag (v2f i) : COLOR
			{
				return float4(i.color.xyz, i.color.a * _NotVisibleAlpha);
			}
			ENDCG
        }
	} 
	FallBack "Diffuse"
}
