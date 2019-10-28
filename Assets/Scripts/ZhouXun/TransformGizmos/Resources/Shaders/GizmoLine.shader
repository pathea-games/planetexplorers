Shader "zhouxun/GizmoLine"
{
    SubShader
    { 
        Tags { "Queue"="Transparent-100" }
        Pass
        { 
            Blend SrcAlpha OneMinusSrcAlpha 
            ZWrite Off Cull Off Fog { Mode Off }
            BindChannels
            {
                Bind "vertex", vertex
                Bind "color", color
            }
        }
    }
}
