Shader "Voxel Creation/Water Mask"
{
	Properties
	{
		_MainTex ("Mask Text", 2D) = "" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent-110"}
		
		Cull Off 
		Lighting Off 
		Fog { Mode Off }
		
		Pass
		{
			Blend One One
			Zwrite On
			
			CGPROGRAM
	    	#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma glsl
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			
			struct v2f 
		 	{
				float4 pos : POSITION;
				float2 texc : TEXCOORD0;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord.xy;
				return o;
			}
			
		 	half4 frag(v2f v) : COLOR0
         	{
         		if ( tex2D(_MainTex, v.texc).r < 0.5 )
         			discard;
				return half4(0.0,0.0,0.0,0.0);
         	}
         	ENDCG
      	} 
	}
}
