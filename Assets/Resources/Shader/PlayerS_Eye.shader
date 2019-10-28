Shader "Custom/PlayerS_Eye" {
Properties {
   _Color ("Main Color", Color) = (1,1,1,1)
   _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
   _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
   _PupilSize("PupilSize", Range (0.01, 1)) = 0.024
   _SkinColor("Skin Color", Color) = (1,1,1,1)
   _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
}

SubShader {
	Tags {"RenderType"="Opaque"}
	
	// Render both front and back facing polygons.
	LOD 400
	
CGPROGRAM
#pragma target 3.0
#pragma surface surf BlinnPhong

sampler2D _MainTex;
fixed4 _Color;
fixed4 _SkinColor;
half   _Shininess;
half   _PupilSize;

struct Input {
	float2 uv_MainTex;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	
	if((IN.uv_MainTex.x - 0.5)*(IN.uv_MainTex.x - 0.5) + (IN.uv_MainTex.y - 0.5)*(IN.uv_MainTex.y - 0.5) < _PupilSize * _PupilSize)
		c = c * _SkinColor;
	o.Albedo = c.rgb;
	o.Gloss = c.a * _Color.a;
	o.Alpha = c.a;
    o.Specular = _Shininess;
}
ENDCG
}

Fallback "Transparent/Cutout/VertexLit"
}
