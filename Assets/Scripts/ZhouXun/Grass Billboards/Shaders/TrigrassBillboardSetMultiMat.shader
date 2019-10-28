Shader "zhouxun/Grass/Tri-billboard Set (MultiMat)" 
{
	Properties 
	{
		_DiffuseTex ("Diffuse Texture", 2D) = "white" {}
		_PropertyTex ("Property Texture", 2D) = "black" {}
		_WindTex ("Wind Texture", 2D) = "black" {}
		_RandTex ("Random Texture", 2D) = "black" {}
		_WaveTex ("Wave Texture", 2D) = "black" {}
		_Settings1 ("Tile width (%), Tile height (%), LOD indent base (px), Tile size (px)", Vector) = (.125,.125,1,256)
		_Settings2 ("Light base, Light amp, Normal bend strength, Reserved", Vector) = (.05,.45,.3,0)
		_Settings3 ("Property index count, Property track count, Reserved, Reserved", Vector) = (64,4,0,0)
    	_AlphaCutoff ("Alpha cutoff", Range(0,1)) = .5
    	_GAmplitude ("Wave Amplitude", Vector) = (0.3 ,0.35, 0.25, 0.25)
		_GFrequency ("Wave Frequency", Vector) = (1.3, 1.35, 1.25, 1.25)
		_GSteepness ("Wave Steepness", Vector) = (1.0, 1.0, 1.0, 1.0)
		_GSpeed ("Wave Speed", Vector) = (1.2, 1.375, 1.1, 1.5)
		_GDirectionAB ("Wave Direction", Vector) = (0.3 ,0.85, 0.85, 0.25)
		_GDirectionCD ("Wave Direction", Vector) = (0.1 ,0.9, 0.5, 0.5)	
		_WaveCenter  ("Wave center", Vector) = (0, 0, 0, 0)
    }

    SubShader 
    {
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        
        Cull Off
        
        CGPROGRAM
        #include "Gerstner.cginc"
        #pragma target 3.0
        #pragma surface surf GrassLambert vertex:vert alphatest:_AlphaCutoff 
        #pragma glsl
		
        sampler2D _DiffuseTex;
        sampler2D _PropertyTex;
        sampler2D _WindTex;
        sampler2D _RandTex;
        sampler2D _WaveTex;
        
        float4 _Settings1;
        float4 _Settings2;
        float4 _Settings3;
        
        uniform float4 _GAmplitude;
		uniform float4 _GFrequency;
		uniform float4 _GSteepness; 									
		uniform float4 _GSpeed;					
		uniform float4 _GDirectionAB;		
		uniform float4 _GDirectionCD;
		
		float4 _WaveCenter;
        
        struct Input
        {
            float3 texCoords;
            float3 tintColor;
            float lodBias;
            float3 worldNormal;
        };
        struct GrassSurfaceOutput
        {
            float3 Albedo;
            float3 Normal;
            float3 Normal_;
            float3 Emission;
            float  Specular;
            float  Alpha;
        };
        
        // Calculate the start UV of each grass type
        float3 CalcBaseUV(float index)
        {
            float roundedIndex = floor(index + 0.5);
            float tileX = roundedIndex * _Settings1.x; 
            float tileY = floor(tileX) * _Settings1.y;
            
            return float3(frac(tileX), frac(tileY), floor(tileY));
        }
        // Get diffuse
        float4 GetDiffuseColor(float3 base_uv, float2 uv, float lod)
        {
            float indent = pow(2,ceil(lod)) * _Settings1.z;
            
            float correction = 0.5/_Settings1.w;
            uv = base_uv.xy + ( (correction * indent) + (frac(uv) * (1 - indent * 2 * correction)) )*_Settings1.xy;
            float4 uv4 = float4(uv.x,uv.y,0,lod);
            
            return tex2Dlod(_DiffuseTex, uv4);
        }
        // Get property
        float4 GetProperty(float index, float track)
        {
        	return tex2Dlod(_PropertyTex, float4((index+0.499)/_Settings3.x, (track+0.499)/_Settings3.y, 0, 0));
        }
        float3 GetRandom(float2 pos)
        {
        	return tex2Dlod(_RandTex, float4(pos,0,0)).xyz;
        }

        // Vertex
        inline void vert (inout appdata_full v, out Input o)
        {
        	// grass type
        	float type = floor(v.texcoord1.x * _Settings3.x);
			// calculate texcoords
 			o.texCoords.x = (v.vertex.x + 1) * 0.5f;
 			o.texCoords.y = (v.vertex.y + 1) * 0.5f;
 			o.texCoords.z = type;
 			
 			// property
 			float4 prop_0 = GetProperty(type, 0).rgba;
 			float4 prop_1 = GetProperty(type, 1).rgba;
        	// rand
        	float3 randval = GetRandom(v.normal.xz);
        	// grass size
        	float2 minsize = prop_0.xy * 2;
        	float2 maxsize = prop_0.zw * 2;
        	float2 sizearea = maxsize - minsize;
        	float2 finalsize = minsize + sizearea*randval.xy;
        	
			// calculate billboard vertices
			float3 norm = float3(v.texcoord.x, 1-sqrt(v.texcoord.x*v.texcoord.x+v.texcoord.y*v.texcoord.y), v.texcoord.y);
			float3 forward = -float3(UNITY_MATRIX_V[2][0], UNITY_MATRIX_V[2][1], UNITY_MATRIX_V[2][2]);
//			float3 y_up = float3(0,1,0); // world y-up
//			float3 right = normalize(cross(y_up, forward)); // horizontal right direction
//			float3 up = normalize(cross(forward, right)); // camera up direction
//			up.y = 1; // constraint for up.y so that grasses will not become very flatwise when looking down from above
//			up = lerp(norm, up, 0.5 + 0.5*abs(dot(forward, norm)));
//			up = normalize(up);
//			float3 billboard_vert = v.vertex.x*finalsize.x*right + (v.vertex.y+0.5)*finalsize.y*up; // vertex local pos in each billboard
			
			// grass bend (wind effect)
 			// calculate final vertices
 			float angle = v.texcoord1.y;
			float abs_x = finalsize.x*cos(angle);
			float abs_z = finalsize.x*sin(angle);
			float ofs = randval.x * 0.5 + 0.1;
			float side = (randval.y) * 1 + forward.y * forward.y;
			float side_factor = (1+side*v.vertex.y);
			float bias_x = -abs_z*ofs;
			float bias_z = abs_x*ofs;
			
			// Gerstner
			half3 vtxForAni = (v.normal).xzz * 1.0; 		
			half3 nrml;
			half3 offsets;
			half3 objectSpaceVert = mul(_World2Object, half4( v.normal, 1)).xyz;
			Gerstner (
				offsets, nrml, v.normal.xyz, vtxForAni, 					// offsets, nrml will be written
				_GAmplitude,					 							// amplitude
				_GFrequency,				 								// frequency
				_GSteepness, 												// steepness
				_GSpeed,													// speed
				_GDirectionAB,												// direction # 1, 2
				_GDirectionCD												// direction # 3, 4
				);
		
			float bendfactor = prop_1.x * 0.5;
			float3 wind_dir = nrml.xyz * 2;
			//wind_dir = normalize(wind_dir);
			//wind_dir.y /= 3;
			wind_dir.y = 0; 
 			
 			half3 wave_ofs_dir;
 			wave_ofs_dir = (tex2Dlod(_WaveTex, half4((v.normal.xz - _WaveCenter.xy + _WaveCenter.zw * 0.5f)/_WaveCenter.zw,0, 0)).xzy - float3(0.5, 1, 0.5));
 			
			float3 vert = float3(v.vertex.x*abs_x+bias_x*side_factor, v.vertex.y*finalsize.y, v.vertex.x*abs_z+bias_z*side_factor);
				
			float4 fix_vert = float4(v.normal, 0) + float4(vert,1) + float4(0,0.6*finalsize.y,0,0);
			float3 dir = wind_dir + float4(wave_ofs_dir.x,0,  wave_ofs_dir.z, 0);
			float4 wave = float4(( dir+normalize(float3(bias_x,0,bias_z))*0.4*length( dir))*bendfactor*finalsize.y*(v.vertex.y+0.7),0); 
			float f = sin( ( length(norm.xz) + 1) *  ((_Time.x + 100)) * 180);
			float4 vofs = wave + sign(wave) * max ( abs(wave) * 0.07 - 0.015, 0) * f * 0.7;
//				v.vertex = float4(v.normal, 0) + float4(vert,1) + float4(0,0.6*finalsize.y,0,0) + float4((wind_dir+normalize(float3(bias_x,0,bias_z))*0.4*length(wind_dir))*bendfactor*finalsize.y*(v.vertex.y+0.7),0) ;//+ half4(offsets, 0);
				//v.vertex = float4(v.normal, 0) + float4(vert,1) + float4(0,0.6*finalsize.y,0,0);// + float4((wind_dir+normalize(float3(bias_x,0,bias_z))*0.4*length(wind_dir))*bendfactor*bendbias*finalsize.y*(v.vertex.y+0.7),0);
			v.vertex = fix_vert + vofs;
				
			v.vertex = mul(_World2Object, v.vertex); // disable obj2world matrix (used for disable camera clipping) 

			
			// fetch vertex normal
			norm.xz *= 2;
			v.normal = norm;
			// normal bend by wind
			v.normal.xz += wind_dir.xz * bendfactor * 0.2f;
			v.normal = normalize(v.normal);
			
		
			
			// calculate tangents
			float3 T1 = float3(0.0, 0.1, 0.0);
			float3 Bi = cross(T1, v.normal);
			float3 newTangent = cross(v.normal, Bi);
			normalize(newTangent);
			v.tangent.xyz = newTangent.xyz; 
			v.tangent.w = sign(dot(cross(v.normal,newTangent),Bi));
			
			// other output
			o.tintColor = v.color;
			o.lodBias = prop_1.y * 8 - 4;
			o.worldNormal = v.normal;
        } 
        
        // Surface
        inline void surf (Input i, inout GrassSurfaceOutput o)
        {
        	float3 base_uv = CalcBaseUV(i.texCoords.z);
        	float dcoordx = length(float2( ddx(i.texCoords.x), ddx(i.texCoords.y) ));
        	float dcoordy = length(float2( ddy(i.texCoords.x), ddy(i.texCoords.y) ));
        	float dcoord = length(float2(dcoordx, dcoordy));
        	float lod = clamp(log2( dcoord * _Settings1.w ) - 1 + i.lodBias,0,4);
        	half4 diff = GetDiffuseColor(base_uv, i.texCoords.xy, lod);
        	
            o.Albedo = diff.rgb * i.tintColor;
            o.Alpha = diff.a;
			o.Normal_ = i.worldNormal;
            o.Specular = 0;
            o.Emission = 0;
        }
        
        // Lighting
		inline fixed4 LightingGrassLambert_PrePass (GrassSurfaceOutput s, half4 light)
		{
			fixed4 c = fixed4(0,0,0,0);
			c.rgb = s.Albedo * (pow(light.rgb*.88, 1.3) * _Settings2.y + _Settings2.x);
			c.a = s.Alpha;
			return c;
		}
		
		inline fixed4 LightingGrassLambert (GrassSurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			fixed diff = max(0, dot(s.Normal_, lightDir));
			
			fixed4 c = fixed4(0,0,0,0);
			c.rgb = s.Albedo * _LightColor0.rgb * (diff * atten * 0.5);
			c.a = s.Alpha;
			return c;
		}
        ENDCG
    }
}
