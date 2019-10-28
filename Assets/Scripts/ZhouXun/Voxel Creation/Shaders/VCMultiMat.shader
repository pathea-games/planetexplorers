Shader "Voxel Creation/MultiMat" 
{
	Properties 
	{
		_DiffuseTex ("Diffuse Texture", 2D) = "white" {}
		_BumpTex ("Bump Texture", 2D) = "bump" {}
		_PropertyTex ("Property Texture", 2D) = "black" {}
		_Settings1 ("Tile width, Tile height, Cell indent, Texture size", Vector) = (0.25,0.25,1,128)
		_Settings2 ("Bump scale, Specular strength scale, Specular power scale, Tile scale", Vector) = (1,1,0,0)
    	//_AlphaCutoff ("Alpha cutoff", Range(0,1)) = .5
    }

    SubShader 
    {
		Tags { "RenderType"="Opaque" "Queue"="Geometry+450" }
		
        Cull Off
        
        CGPROGRAM
        #pragma target 3.0
//        #pragma surface surf VCLambert vertex:vert alphatest:_AlphaCutoff addshadow nolightmap nodirlightmap exclude_path:prepass
        #pragma surface surf VCLambert vertex:vert addshadow nolightmap nodirlightmap exclude_path:prepass
        #pragma glsl
        
        sampler2D _DiffuseTex;
        sampler2D _BumpTex;
        sampler2D _PropertyTex;
        
        float4 _Settings1;
        float4 _Settings2;
        
        struct VCSurfaceOutput
        {
            float3 Albedo;
            float3 Normal;
            float3 Normal_;
            float3 Emission;
            float3 SpecularColor;
            float  Specular;
            float  SpecularStrength;
            float  Alpha;
        };
            
        struct Input
        {
            float3 localPos;
            float3 localNormal;
            float3 weightMask;
            float4 vertColor;
        };
        
        // Vertex
        void vert (inout appdata_full v, out Input o)
        {  
            int mat0 = v.texcoord1.x/4;
            int mat12 = v.texcoord1.y;
            int mat1 = (float)mat12/256.0;
            int mat2 = mat12 - mat1 * 256;
            float3 matIdx = float3(mat0, mat1, mat2); // 3 vertex matidx in triangle
            int idx = (v.texcoord1.y-mat12)*10.0f+0.499999f;  // vertex index in triangle 
            float3 blendMask = float3(idx == 0, idx == 1, idx == 2) * 0.004f + 0.498f; // blend mask 
            
            o.weightMask = matIdx + blendMask; // Combine matidx and blend mask
            //v.normal = float3(v.texcoord.x, v.texcoord.y, v.texcoord1.x-4*mat0-2);
            o.localPos = v.vertex.xyz;
            o.localNormal = normalize(v.normal.xyz);
            o.vertColor = v.color; 
        } 
        
        // Calculate the start UV of each voxel type
        float3 CalcBaseUV(float index)
        {
            int roundedIndex = floor(index + 0.5);
            float tileX = roundedIndex * _Settings1.x; 
            float tileY = floor(tileX) * _Settings1.y;
            
            return float3(frac(tileX), frac(tileY), floor(tileY));
        }
        float4 GetDiffuseColor(float3 base_uv, float2 uv, float lod)
        {
            float indent = pow(2,ceil(lod));
            
            float correction = 0.5/_Settings1.w;
            uv = base_uv.xy + ( (correction * indent) + (frac(uv) * (1 - indent * 2 * correction)) )*_Settings1.xy;
            float4 uv4 = float4(uv.x,uv.y,0,lod);
            
            return tex2Dlod(_DiffuseTex, uv4);
        }
        float4 GetBumpedNormal(float3 base_uv, float2 uv, float lod)
        { 
            float indent = pow(2,ceil(lod));
            
            float correction = 0.5/_Settings1.w;
            uv = base_uv.xy + ( (correction * indent) + (frac(uv) * (1 - indent * 2 * correction)) )*_Settings1.xy;
            float4 uv4 = float4(uv.x,uv.y,0,lod);
            
            return tex2Dlod(_BumpTex, uv4) - 0.49803922f; // 127 is 0, 254 is full
        }
        #define MAT_NUM    16.0
        #define TRACK_NUM  4.0
        float4 GetProperty(float3 mat012, float3 blend_mask, float track)
        {
        	float4 p0 = tex2Dlod(_PropertyTex, float4((mat012.x+0.499)/MAT_NUM, (track+0.499)/TRACK_NUM, 0, 0));
        	float4 p1 = tex2Dlod(_PropertyTex, float4((mat012.y+0.499)/MAT_NUM, (track+0.499)/TRACK_NUM, 0, 0));
        	float4 p2 = tex2Dlod(_PropertyTex, float4((mat012.z+0.499)/MAT_NUM, (track+0.499)/TRACK_NUM, 0, 0));
        	return p0*blend_mask.x + p1*blend_mask.y + p2*blend_mask.z;
        }
        float4 GetPropertyNoBlend(float mat, float track)
        {
        	return tex2Dlod(_PropertyTex, float4((mat+0.499)/MAT_NUM, (track+0.499)/TRACK_NUM, 0, 0));
        }
        float3 CalNormalContributions(float3 base_uv, float3 localPos, float3 abs_nw, float3 sign_nw, float lod)
        {
            // Sample bump maps too, and generate bump vectors. 
            float2 bumpFetch1 = GetBumpedNormal(base_uv, localPos.zy * float2(sign_nw.x, 1), lod).xy;
            float2 bumpFetch2 = GetBumpedNormal(base_uv, localPos.xz * float2(sign_nw.y, 1), lod).xy;
            float2 bumpFetch3 = GetBumpedNormal(base_uv, localPos.xy * float2(-sign_nw.z, 1), lod).xy; 

            // (Note: this uses an oversimplified tangent basis.) 
            float3 bump1 = float3(0, bumpFetch1.x, bumpFetch1.y); 
            float3 bump2 = float3(bumpFetch2.y, 0, bumpFetch2.x); 
            float3 bump3 = float3(bumpFetch3.x, bumpFetch3.y, 0);

            return  bump1.xyz * abs_nw.xxx + 
                    bump2.xyz * abs_nw.yyy + 
                    bump3.xyz * abs_nw.zzz;
        }
        
        // Surface
        void surf (Input i, inout VCSurfaceOutput o)
        {
        	float3 matid012 = trunc(i.weightMask);
        	float3 blend_mask = (frac(i.weightMask) - 0.498) * 250;
        	float sum = blend_mask.x + blend_mask.y + blend_mask.z;
        	blend_mask = sum == 0 ? float3(1,0,0) : blend_mask/sum;
        		
        	float tile0 = _Settings2.w * GetPropertyNoBlend(matid012.x, 3).r * 17;
        	float tile1 = _Settings2.w * GetPropertyNoBlend(matid012.y, 3).r * 17;
        	float tile2 = _Settings2.w * GetPropertyNoBlend(matid012.z, 3).r * 17;
        
            float4 contributions = (0, 0, 0, 0); 
            float3 contributions_normal = (0, 0, 0);
            float3 normalweights = i.localNormal;
        	float3 sign_nw = sign(normalweights); 
        	float3 abs_nw = abs(normalweights);
        	abs_nw = normalize(abs_nw);
        	abs_nw -= 0.35f;
        	abs_nw = max(abs_nw, 0);
            float dim_l = abs_nw.x + abs_nw.y + abs_nw.z;
            abs_nw /= dim_l;

            float3 uv0 = CalcBaseUV(matid012.x);
            float3 uv1 = CalcBaseUV(matid012.y);
            float3 uv2 = CalcBaseUV(matid012.z); 
 
        	float dcoordx = length(float3( ddx(i.localPos.x), ddx(i.localPos.y), ddx(i.localPos.z) ));
        	float dcoordy = length(float3( ddy(i.localPos.x), ddy(i.localPos.y), ddy(i.localPos.z) ));
        	float dpos = length(float2(dcoordx, dcoordy));
        	float lod0 = clamp(log2( dpos * tile0 * _Settings1.w ) - 1,0,5);
        	float lod1 = clamp(log2( dpos * tile1 * _Settings1.w ) - 1,0,5);
        	float lod2 = clamp(log2( dpos * tile2 * _Settings1.w ) - 1,0,5);
        	
            
            contributions = blend_mask.x * (GetDiffuseColor(uv0, i.localPos.xy * float2(-tile0 * sign_nw.z, tile0), lod0) * abs_nw.z + 
                                            GetDiffuseColor(uv0, i.localPos.zy * float2( tile0 * sign_nw.x, tile0), lod0) * abs_nw.x + 
                                            GetDiffuseColor(uv0, i.localPos.xz * float2( tile0 * sign_nw.y, tile0), lod0) * abs_nw.y)
                          + blend_mask.y * (GetDiffuseColor(uv1, i.localPos.xy * float2(-tile1 * sign_nw.z, tile1), lod1) * abs_nw.z +
                                            GetDiffuseColor(uv1, i.localPos.zy * float2( tile1 * sign_nw.x, tile1), lod1) * abs_nw.x + 
                                            GetDiffuseColor(uv1, i.localPos.xz * float2( tile1 * sign_nw.y, tile1), lod1) * abs_nw.y)
                          + blend_mask.z * (GetDiffuseColor(uv2, i.localPos.xy * float2(-tile2 * sign_nw.z, tile2), lod2) * abs_nw.z +
                                            GetDiffuseColor(uv2, i.localPos.zy * float2( tile2 * sign_nw.x, tile2), lod2) * abs_nw.x + 
                                            GetDiffuseColor(uv2, i.localPos.xz * float2( tile2 * sign_nw.y, tile2), lod2) * abs_nw.y);
                                 
            contributions_normal =	blend_mask.x * (CalNormalContributions(uv0, i.localPos * tile0, abs_nw, sign_nw, lod0)) * 2
                                  + blend_mask.y * (CalNormalContributions(uv1, i.localPos * tile1, abs_nw, sign_nw, lod1)) * 2
                                  + blend_mask.z * (CalNormalContributions(uv2, i.localPos * tile2, abs_nw, sign_nw, lod2)) * 2;

            o.Albedo = (contributions.rgb * i.vertColor.rgb) * (1-i.vertColor.a) + i.vertColor.rgb * i.vertColor.a;
            float4 n = normalize(float4(contributions_normal.x, contributions_normal.y, contributions_normal.z, 1));
            float3 wn = mul(_Object2World, float4(i.localNormal,0));
            float3 N_for_lighting = normalize(normalize(wn) - (n.xyz) * GetProperty(matid012, blend_mask, 2).r * _Settings2.x);

            o.Normal_ = N_for_lighting;
            o.Specular = GetProperty(matid012, blend_mask, 2).b * 255;
            o.Emission = GetProperty(matid012, blend_mask, 1).rgb;
            o.SpecularColor = GetProperty(matid012, blend_mask, 0).rgb;
            o.SpecularStrength = GetProperty(matid012, blend_mask, 2).g * 2 * _Settings2.y;
            o.Alpha = contributions.a;
        }
        
        // Lighting 
        half4 LightingVCLambert (VCSurfaceOutput s, half3 normLightDir, half3 normViewDir, half atten)
        {
            // Lighting model
            half NdotL = clamp( dot(s.Normal_, normLightDir ), 0, 1 );
            half3 h = normalize ( normLightDir + normViewDir );
            half nh = max (0, dot (s.Normal_, h));
            half spec = smoothstep(0, 1.0, pow(nh, _Settings2.z * s.Specular)) * s.SpecularStrength; 
			
			_SpecColor = float4(s.SpecularColor,1);
			
            half4 c = (0,0,0,0);
            c.rgb = (s.Albedo * _LightColor0.rgb * NdotL + _LightColor0.rgb * _SpecColor * spec) * atten;
            c.a = s.Alpha;
            
            return c;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
