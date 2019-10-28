Shader "Voxel Creation/LineShader Always Visible"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent+20" }
		Pass
		{
			Fog { Mode Off } 
			Blend One One
			Cull Off
			ZWrite Off 
			
			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
	
			float4 _Color;
			sampler2D _MainTex; 
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 color : COLOR0;
			};
			struct v2f 
			{
				float3 texc : TEXCOORD0;
				float4 pos : POSITION0;
				float4 color : COLOR;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul( UNITY_MATRIX_MVP, v.vertex);				
				o.texc = v.texcoord;			
				o.color = v.color;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 c = tex2D (_MainTex, i.texc.xy) * _Color * i.color;
				return c;
			}
			ENDCG 
		}
		Pass
		{
        	ZTest Greater
			Fog { Mode Off } 
			Blend One One
			Cull Off
			ZWrite Off 
			
			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
	
			float4 _Color;
			sampler2D _MainTex; 
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
				float4 color : COLOR0;
			};
			struct v2f 
			{
				float3 texc : TEXCOORD0;
				float4 pos : POSITION0;
				float4 color : COLOR;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul( UNITY_MATRIX_MVP, v.vertex);				
				o.texc = v.texcoord;			
				o.color = v.color;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 c = tex2D (_MainTex, i.texc.xy) * _Color * i.color * 0.3f;
				return c;
			}
			ENDCG 
		}
	} 
}
