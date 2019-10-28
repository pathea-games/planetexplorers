Shader "Custom/HidingWave" {
	Properties 
	{
		_SrcMap ("Src Map", 2D) = "white" {}
		_NormMap ("Normal Map", 2D) = "bump" {}
		_WaveMap ("wave Map", 2D) = "bump" {}
		
		_DepthColor ("Depth Color", Color) = (1,1,1,1)
		
		_Tile ("Main Tile", Range(0.005, 0.05)) = 0.05
		_RefractionAmt  ("Refraction Amount", range (0,1000)) = 200.0
		
		_DensityParams ("(Density, Base Density, Underwater, )", Vector) = (0.02 ,0.1, 0, 0)
		
		_WorldLightDir ("light direction(Only one)", Vector) = (0.0, 0.1, -0.5, 0.0)
		
		_shaderChange("Control shader", range(0,1)) = 1
	}
	
	CGINCLUDE
	#include "PEWaterInc.cginc"
	
	struct appdata_water
	{
		float4 vertex : POSITION;
		float3 normal : NORMAL;
		float4 texcoord : TEXCOORD0;
	};

	sampler2D _GrabTex_Origin;
	float4 _GrabTex_Origin_TexelSize;
	
	sampler2D _SrcMap;
	sampler2D _NormMap;
	sampler2D _WaveMap;
	
	float4 _SrcMap_ST;
	uniform float4 _DepthColor;
	
	uniform float _Tile;
	uniform float  _RefractionAmt;
	uniform float4 _DensityParams;
	
	uniform float4 _WorldLightDir;
	uniform float _shaderChange;
	ENDCG
	
	SubShader 
	{
		Tags {"RenderType"="Transparent" "Queue"="Transparent"}
//		GrabPass 
//		{							
//			"_GrabTex_Origin"
//		}
		Pass 
		{
			Name "Underwater"
			ZWrite On
			Cull off
			Blend SrcAlpha OneMinusSrcAlpha
			
			CGPROGRAM
			#pragma target 3.0 
			#pragma vertex vert
			#pragma fragment frag
			
			struct v2f 
			{
				float4 pos 			: SV_POSITION;
				float4 projpos      : TEXCOORD1;
				float3 worldPos		: TEXCOORD2;
				UNITY_FOG_COORDS(3)
				float2 uv			: TEXCOORD0;
				float3 col			: color;
			};
			
			v2f vert(appdata_water v)
			{
				v2f o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				o.projpos = o.pos;
				o.worldPos = mul(_Object2World,(v.vertex)).xyz;
				UNITY_TRANSFER_FOG(o,o.pos);
				
				o.uv = TRANSFORM_TEX(v.texcoord,_SrcMap);
				//这里可以直接写v.texcoord/v.texcoord.xy,写成这样之后而且不用定义上面的_SrcMap_ST
				//这样写的好处是可以取到某个Texture的tiling和offset
				o.col = ShadeVertexLights(v.vertex, v.normal); 
				//设置了顶点光照的模型(受到法线影响),这一句会得到顶点的初始颜色
				return o;
			}
						
			half4 frag( v2f input ) : COLOR
			{
				float4 screenPos = ComputeScreenPos(input.projpos);
				
				float2 vel = normalize(float2(input.worldPos.z, -input.worldPos.x)) * 0;
				half3 waveNorm = tex2Dproj(_WaveMap, screenPos).xzy - half3(0.5, 1, 0.5);
				half3 worldNormal = FinalWaterNormal(_NormMap, _Tile*2, input.worldPos.xz, vel, 10) + waveNorm;

				float depth = screenPos.w;
				
				float sink = CalcSink(depth, _DensityParams.x*0.5);
				float4 refract_projpos = RefractionProjectionPos(input.projpos, worldNormal, sink, _RefractionAmt, _GrabTex_Origin_TexelSize.xy*8); 
				float4 refract_grabpos = ComputeGrabScreenPos(refract_projpos);
				float4 viewVector = float4(input.worldPos.xyz - _WorldSpaceCameraPos.xyz, 1);
				float3 viewDir = normalize(viewVector.xyz);
				half vdotn = pow(saturate(dot(viewDir, float3(0,1,0))), _DensityParams.z);
				half vdotl = pow(saturate(dot(viewDir, -_WorldLightDir.xyz)), _DensityParams.z * 4);
				half3 grabcolor = GetGrabColor(_GrabTex_Origin, refract_grabpos).rgb * (1+vdotl*_DensityParams.z+vdotn);
				half3 refraction = lerp(grabcolor, _DepthColor.rgb, saturate(sink+ _DensityParams.y)).rgb; 
				refraction = lerp(refraction, _DepthColor.rgb, saturate(1-vdotn)).rgb; 
				half4 retval = half4(refraction, saturate(depth*2));
				UNITY_APPLY_FOG(input.fogCoord, retval);
				
				half4 entityColor = half4(tex2D(_SrcMap,input.uv).rgb,0.8);
				//entityColor = entityColor * half4(input.col,1);
				retval = lerp(retval,entityColor,_shaderChange);
				//clamp((abs(frac(_Time.x * 2)- 0.5) * 2),0,1)
				return retval;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
