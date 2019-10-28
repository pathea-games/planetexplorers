Shader "wuyiqiu/ProjectorColor" 
{
	Properties 
	{
		_TintColor ("Main Color",Color) = (1,1,1,1)
		_CenterAndRadius ("Center(xyz) and Radius(w)", Vector) = (0,0,0,10)
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
		//Blend DstColor Zero
		Cull off
		Zwrite off
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		    #pragma target 3.0
			#include "UnityCG.cginc"
			
			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;	   
				float3 wPos : COLOR0;	 
			};
	
			struct v2f 
			{
				float3 wPos : TEXCOORD0;
				float4 pos : POSITION0;
			};
			
			float4 _TintColor;
			float4 _CenterAndRadius;
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.wPos = mul(_Object2World, v.vertex).xyz;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
//				float dist = length(i.wPos.xz - _CenterAndRadius.xz);
				float distX = abs(i.wPos.x - _CenterAndRadius.x);
				float distZ = abs(i.wPos.z - _CenterAndRadius.z);
//				float brightness_body = clamp((_CenterAndRadius.w - dist) / _CenterAndRadius.w * 5, 0,1);
//				float brightness_border = clamp(0.005-abs((_CenterAndRadius.w - dist)/_CenterAndRadius.w - 0.005),0,1)*50;
				//float4 texc = tex2D(_DecalTex, (i.wPos.xz-_CenterAndRadius.xz+_CenterAndRadius.ww)/(_CenterAndRadius.w*2));
				float4 color =  (0,0,0,0);
//				color.rgb =  _TintColor.xyz * brightness_body;
//				color.a = 1;  
				
				float brightness_body = clamp((_CenterAndRadius.w - distX + _CenterAndRadius.w - distZ) * 0.5 / _CenterAndRadius.w * 10, 0, 1);
				
				//if (_CenterAndRadius.w - dist > 0)
//				if (distX < _CenterAndRadius.w && distZ < _CenterAndRadius.w)
//				{
//					color.rgb =  _TintColor.xyz;
//					color.a = 1;
//				}
				color.rgb = _TintColor.xyz * brightness_body;
				color.a = 1;
				return color;
			}
			
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
