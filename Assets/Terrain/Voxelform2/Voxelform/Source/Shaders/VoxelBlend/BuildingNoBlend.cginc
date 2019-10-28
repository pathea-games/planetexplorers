/*
BlendEdgeModifier is commented because no building type(which would no blend with each other) exists now
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
*/
