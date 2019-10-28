Shader "Custom/PointSprite" {
	Properties
	{
		_PaletteTex ("Palette Img", 2D) = "white" {}
		_PointSize("PointSize", Float) = 10
		_ClipSurface("clip sphere coef(center pos(x,y,z), radius)", vector) = (0,0,0,0.5)
	}
	
	SubShader{
		Pass
		{
			CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members worldPos)
#pragma exclude_renderers d3d11 xbox360
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct v2f
			{
				float4 pos : SV_POSITION;
				half size:PSIZE;
				half2 texcoord : TEXCOORD0;
				float3 wPos : TEXCOORD1; 
			};
			
			float4 _ClipSurface;
			half _PointSize;
			sampler2D _PaletteTex;
			
			v2f vert (appdata_full v)
			{
				v2f o;
				o.wPos = mul (_Object2World, v.vertex).xyz;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.size = _PointSize;
				o.texcoord = v.color.xy;	// color has been mapped to 0-1
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				float dx = i.wPos.x-_ClipSurface.x;
				float dz = i.wPos.z-_ClipSurface.z;
				clip ( (dy*dy+dx*dx+dz*dz > _ClipSurface.w*_ClipSurface.w) ? -1 : 1); //1 no clip
				//half4 c = half4(i.texcoord,0,1);
				half4 c = tex2D(_PaletteTex,i.texcoord);
				return c;
			}
			ENDCG
		}
	}
}