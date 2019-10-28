Shader "zhouxun/Grass/Particle Set (MultiMat)" 
{
	Properties 
	{
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_DiffuseTex ("Diffuse Texture", 2D) = "black" {}
		_PropertyTex ("Property Texture", 2D) = "black" {}
		_WindTex ("Wind Texture", 2D) = "black" {}
		_RandTex ("Random Texture", 2D) = "black" {}
		_Settings1 ("Tile width (%), Tile height (%), LOD indent base (px), Tile size (px)", Vector) = (.125,.125,1,256)
		_Settings2 ("Light base, Light amp, Normal bend strength, Reserved", Vector) = (.05,.45,.3,0)
		_Settings3 ("Property index count, Property track count, Reserved, Reserved", Vector) = (64,4,0,0)
    	_AlphaCutoff ("Alpha cutoff", Range(0,1)) = .5
    }

    SubShader 
    {
    	//Blend SrcAlpha OneMinusSrcAlpha
    	Cull Off
    	Blend One One
    	Zwrite Off
    	Fog { Mode Off }
    	
		Tags { "RenderType" = "Opaque" "Queue" = "Transparent" }
        
        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf GrassLambert vertex:vert 
        #pragma glsl
		
		float4 _TintColor;
        sampler2D _DiffuseTex;
        sampler2D _PropertyTex;
        sampler2D _WindTex;
        sampler2D _RandTex;
        
        float4 _Settings1;
        float4 _Settings2;
        float4 _Settings3;
        
        struct Input
        {
        	float strength;
            float3 texCoords;
        };
        struct GrassSurfaceOutput
        {
            float3 Albedo;
            float3 Normal;
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
        	float type = v.texcoord1.x * _Settings3.x;
        	
			// calculate texcoords
 			o.texCoords.x = (v.vertex.x + 1) * 0.5f;
 			o.texCoords.y = (v.vertex.y + 1) * 0.5f;
 			o.texCoords.z = type;
 			
         	// rand
        	float3 randval = GetRandom(v.normal.xz);
        	
			// life-time
 			float life = frac(_Time.x*5+randval.x);
 			o.strength = sin(life*3.14159);
        	
			// calculate billboard vertices
			float3 norm = float3(v.texcoord.x, 1-sqrt(v.texcoord.x*v.texcoord.x+v.texcoord.y*v.texcoord.y), v.texcoord.y);
        	float3 forward = -float3(UNITY_MATRIX_V[2][0], UNITY_MATRIX_V[2][1], UNITY_MATRIX_V[2][2]); // camera forward direction
            float3 y_up = float3(0,1,0); // world y-up
			float3 right = normalize(cross(y_up, forward)); // horizontal right direction
			float3 up = normalize(cross(forward, right)); // camera up direction
			float c = cos(life);
			float s = sin(life);
			float3 axis_a = c * right + s * up;
			float3 axis_b = -s * right + c * up;
			
			float3 billboard_vert = (v.vertex.x*2*axis_a + v.vertex.y*2*axis_b) * o.strength; // vertex local pos in each billboard
			
			// grass bend (wind effect)
			float bendbias = sin(_Time.y*2 + v.normal.z*0.3);
			float bendfactor = 0.2;
 			
 			// calculate final vertices
			v.vertex = float4(v.normal, 0) + float4(billboard_vert, 1) + float4(life*life*(randval.x*2-1),life*life*(2.5*randval.y+1.5),life*life*(randval.z*2-1),0); 
			v.vertex = mul(_World2Object, v.vertex); // disable obj2world matrix (used for disable camera clipping)
			
			// fetch vertex normal
			v.normal = norm;
			// normal bend by wind
			v.normal.z += bendbias*bendfactor*0.3;
			v.normal = normalize(v.normal);
			
			// calculate tangents
			float3 T1 = float3(0.0, 0.1, 0.0);
			float3 Bi = cross(T1, v.normal);
			float3 newTangent = cross(v.normal, Bi);
			normalize(newTangent);
			v.tangent.xyz = newTangent.xyz; 
			v.tangent.w = sign(dot(cross(v.normal,newTangent),Bi));		
        } 
        
        // Surface
        inline void surf (Input i, inout GrassSurfaceOutput o)
        {
        	float3 base_uv = CalcBaseUV(i.texCoords.z);
        	float dcoordx = length(float2( ddx(i.texCoords.x), ddx(i.texCoords.y) ));
        	float dcoordy = length(float2( ddy(i.texCoords.x), ddy(i.texCoords.y) ));
        	float dpos = length(float2(dcoordx, dcoordy));
        	float lod = clamp(log2( dpos * _Settings1.w ) - 1,0,7);
        	half4 diff = GetDiffuseColor(base_uv, i.texCoords.xy, lod);
        	if ( diff.r + diff.g + diff.b < 0.1 ) discard;
            o.Albedo = diff.rgb * i.strength * _TintColor.rgb;
            o.Alpha = diff.a;
			o.Normal = float3(0,0,1);
            o.Specular = 0;
            o.Emission = 0;
        }
        
        // Lighting
		inline fixed4 LightingGrassLambert_PrePass (GrassSurfaceOutput s, half4 light)
		{
			return float4(s.Albedo, 1);
		}
		
		inline fixed4 LightingGrassLambert (GrassSurfaceOutput s, fixed3 lightDir, fixed atten)
		{
			return float4(s.Albedo, 1);
		}
        ENDCG
    }
}
