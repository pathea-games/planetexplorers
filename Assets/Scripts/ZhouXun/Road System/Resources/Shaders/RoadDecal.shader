Shader "zhouxun/RoadDecal"
{
	Properties
	{
		_DecalTex ("Decal Texture", 2D) = "black" {}
		_xzMask ("xz Mask", 2D) = "black" {}
		_y0Mask ("y Mask 0", 2D) = "black" {}
		_y1Mask ("y Mask 1", 2D) = "black" {}
		_TileOffset ("Tile Offset", Vector) = (0,0,0,1)
		_TileSize ("Tile Size", Float) = 128
		_MaxHeight ("Max Height", Float) = 512
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}
		LOD 200
		Fog { Mode off }
		Blend One One
		Cull off
		Zwrite off
            
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		    #pragma target 3.0
			#include "UnityCG.cginc"
	 
			sampler2D _DecalTex;
			sampler2D _xzMask;
			sampler2D _y0Mask;
			sampler2D _y1Mask;
			
	 		float4 _TileOffset;
			float _TileSize;
			float _MaxHeight;

			struct appdata_road
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};
	
			struct v2f 
			{
				float3 tPos : TEXCOORD0;
				float4 pos : POSITION0;
			};
	
			v2f vert (appdata_road v)
			{
				v2f o; 
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.tPos = mul(_Object2World, v.vertex).xyz - _TileOffset;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half3 mask_coord = half3(i.tPos.x / _TileSize, i.tPos.y / _MaxHeight, i.tPos.z / _TileSize);
				if ( mask_coord.x < 0 ) discard;
				if ( mask_coord.y < 0 ) discard;
				if ( mask_coord.z < 0 ) discard;
				if ( mask_coord.x > 1 ) discard;
				if ( mask_coord.y > 1 ) discard;
				if ( mask_coord.z > 1 ) discard;
				half3 texc = tex2D(_DecalTex, i.tPos.xz).rgb;
				half3 maskc = tex2D(_xzMask, mask_coord.xz).rgb;
				half4 retc = half4(0,0,0,0);
				retc.rgb = 0.3 * texc.rgb * maskc;
				retc.a = 1;
				return retc;
			}
			ENDCG
	    }
	} 
	FallBack "Transparent/Diffuse"
}
