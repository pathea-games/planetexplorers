Shader "wuyiqiu/ProjectorRange"
 {
	Properties 
	{
		_DecalTex ("Decal Texture", 2D) = "white" {}
		_CenterAndRadius ("Center(xyz) and Radius(w)", Vector) = (0,0,0,10)
		_TintColor ("Main Color",Color) = (1,1,1,1)
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
	 
	 		float4 _CenterAndRadius;
	 		float4 _TintColor;
			sampler2D _DecalTex;
	
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
	
			v2f vert (appdata_v v)
			{
				v2f o; 
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.wPos = mul(_Object2World, v.vertex).xyz;
				return o;
			}
			

			
			half4 frag (v2f i) : COLOR
			{
				float dist = length(i.wPos.xyz - _CenterAndRadius.xyz);
				float brightness_body = clamp((_CenterAndRadius.w - dist) / _CenterAndRadius.w * 5, 0,1);
				float3 texc = tex2D(_DecalTex, (i.wPos.xz-_CenterAndRadius.xz+_CenterAndRadius.ww)/(_CenterAndRadius.w*2)).rgb;
				float4 retc = (0,0,0,0);
				retc.rgb = texc.rgb * _TintColor * brightness_body * (sin( _Time.y * 5) + 1) * 0.25;
				retc.a = 1;
				return retc;
			}
			ENDCG
	    }
	} 
	FallBack "Diffuse"
}
