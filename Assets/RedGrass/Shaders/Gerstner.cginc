#ifndef WATER_CG_INCLUDED
#define WATER_CG_INCLUDED

half3 GerstnerOffset4 (half2 xzVtx, half4 steepness, half4 amp, half4 freq, half4 speed, half4 dirAB, half4 dirCD) 
{
	half3 offsets;
	
	half4 AB = steepness.xxyy * amp.xxyy * dirAB.xyzw;
	half4 CD = steepness.zzww * amp.zzww * dirCD.xyzw;
	
	half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
	half4 TIME = _Time.yyyy * speed;
	
	half4 COS = cos (dotABCD + TIME);
	half4 SIN = sin (dotABCD + TIME);
	
	offsets.x = dot(COS, half4(AB.xz, CD.xz));
	offsets.z = dot(COS, half4(AB.yw, CD.yw));
	offsets.y = dot(SIN, amp);

	return offsets;			
}	

half3 GerstnerNormal4 (half2 xzVtx, half4 amp, half4 freq, half4 speed, half4 dirAB, half4 dirCD) 
{
	half3 nrml = half3(0,2.0,0);
	
	half4 AB = freq.xxyy * amp.xxyy * dirAB.xyzw;
	half4 CD = freq.zzww * amp.zzww * dirCD.xyzw;
	
	half4 dotABCD = freq.xyzw * half4(dot(dirAB.xy, xzVtx), dot(dirAB.zw, xzVtx), dot(dirCD.xy, xzVtx), dot(dirCD.zw, xzVtx));
	half4 TIME = _Time.yyyy * speed;
	
	half4 COS = cos (dotABCD + TIME);
	
	nrml.x -= dot(COS, half4(AB.xz, CD.xz));
	nrml.z -= dot(COS, half4(AB.yw, CD.yw));
	
	nrml = normalize (nrml);

	return nrml;			
}

void Gerstner (	out half3 offs, out half3 nrml,
				half3 vtx, half3 tileableVtx, 
				half4 amplitude, half4 frequency, half4 steepness, 
				half4 speed, half4 directionAB, half4 directionCD ) 
{
	offs = GerstnerOffset4(tileableVtx.xz, steepness, amplitude, frequency, speed, directionAB, directionCD);
	nrml = GerstnerNormal4(tileableVtx.xz + offs.xz, amplitude, frequency, speed, directionAB, directionCD);		
}

#endif