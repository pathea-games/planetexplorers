Shader "Hidden/CaveColorCorrection" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Pass {
		ZTest Always Cull Off ZWrite Off
		Fog { Mode off }

CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

float _threshold;
float _maxMultiL;
float _maxMultiR;
uniform sampler2D _MainTex;

fixed4 frag (v2f_img i) : COLOR
{
	fixed4 orig = tex2D(_MainTex, i.uv);

	fixed4 newCol = (orig>=_threshold)*orig*_maxMultiR + (orig<_threshold)*orig*_maxMultiL;
	fixed4 color = fixed4(newCol.r,newCol.g,newCol.b,orig.a);

	return color;
}
ENDCG

	}
}

Fallback off

}