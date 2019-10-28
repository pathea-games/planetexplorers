Shader "MyShader/MySpecular" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
    _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
    _Shininess ("Shininess", Range (0.03, 1)) = 0.078125
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.99
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BumpMap ("Normalmap", 2D) = "bump" {}
}

SubShader {
	LOD 200
//Tags { "Queue" = "Transparent" "RenderType" = "Transparent" } 
Tags{"RenderType" = "Opaque"}
CGPROGRAM
#pragma surface surf BlinnPhong alphatest:_Cutoff 

struct Input {
  float2 uv_MainTex;
  float2 uv_BumpMap;
};
sampler2D _MainTex;
sampler2D _BumpMap;
float4 _Color;
float _Shininess;

void surf (Input IN, inout SurfaceOutput o)
{
  half4 tex = tex2D (_MainTex, IN.uv_MainTex);
  o.Albedo = tex.rgb * _Color;
  o.Gloss = tex.a;
  o.Alpha = 1;//tex.a;
  o.Specular = _Shininess;
  o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
}
ENDCG
}
FallBack "Diffuse"
}