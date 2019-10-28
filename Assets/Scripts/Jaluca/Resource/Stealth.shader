Shader "SpecialItem/Stealth" 
{
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_Disturb ("Disturb", 2D) = "black" {}
		
		_Intensity ("Disturb Intensity", Range(0,0.2)) = 0.05
		_Self("Self Disturb", Range(0,1)) = 0.2
		_Trans("Transparency", Range(0,1)) = 0.05
	}
	SubShader {
		Tags{"Queue"="Transparent"}
		GrabPass{}
		CGPROGRAM
		//#pragma target 3.0
		#pragma surface surf SimpleSpecular vertex:vert
		#include "UnityCG.cginc"

		sampler2D _MainTex;
		sampler2D _Disturb;
		half _Intensity;
		half _Self;
		half _Trans;
		sampler2D _GrabTexture;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 world_pos;
			float4 screenPos;
		};
		void vert (inout appdata_full v, out Input o)
		{
		    o.world_pos = v.vertex;
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			float4 color4 = tex2D(_MainTex, IN.uv_MainTex) * _Trans;
			float scw = IN.screenPos.w;
			float2 uvbase = float2(IN.screenPos.x / scw, 1 - IN.screenPos.y / scw);
			float3 grabbase = (tex2D(_GrabTexture, uvbase + (tex2D(_Disturb, IN.screenPos.xy / scw + _Time.xx * _Self).rg -0) * _Intensity)) * (1 - _Trans) + color4.rgb;
			
			o.Emission = grabbase;
		}
		
		half4 LightingSimpleSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten){
			return half4(0,0,0,0);
		}
		ENDCG
	} 
	FallBack "Diffuse"	

//    SubShader
//    {
//        Tags{"Queue"="Transparent"}
//     
//        //GrabPass
//        //{
//        //}
//       
//        //pass
//        //{
//
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//           
//            #include "UnityCG.cginc"
//            sampler2D _MainTex;
//            //sampler2D _GrabTexture;
//            float4 _MainTex_ST;
//            //float4 _GrabTexture_ST;
//            struct Input 
//            {
//            	float4 world_pos;
//				float4 screenPos;
//			};
//            
//            struct v2f {
//                float4  pos : SV_POSITION;
//                float2  uv : TEXCOORD0;
//            } ;
//            v2f vert (appdata_base v)
//            {
//                v2f o;
//                o.pos = mul(UNITY_MATRIX_MVP,v.vertex);
//                o.uv =  TRANSFORM_TEX(v.texcoord,_MainTex);
//                return o;
//            }
//            float4 frag (v2f i, Input IN) : COLOR
//            {
//                //float4 texCol = tex2D(_GrabTexture,float2(i.uv.x, 1 - i.uv.y));
//                //float4 texCol = tex2D(_GrabTexture,float2(1-IN.screenPos.x,IN.screenPos.y));                
//                return float4(1,1,1,1)* IN.screenPos.x;
//                //return texCol;
//               
//            }
//            ENDCG
//        //}
//    }
}