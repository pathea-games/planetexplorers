Shader "UI/GridLightShader" 
{
	Properties
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Pos_y("Pos_y",Float) = 1
	}
	
	SubShader
	{		
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off Cull Off ZWrite Off Fog { Mode Off } 



		Pass
		{

			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
	
			float4 _Color;
			sampler2D _MainTex; 
			float _Pos_y; 
			float _Alpha;
			float _Bright;
	
	
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
				half4 c = tex2D (_MainTex, i.texc.xy)  * i.color;
				
//				if ((i.texc.y - _Pos_y) > 0)
//					c.a = 1; 
				
				return c ;
			}
			ENDCG 
		}
	} 
}