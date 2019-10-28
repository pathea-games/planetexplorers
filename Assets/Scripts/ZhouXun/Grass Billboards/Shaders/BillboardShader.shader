Shader "Custom/Billboard" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {} 
    	_Cutoff ("Alpha cutoff", Range(0.25,0.9)) = 0.5		
    }
    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        pass{
        Cull Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag  alphatest:_Cutoff
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        struct v2f {
            float4 pos:SV_POSITION;
            float2 texc:TEXCOORD0;
        };
        v2f vert(appdata_base v)
        {
            v2f o;
            float4 ori=mul(UNITY_MATRIX_MV,float4(0,0,0,1));
            float4 vt=v.vertex;
            //vt.y=vt.z; 
            float2 r1=float2(_Object2World[0][0],_Object2World[0][2]);
            float2 r2=float2(_Object2World[2][0],_Object2World[2][2]);
            float2 vt0=vt.x*r1;
            vt0+=vt.z*r2;
            vt.xy=vt0;
            vt.z=0;
            vt.xyz+=ori.xyz;//result is vt.z==ori.z ,so the distance to camera keeped ,and screen size keeped
            o.pos=mul(UNITY_MATRIX_P,vt) + float4(v.normal,0);
 
            o.texc=v.texcoord;
            return o;
        }
        float4 frag(v2f i):COLOR
        {
            return tex2D(_MainTex,i.texc);
        }
        ENDCG
        }//endpass
    }
}