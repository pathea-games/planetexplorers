Shader "NovaEnv/SkySphere"
{
	Properties
	{
		_SkyColor ("Sky sphere color", Color) = (1,1,1,1)
		_FogColorA ("Fog colorA", Color) = (0,0,0,0)
		_FogColorB ("Fog colorB", Color) = (0,0,0,0)
		_FogHeight ("Fog height", Float) = 0.7
		_SunBloomColor ("Sun Bloom Color", Color) = (1,1,1,1)
		_SunColor ("Sun Color", Color) = (1,1,1,1)
		_SunSize ("Sun Size", Float) = 0.5
		_SunBorder ("Sun Border", Float) = 1
		_SunPos ("Sun local position", Vector) = (0,1,0)
		_SunPower("Sun fading power", Float) = 100
		_Overcast("Overcast", Float) = 1
		_UniverseTexture("Universe Noise Texture", 2D) = "white" {}
		_StarNoiseTexture("Star Noise Texture", 2D) = "white" {}
		_StarTile("Star Tile", Float) = 90
		_StarSaturation("Star Saturation", Float) = 0.3
		_StarAxisX("Star Axis X", Vector) = (1,0,0,0)
		_StarAxisY("Star Axis Y", Vector) = (0,1,0,0)
		_StarAxisZ("Star Axis Z", Vector) = (0,0,1,0)
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Background+1"
		}
		Fog { Mode Off }
		Pass
	  	{
			Blend SrcAlpha OneMinusSrcAlpha
			Cull Front
			ZWrite Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
	    	#pragma target 3.0
						
			float4 _SkyColor;
			float4 _FogColorA;
			float4 _FogColorB;
			float  _FogHeight;
			float4 _SunBloomColor;
			float4 _SunColor;
			float  _SunSize;
			float  _SunBorder;
			float4 _SunPos;
			float  _SunPower;
			float _Overcast;
			sampler2D _UniverseTexture;
			sampler2D _StarNoiseTexture;
			float  _StarTile;
			float  _StarSaturation;
			float4 _StarAxisX;
			float4 _StarAxisY;
			float4 _StarAxisZ;

			struct appdata_v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float3 vertPos : TEXCOORD0;
				float4 pos : POSITION0;
			};

			float3 randNum (float2 coord)
			{
				return tex2D(_StarNoiseTexture, coord).rgb - 0.5;
			}
			
			float3 StarMap3D(float3 p)
			{
				return normalize(randNum(p.xy) + randNum(p.yz) + randNum(p.xz));
			}

			float3 UniverseMap3D(float3 p)
			{
				return tex2D(_UniverseTexture, p.xy * 0.5).rgb * (abs(p.z)-0.3)
				     + tex2D(_UniverseTexture, p.yz * 0.5).rgb * (abs(p.x)-0.3)
				     + tex2D(_UniverseTexture, p.xz * 0.5).rgb * (abs(p.y)-0.3);
			}

			v2f vert (appdata_v v)
			{
				v2f o;
				o.vertPos.xyz = v.vertex.xyz;	
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
                // simple sky sphere
                //
				float3 sunvec = _SunPos.xyz;    // the dir of sun
				float3 skyvec = i.vertPos;      // the dir of a sky point
				float sun_dist = length(normalize(skyvec) - normalize(sunvec));
				float sun_size = _SunSize * 0.04;
				float sun_body_density = pow(clamp(((1+_SunBorder) - (sun_dist/sun_size)) / _SunBorder, 0, 1), 2);
				float dot_power = dot(normalize(sunvec), normalize(skyvec));
				dot_power = dot_power > 0 ? pow( dot_power, _SunPower ) : 0;
				dot_power *= 0.4;
				float dot_power2 = dot(normalize(sunvec), normalize(skyvec)) + 0.7;
				float4 _FogColor = lerp(_FogColorB, _FogColorA, saturate(dot_power2*0.7));
				dot_power2 *= 0.3;
				float height = saturate(normalize(skyvec).y);    // the height of a sky point
				// Calculate the color of this sky point
				float fog_density = pow((1 - clamp( height/_FogHeight, 0, 1 )),1.5) * _FogColor.a;
				half4 retcolor = (0,0,0,0);
				retcolor.rgb = lerp(_SkyColor.rgb, _FogColor, saturate(fog_density)) 
				+ _SunBloomColor.rgb * dot_power 
				+ _SunBloomColor.rgb * dot_power2
				+ _SunColor.rgb * sun_body_density * _Overcast;
				retcolor.a = _SkyColor.a;
				
				// stars
				//
				float3 starvec_w = skyvec;
				float3 starvec = 0;
                // get the transformed sky point as starvec
				starvec.x = dot(starvec_w, _StarAxisX);
				starvec.y = dot(starvec_w, _StarAxisY);
				starvec.z = dot(starvec_w, _StarAxisZ);
				// scale starvec by _StarTile
				starvec = normalize(starvec)*_StarTile;
				// devide the sky sphere into tiny cells by _StarTile
				float3 starcell = trunc(starvec)/_StarTile;     // the cell number
				// coord in each cell
				float3 starcoord = frac(starvec);   // the cell coord
				// gen properies for each cell
				float3 rand = StarMap3D(starcell) + 0;
				float alpha = pow(clamp(frac((rand.r+rand.g+rand.b)*16) * 4 - 2, 0,3),2);
				float size = clamp(pow(alpha*0.25, 1000), 0,1);
				// get a star as a increasement var
				float3 increasement = alpha * lerp(float3(1,1,1),rand,_StarSaturation) * pow(clamp( 0.15 - (length(starcoord - 0.7 + (rand-0.5)*0.5)-0.15) * 6.6666, 0,1), 6-size*3);
				increasement = clamp(increasement, 0,1);
				// stars don't shine in bright sky
				float fade = 1.1 - (retcolor.r * 0.3 + retcolor.g * 0.6 + retcolor.b * 0.1);
				increasement *= saturate((pow(fade, 4) - 0.3f));
				// stars don't shine when overcast
				increasement *= _Overcast;
				// add star increasement
				retcolor.rgb += increasement;
				
				// Universe
				//
				float3 universe = UniverseMap3D(normalize(skyvec));
				retcolor.rgb += universe*max(fade-0.5,0)*0.04;
				
				return retcolor;
			}
			ENDCG 
		}
	}
}