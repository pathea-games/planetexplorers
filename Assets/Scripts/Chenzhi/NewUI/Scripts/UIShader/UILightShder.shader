Shader "UI/UILightShder" 
{

	Properties 
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
	}

	Category 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend  SrcAlpha one        
		AlphaTest Greater .01
		Cull off Lighting off ZWrite Off Fog { Color (0,0,0,0) }
		BindChannels 
		{
			Bind "Color", color
			Bind "Vertex", vertex
			Bind "TexCoord", texcoord
		}
	
		SubShader 
		{
			Pass 
			{
				Material {
				Diffuse [_Color]
				Ambient [_Color]
				}
				SeparateSpecular on
				SetTexture [_MainTex] {
					constantColor [_TintColor]
					Combine texture * primary DOUBLE, texture * primary
				}
			}
		}
	}
}
