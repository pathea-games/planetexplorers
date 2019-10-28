using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

public partial class VFDataRTGen
{//town
	public static int MaxTownHeight { get { return s_seaDepth + 256 + 40; } }
	//	const float TownHillRadiusMax = 128;
	//	const float TownHillRadiusMin = 32;
	const float TownHillDistance = 64;//96;
	const float TownHillChangeFactor = 0;
	//	const float TownWaterRadiusMax = 128;
	//	const float TownWaterRadiusMin = 64;
	const float TownWaterDistance = 48;//64;
	const float TownConnectionWidth = 4;
	const float TownConnectionHillDistance=64;
	const float TownConnectionWaterDistance=48;
	static float TownConnectionFlatMin{
		get{return 1f;}
	}
	const float TownConnectionFlatDistance = 4;
	static float TownFTerTypeTop(float[] terTypeInc){
		return terTypeInc[HillStartIndex];
	}
	static float TownFTerTypeBottom(float[] terTypeInc){
		return terTypeInc[PlainStartIndex]*3/4+terTypeInc[SeasideStartIndex]*1/4;
	}
	
	const float NoTownStartDistance = 96;


	public static float GetOriginalFterType(IntVector2 worldXZ,out float fTerTypeBridge,out bool caveEnable,out bool riverArea ,out bool lakeArea,out float riverValue, out float bridgeValue,out float bridge2dFactor,out RandomMapType mapType,out float[] terTypeInc){
		float scaledX = worldXZ.x * s_detailScale;
		float scaledZ = worldXZ.y * s_detailScale;

		mapType = GetMapTypeAndTerInc(scaledX,scaledZ,out terTypeInc);
		float fNoise12D1ten = GetfNoise12D1ten(scaledX,scaledZ);
		
		float continentBound = GetContinentValue(worldXZ.x, worldXZ.y);
		
		//int nTerType = 0;
		float fTerType = fNoise12D1ten / 100;//[0,1]

		fTerType = BlendContinentBound(fTerType,continentBound,terTypeInc);

		if (fTerType < 0){
			fTerType = 0;
		}

		caveEnable = true;
		riverArea = false;
		lakeArea = false;
		riverValue = GetRiverValue(worldXZ.x, worldXZ.y, ref fTerType,ref caveEnable,ref lakeArea, out bridgeValue,out bridge2dFactor,terTypeInc);
		if(riverValue<0.85f)
			riverArea = true;
		float fTerTypeRiver = fTerType * riverValue;
		fTerTypeBridge=-1;
		if(bridgeValue!=-1){
			fTerTypeBridge = fTerType * bridgeValue;
		}

		return fTerTypeRiver;
	}
	
	public static float GetOriginalFterType(IntVector2 worldXZ,out float fTerTypeBridge,out float[] terTypeInc){
		float scaledX = worldXZ.x * s_detailScale;
		float scaledZ = worldXZ.y * s_detailScale;
		
		GetMapTypeAndTerInc(scaledX,scaledZ,out terTypeInc);
		float fNoise12D1ten = GetfNoise12D1ten(scaledX,scaledZ);
		
		float continentBound = GetContinentValue(worldXZ.x, worldXZ.y);
		
		//int nTerType = 0;
		float fTerType = fNoise12D1ten / 100;//[0,1]

		fTerType = BlendContinentBound(fTerType,continentBound,terTypeInc);
		
		if (fTerType < 0){
			fTerType = 0;
		}
		
		float bridgeValue;
		float bridge2dFactor;
		bool caveEnable = true;
		bool lakeArea = false;
		//bool riverArea = false;
		float riverValue = GetRiverValue(worldXZ.x, worldXZ.y,ref fTerType,ref caveEnable,ref lakeArea, out bridgeValue,out bridge2dFactor,terTypeInc);
		//if(riverValue<0.85f)
		//	riverArea = true;
		float fTerTypeRiver = fTerType * riverValue;
		fTerTypeBridge=-1;
		if(bridgeValue!=-1){
			fTerTypeBridge = fTerType * bridgeValue;
		}

		return fTerTypeRiver;
	}
	
	public static float GetFinalFterType(IntVector2 worldXZ,out float fTerTypeBridge,out bool caveEnable,out bool riverArea ,out bool lakeArea,out float riverValue, out float bridgeValue,out float bridge2dFactor,out RandomMapType mapType,out float[] terTypeInc){
		
		float finalFTerType =GetOriginalFterType(worldXZ,out fTerTypeBridge,out caveEnable,out riverArea,out lakeArea,out riverValue,out bridgeValue,out bridge2dFactor,out mapType,out terTypeInc);
		//		finalFTerType= GetTownAreaValue(finalFTerType,worldXZ);
		return finalFTerType;
	}
	
	
	public static float GetFinalFterType(IntVector2 worldXZ,out float fTerTypeBridge,out float[] terTypeInc){
		
		float finalFTerType =GetOriginalFterType(worldXZ,out fTerTypeBridge,out terTypeInc);
		//		finalFTerType= GetTownAreaValue(finalFTerType,worldXZ);
		return finalFTerType;
	}
	
	static float GetBiomaChangeValue(float finalFTerType, IntVector2 worldXZ){
		return finalFTerType;
	}
	
	static float GetTownChangeValue(float finalFTerType,IntVector2 worldXZ,float[] terTypeInc){
		if(VArtifactUtil.HasTown()){
			if(finalFTerType>TownFTerTypeTop(terTypeInc))
			{
				VArtifactTown  vat;
				float factorValue =1;
				float nearTownDistance =VATownGenerator.Instance.GetAreaTownDistance(worldXZ.x,worldXZ.y,out vat);
				//float terTypeDistanceFactor = 1;//+(finalFTerType-TownFTerTypeTop)/(1-TownFTerTypeTop)*TownHillChangeFactor;
				
				float scaledX = worldXZ.x * s_detailScale;
				float scaledZ = worldXZ.y * s_detailScale;
				float changeNoise = Mathf.Abs((float)myNoise[TownChangeIndex].Noise(scaledX * TownChangeFrequency, scaledZ * TownChangeFrequency));//1~0~1
				//				changeNoise = 1-changeNoise; //0~1~0;
				float changeFactor = 1+changeNoise*TownChangeFactor;
				if(vat!=null){
					if(nearTownDistance <vat.SmallRadius*changeFactor)
						factorValue=0;
					else if(nearTownDistance<(vat.SmallRadius+TownHillDistance)*changeFactor){
						factorValue = (nearTownDistance-vat.SmallRadius*changeFactor)/(TownHillDistance*changeFactor);
					}
				}
				if(factorValue>0){
					//					float nearConnectionDistance = VATownGenerator.Instance.GetConnectionLineDistance(worldXZ);
					//					if(nearConnectionDistance <TownConnectionWidth*changeFactor){
					//						factorValue=0;
					//					}
					//					else if(nearConnectionDistance<(TownConnectionWidth+TownConnectionHillDistance)*changeFactor){
					//						float temp = (nearConnectionDistance-TownConnectionWidth*changeFactor)/(TownConnectionHillDistance*changeFactor);
					//						if(factorValue >temp)
					//							factorValue = temp;
					//					}
					
					float temp = GetTownConnectionFactor(worldXZ.x,worldXZ.y,TownConnectionHillDistance,changeFactor);
					if(factorValue >temp)
						factorValue = temp;
				}
				finalFTerType = factorValue*(finalFTerType-TownFTerTypeTop(terTypeInc))+TownFTerTypeTop(terTypeInc);
			}
			else if(finalFTerType<TownFTerTypeBottom(terTypeInc))
			{
				VArtifactTown vat;
				float factorValue =1;
				float nearTownDistance = VATownGenerator.Instance.GetAreaTownDistance(worldXZ.x,worldXZ.y,out vat);
				float scaledX = worldXZ.x * s_detailScale;
				float scaledZ = worldXZ.y * s_detailScale;
				float changeNoise = ((float)myNoise[TownChangeIndex].Noise(scaledX * TownChangeFrequency, scaledZ * TownChangeFrequency)+1)/2;//0~1
				float changeFactor = 1+changeNoise*TownChangeFactor;
				if(vat!=null){
					if(nearTownDistance <vat.MiddleRadius*changeFactor)
						factorValue = 0;
					else if(nearTownDistance<(vat.MiddleRadius+TownWaterDistance)*changeFactor){
						factorValue = (nearTownDistance-vat.MiddleRadius*changeFactor)/(TownWaterDistance*changeFactor);
					}
				}
				if(factorValue>0){
					//					float nearConnectionDistance = VATownGenerator.Instance.GetConnectionLineDistance(worldXZ);
					//					if(nearConnectionDistance <TownConnectionWidth*changeFactor){
					//						factorValue=0;
					//					}
					//					else if(nearConnectionDistance<(TownConnectionWidth+TownConnectionWaterDistance)*changeFactor){
					//						float temp = (nearConnectionDistance-TownConnectionWidth*changeFactor)/(TownConnectionWaterDistance*changeFactor);
					//						if(factorValue >temp)
					//							factorValue = temp;
					//					}
					float temp = GetTownConnectionFactor(worldXZ.x,worldXZ.y,TownConnectionWaterDistance,changeFactor);
					if(factorValue >temp)
						factorValue = temp;
				}
				finalFTerType = factorValue*(finalFTerType-TownFTerTypeBottom(terTypeInc))+TownFTerTypeBottom(terTypeInc);
			}
		}
		else{
			int nearStartDistanceSqr = IntVector2.SqrMagnitude(worldXZ- noTownStartPos);
			if(nearStartDistanceSqr<(NoTownStartDistance*(TownChangeFactor+1))*(NoTownStartDistance*(TownChangeFactor+1))){
				float noTownStartFTerType;
				if(sceneClimate==ClimateType.CT_Wet)
					noTownStartFTerType = terTypeInc[MountainStartIndex];
				else
					noTownStartFTerType = terTypeInc[HillStartIndex];
				if(finalFTerType<noTownStartFTerType){
					float nearStartDistance = Mathf.Sqrt(nearStartDistanceSqr);
					float scaledX = worldXZ.x * s_detailScale;
					float scaledZ = worldXZ.y * s_detailScale;
					float changeNoise = ((float)myNoise[TownChangeIndex].Noise(scaledX * TownChangeFrequency, scaledZ * TownChangeFrequency)+1)/2;//0~1
					float changeFactor = 1+changeNoise*TownChangeFactor;
					float factorValue =1;
					if(nearStartDistance<NoTownStartDistance*changeFactor){
						factorValue = nearStartDistance/(NoTownStartDistance*changeFactor);
					}
					finalFTerType = factorValue*(finalFTerType-noTownStartFTerType)+noTownStartFTerType;
				}
			}
		}
		return finalFTerType;
	}
	
	public static bool IsTownConnectionType(int x,int z){
		float scaledX = x * s_detailScale;
		float scaledZ = z * s_detailScale;
		float changeNoise = ((float)myNoise[TownChangeIndex].Noise(scaledX * TownChangeFrequency, scaledZ * TownChangeFrequency)+1)/2;//0~1
		float changeFactor = 1+changeNoise*TownChangeFactor;
		float nearConnectionDistance = VATownGenerator.Instance.GetConnectionLineDistance(new IntVector2(x,z),true);
		if(nearConnectionDistance <TownConnectionWidth*changeFactor){
			return true;
		}
		return false;
	}
	
	public static float GetTownConnectionFactor(int x,int z,float distance,float changeFactor =-1){
		if(changeFactor==-1){
			float scaledX = x * s_detailScale;
			float scaledZ = z * s_detailScale;
			float changeNoise = Mathf.Abs((float)myNoise[TownChangeIndex].Noise(scaledX * TownChangeFrequency, scaledZ * TownChangeFrequency));//1~0~1
			changeFactor = 1+changeNoise*TownChangeFactor;			
		}
		float factorValue =1;
		float nearConnectionDistance = VATownGenerator.Instance.GetConnectionLineDistance(new IntVector2(x,z));
		if(nearConnectionDistance <TownConnectionWidth*changeFactor){
			factorValue=0;
		}
		else if(nearConnectionDistance<(TownConnectionWidth+distance)*changeFactor){
			factorValue = (nearConnectionDistance-TownConnectionWidth*changeFactor)/(distance*changeFactor);
		}
		return factorValue;
	}
	
	public static int GetPosHeight(float x, float z, bool inWater = false)
	{
		IntVector2 worldPosXZ = new IntVector2(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
		return GetPosHeight(worldPosXZ, inWater);
	}
	
	/// <summary>
	/// 2013-9-28 by PuJi
	/// </summary>
	/// <param name="worldXZ">the world position of x and z</param>
	/// <returns></returns>
	public static int GetPosHeight(IntVector2 worldXZ, bool inWater = false)
	{
		float[] fTerDensityClamp;
		ReGenDensityClampStatic(worldXZ, out fTerDensityClamp);
		
		float scaledX = worldXZ.x * s_detailScale;
		float scaledZ = worldXZ.y * s_detailScale;
		
		float fTerTypeBridge;
		float[] terTypeInc;
		float fTerTypeRiver = GetFinalFterType(worldXZ,out fTerTypeBridge,out terTypeInc);
		float valueTerType = Mathf.Max(fTerTypeRiver,fTerTypeBridge);//[0,1]
		float fTerType = valueTerType;//for compute 
		float fTerNoise = (fTerType*2-1)*0.05f;
		int nTerType;
		GetFTerTypeAndZnTerType(ref fTerType,out nTerType,terTypeInc);
		
		// Compute ter Noise/Smooth level based on TerType
		ComputeTerNoise(ref fTerNoise, nTerType, fTerType);
		
		float flatFactor = (float)myNoise[FlatIndex].Noise(scaledX * flatFrequency, scaledZ * flatFrequency);
		float flatParam = GetFlatParamFromFactor(flatFactor,worldXZ.x,worldXZ.y);
		
		//get max type 
		int maxHeight = s_noiseHeight - 1;
		float maxHeightValue; 
		float bottomVy=s_seaDepth + 1;
		int vy = maxHeight;
//		if(worldXZ.x>-2650 && worldXZ.x<-2640 && worldXZ.y>=3800&&worldXZ.y<3810)
//			Debug.Log("test point");
		if(!IsWaterTerrain(nTerType))
		{
			if(IsHillTerrain(nTerType)){
				maxHeightValue = s_noiseHeight;
				maxHeight = s_noiseHeight;
			}
			else if (!IsSeaSideTerrain(nTerType))
			{
				maxHeightValue = PlainTopHeight(HASMid) + (valueTerType - terTypeInc[PlainStartIndex]) / (terTypeInc[HillStartIndex] - terTypeInc[PlainStartIndex]) * (HillBottomHeight(HASMid) - PlainTopHeight(HASMid));
				maxHeight = Mathf.FloorToInt(maxHeightValue);
			}
			else
			{
				//seaside
				float zeroPoint = terTypeInc[SeasideStartIndex] / 16;
				float onePoint =  terTypeInc[SeasideStartIndex]*17/16;//(terTypeInc[PlainStartIndex] - terTypeInc[SeasideStartIndex]) / 3 + terTypeInc[SeasideStartIndex];
				float sinFactor=1;
				if(fTerTypeRiver<onePoint)
					sinFactor = (Mathf.Sin(((fTerTypeRiver - zeroPoint) / (onePoint - zeroPoint) - 0.5f) * Mathf.PI) + 1) / 2;

				bottomVy = (s_seaDepth + 1) * sinFactor;
				maxHeightValue = PlainTopHeight(HASMid);
				maxHeight = PlainTopHeight(HASMid);
			}
			
			//step2.optimise maxheight
			OptimiseMaxHeight(ref maxHeight, scaledX, scaledZ, flatParam, fTerType, nTerType, fTerNoise, bottomVy, fTerNoiseHeight, fTerDensityClamp, HASMid,1);

			vy = Mathf.Clamp(maxHeight, 0, s_noiseHeight - 1);
			for (; vy > bottomVy; vy--)
			{
				if (
					GenTileVoxelVolume(scaledX, fTerNoiseHeight[vy], scaledZ, flatParam,
				                   fTerDensityClamp[vy], fTerType, nTerType, fTerNoise, HASMid)
					)
				{
					break;
				}
			}
		}
		else
		{
			//water
			float zeroPoint = terTypeInc[SeasideStartIndex] / 16;
			float onePoint =  terTypeInc[SeasideStartIndex]*17/16;//(terTypeInc[PlainStartIndex] - terTypeInc[SeasideStartIndex]) / 3 + terTypeInc[SeasideStartIndex];

			float floatHeight;
			if (valueTerType < zeroPoint)
			{
				floatHeight = 0;
			}
			else
			{
				//float sinFactor = (Mathf.Sin(((baseFTerType - seasideStart / 2) / plainStart - 0.5f) * Mathf.PI) + 1) / 2;
				float sinFactor = (Mathf.Sin(((valueTerType - zeroPoint) / (onePoint - zeroPoint) - 0.5f) * Mathf.PI) + 1) / 2;
				floatHeight = sinFactor * (s_seaDepth + 1);
			}
			maxHeight = Mathf.CeilToInt(floatHeight);

			vy = Mathf.FloorToInt(floatHeight);
		}
		
		//if under water
		if (vy < waterHeight+1)
		{
			if (!inWater)//height must on water
			{
				return Mathf.CeilToInt(waterHeight+1);
			}
		}

//		if(inWater)
//			Debug.Log("getPosHeight ture:"+worldXZ.ToString()+vy);
		return vy;
	}
	
	/// <summary>
	/// get the top position y of the scene (include the building) 
	/// </summary>
	/// <returns></returns>
	public static float GetPosTop(IntVector2 worldPosXZ, out bool canGenNpc)
	{
		//int x = worldPosXZ.x >> VoxelTerrainConstants._shift;
		//int z = worldPosXZ.y >> VoxelTerrainConstants._shift;
		float top = GetPosHeight(worldPosXZ,true);
		
		float townHeight = VArtifactUtil.IsInTown(worldPosXZ);
		if (townHeight != 0)
		{
			if(top<townHeight)
				top = townHeight;
			canGenNpc = false;
			return top+1.5f;
		}
		
		if(IsSea(Mathf.FloorToInt(top)))
		{
			canGenNpc = false;
			return top+1.5f;
		}
		
		canGenNpc = true;
		return top+1.5f;
	}
	
	public static float GetPosTop(IntVector2 worldPosXZ)
	{
		//int x = worldPosXZ.x >> VoxelTerrainConstants._shift;
		//int z = worldPosXZ.y >> VoxelTerrainConstants._shift;
		float top = GetPosHeight(worldPosXZ) + 1.5f;
		
		float townHeight = VArtifactUtil.IsInTown(worldPosXZ);
		if (townHeight != 0)
		{
			if (top < townHeight)
				top = townHeight+1.5f;
			return top;
		}
		return top;
	}
	public static float GetPosHeightWithTown(IntVector2 worldPosXZ,bool InWater=false)
	{
		float top = GetPosHeight(worldPosXZ,InWater) + 1.5f;
		
		float townHeight = VArtifactUtil.IsInTown(worldPosXZ);
		if (townHeight != 0)
		{
			if (top < townHeight)
				top = townHeight;
		}
		return top;
	}
	
	public static bool IsSpawnAvailable(IntVector2 worldXZ)
	{
		int x = worldXZ.x;
		int z = worldXZ.y;
		
		int maxDistance = RandomMapConfig.Instance.mapRadius - RandomMapConfig.Instance.boundOffset; 
		
		if (x >= maxDistance || z >= maxDistance)
		{
			return true;
		}
		
		if (IsSea(x, z))
		{
			return false;
		}
		
		
		return true;
	}
	
	//is townAvailable place
	public static bool IsTownAvailable(int x, int z)
	{
		IntVector2 worldXZ = new IntVector2(x,z);
		
		float fTerTypeBridge;
		float[] terTypeInc;
		float fTerTypeRiver = GetOriginalFterType(worldXZ,out fTerTypeBridge,out terTypeInc);
		float fTerType = Mathf.Max(fTerTypeRiver,fTerTypeBridge);
		
		if (RandomMapConfig.ScenceClimate == ClimateType.CT_Wet)
		{
			if (fTerType < terTypeInc[PlainStartIndex] + (terTypeInc[HillStartIndex] - terTypeInc[PlainStartIndex]) * 0.85f&&PlainMax(HASMid)<s_seaDepth+WET_WATER_PLUS_MAX+10)
			{
				return false;
			}
		}
		else
		{
			if (fTerType > terTypeInc[PlainStartIndex] + (terTypeInc[HillStartIndex] - terTypeInc[PlainStartIndex]) * 0.85f)
			{
				return false;
			}
		}
		
		
		//float fTerNoise = (fTerType*2-1) * 0.05f;
		int nTerType;
		GetFTerTypeAndZnTerType(ref fTerType,out nTerType,terTypeInc);
		
		if (RandomMapConfig.ScenceClimate != ClimateType.CT_Wet&&IsHillTerrain(nTerType))
		{
			return false;
		}
		
		if (RandomMapConfig.ScenceClimate != ClimateType.CT_Wet)
		{
			if (IsSea(x, z))
			{
				return false;
			}
		}
		
		return true;
	}
	
	public static bool IsDungeonEntranceAvailable(IntVector2 genPos){
		float fTerTypeBridge;
		float[] terTypeInc;
		float fTerTypeRiver = GetFinalFterType(genPos,out fTerTypeBridge,out terTypeInc);
		float fTerType  = Mathf.Max(fTerTypeRiver,fTerTypeBridge);
		
		//float fTerNoise = (fTerType*2-1) * 0.05f;
		int nTerType;
		GetFTerTypeAndZnTerType(ref fTerType,out nTerType,terTypeInc);
		
		if(IsHillTerrain(nTerType))
			return false;
		return true;
	}

	private static void GetFTerTypeAndZnTerType(ref float fTerType,out int nTerType,float[] terTypeInc){
		if(fTerType>=terTypeInc[MountainEndIndex]){
			fTerType = TEST_REGION_THRESHOLD[TEST_REGION_CNT];
			nTerType = TEST_REGION_CNT-1;
		}else{
			for (nTerType=0; nTerType < TEST_REGION_CNT; nTerType++)
			{
				
				if(nTerType==0){
					if (fTerType < terTypeInc[nTerType])
					{
						fTerType = (TEST_REGION_THRESHOLD[nTerType + 1] - TEST_REGION_THRESHOLD[nTerType]) / terTypeInc[nTerType]
						* fTerType + TEST_REGION_THRESHOLD[nTerType];
						break;
					}
				}else {
					if (fTerType < terTypeInc[nTerType])
					{
						fTerType = (TEST_REGION_THRESHOLD[nTerType + 1] - TEST_REGION_THRESHOLD[nTerType]) / (terTypeInc[nTerType]-terTypeInc[nTerType-1])
						* (fTerType - terTypeInc[nTerType-1]) + TEST_REGION_THRESHOLD[nTerType];
						break;
					}
				}
				
			}
		}
	}
	
//	private static void GetNTerType(float fTerType,out int nTerType){
//		nTerType = 0;
//		if(fTerType>=terTypeChanceInc[TEST_REGION_CNT] && fTerType<=1){
//			nTerType = TEST_REGION_CNT-1;
//		}else{
//			for (; nTerType < TEST_REGION_CNT; nTerType++)
//			{
//				if (fTerType < terTypeChanceInc[nTerType + 1])
//				{
//					break;
//				}
//			}
//		}
//	}
	
	//is sea
	public static bool IsSea(int x, int z)
	{
		float posHeight  = GetPosHeightWithTown(new IntVector2(x,z),true);
		return posHeight < waterHeight;
	}
	public static bool IsSea(int posHeight)
	{
		return posHeight < waterHeight;
	}
	
//	public static bool IsCached(IntVector2 tileIndex)
//	{
//		bool flag=false;
//		int worldX = tileIndex.x << VoxelTerrainConstants._shift;
//		int worldZ = tileIndex.y << VoxelTerrainConstants._shift;
//		float scaledX = worldX * s_detailScale;
//		float scaledZ = worldZ * s_detailScale;
//		IntVector2 worldXZ = new IntVector2 (worldX,worldZ);
//		float[] fTerDensityClamp;
//		ReGenDensityClampStatic(worldXZ, out fTerDensityClamp);
//		
//		float fTerTypeBridge;
//		float fTerTypeRiver = GetFinalFterType(worldXZ,out fTerTypeBridge);
//		float fTerType = Mathf.Max(fTerTypeRiver,fTerTypeBridge);
//		
//		int nTerType;
//		GetNTerType(fTerType,out nTerType);
//		
//		flag = IsHillTerrain(nTerType);
//		return flag;
//	}
	
	public static float GetBaseTerHeight(IntVector2 worldXZ)
	{
		
		float fTerTypeBridge;
		float[] terTypeInc;
		float fTerTypeRiver = GetFinalFterType(worldXZ,out fTerTypeBridge,out terTypeInc);
		float fTerType = Mathf.Max(fTerTypeRiver,fTerTypeBridge);
		
		float terBaseHeight = fTerType * s_noiseHeight+128;
		
		return Mathf.Clamp(terBaseHeight,waterHeight, terBaseHeight);
	}
	#region river
	static float GetRiverValue(int worldX, int worldZ,ref float fTerType,ref bool caveEnable,ref bool lakeArea,out float bridgeValue,out float bridge2dFactor,float[] terTypeInc)
	{
		//river
		bridgeValue = -1;
		bridge2dFactor = -1;
		float value = 1;
		float scaledX = worldX * s_detailScale;
		float scaledZ = worldZ * s_detailScale;
		float riverScaledX = scaledX * riverFrequencyX;
		float riverScaledZ = scaledZ * riverFrequencyZ;
		float river2D = (float)myRiverNoise[RiverIndex].Noise(riverScaledX, riverScaledZ);
		float threshold = ResetThreshold(riverThreshold, fTerType, scaledX, scaledZ);
		float riverBottomThreshold=0.5f;
		river2D = RiverDisturb(river2D, scaledX, scaledZ);
		if(Mathf.Abs(river2D)< threshold+0.01f)
			caveEnable = false;
		
		if (Mathf.Abs(river2D) < threshold)
		{
			riverBottomThreshold = threshold * (riverBottomPercentNow - Mathf.Pow(fTerType, 2) * 0.1f);
			float riverBottomHeightFactor = (float)myRiverNoise[RiverBottomChangeIndex].Noise(scaledX * terrainFrequencyX*0.25f, scaledZ * terrainFrequencyZ*0.25f);
			riverBottomHeightFactor = (riverBottomHeightFactor + 1) * 0.5f;//0-1
			riverBottomHeightFactor = 1-riverBottomHeightFactor*riverBottomHeightFactor*riverBottomHeightFactor*riverBottomHeightFactor*riverBottomHeightFactor;
			riverBottomHeightFactor = terTypeInc[SeasideStartIndex]*riverBottomHeightFactor*1.5f/terTypeInc[MountainEndIndex];

			float p = riverBottomHeightFactor;//0.70f * (1 - fTerType * 0.80f);
			if (Mathf.Abs(river2D) < riverBottomThreshold)
			{
				value = p;
			}
			else
			{
				float per = (Mathf.Abs(river2D) - riverBottomThreshold) / (threshold - riverBottomThreshold);
				float paramPer = 1;
				if (fTerType > terTypeInc[HillStartIndex])
				{
					paramPer = 1 / (Mathf.Pow((fTerType - terTypeInc[HillStartIndex]) / (terTypeInc[MountainEndIndex] - terTypeInc[HillStartIndex]), 0.25f) * 8 + 1);
				}
				Mathf.Pow(per, paramPer);
				value = per * (1 - p) + p;
			}
		}
		//lake
		//int lakeHeight = 0;
		//float lakeBottom = 0;
		//float bottom = 0;
		float threshold0 = 0;
		float lake2D = 0;
		float value2 = 1;
		
		lake2D = (float)myRiverNoise[LakeIndex].Noise(scaledX * lakeFrequency, scaledZ * lakeFrequency);
		threshold0 = ResetLakeThreshold(lakeThreshold, fTerType, scaledX, scaledZ);
		if(Mathf.Abs(lake2D) > threshold0-0.01f)		caveEnable = false;
		if(Mathf.Abs(lake2D) > threshold0-0.008f)		lakeArea = true;
		if (Mathf.Abs(lake2D) > threshold0){
			float bottomThreshold = ResetLakeBottomWidth(threshold0, fTerType);;
			float lakeBottomHeightFactor = (float)myRiverNoise[LakeBottomHeightIndex].Noise(scaledX * lakeFrequency * 0.1f, scaledZ * lakeFrequency * 0.1f);
			lakeBottomHeightFactor = (lakeBottomHeightFactor + 1) * 0.5f;
			lakeBottomHeightFactor= 1-lakeBottomHeightFactor*lakeBottomHeightFactor;
			lakeBottomHeightFactor= terTypeInc[SeasideStartIndex]*lakeBottomHeightFactor/terTypeInc[MountainStartIndex];
			float p = lakeBottomHeightFactor;//0.70f * (1 - fTerType * 0.80f);
			if (Mathf.Abs(lake2D) > bottomThreshold)
			{
				value2 = p;
			}
			else
			{
				float per = (Mathf.Abs(lake2D) - bottomThreshold) / (threshold0 - bottomThreshold);
				float paramPer = 1;
				if (fTerType > terTypeInc[HillStartIndex])
				{
					paramPer = 1 / (Mathf.Pow((fTerType - terTypeInc[HillStartIndex]) / (terTypeInc[MountainEndIndex] - terTypeInc[HillStartIndex]), 0.25f) * 8 + 1);
				}
				Mathf.Pow(per, paramPer);
				value2 = per * (1 - p) + p;
			}
		}
		
		if(value2<value)
			value=value2;
		
		
		float fTerTypeRiverOrigin = fTerType*value;
		float finalFTerType= GetTownChangeValue(fTerTypeRiverOrigin,new IntVector2(worldX,worldZ),terTypeInc);
		
		if(finalFTerType>=fTerType)
		{
			value = 1;
			fTerType = finalFTerType;
		}else {
			if(value<1)
				value = finalFTerType/fTerType;
			else
				fTerType = finalFTerType;
		}
		
		//bridge
		if(bridgeMaxHeight>PETools.PEMath.Epsilon){
			float startTerType = terTypeInc[HillStartIndex]+(terTypeInc[MountainEndIndex]-terTypeInc[HillStartIndex])*bridgeMaxHeight;
			
			if (value < bridgeStart)
			{
				float bridge2D = (float)myRiverNoise[BridgeIndex].Noise(scaledX * bridgeFrequencyX * bridgeCof, scaledZ * bridgeFrequencyZ * bridgeCof);
				float bThreshold = ResetBridgeThreshold(bridgeThreshold, fTerType,terTypeInc);
				//float FCof = riverFrequencyX * 2;
				if (Mathf.Abs(bridge2D) < bThreshold)
				{
					float parm = 1;
					if (Mathf.Abs(bridge2D) <= bThreshold / 2)
					{
						parm = 0;
					}
					else
					{
						parm = (Mathf.Abs(bridge2D) - bThreshold / 2) / (bThreshold/2);
					}
					
					//					float bvTop = bridgeTopValue/fTerType;
					//					if(value>bridgeTopThreshold)
					//					{
					//						float bvFactor = (value-bridgeStart)/(bridgeTopThreshold-bridgeStart);
					//						bvTop = (Mathf.Pow (bvFactor,1)*(bridgeTopValue-fTerType*bridgeStart)+fTerType*bridgeStart)/fTerType;
					//					}
					float bvTop = bridgeStart;
					bridgeValue = bvTop - (bvTop - value) * parm;
					bridge2dFactor = Mathf.Abs(bridge2D)/bThreshold;
					if(value*fTerType<startTerType && bridgeValue*fTerType>startTerType){
						float bridgeCutValue = 1;
						if(value*fTerType < terTypeInc[HillStartIndex] )
							bridgeCutValue =0;
						else
							bridgeCutValue = (value*fTerType-terTypeInc[HillStartIndex])/(startTerType-terTypeInc[HillStartIndex]);
						bridgeValue = bridgeValue*bridgeCutValue+ startTerType/fTerType*(1-bridgeCutValue);
					}
						//					if(startTerType<fTerType+0.1f)
//						bridgeValue = value+(bridgeValue-value)*(startTerType-fTerType)/0.1f;//when fTerType approach to startTerType,bridgeValue approach to value
					//					
					//					if(bridgeValue<bridgeEnd)
					//						bridgeValue=-1;
				}
			}
		}
		return value;
	}
	
	static float RiverDisturb(float river2D, float scaledX, float scaledZ)
	{
		float a = 1;
		float widthFactor = 1 - (riverWidthNow - riverWidth1) / (riverWidth100 - riverWidth1);
		float valueCoef1 = riverThreshold1 / 2 * Mathf.Pow(widthFactor, 5);
		float coef = 3.0f / 4;
		float value = river2D + valueCoef1
			* (Mathf.Sin(scaledX * coef * 2) * 2 + Mathf.Sin(scaledZ * coef * 2) * 2 + Mathf.Sin(scaledX * coef * 3.4f) + Mathf.Sin(scaledZ * coef * 3.4f))
				* Mathf.Pow(riverFrequency1 / riverFrequencyNow, a);
		return value;
	}
	
	
	float RiverWidthCorrection(float threshold)
	{
		float value = threshold * (1 + 3 * (1 - riverWidthNow / riverWidth100) * (1 - riverFrequencyNow / riverFrequency100));
		return value;
	}
	
//	static float Get1DNoiseFromAngle(float angle)
//	{
//		float param = (angle + angleStart) * angleScale;
//		float continent1D = (float)myRiverNoise[ContinentBoundIndex].Noise(param);//(-0.098,0.16)
//		continent1D = (continent1D + 0.098f) / (0.16f + 0.098f);//(0,1)
//		return continent1D;
//	}
	//river reset
	static float ResetBottomHeight(float riverBottomMax, float fTerType)
	{
		float OffsetByTerrain = (fTerType - c_fTerTypeMin) / (c_fTerTypeMax - c_fTerTypeMin);
		return riverBottomMax * OffsetByTerrain;
	}
	static float ResetBottomWidth(float threshold, float fTerType)
	{
		float OffsetByTerrain = (fTerType - c_fTerTypeMin) / (c_fTerTypeMax - c_fTerTypeMin);
		return threshold * riverBankPercent * (1 - OffsetByTerrain);
		
	}
	static float ResetThreshold(float riverThreshold, float fTerType, float scaledX = 0, float scaledZ = 0)
	{
		float OffsetByTerrain = (fTerType - c_fTerTypeMin) / (c_fTerTypeMax - c_fTerTypeMin);//[0,1]
		float offset;
		float b = 8.3045f;
		if (OffsetByTerrain < 0.15){
			offset = 2f;
		} else {
			offset = b * Mathf.Pow(OffsetByTerrain - 0.575f, 2) + 0.5f;
		}
		float value = riverThreshold * offset;
		value *= 1 + 0.2f * (Mathf.Sin(scaledX * 2) + Mathf.Sin(scaledZ * 2)
		                     + Mathf.Sin(scaledX * 3.4f) + Mathf.Sin(scaledZ * 3.4f)
		                     );
		return value;
	}
	static float ResetBridgeThreshold(float riverThreshold, float fTerType,float[] terTypeInc, float scaledX = 0, float scaledZ = 0)
	{
		float OffsetByTerrain = (fTerType - c_fTerTypeMin) / (c_fTerTypeMax - c_fTerTypeMin);//[0,1]
		float offset;
		float b = 8.0f;
		offset = b * Mathf.Pow(OffsetByTerrain - 0.2f, 2) + 1f;
		float value = riverThreshold * offset;
		value *= 1 + 0.1f * (Mathf.Sin(scaledX * 2) + Mathf.Sin(scaledZ * 2)
		                     + Mathf.Sin(scaledX * 3.4f) + Mathf.Sin(scaledZ * 3.4f)
		                     //+ Mathf.Cos(scaledX * 2 + scaledZ * 2)
		                     );
		if(fTerType>terTypeInc[HillStartIndex])
			value*=1+(fTerType-terTypeInc[HillStartIndex])/(1-terTypeInc[HillStartIndex])*2;
		return value;
	}
	void DebuffRiverBankTerrain(ref byte volume, ref byte type, float bottomHeight, float noise2D, float fTerType, IntVector2 worldxz, int vy)
	{
		if (vy > Mathf.CeilToInt(bottomHeight))
		{
			volume = 0;
			type = BLOCK_AIR;
		}
		else if (vy == Mathf.CeilToInt(bottomHeight))
		{
			float height = bottomHeight % 1;
			if (height == 0)
			{
				height = 1;
			}
			volume = TerrainUtil.HeightToVolume(height);
		}
		else if (vy == Mathf.CeilToInt(bottomHeight) - 1)
		{
			volume = 255;
		}
	}
	#endregion
	
	#region bioma & terType
	static RandomMapType GetMapType(float scaledX, float scaledZ,out RandomMapType firstType,out RandomMapType secondType,out float diffValue)
	{
		IntVector2 worldPos = new IntVector2 (Mathf.RoundToInt(scaledX/s_detailScale),Mathf.RoundToInt(scaledZ/s_detailScale));
		return GetMapTypeNew(worldPos,out firstType,out secondType,out diffValue);
	}
	static RandomMapType GetMapType(float scaledX, float scaledZ)
	{
		IntVector2 worldPos = new IntVector2 (Mathf.RoundToInt(scaledX/s_detailScale),Mathf.RoundToInt(scaledZ/s_detailScale));
		float diffValue;
		RandomMapType firstType;
		RandomMapType secondType;
		return GetMapTypeNew(worldPos,out firstType,out secondType,out diffValue);
	}
	
	static RandomMapType GetMapTypeNew(IntVector2 worldPos,out RandomMapType firstType,out RandomMapType secondType,out float diffValue){
		List<RandomMapTypeDist> rmtdList = new List<RandomMapTypeDist> ();
		for(int i=0;i<BiomaDistList.Count;i++)
		{
			rmtdList.Add(new RandomMapTypeDist (BiomaDistList[i].type,BiomaDistList[i].GetDistance(worldPos)));
		}
		rmtdList.Sort();
		firstType = rmtdList[0].type;
		secondType = rmtdList[1].type;
		diffValue = rmtdList[1].distance-rmtdList[0].distance;

		
		float scaledX = worldPos.x*s_detailScale;
		float scaledZ = worldPos.y*s_detailScale;
		
		
		if (diffValue < changeBiomaDiff){
			//from large to small, when diff gets larger, type[0] more possible
			float chance = (float)myBiomaNoise[ChangeIndex].Noise(scaledX * changeMapTypeFrequency, scaledZ * changeMapTypeFrequency);
			chance = chance * 0.5f + 0.5f;
			float chance2 = (1 - diffValue / changeBiomaDiff) / 2;
			if ((int)firstType < (int)secondType){
				return (chance < chance2) ? secondType : firstType;
			} else {
				return (chance < 1 - chance2) ? firstType : secondType;
			}
		} else {
			return firstType;
		}
		//return firstType;
	}
		
	static RandomMapType GetMapTypeAndTerInc(float scaledX, float scaledZ,out float[] terTypeInc)
	{
		IntVector2 worldPos = new IntVector2 (Mathf.RoundToInt(scaledX/s_detailScale),Mathf.RoundToInt(scaledZ/s_detailScale));
		float diffValue;
		RandomMapType firstType;
		RandomMapType secondType;
		return GetMapTypeAndTerInc(worldPos,out firstType,out secondType,out diffValue,out terTypeInc);
	}

	static RandomMapType GetMapTypeAndTerInc(IntVector2 worldPos,out RandomMapType firstType,out RandomMapType secondType,out float diffValue, out float[] terTypeInc){
		List<RandomMapTypeDist> rmtdList = new List<RandomMapTypeDist> ();
		for(int i=0;i<BiomaDistList.Count;i++)
		{
			rmtdList.Add(new RandomMapTypeDist (BiomaDistList[i].type,BiomaDistList[i].GetDistance(worldPos)));
		}
		rmtdList.Sort();
		firstType = rmtdList[0].type;
		secondType = rmtdList[1].type;
		diffValue = rmtdList[1].distance-rmtdList[0].distance;
		
		//compute TerTypeInc
		List<float[]> terTypeList = new List<float[]>();
		List<float> distList = new List<float> ();
		for(int i=0;i<rmtdList.Count;i++){
			if(rmtdList[i].distance-rmtdList[0].distance<terTypeChangeDist){
				terTypeList.Add(terTypeChanceIncList[(int)rmtdList[i].type-1]);
				distList.Add(rmtdList[i].distance-rmtdList[0].distance);
			}
		}
		terTypeInc = GetTerTypeInc(terTypeList,distList);

		float scaledX = worldPos.x*s_detailScale;
		float scaledZ = worldPos.y*s_detailScale;
		
		
		if (diffValue < changeBiomaDiff){
			//from large to small, when diff gets larger, type[0] more possible
			float chance = (float)myBiomaNoise[ChangeIndex].Noise(scaledX * changeMapTypeFrequency, scaledZ * changeMapTypeFrequency);
			chance = chance * 0.5f + 0.5f;
			float chance2 = (1 - diffValue / changeBiomaDiff) / 2;
			if ((int)firstType < (int)secondType){
				return (chance < chance2) ? secondType : firstType;
			} else {
				return (chance < 1 - chance2) ? firstType : secondType;
			}
		} else {
			return firstType;
		}
		//return firstType;
	}

	static float[] GetTerTypeInc(List<float[]> terTypeList,List<float> distList){
		if(terTypeList.Count==1)
			return terTypeList[0];
		float sum = 0;
		for(int i=0;i<distList.Count;i++)
		{
			distList[i]=terTypeChangeDist-distList[i];
			sum+=distList[i];
		}
		float[] result = new float[GRASS_REGION_CNT];
		for(int j = 0;j<result.Length;j++){
			result[j]=0;
			for(int i=0;i<terTypeList.Count;i++){
				result[j]+= terTypeList[i][j]*distList[i]/sum;
			}
		}
		return result;
	}

	#endregion
	public static bool IsNoPlantType(int vType){
		return vType ==FLOOR_TYPE || vType == BLOCK_TOWN||vType==BlOCK_TOWN_CONNECTION;
	}
}
