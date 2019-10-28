Shader "White Cat/Line" 
{
	SubShader
	{
		Pass
		{
			BindChannels
			{
				Bind "vertex", vertex
				Bind "color", color
			}
			Blend One One
			ZWrite Off
			Cull Off
			Fog { Mode Off }
		}
	}
}