Shader "Voxel Creation/Gizmo AlphaBlend"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_MainColor ("Main Color", Color) = (0,0,0,0)
		_NotVisibleColor ("Not Visible Color", Color) = (0,0,0,0)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent+1" "IgnoreProjector"="True" }
		Pass
		{
			Fog { Mode Off } 
			Blend SrcAlpha SrcAlpha 
			//Cull Off
			//ZWrite Off 
			
			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
	
			sampler2D _MainTex; 
			float4 _MainColor;
	
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
				float2 tex = abs(frac(i.texc.xy) - 0.5);
				float coord = min(tex.x, tex.y);
				float bright = pow(frac(-_Time.y/2)*0.8,3);
				float increase = pow(clamp((0.06 - abs(coord - bright)) * 10, 0,1),2);
				
				half4 c = tex2D (_MainTex, i.texc);
				float scale = (c.r+c.g+c.b)/3;
				float u = pow(clamp((scale - 0.4), 0,1) * 5,5);
				
				return (c + increase*u) * _MainColor;
			}
			ENDCG 
		}
		Pass
		{
        	ZTest Greater
			Blend SrcAlpha OneMinusSrcAlpha 
			//Cull Off
			ZWrite Off 
			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
	
			sampler2D _MainTex; 
			float4 _NotVisibleColor;
			
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
				float2 tex = abs(frac(i.texc.xy) - 0.5);
				float coord = min(tex.x, tex.y);
				float bright = pow(frac(-_Time.y/2)*0.8,3);
				float increase = pow(clamp((0.06 - abs(coord - bright)) * 10, 0,1),2);
				
				half4 c = tex2D (_MainTex, i.texc);
				c *= _NotVisibleColor;	
				return c;
			}
			ENDCG 
			
//	        Material
//	        {
//		 		Diffuse [_NotVisibleColor]
//	      		Ambient [_NotVisibleColor]
//	        }
//	        Lighting On
        }
	} 
	FallBack "Diffuse"
}
