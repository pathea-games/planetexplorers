using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public partial class VFDataRTGen 
{
	
	/// <summary>
	/// gen voxel volume directly
	/// </summary>
	private void GenTileVoxelWithHeightMap(float voxelX, float voxelY, float voxelZ, float fTerNoise, ref float fNoiseXZ, ref byte volume, ref byte type, int topCorrection,int vy)
	{
		//float fDensity = DensityThresholdMinusDelta + voxelY / 10;
		//volume = fDensity > (DensityThresholdPlusDelta) ? (byte)255 : (byte)((fDensity - DensityThresholdMinusDelta) * DensityDeltaHalf255Reci * fTerNoise);
		//volume = 170;
		//volume = (byte)(Mathf.RoundToInt(FILL_VOLUME + Mathf.Abs(255 * voxelY)));
		//volume = (byte)Mathf.Clamp(volume, FILL_VOLUME, 255);
		volume = 255;
		type = BLOCK_STONE;
		
		//top correction
		if (volume > 127.5 && voxelY < fTerNoiseHeight[topCorrection-1])
		{
			volume = 0;
		}
		else if (volume > 127.5 && voxelY < fTerNoiseHeight[topCorrection - 2])
		{
			float p = (float)myNoise[TopCorrectionIndex].Noise2DFBM(voxelX * disturbFrequency, voxelZ * disturbFrequency, 2);
			volume = (Byte)(volume * (p * 0.5 + 0.5));
		}
	}
	
	
	/// <summary>
	/// compute voxel,return true,when volume>196
	/// </summary>
	private static bool GenTileVoxelOnly(float voxelX, float voxelY, float voxelZ, float flatParam, 
	                                     float fDensityClamp, float fTerType, int nTerType, float fTerNoise,
	                                     ref byte volume, ref byte type,float PlainThickness)
	{
		int nOctaves = GetnOcvtaves(nTerType);
		
		float HASChangeFactor = GetHASChangeFactor(fTerType);
		
		float testParam = flatParam;
		
		
		float fNoise83DFBM1 = (float)myNoise[Volume3DNoiseIndex].Noise3DFBM(voxelX * testParam, voxelY * testParam, voxelZ * testParam, 1);
		float fDensity =
			-fDensityClamp
				+ Mathf.Abs(fNoise83DFBM1 * TerBaseNoisePara) // make a base terrain and hole in terrain
				+ (_terTypeCoef + _terDecreaseLarge*fNoise83DFBM1 + _terDecreaseSmall*(float)myNoise[Volume3DNoiseIndex].Noise3DFBM(voxelX * 2f * testParam, voxelY * 2f * testParam, voxelZ * 2f * testParam, nOctaves) * 0.5f) * fTerType//+( 1.0f + fNoise83DFBM1 + (float)myNoise[5].Noise3DFBM( voxelX*2f, voxelY*2f, voxelZ*2f, 3)*0.5f )*fTerType
				+ GetHASValue(voxelX,voxelZ,HASChangeFactor)
				+ Mathf.Pow(GetFinalSierraNoise(voxelX,voxelZ),2)*HASChangeValueSierra*HASChangeFactor
				;
		
		
		if (fDensity <= DensityThresholdMinusDelta)
		{
			volume = 0;
			type = BLOCK_AIR;
			
		}
		else
		{
			//volume = 255;
			volume = fDensity > (DensityThresholdPlusDelta) ? (byte)255 : (byte)((fDensity - DensityThresholdMinusDelta) * DensityDeltaHalf255Reci * fTerNoise);
			type = BLOCK_STONE;//top correction
			
			if (volume > 127.5 && voxelY < fTerNoiseHeight[MountainTopCorrection-1])
			{
				volume = 0;
			}
			else if (volume > 127.5 && voxelY < fTerNoiseHeight[MountainTopCorrection - 2])
			{
				float p = (float)myNoise[TopCorrectionIndex].Noise2DFBM(voxelX * disturbFrequency, voxelZ * disturbFrequency, 2);
				volume = (Byte)(volume * (p * 0.5 + 0.5));
			}
			if (volume > (byte)GenVolumeThreshold)//196
			{
				return true;
			}
		}
		return false;
	}
	
	
	/// <summary>
	/// compute voxel
	/// </summary>
	private void GenTileVoxel(float voxelX, float voxelY, float voxelZ, float flatParam, 
	                          float fDensityClamp, float fTerType, int nTerType, float fTerNoise,
	                          ref float fNoiseXZ, ref byte volume, ref byte type, bool hillNotTop, int topCorrection,float PlainThickness)
	{
		int nOctaves = GetnOcvtaves(nTerType);
		
		float HASChangeFactor = GetHASChangeFactor(fTerType);
		
		float testParam = flatParam;
		
		float fNoise83DFBM1 = (float)myNoise[Volume3DNoiseIndex].Noise3DFBM(voxelX * testParam, voxelY * testParam*0.1f, voxelZ * testParam, 1);
		float fDensity =
			-fDensityClamp
				+ Mathf.Abs(fNoise83DFBM1 * TerBaseNoisePara) // make a base terrain and hole in terrain
				+ (_terTypeCoef +_terDecreaseLarge* fNoise83DFBM1 + _terDecreaseSmall*(float)myNoise[Volume3DNoiseIndex].Noise3DFBM(voxelX * 2f * testParam, voxelY * 2f * testParam, voxelZ * 2f * testParam, nOctaves) * 0.5f) * fTerType//+( 1.0f + fNoise83DFBM1 + (float)myNoise[5].Noise3DFBM( voxelX*2f, voxelY*2f, voxelZ*2f, 3)*0.5f )*fTerType
				+GetHASValue(voxelX,voxelZ,HASChangeFactor)
				+ Mathf.Pow(GetFinalSierraNoise(voxelX,voxelZ),2)*HASChangeValueSierra*HASChangeFactor
				;
		
		if (fDensity <= DensityThresholdMinusDelta)
		{
			volume = 0;
			type = BLOCK_AIR;
		}
		else
		{
			//volume = 255;
			volume = fDensity > (DensityThresholdPlusDelta) ? (byte)255 : (byte)((fDensity - DensityThresholdMinusDelta) * DensityDeltaHalf255Reci * fTerNoise);
			type = BLOCK_STONE;
			#if CAVE_DEBUG		
			volume = 0;	
			type = BLOCK_AIR;
			//VFVoxelTerrain.self.InfiniteTerrainCaveCellY = 0.4f;
			//VFVoxelTerrain.self.InfiniteTerrainCaveFadeCellY = 0.44f;
			#endif
			//top correction
			if (volume > 127.5 && voxelY < fTerNoiseHeight[topCorrection-1])
			{
				volume = 0;
			}
			else if (volume > 127.5 && voxelY < fTerNoiseHeight[topCorrection - 2])
			{
				float p = (float)myNoise[TopCorrectionIndex].Noise2DFBM(voxelX * disturbFrequency, voxelZ * disturbFrequency, 2);
				volume = (Byte)(volume * (p * 0.5 + 0.5));
			}
		}
	}
	
	
	private static bool GenTileVoxelVolume(float voxelX, float voxelY, float voxelZ, float flatParam, 
	                                       float fDensityClamp, float fTerType, int nTerType, float fTerNoise,float PlainThickness)
	{
		int nOctaves = GetnOcvtaves(nTerType);
		float HASChangeFactor = GetHASChangeFactor(fTerType);
		byte volume = 0;
		float testflat = flatParam;
		
		float fNoise83DFBM1 = (float)myNoise[Volume3DNoiseIndex].Noise3DFBM(voxelX * testflat, voxelY * testflat, voxelZ * testflat, 1);
		float fDensity =
			-fDensityClamp
				+ Mathf.Abs(fNoise83DFBM1 * TerBaseNoisePara) // make a base terrain and hole in terrain
				+ (_terTypeCoef + _terDecreaseLarge* fNoise83DFBM1 +_terDecreaseSmall* (float)myNoise[Volume3DNoiseIndex].Noise3DFBM(voxelX * 2f * testflat, voxelY * 2f * testflat, voxelZ * 2f * testflat, nOctaves) * 0.5f) * fTerType
				+ GetHASValue(voxelX,voxelZ,HASChangeFactor)
				+ Mathf.Pow(GetFinalSierraNoise(voxelX,voxelZ),2)*HASChangeValueSierra*HASChangeFactor
				;
		if (fDensity <= DensityThresholdMinusDelta)
		{
			volume = 0;
		}
		else
		{
			volume = fDensity > (DensityThresholdPlusDelta) ? (byte)255 : (byte)((fDensity - DensityThresholdMinusDelta) * DensityDeltaHalf255Reci * fTerNoise);
			if (volume > 127.5)
				return true;
		}
		return false;
	}
	
	
	private static int GetnOcvtaves(int nTerType){
		if(IsHillTerrain(nTerType))
			return 7;
		if(IsPlainTerrain(nTerType))
			return 3;
		if(IsSeaSideTerrain(nTerType))
			return 2;
		if(IsWaterTerrain(nTerType))
			return 1;
		return 3;
	}
	private static float GetHASChangeFactor(float fTerType){
		if(fTerType<c_seasideStart)
		{
			return 0;
		}else if(fTerType>c_plainStart){
			return 1;
		}else{
			return Mathf.Pow((fTerType-c_seasideStart)/(c_plainStart-c_seasideStart),2);
		}
	}

	private static float GetHASValue(float scaledX,float scaledZ,float HASChangeFactor){
		return (float)myNoise[HASFilterIndex].Noise(scaledX*HASChangeFrequency,scaledZ*HASChangeFrequency)*HASChangeValue*HASChangeFactor;
	}

	private static float GetMountain(float scaledX,float scaledZ,int startNoiseIndex,float topValue,float frequencyF){
		float factorMountain = topValue;//20;
		float mountain01 = (float)myNoise[startNoiseIndex+1].Noise(scaledX * terrainFrequencyX*MountainFrequencyFactor*frequencyF, scaledZ * terrainFrequencyZ*MountainFrequencyFactor*frequencyF);
		float mountain02 = (float)myNoise[startNoiseIndex+2].Noise(scaledX * terrainFrequencyX*MountainFrequencyFactor*0.375f*frequencyF, scaledZ * terrainFrequencyZ*MountainFrequencyFactor*0.375f*frequencyF);
		float m1 = 1-Mathf.Abs(mountain01);
		float m2 = Mathf.Sqrt((mountain02+1)*0.5f);
		float finalMountain = m1*m2*factorMountain;
		if(finalMountain<factorMountain/10)
			finalMountain = 0;
		else
			finalMountain = (finalMountain-factorMountain/10)*10/9;
		return finalMountain;
	}
	
	
	private static float GetFinalSierra(float scaledX,float scaledZ){
		float factorSierra = 30;
		float s1 = GetFinalSierraNoise(scaledX,scaledZ);
		//float s2 = (sierra02+1)*0.5f;
		float finalSierra = Mathf.Sqrt(s1)*factorSierra;
		if(finalSierra<factorSierra/16*15)
			finalSierra = 0;
		else{
			finalSierra = (finalSierra-factorSierra/16*15)*16;
		}
		return finalSierra;
	}
	
	private static float GetFinalSierraNoise(float scaledX,float scaledZ){
		float sierra01 = (float)myNoise[SierraIndex].Noise(scaledX * terrainFrequencyX*SierraFrequencyFactor, scaledZ * terrainFrequencyZ*SierraFrequencyFactor);
		//float sierra02 = (float)myNoise[FTerTypeIndex+2].Noise(scaledX * terrainFrequencyX*0.75f/4, scaledZ * terrainFrequencyZ*0.75f/4);
		return (sierra01+1)*0.5f;
	}
	
}
