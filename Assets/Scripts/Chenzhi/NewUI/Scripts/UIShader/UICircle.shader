Shader "UI/UICircle"
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Distence("Distence",Float) = 0.24
		_Center_x("Center_x",Float) = 0.5
		_Center_y("Center_y",Float) = 0.5
		_Falloff ("Falloff",Float) = 30
		_Alpha ("Alpha",Float) = 0.5
		_Bright ("Bright",Float) = 2.5
	}
	
	SubShader
	{

		Pass
		{
			Tags { "RenderType"="Opaque" "Queue"="Transparent" "IgnoreProjector"="True" }
			Fog { Mode Off } 
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Off 
			ZWrite Off 
			
			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
	
			float4 _Color;
			sampler2D _MainTex; 
			float _Distence;
			float _Falloff; 
			float _Center_x;
			float _Center_y; 
			float _Alpha;
			float _Bright;
	
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
				o.pos = mul( UNITY_MATRIX_MVP, v.vertex);				
				o.texc = v.texcoord;	
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half4 c = tex2D (_MainTex, i.texc.xy)  * _Color;
				float d_x = i.texc.x - _Center_x;
				float d_y = i.texc.y - _Center_y;
				float u_x = d_x/_Distence;
				float u_y = d_y/_Distence;
				
				c.a = saturate((1-u_x)*(1-u_y)*_Falloff) * _Alpha;
				c.rgb *= _Bright ;
				
				return c ;
			}
			ENDCG 
		}
	} 
}
