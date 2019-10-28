#include "UnityCG.cginc"
//#include "BuildingNoBlend.cginc"

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


float _lodLevel;
float _TileSize;
float _TileSize_1;
float _Repetition;
float _Repetition_1;
float _Indent;
float _LogCoefficient;
float _DotCoefficient;
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
	float3 worldNormal; //INTERNAL_DATA 
	float4 weights;
    float4 blendMask; 
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
float4 GetDiffuseTex(float3 baseUV, float2 uv, float tileSize)
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
	float4 uv4 = float4(uv.x,uv.y,0,_lodLevel);
	return 	tex2Dlod(_DiffuseTex_0, uv4)*(baseUV.z==0)+
			tex2Dlod(_DiffuseTex_1, uv4)*(baseUV.z==1)+
			tex2Dlod(_DiffuseTex_2, uv4)*(baseUV.z==2)+
			tex2Dlod(_DiffuseTex_3, uv4)*(baseUV.z==3)+
			tex2Dlod(_DiffuseTex_4, uv4)*(baseUV.z==4)+
			tex2Dlod(_DiffuseTex_5, uv4)*(baseUV.z==5);
}

float3 GetNormalMap(float3 baseUV, float2 uv, float tileSize)
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
	float4 uv4 = float4(uv.x,uv.y,0,_lodLevel);
	return  UnpackNormal(
			tex2Dlod(_NormalMap_0, uv4)*(baseUV.z==0)+
			tex2Dlod(_NormalMap_1, uv4)*(baseUV.z==1)+
			tex2Dlod(_NormalMap_2, uv4)*(baseUV.z==2)+
			tex2Dlod(_NormalMap_3, uv4)*(baseUV.z==3)+
			tex2Dlod(_NormalMap_4, uv4)*(baseUV.z==4)+
			tex2Dlod(_NormalMap_5, uv4)*(baseUV.z==5)
			);
}
float3 CalNormalContributions(float3 baseUV, float3 worldPos, float3 blend_weights, float tileSize)
{
	// Sample bump maps too, and generate bump vectors. 
	float2 bumpFetch1 = GetNormalMap(baseUV, worldPos.zy, tileSize).xy;   // yz->zy to make horizontal texture mapping in zy plane
	float2 bumpFetch2 = GetNormalMap(baseUV, worldPos.zx, tileSize).xy;
	float2 bumpFetch3 = GetNormalMap(baseUV, worldPos.xy, tileSize).xy;

	// (Note: this uses an oversimplified tangent basis.) 
	float3 bump1 = float3(0, bumpFetch1.x, bumpFetch1.y); 
	float3 bump2 = float3(bumpFetch2.y, 0, bumpFetch2.x); 
	float3 bump3 = float3(bumpFetch3.x, bumpFetch3.y, 0);

	return  bump1.xyz * blend_weights.xxx + 
			bump2.xyz * blend_weights.yyy + 
			bump3.xyz * blend_weights.zzz;
			
//	return  GetNormalMap(baseUV, worldPos.xy) * blend_weights.z + 
//			GetNormalMap(baseUV, worldPos.yz) * blend_weights.x + 
//			GetNormalMap(baseUV, worldPos.zx) * blend_weights.y;		
}

half4 LightingSimpleLambert (CustomSurfaceOutput s, half3 normLightDir, half3 normViewDir, half atten)
{ 
	//normLightDir = normalize(normLightDir); 
	//normViewDir = normalize(normViewDir);
	half NdotL = clamp(dot(s.Normal_, normLightDir),0,1);	
	half3 h = normalize (normLightDir + normViewDir);
	
	half nh = max (0.0001f, dot (s.Normal_, h));
	half spec = smoothstep(0, 1.0, pow(nh, 32.0 * s.Specular)) * s.Specular; 
	half3 ambient_new = clamp(UNITY_LIGHTMODEL_AMBIENT*atten, _AmbientThres.rgb, half3(1,1,1));
	
	half4 c;
	c.a = 1;
	//c.rgb = s.Normal_;
	c.rgb = (ambient_new*s.Albedo + s.Albedo * _LightColor0.rgb * NdotL + _LightColor0.rgb * spec)*atten*0.5;
	return c;
}
half4 LightingSimpleLambert_PrePass (CustomSurfaceOutput s, half4 light)
{
	half spec = clamp(light.a,0,1)*s.Specular;
	half3 ambient_new = clamp(UNITY_LIGHTMODEL_AMBIENT*pow(light.rgb*0.66, 2), _AmbientThres.rgb, half3(1,1,1));
	half4 c;
	//c.a = s.Alpha + spec * _SpecColor.a;  
	// 0.66,0.5 used for these param make deferred lighting' result is almost same as forward lighting
	c.a = 1;	
	c.rgb = ambient_new*s.Albedo + (s.Albedo * (light.rgb*0.5) + light.rgb * spec);
	//c.rgb = s.Albedo * light.rgb;
	//c.a = s.Alpha;
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
	normalweights = max(normalweights, 0.01);      // Force weights to sum to 1.0 (very important!) 
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
#if SHADER_API_D3D9		
	_lodLevel = log(_DotCoefficient*(dot(dist,dist))) * _LogCoefficient;
#else
	_lodLevel = 0.73*log(0.04*dot(dist,dist));
#endif	
    
    // yz->zy to make horizontal texture mapping in zy plane
	contributions = i.blendMask.x *
						(GetDiffuseTex(uvR, i.worldPos.xy, tileSizeR) * normalweights.z + 
						 GetDiffuseTex(uvR, i.worldPos.zy, tileSizeR) * normalweights.x + 
						 GetDiffuseTex(uvR, i.worldPos.zx, tileSizeR) * normalweights.y)
						 +
					i.blendMask.y * 
						(GetDiffuseTex(uvG, i.worldPos.xy, tileSizeG) * normalweights.z +
						 GetDiffuseTex(uvG, i.worldPos.zy, tileSizeG) * normalweights.x + 
						 GetDiffuseTex(uvG, i.worldPos.zx, tileSizeG) * normalweights.y)
						 +
					i.blendMask.z * 
						(GetDiffuseTex(uvB, i.worldPos.xy, tileSizeB) * normalweights.z +
						 GetDiffuseTex(uvB, i.worldPos.zy, tileSizeB) * normalweights.x + 
						 GetDiffuseTex(uvB, i.worldPos.zx, tileSizeB) * normalweights.y);
						 
	contributions_normal = i.blendMask.x * (CalNormalContributions(uvR, i.worldPos, normalweights, tileSizeR))
						 +
						 i.blendMask.y * (CalNormalContributions(uvG, i.worldPos, normalweights, tileSizeG))
						 +
						 i.blendMask.z * (CalNormalContributions(uvB, i.worldPos, normalweights, tileSizeB));

	o.Albedo = contributions.rgb;//*i.color.rgb;  
	 
	float4 n = normalize(float4(contributions_normal.x, contributions_normal.y, contributions_normal.z, 1));
#if SHADER_API_D3D9	
	float3 N_for_lighting = normalize(i.worldNormal - (n.xyz) * i.weights.w);
#else
	float3 N_for_lighting = normalize(i.worldNormal);// - (n.xyz));// * i.weights.w);
	//N_for_lighting = float3(0,1,0);
#endif
	 
	o.Normal_ = N_for_lighting;  
	o.Specular = i.blendMask.w;
}
void surf_pre (Input i, inout CustomSurfaceOutput o)
{
	float4 contributions; 
	float3 contributions_normal; 
	float3 normalweights = normalize(abs(i.worldNormal));
	normalweights = (normalweights - 0.2) * 7; 
	normalweights = max(normalweights, 0.01);      // Force weights to sum to 1.0 (very important!) 
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
#if SHADER_API_D3D9		
	_lodLevel = log(_DotCoefficient*(dot(dist,dist))) * _LogCoefficient;
#else
	_lodLevel = 0.73*log(0.04*dot(dist,dist));
#endif
						 
	contributions_normal = i.blendMask.x * (CalNormalContributions(uvR, i.worldPos, normalweights, tileSizeR))
						 +
						 i.blendMask.y * (CalNormalContributions(uvG, i.worldPos, normalweights, tileSizeG))
						 +
						 i.blendMask.z * (CalNormalContributions(uvB, i.worldPos, normalweights, tileSizeB));

	float4 n = normalize(float4(contributions_normal.x, contributions_normal.y, contributions_normal.z, 1));
#if SHADER_API_D3D9	
	float3 N_for_lighting = normalize(i.worldNormal - (n.xyz) * i.weights.w);
#else
	float3 N_for_lighting = normalize(i.worldNormal);// - (n.xyz));// * i.weights.w);
	//N_for_lighting = float3(0,1,0);
#endif
	
	o.Normal = N_for_lighting; 
	o.Specular = i.blendMask.w;
}
void surf_final (Input i, inout CustomSurfaceOutput o)
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
#if SHADER_API_D3D9		
	_lodLevel = log(_DotCoefficient*(dot(dist,dist))) * _LogCoefficient;
#else
	_lodLevel = 0.73*log(0.04*dot(dist,dist));
#endif

	// yz->zy to make horizontal texture mapping in zy plane
	contributions = i.blendMask.x *
						(GetDiffuseTex(uvR, i.worldPos.xy, tileSizeR) * normalweights.z + 
						 GetDiffuseTex(uvR, i.worldPos.zy, tileSizeR) * normalweights.x + 
						 GetDiffuseTex(uvR, i.worldPos.zx, tileSizeR) * normalweights.y)
						 +
					i.blendMask.y * 
						(GetDiffuseTex(uvG, i.worldPos.xy, tileSizeG) * normalweights.z +
						 GetDiffuseTex(uvG, i.worldPos.zy, tileSizeG) * normalweights.x + 
						 GetDiffuseTex(uvG, i.worldPos.zx, tileSizeG) * normalweights.y)
						 +
					i.blendMask.z * 
						(GetDiffuseTex(uvB, i.worldPos.xy, tileSizeB) * normalweights.z +
						 GetDiffuseTex(uvB, i.worldPos.zy, tileSizeB) * normalweights.x + 
						 GetDiffuseTex(uvB, i.worldPos.zx, tileSizeB) * normalweights.y);

	o.Albedo = contributions.rgb;//*i.color.rgb;  
	o.Specular = i.blendMask.w;
}