Shader "Voxel Creation/SelectBound"
{
	Properties
	{
		_TintColor ("Main Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 200
		//Alphatest Greater 0 ZWrite Off ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma surface surf Lambert

		float4 _TintColor;

		struct Input
		{
			float3 localPos;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			half4 c = _TintColor;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
