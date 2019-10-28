Shader "Custom/PlayerBS" {
Properties {
   _Color ("Main Color", Color) = (1,1,1,1)
   _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
   _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
   _AlphaCutOff("AlphaCutOff", Range (0, 1)) = 0
   _HeadMask("HeadMask", Range (0, 1)) = 0
   _SkinColor("Skin Color", Color) = (1,1,1,1)
   _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
   _BumpMap ("Normalmap", 2D) = "bump" {}
}

SubShader {
	Tags {"RenderType"="Opaque"}
	
	// Render both front and back facing polygons.
	LOD 400
	
CGPROGRAM
#pragma target 3.0
#pragma surface surf BlinnPhong alphatest:_AlphaCutOff

sampler2D _MainTex;
sampler2D _BumpMap;
fixed4 _Color;
fixed4 _SkinColor;
half   _Shininess;
half   _HeadMask;

struct Input {
	float2 uv_MainTex;
    float2 uv_BumpMap;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	if(IN.uv_MainTex.x < 0.5)
	{
		if(IN.uv_MainTex.y < 0.5)
			c = c * _SkinColor;
	}
	if(_HeadMask > 0.5)
		c = c * 224 / 255;
	o.Albedo = c.rgb;
	o.Gloss = c.a * _Color.a;
	o.Alpha = c.a;
    o.Specular = _Shininess;
    o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
}
ENDCG
}

Fallback "Transparent/Cutout/VertexLit"
}
