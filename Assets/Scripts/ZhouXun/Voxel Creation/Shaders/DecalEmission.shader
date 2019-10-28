Shader "Voxel Creation/Decal Emission"
{
	Properties
	{
		_Texture ("Texture", 2D) = "white" {}
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_Center ("Center", Vector) = (0,0,0,1)
		_Forward ("Forward", Vector) = (0,0,1,0)
		_Right ("Right", Vector) = (1,0,0,0)
		_Up ("Up", Vector) = (0,1,0,0)
		_Size ("Size", Float) = 1
		_Depth ("Depth", Float) = 0.01
	}
	SubShader
	{
		Tags{ "RenderType"="Opaque" "Queue"="Geometry+451" }
		Pass
	  	{
			Fog { Mode Off }
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
			
			sampler2D _Texture;
			float4 _TintColor;
			float4 _Center;
			float4 _Forward;
			float4 _Right;
			float4 _Up;
			float _Size;
			float _Depth;

			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;	
				float4 color : COLOR;
			};

			struct v2f 
			{
				float3 wPos : TEXCOORD0;
				float4 pos : POSITION0;
				float4 color : COLOR;
			};

			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.wPos.xyz = mul(_Object2World, v.vertex).xyz;
				o.color = v.color;
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				float3 ofs = i.wPos.xyz - _Center.xyz;
				float3 lpos = 0;
				lpos.x = dot(ofs, _Right) / _Size;
				lpos.y = dot(ofs, _Up) / _Size;
				lpos.z = dot(ofs, _Forward);
				if ( abs(lpos.x) > 0.999 )
					discard;
				if ( abs(lpos.y) > 0.999 )
					discard;
				if ( abs(lpos.z) > _Depth - 0.001 )
					discard;
				
				float4 diff = tex2D(_Texture, (lpos.xy+1)*0.5);
				//return float4((diff.rgb * i.color.rgb) * (1-i.color.a) + i.color.rgb * i.color.a, diff.a) * _TintColor;
				//return float4(diff.rgb * i.color.rgb, diff.a) * _TintColor;
				return diff * _TintColor;
			}
			ENDCG 
		}
	}
	Fallback "Transparent/Cutout/VertexLit"
}
