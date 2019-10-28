Shader "Unlit/Color Multiply"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent" }
		Pass
		{
			Fog { Mode Off } 
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZWrite Off 
			
			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
	
			sampler2D _MainTex; 
			float4 _Color;
	
			struct appdata_v
			{
				float4	vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			struct v2f 
			{
				float3 texc : TEXCOORD0;
				float4 pos : POSITION0;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul( UNITY_MATRIX_MVP, v.vertex);				
				o.texc = v.texcoord;	
				return o;
			}
			half4 frag (v2f i) : COLOR
			{
				half4 c = tex2D (_MainTex, i.texc) * _Color;
				return c;
			}
			ENDCG 
		}
		
		Pass
		{
			Fog { Mode Off } 
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off
			ZTest Greater 
			
			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
	
			sampler2D _MainTex; 
			float4 _Color;
	
			struct appdata_v
			{
				float4	vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};
			struct v2f 
			{
				float3 texc : TEXCOORD0;
				float4 pos : POSITION0;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul( UNITY_MATRIX_MVP, v.vertex);				
				o.texc = v.texcoord;	
				return o;
			}
			half4 frag (v2f i) : COLOR
			{
				half4 c = tex2D (_MainTex, i.texc) * _Color;
				c.a *= 0.15f;
				return c;
			}
			ENDCG 
		}
	} 
}
