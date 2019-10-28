Shader "Voxel Creation/HoloBoardShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_TexCoordRect ("TexCoord Rect", Vector) = (0,0,1,1)
		_NoiseTex ("Texture", 2D) = "black" {}
		_Fade ("Fade Factor", Float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue"="Transparent+100" }
		Pass
		{
			Fog { Mode Off } 
			Blend One One 
			Cull Off
			ZTest Off
			ZWrite Off 
			
			CGPROGRAM 
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
			
	
			sampler2D _MainTex; 
			float4 _TexCoordRect;
			sampler2D _NoiseTex; 
			float _Fade;
	
			struct appdata_v
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float3 texc : TEXCOORD0;
				float4 pos : POSITION0;
			};
			
			v2f vert (appdata_v v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.texc = v.texcoord;
				return o;
			}
			
			half4 frag (v2f i) : COLOR
			{
				half brightness = clamp(i.texc.x * 4, 0,1) * clamp((1-i.texc.x) * 4, 0,1);
				i.texc.x = clamp(lerp(_TexCoordRect.x, _TexCoordRect.z, i.texc.x), 0,1);
				i.texc.y = clamp(lerp(_TexCoordRect.y, _TexCoordRect.w, i.texc.y), 0,1);
				float fade = pow(_Fade, 2-i.texc.y);
				half4 rand1 = tex2D(_NoiseTex, float2(0.5,i.texc.y*50*(0.45-fade)));
				half4 rand2 = tex2D(_NoiseTex, float2(i.texc.x*50*(0.45-fade),0.5));
				float2 texc_ofs = float2((rand1.g-0.5)*2,(rand2.g-0.5)*0.02)*clamp((0.4-fade)*2.5,0,1);
				half4 c = tex2D (_MainTex, (i.texc.xy+texc_ofs)) * (5*fade-4*fade*fade) * pow(brightness,10);
				return c;
			}
			ENDCG 
		}
	} 
}
