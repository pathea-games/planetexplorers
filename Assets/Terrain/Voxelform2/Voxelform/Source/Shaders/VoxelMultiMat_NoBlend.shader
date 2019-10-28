Shader "VoxelMultiMaterial_NoBlend" {
Properties {
	_NormSpecPowerTex ("Normal Specular Power Texture", 2D) = "grey"{}
	_DiffuseTex_0 ("Diffuse Texture(0)", 2D) = "white" { } 
    _NormalMap_0 ("Normal Map(0)", 2D) = "bump" { } 
	_DiffuseTex_1 ("Diffuse Texture(1)", 2D) = "white" { } 
    _NormalMap_1 ("Normal Map(1)", 2D) = "bump" { } 
	_DiffuseTex_2 ("Diffuse Texture(2)", 2D) = "white" { } 
    _NormalMap_2 ("Normal Map(2)", 2D) = "bump" { } 
	_DiffuseTex_3 ("Diffuse Texture(3)", 2D) = "white" { } 
    _NormalMap_3 ("Normal Map(3)", 2D) = "bump" { } 
	_DiffuseTex_4 ("Diffuse Texture(4)", 2D) = "white" { } 
    _NormalMap_4 ("Normal Map(4)", 2D) = "bump" { } 

	_NormalPower ("Normal Power", Float) = 2
//	_SpecularPower ("Specular Power", Float) = 0.2
	_TileSize("Tile Size", Float)= 0.125
	_Repetition("_Repetition Size", Float)= 2
	_Indent("_Indent", Float)= 0
	_LogCoefficient("LogCoefficient", Float)= 0.82
	_DotCoefficient("DotCoefficient", Float)= 0.71
    }

    SubShader {
		Tags { "RenderType" = "Opaque" }

		CGPROGRAM
// Upgrade NOTE: excluded shader from Xbox360 and OpenGL ES 2.0 because it uses unsized arrays
#pragma exclude_renderers xbox360 gles
		#pragma target 3.0
		#pragma surface surf SimpleLambert vertex:vert 
		//approxview halfasview
	
		#pragma glsl
		
		sampler2D _NormSpecPowerTex;
		sampler2D _DiffuseTex_0;
		sampler2D _NormalMap_0;
		sampler2D _DiffuseTex_1;
		sampler2D _NormalMap_1;
		sampler2D _DiffuseTex_2;
		sampler2D _NormalMap_2;
		sampler2D _DiffuseTex_3;
		sampler2D _NormalMap_3;
		sampler2D _DiffuseTex_4;
		sampler2D _NormalMap_4;
		
		float _NormalPower;
//		float _SpecularPower;
		float  _TileSize;
		float  _Repetition;
		float _Indent;
		float  _LogCoefficient;
		float  _DotCoefficient;

	 	struct CustomSurfaceOutput
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Alpha;
			half3 normBumpNormal;
			half Specular;
		};

		half4 LightingSimpleLambert (CustomSurfaceOutput s, half3 normLightDir, half3 normViewDir, half atten)
		{ 
			//normLightDir = normalize(normLightDir); 
			//normViewDir = normalize(normViewDir);
			half NdotL = dot(s.normBumpNormal, normLightDir);
			
			half3 h = normalize (normLightDir + normViewDir);
			
			half nh = max (0, dot (s.normBumpNormal, h));
			half spec = smoothstep(0, 1.0, pow(nh, 32.0 * s.Specular)) * s.Specular; 
			
			half4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * NdotL + _LightColor0.rgb * spec) * atten; 
			c.a = s.Alpha;

			return c;
		}

			
		struct Input {
			float3 worldPos;
			float3 worldNormal;
			float4 weights;
		    float4 blendMask; 
		    float4 color : COLOR;
		};
		void vert (inout appdata_full v, out Input o)
		{  
			UNITY_INITIALIZE_OUTPUT(Input, o);
		    o.weights.xyz = v.tangent.xyz * 255.0f;
		    int idx = floor(v.tangent.w*10.0f+0.5f); 			// voxel type stores in tangent
		    o.blendMask.xyz = float3(idx == 0,idx == 1,idx == 2); 	// may use step
		    float2 normSpec = tex2Dlod(_NormSpecPowerTex, float4(dot(o.blendMask,v.tangent.xyz),0.1,0,0)).xy;
		    o.weights.w = normSpec.x*16-8;
		    o.blendMask.w = normSpec.y*4;
		} 
		
		float3 CalcBaseUV(float index)
		{
			int roundedIndex = floor(index +0.5f);
			float tileX = roundedIndex*_TileSize; 
			float tileY = floor(tileX)*_TileSize;
			
			return float3(frac(tileX), frac(tileY), floor(tileY));
		}
		float4 GetDiffuseTex(float3 baseUV, float2 uv)
		{
			float correction = 1.0f/512.0f;
			uv /= _Repetition;
			uv = baseUV.xy + (correction * _Indent + frac(uv) * (1 - _Indent *2* correction))*_TileSize;
			//uv = baseUV + frac(uv)*_TileSize;
			//uv = baseUV + frac(uv)*_TileSize*0.99f;
			float4 uv4 = float4(uv.x,uv.y,0,_NormalPower);
			return 
			 		tex2Dlod(_DiffuseTex_0, uv4)*(baseUV.z==0)
			 		//tex2D(_DiffuseTex_0, uv)*(baseUV.z==0)
					+
					tex2D(_DiffuseTex_1, uv)*(baseUV.z==1)
					+
					tex2D(_DiffuseTex_2, uv)*(baseUV.z==2)
					+
					tex2D(_DiffuseTex_3, uv)*(baseUV.z==3)
					+
					tex2D(_DiffuseTex_4, uv)*(baseUV.z==4)
					;
		}
		
		float4 GetNormalMap(float3 baseUV, float2 uv)
		{ 
			float correction = 1.0f/512.0f;
			uv /= _Repetition;
			uv = baseUV.xy + (correction * _Indent + frac(uv) * (1 - _Indent *2* correction))*_TileSize;
			float4 uv4 = float4(uv.x,uv.y,0,_NormalPower);
			return  
					tex2Dlod(_NormalMap_0, uv4)*(baseUV.z==0)
					//tex2D(_NormalMap_0, uv)*(baseUV.z==0)
					+
					tex2D(_NormalMap_1, uv)*(baseUV.z==1)
					+
					tex2D(_NormalMap_2, uv)*(baseUV.z==2)
					+
					tex2D(_NormalMap_3, uv)*(baseUV.z==3)
					+
					tex2D(_NormalMap_4, uv)*(baseUV.z==4)
					- 0.5;
		}
		float3 CalNormalContributions(float3 baseUV, float3 worldPos, float3 blend_weights)
		{
			// Sample bump maps too, and generate bump vectors. 
			float2 bumpFetch1 = GetNormalMap(baseUV, worldPos.yz).xy;
			float2 bumpFetch2 = GetNormalMap(baseUV, worldPos.zx).xy;
			float2 bumpFetch3 = GetNormalMap(baseUV, worldPos.xy).xy; 

			// (Note: this uses an oversimplified tangent basis.) 
			float3 bump1 = float3(0, bumpFetch1.x, bumpFetch1.y); 
			float3 bump2 = float3(bumpFetch2.y, 0, bumpFetch2.x); 
			float3 bump3 = float3(bumpFetch3.x, bumpFetch3.y, 0);

			return  bump1.xyz * blend_weights.xxx + 
					bump2.xyz * blend_weights.yyy + 
					bump3.xyz * blend_weights.zzz;
					
//			return  GetNormalMap(baseUV, worldPos.xy) * blend_weights.z + 
//					GetNormalMap(baseUV, worldPos.yz) * blend_weights.x + 
//					GetNormalMap(baseUV, worldPos.zx) * blend_weights.y;		
		}
		void surf (Input i, inout CustomSurfaceOutput o)
		{
			float4 contributions; 
			float3 contributions_normal; 
			float3 normalweights = normalize(abs(i.worldNormal));
			normalweights = (normalweights - 0.2) * 7; 
			normalweights = max(normalweights, 0);      // Force weights to sum to 1.0 (very important!) 
			normalweights /= (normalweights.x + normalweights.y + normalweights.z ).xxx; 

			float3 uvR = CalcBaseUV(i.weights.r);
			float3 uvG = CalcBaseUV(i.weights.g);
			float3 uvB = CalcBaseUV(i.weights.b); 

			float3 dist = i.worldPos-_WorldSpaceCameraPos;
			_NormalPower = log(_DotCoefficient*(dot(dist,dist))) * _LogCoefficient;
			int typeCount = 3;
			if(i.weights.r == i.weights.b ||
			i.weights.r == i.weights.g || 
			i.weights.g == i.weights.b )
				typeCount = 2; 
			
			if(i.weights.r == i.weights.g &&
			i.weights.g == i.weights.b )
				typeCount = 1;
				
			if(typeCount == 3)
			{
				// 3 distinct types
				if(max(max (i.blendMask.x, i.blendMask.y), i.blendMask.z) == i.blendMask.z)
				{
					i.blendMask.xyz = float3(0,0,1);
				}
				if(max(max (i.blendMask.z, i.blendMask.y), i.blendMask.x) == i.blendMask.x)
				{
					i.blendMask.xyz = float3(1,0,0);
				}
				if(max(max (i.blendMask.x, i.blendMask.z), i.blendMask.y) == i.blendMask.y)
				{
					i.blendMask.xyz = float3(0,1,0);
				}
			}
			if(typeCount == 2 )
			{
				if(i.weights.r == i.weights.g ) 
				{
					i.blendMask.xy = (i.blendMask.z < 0.5) ? 0.5: 0;
					i.blendMask.z = (i.blendMask.z < 0.5) ? 0: 1;
				}
				if(i.weights.b == i.weights.g ) 
				{
					i.blendMask.yz = (i.blendMask.x < 0.5) ? 0.5: 0;
					i.blendMask.x = (i.blendMask.x < 0.5) ? 0: 1;
				}
				if(i.weights.r == i.weights.b )
				{
					i.blendMask.xz = (i.blendMask.y < 0.5) ? 0.5: 0;
					i.blendMask.y = (i.blendMask.y < 0.5) ? 0: 1;
				}
			}

			contributions = i.blendMask.x * (GetDiffuseTex(uvR, i.worldPos.xy) * normalweights.z + 
								 GetDiffuseTex(uvR, i.worldPos.yz) * normalweights.x + 
								 GetDiffuseTex(uvR, i.worldPos.zx) * normalweights.y)
								 +
							i.blendMask.y * (GetDiffuseTex(uvG, i.worldPos.xy) * normalweights.z +
								 GetDiffuseTex(uvG, i.worldPos.yz) * normalweights.x + 
								 GetDiffuseTex(uvG, i.worldPos.zx) * normalweights.y)
								 +
							i.blendMask.z * ( GetDiffuseTex(uvB, i.worldPos.xy) * normalweights.z +
								 GetDiffuseTex(uvB, i.worldPos.yz) * normalweights.x + 
								 GetDiffuseTex(uvB, i.worldPos.zx) * normalweights.y);
								 
			contributions_normal = i.blendMask.x * (CalNormalContributions(uvR, i.worldPos, normalweights)) * 2
								 +
								 i.blendMask.y * (CalNormalContributions(uvG, i.worldPos, normalweights)) * 2
								 +
								 i.blendMask.z * (CalNormalContributions(uvB, i.worldPos, normalweights))*2;
 
			o.Albedo = contributions.rgb;//*i.color.rgb; 
			//o.Albedo = i.blendMask;
			//o.Albedo =  tex2D(_DiffuseTex_0, i.worldPos.xz / _Repetition).xyz;
			
			//if(i.blendMask.x  <= 0.5f)
				//o.Albedo = float3(1,0,0);
			 
			float4 n = float4(contributions_normal.x, contributions_normal.y, contributions_normal.z, 1);
			float4 camVec = normalize(n);
			
			float3 N_for_lighting = normalize(i.worldNormal - (camVec.xyz) * i.weights.w);// _NormalPower); 
			//N_for_lighting = normalize(float4(0,1,0,1) - camVec);
			//N_for_lighting = normalize(i.worldNormal - camVec);
			  
			o.normBumpNormal = N_for_lighting;
			o.Specular = 0;//i.blendMask.w;//_SpecularPower;
		}

		ENDCG
    }

    Fallback "Diffuse"
}
	