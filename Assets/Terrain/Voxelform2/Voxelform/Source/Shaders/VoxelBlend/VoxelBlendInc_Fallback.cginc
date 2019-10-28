
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
sampler2D _DiffuseTex_5;
sampler2D _NormalMap_5;

//float _lodLevel;
float _TileSize;
float _TileSize_1;
float _Repetition;
float _Repetition_1;
float _Indent;
//float _LogCoefficient;
//float _DotCoefficient;
float4 _AmbientThres;

struct CustomSurfaceOutput
{
	half3 Albedo;
	half3 Normal;
	half3 Normal_;
	half3 Emission;
	half Specular;
	half Gloss;
	half Alpha;
};
struct Input {
	float3 worldPos; 
	float3 worldNormal;// INTERNAL_DATA 
	float4 weights;
    float4 blendMask; 
    float4 color : COLOR;
};

float3 CalcBaseUV(float index, out float TileSize)
{
	float tileX, tileY;
	if(index >= 68)
	{
		TileSize = _TileSize;
		tileX = (index - 68) * TileSize;
		tileY = floor(tileX) * TileSize;
		return float3(frac(tileX), frac(tileY), 5);
	}
	if(index >= 64)
	{
		TileSize = _TileSize_1;
		tileX = (index - 64) * TileSize;
		tileY = floor(tileX) * TileSize;
		return float3(frac(tileX), frac(tileY), 4);
	}
	TileSize = _TileSize;
	tileX = index*TileSize; 
	tileY = floor(tileX)*TileSize;
	return float3(frac(tileX), frac(tileY), floor(tileY));
}
float4 GetDiffuseTex(float3 baseUV, float2 uv, float tileSize, float n_lodLevel)
{
	float correction = 0.00390625;	// (1/512)*2
	//correction /= _TileSize / tileSize;
	if( _TileSize != tileSize )
	{
		correction *= 0.5;
		uv /= _Repetition_1;
	}
	else 
	{
		uv /= _Repetition;
	}
	uv = baseUV.xy + (correction * _Indent + frac(uv) * (1 - _Indent *2* correction))*tileSize;
	//uv = baseUV + frac(uv)*_TileSize;
	//uv = baseUV + frac(uv)*_TileSize*0.99f;
	float4 uv4 = float4(uv.x,uv.y,0,n_lodLevel);
	return 	tex2Dlod(_DiffuseTex_0, uv4)*(baseUV.z==0)+
			tex2Dlod(_DiffuseTex_1, uv4)*(baseUV.z==1)+
			tex2Dlod(_DiffuseTex_2, uv4)*(baseUV.z==2)+
			tex2Dlod(_DiffuseTex_3, uv4)*(baseUV.z==3)+
			tex2Dlod(_DiffuseTex_4, uv4)*(baseUV.z==4)+
			tex2Dlod(_DiffuseTex_5, uv4)*(baseUV.z==5);
}

float4 GetNormalMap(float3 baseUV, float2 uv, float tileSize, float n_lodLevel)
{ 
	float correction = 0.001953125;	// (1/512)
	//correction /= _TileSize / tileSize;
	if( _TileSize != tileSize )
	{
		correction *= 0.5;
		uv /= _Repetition_1;
	}
	else 
	{
		uv /= _Repetition;
	}
	uv = baseUV.xy + (correction * _Indent + frac(uv) * (1 - _Indent *2* correction))*tileSize;
	float4 uv4 = float4(uv.x,uv.y,0,n_lodLevel);
	return  2*(
			tex2Dlod(_NormalMap_0, uv4)*(baseUV.z==0)+
			tex2Dlod(_NormalMap_1, uv4)*(baseUV.z==1)+
			tex2Dlod(_NormalMap_2, uv4)*(baseUV.z==2)+
			tex2Dlod(_NormalMap_3, uv4)*(baseUV.z==3)+
			tex2Dlod(_NormalMap_4, uv4)*(baseUV.z==4)+
			tex2Dlod(_NormalMap_5, uv4)*(baseUV.z==5)
			- 0.5);
}

float3 CalNormalContributions(float3 baseUV, float3 worldPos, float3 blend_weights, float tileSize, float n_lodLevel)
{
	// Sample bump maps too, and generate bump vectors. 
	float2 bumpFetch1 = GetNormalMap(baseUV, worldPos.yz, tileSize,n_lodLevel).xy;
	float2 bumpFetch2 = GetNormalMap(baseUV, worldPos.zx, tileSize,n_lodLevel).xy;
	float2 bumpFetch3 = GetNormalMap(baseUV, worldPos.xy, tileSize,n_lodLevel).xy; 

	// (Note: this uses an oversimplified tangent basis.) 
	float3 bump1 = float3(0, bumpFetch1.x, bumpFetch1.y); 
	float3 bump2 = float3(bumpFetch2.y, 0, bumpFetch2.x); 
	float3 bump3 = float3(bumpFetch3.x, bumpFetch3.y, 0);

	return  bump1.xyz * blend_weights.xxx + 
			bump2.xyz * blend_weights.yyy + 
			bump3.xyz * blend_weights.zzz;
			
//	return  GetNormalMap(baseUV, worldPos.xy,n_lodLevel) * blend_weights.z + 
//			GetNormalMap(baseUV, worldPos.yz,n_lodLevel) * blend_weights.x + 
//			GetNormalMap(baseUV, worldPos.zx,n_lodLevel) * blend_weights.y;		
}

float3 BlendEdgeModifier(float3 weights, float3 blendMask)
{
	int typeCount = 3;
	float3 centerTriTypeRef = 0;

	float3 types;
	if(
	weights.r == weights.b ||
	weights.r == weights.g || 
	weights.g == weights.b )
	{
		if(weights.r == weights.b)
		{
			centerTriTypeRef = float3(1, 0, 0);
			types.x = weights.r;
			types.y = weights.g;
		}
		else if(weights.r == weights.g)
		{
			centerTriTypeRef = float3(1, 0, 0);
			types.x = weights.r;
			types.y = weights.b;
		}
		else if(weights.g == weights.b)
		{
			centerTriTypeRef = float3(0, 1, 0);
			types.x = weights.g;
			types.y = weights.r;
		}
		
		typeCount = 2; 
	}
	
	if(weights.r == weights.g &&
	weights.g == weights.b )
		typeCount = 1;
		
	int cut_off = 64;
	float maxBlendMask = max(max (blendMask.x, blendMask.y), blendMask.z);
	if(typeCount == 3)
	{
		// 3 distinct types
		// in this case we need to further divide this case into two subcases:
		// 1. when there are two natural terrain types and one building type, blend all three transitions
		// 2. when there is one natural terrain type and two building types, blend the terrain type's edges.
		// the two building types need not blend.
		
		// building block type count
		int bb_type_count = 0;
		
		// count the sub-cases.
		if(weights.r >= cut_off)
		{
			bb_type_count++;
		}
		if(weights.g >= cut_off)
		{
			bb_type_count++;
		}
		if(weights.b >= cut_off)
		{
			bb_type_count++;
		}
		
		// when bb_type_count == 0 or 1, the 3 types blend normally. no processing is needed
		// B1 T1 T2
		// T1 T2 T3
		if(bb_type_count == 2 )
		{
			// B1 B2 T1
			float barycentricCoordsDistanceToCenter;
			if( blendMask.z < 0.5 && blendMask.y < blendMask.x && weights.b < cut_off)
			{
				//blendMask.xyz = lerp(blendMask.xyz, float3(1,0,0), 1 - blendMask.z * 3.0f);
				blendMask.xyz = float3(1,0,0);
//				blendMask.xyz = 0;
			}			
			if( blendMask.z < 0.5 && blendMask.x < blendMask.y && weights.b < cut_off)
			{
				//blendMask.xyz = lerp(blendMask.xyz, float3(1,0,0), 1 - blendMask.z * 3.0f);
				blendMask.xyz = float3(0,1,0);
			}
			if( blendMask.y < 0.5 && blendMask.z < blendMask.x && weights.g < cut_off)
			{
				//blendMask.xyz = lerp(blendMask.xyz, float3(1,0,0), 1 - blendMask.z * 3.0f);
				blendMask.xyz = float3(1,0,0);
			}			
			if( blendMask.y < 0.5 && blendMask.x < blendMask.z && weights.g < cut_off)
			{
				//blendMask.xyz = lerp(blendMask.xyz, float3(1,0,0), 1 - blendMask.z * 3.0f);
				blendMask.xyz = float3(0,0,1);
			}
			if( blendMask.x < 0.5 && blendMask.y < blendMask.z && weights.r < cut_off)
			{
				//blendMask.xyz = lerp(blendMask.xyz, float3(1,0,0), 1 - blendMask.z * 3.0f);
				blendMask.xyz = float3(0,0,1);
			}			
			if( blendMask.x < 0.5 && blendMask.z < blendMask.y && weights.r < cut_off)
			{
				//blendMask.xyz = lerp(blendMask.xyz, float3(1,0,0), 1 - blendMask.z * 3.0f);
				blendMask.xyz = float3(0,1,0);
			}
		}
		else if(bb_type_count == 3)
		{
			// the 3 distinct voxel types are 3 different building block types.
			// B1 B2 B3
			if(maxBlendMask == blendMask.z )
			{
				blendMask.xyz = float3(0,0,1);
			}
			if(maxBlendMask == blendMask.x )
			{
				blendMask.xyz = float3(1,0,0);
			}
			if(maxBlendMask == blendMask.y )
			{
				blendMask.xyz = float3(0,1,0);
			}
		}
	}
	
	if(typeCount == 2 
	&& types.x >= cut_off 
	&& types.y >= cut_off
	)
	{
		if(blendMask.x > 0.5f)
		{
			blendMask.xyz = float3(1,0,0);
		}
		if(blendMask.y > 0.5f)
		{
			blendMask.xyz = float3(0,1,0);
		}
		if(blendMask.z > 0.5f )
		{
			blendMask.xyz = float3(0,0,1);
		}
		if(blendMask.x <= 0.5 &&
			blendMask.y <= 0.5 &&
			blendMask.z <= 0.5 )
		{
			blendMask.xyz = centerTriTypeRef;
		}
//		blendMask.xyz = 0;
	}

	return blendMask;
}

half4 LightingSimpleLambert (CustomSurfaceOutput s, half3 normLightDir, half3 normViewDir, half atten)
{ 
	//normLightDir = normalize(normLightDir); 
	//normViewDir = normalize(normViewDir);
	half NdotL = clamp(dot(s.Normal_, normLightDir),0,1);
	half3 h = normalize (normLightDir + normViewDir);
	half nh = max (0, dot (s.Normal_, h));
	half spec = smoothstep(0, 1.0, pow(nh, 32.0 * s.Specular)) * s.Specular; 
	half3 ambient_new = clamp(UNITY_LIGHTMODEL_AMBIENT*atten, _AmbientThres.rgb, half3(1,1,1));
	
	half4 c;
	c.rgb = (ambient_new*s.Albedo + s.Albedo * _LightColor0.rgb * NdotL + _LightColor0.rgb * spec)*atten;  
	return c;
}
void vert (inout appdata_full v, out Input o)	// same as above
{  
	UNITY_INITIALIZE_OUTPUT(Input, o);
	
	int mat0 = v.texcoord1.x/4;
    int mat12 = v.texcoord1.y;
	int mat1 = mat12/256;
	float3 matIdx = float3(mat0,mat1,(mat12-mat1*256));	// No % operator in GLShader
    o.weights.xyz = matIdx-1;
    
    int idx = (v.texcoord1.y-mat12)*10.0f+0.4999f;	// voxel type
    o.blendMask.xyz = float3(idx == 0,idx == 1,idx == 2); 	// may use step
    float2 normSpec = tex2Dlod(_NormSpecPowerTex, float4(dot(o.blendMask,matIdx)/255.0f,0.1,0,0)).xy;
    o.weights.w = normSpec.x*16-8;	// NormalPower
    o.blendMask.w = normSpec.y*4; 	// SpecPower
	v.normal = float3(v.texcoord.x,v.texcoord.y,v.texcoord1.x-4*mat0-2); 
}
void surf (Input i, inout CustomSurfaceOutput o)
{
	float4 contributions; 
	float3 contributions_normal; 
	float3 normalweights = normalize(abs(i.worldNormal));
	normalweights = (normalweights - 0.2) * 7; 
	normalweights = max(normalweights, 0);      // Force weights to sum to 1.0 (very important!) 
	normalweights /= (normalweights.x + normalweights.y + normalweights.z ).xxx; 
	float tileSizeR; 
	float tileSizeG;
	float tileSizeB;
	
	i.weights.xyz = floor(i.weights + 0.5f);
	//i.blendMask.xyz = BlendEdgeModifier(i.weights.xyz, i.blendMask.xyz);

	float3 uvR = CalcBaseUV(i.weights.r, tileSizeR);
	float3 uvG = CalcBaseUV(i.weights.g, tileSizeG);
	float3 uvB = CalcBaseUV(i.weights.b, tileSizeB); 

	float3 dist = i.worldPos-_WorldSpaceCameraPos;
	//_lodLevel = log(_DotCoefficient*(dot(dist,dist))) * _LogCoefficient;
	float n_lodLevel = 1.1*log(0.04*dot(dist,dist));

	contributions = i.blendMask.x *
						(GetDiffuseTex(uvR, i.worldPos.xy, tileSizeR,n_lodLevel) * normalweights.z + 
						 GetDiffuseTex(uvR, i.worldPos.yz, tileSizeR,n_lodLevel) * normalweights.x + 
						 GetDiffuseTex(uvR, i.worldPos.zx, tileSizeR,n_lodLevel) * normalweights.y)
						 +
					i.blendMask.y * 
						(GetDiffuseTex(uvG, i.worldPos.xy, tileSizeG,n_lodLevel) * normalweights.z +
						 GetDiffuseTex(uvG, i.worldPos.yz, tileSizeG,n_lodLevel) * normalweights.x + 
						 GetDiffuseTex(uvG, i.worldPos.zx, tileSizeG,n_lodLevel) * normalweights.y)
						 +
					i.blendMask.z * 
						(GetDiffuseTex(uvB, i.worldPos.xy, tileSizeB,n_lodLevel) * normalweights.z +
						 GetDiffuseTex(uvB, i.worldPos.yz, tileSizeB,n_lodLevel) * normalweights.x + 
						 GetDiffuseTex(uvB, i.worldPos.zx, tileSizeB,n_lodLevel) * normalweights.y);
						 
	contributions_normal=i.blendMask.x * (CalNormalContributions(uvR, i.worldPos, normalweights, tileSizeR,n_lodLevel))
						 +
						 i.blendMask.y * (CalNormalContributions(uvG, i.worldPos, normalweights, tileSizeG,n_lodLevel))
						 +
						 i.blendMask.z * (CalNormalContributions(uvB, i.worldPos, normalweights, tileSizeB,n_lodLevel));

	o.Albedo = contributions.rgb;//*i.color.rgb;  
	//o.Albedo = i.blendMask;
	//o.Albedo = i.weights.x == 5;
	//o.Albedo = i.worldNormal;
	//o.Albedo = float3(tileSizeR, tileSizeG, tileSizeB);
	//o.Albedo =  tex2D(_DiffuseTex_0, i.worldPos.xz / _Repetition).xyz;
	
	//if(i.blendMask.x  <= 0.5f)
		//o.Albedo = float3(1,0,0);
	 
	float4 n = normalize(float4(contributions_normal.x, contributions_normal.y, contributions_normal.z, 1));
#if SHADER_API_D3D9	
	float3 N_for_lighting = normalize(i.worldNormal - (n.xyz) * i.weights.w);
#else
	float3 N_for_lighting = normalize(float4(0,1,0,1));
#endif
	 
	o.Normal_ = N_for_lighting;  
	o.Specular = i.blendMask.w;
}
