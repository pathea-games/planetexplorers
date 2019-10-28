Shader "Voxel Creation/Decal Additive"
{
	Properties
	{
		_Texture ("Texture", 2D) = "white"
		_Center ("Center", Vector) = (0,0,0,1)
		_Forward ("Forward", Vector) = (0,0,1,0)
		_Right ("Right", Vector) = (1,0,0,0)
		_Up ("Up", Vector) = (0,1,0,0)
		_Size ("Size", Float) = 1
		_Depth ("Depth", Float) = 0.01
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent+21" }
		Pass
	  	{
			Fog { Mode Off }
			Blend One One
			Cull Off
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
			#include "UnityCG.cginc"
			
			sampler2D _Texture;
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
			};

			struct v2f 
			{
				float3 wPos : TEXCOORD0;
				float4 pos : POSITION0;
			};

			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.wPos.xyz = mul(_Object2World, v.vertex).xyz;	
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
				if ( lpos.z > _Depth - 0.0001 )
					discard;
				if ( lpos.z < -0.001 )
					discard;
				
				return tex2D(_Texture, (lpos.xy+1)*0.5);
			}
			ENDCG 
		}
	}

}
