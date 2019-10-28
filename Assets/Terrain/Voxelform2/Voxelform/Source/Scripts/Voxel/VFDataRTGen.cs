#define NOISE_3D
//#define GEN_LOD_WITH_LOD0
#define GEN_LOD_DIRECTLY
#define USE_HEIGHT_BUFF
#define VOXEL_CACHE_ENABLE
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Runtime.InteropServices;
using VoxelPaintXML;
using TownData;
using Pathea;

// FileFormat:
// x,z,h----- 3 int
// raw data-- bytes
// Then Next x,z,h, rawdata ....
public class VFTerTileCacheDesc
{
	public IntVector4 xzlh;	// VFTerTile. tileX, tileZ, tileL(lod), tileH(height)
	public int bitMask;
	public int dataLen;
	public const int DataOfs = 6 * sizeof(int); // All the above is in cache file
	
	public long pos;
	public const int c_dataAxisLen = VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE;
	public const int c_bitMaskBTown = 0x0001;
	public const int c_bitMaskVData = 0x0002;
	public const int c_bitMaskNData = 0x0004;
	public const int c_bitMaskHData = 0x0008;
	public const int c_bitMaskGData = 0x0010;
	public const int c_bitMaskTData = 0x0020;
	public const int c_bitMaskAllData = c_bitMaskVData | c_bitMaskNData | c_bitMaskHData | c_bitMaskGData | c_bitMaskTData;
	public const int c_bitMaskVoxData = c_bitMaskVData;
	// the follow data is ordered with v,n,h,g
	public const int c_nDataLen = sizeof(double) * c_dataAxisLen * c_dataAxisLen;			// noise data size
	public const int c_hDataLen = sizeof(float) * c_dataAxisLen * c_dataAxisLen;			// height data size
	public const int c_gDataLen = sizeof(float) * c_dataAxisLen * c_dataAxisLen;			// gradient data size
	public const int c_tDataLen = sizeof(byte) * c_dataAxisLen * c_dataAxisLen;			// ter type data size
	public int vDataLen
	{
		get
		{
			return dataLen -
				((bitMask & c_bitMaskNData) != 0 ? c_nDataLen : 0) -
					((bitMask & c_bitMaskHData) != 0 ? c_hDataLen : 0) -
					((bitMask & c_bitMaskGData) != 0 ? c_gDataLen : 0) -
					((bitMask & c_bitMaskTData) != 0 ? c_tDataLen : 0);
		}
	}
	
	public static VFTerTileCacheDesc ReadDescFromCache(BinaryReader br)
	{
		int x = br.ReadInt32();
		int z = br.ReadInt32();
		int l = br.ReadInt32();
		int h = br.ReadInt32();
		int bitMask = br.ReadInt32();
		int dataLen = br.ReadInt32();
		
		VFTerTileCacheDesc desc = new VFTerTileCacheDesc();
		desc.xzlh = new IntVector4(x, z, l, h);
		desc.bitMask = bitMask;
		desc.dataLen = dataLen;
		desc.pos = br.BaseStream.Position;
		return desc;
	}
	public void WriteDescToCache(BinaryWriter bw)
	{
		bw.Write(xzlh.x);
		bw.Write(xzlh.y);
		bw.Write(xzlh.z);
		bw.Write(xzlh.w);
		bw.Write(bitMask);
		bw.Write(dataLen);
	}
	public void ReadDataFromCache(BinaryReader br, VFTile tile, double[][] nData, float[][] hData, float[][] gData, RandomMapType[][] tData)
	{
		//Debug.LogWarning("[VFTerTileCacheDesc]Reading:"+xzlh+","+bitMask+","+pos);
		br.BaseStream.Seek(pos, SeekOrigin.Begin);
		if (0 != (bitMask & c_bitMaskVData))
		{
			tile.tileX = xzlh.x; 
			tile.tileZ = xzlh.y;
			tile.tileL = xzlh.z;
			tile.tileH = xzlh.w;
			int[] terraYLens, waterYLens;
			byte[][] terraVoxels, waterVoxels;
			int maxLenY = tile.MaxDataLenY;
			int lenx = VFTile.DataLenX, lenz = VFTile.DataLenZ, leny;
			for (int i = 0; i < lenz; i++)
			{
				terraYLens = tile.nTerraYLens[i];
				waterYLens = tile.nWaterYLens[i];
				terraVoxels = tile.terraVoxels[i];
				waterVoxels = tile.waterVoxels[i];
				for (int j = 0; j < lenx; j++)
				{
					// terra
					leny = br.ReadUInt16();
					terraYLens[j] = leny;
					Array.Clear(terraVoxels[j], 0, maxLenY);				
					br.Read(terraVoxels[j], 0, leny);
					// water
					leny = br.ReadUInt16();
					waterYLens[j] = leny;
					Array.Clear(waterVoxels[j], 0, maxLenY);
					br.Read(waterVoxels[j], 0, leny);
				}
			}
		}
		if (0 != (bitMask & c_bitMaskNData))
		{
			for (int i = 0; i < c_dataAxisLen; i++)
			{
				for (int j = 0; j < c_dataAxisLen; j++)
					nData[i][j] = br.ReadDouble();
			}
		}
		if (0 != (bitMask & c_bitMaskHData))
		{
			for (int i = 0; i < c_dataAxisLen; i++)
			{
				for (int j = 0; j < c_dataAxisLen; j++)
					hData[i][j] = br.ReadSingle();
			}
		}
		if (0 != (bitMask & c_bitMaskGData))
		{
			for (int i = 0; i < c_dataAxisLen; i++)
			{
				for (int j = 0; j < c_dataAxisLen; j++)
					gData[i][j] = br.ReadSingle();
			}
		}
		if (0 != (bitMask & c_bitMaskTData))
		{
			for (int i = 0; i < c_dataAxisLen; i++)
			{
				for (int j = 0; j < c_dataAxisLen; j++)
					tData[i][j] = (RandomMapType)br.ReadByte();
			}
		}
		return;
	}
	public static VFTerTileCacheDesc WriteDataToCache(BinaryWriter bw, int bitMask, VFTile tile, double[][] nData, float[][] hData, float[][] gData, RandomMapType[][] tData)
	{
		VFTerTileCacheDesc desc = new VFTerTileCacheDesc();
		desc.xzlh = new IntVector4(tile.tileX, tile.tileZ, tile.tileL, tile.tileH);
		desc.bitMask = bitMask;
		desc.dataLen = 0;			// placeholder
		desc.WriteDescToCache(bw);
		desc.pos = bw.BaseStream.Position;
		if (0 != (bitMask & c_bitMaskVData))
		{
			int[] terraYLens, waterYLens;
			byte[][] terraVoxels, waterVoxels;
			int lenx = VFTile.DataLenX, lenz = VFTile.DataLenZ, leny;
			for (int i = 0; i < lenz; i++)
			{
				terraYLens = tile.nTerraYLens[i];
				waterYLens = tile.nWaterYLens[i];
				terraVoxels = tile.terraVoxels[i];
				waterVoxels = tile.waterVoxels[i];
				for (int j = 0; j < lenx; j++)
				{
					// terra
					leny = terraYLens[j];
					bw.Write((ushort)leny);
					bw.Write(terraVoxels[j], 0, leny);
					// water
					leny = waterYLens[j];
					bw.Write((ushort)leny);
					bw.Write(waterVoxels[j], 0, leny);
				}
			}
		}
		if (0 != (bitMask & c_bitMaskNData))
		{
			for (int i = 0; i < c_dataAxisLen; i++)
			{
				for (int j = 0; j < c_dataAxisLen; j++)
					bw.Write(nData[i][j]);
			}
		}
		if (0 != (bitMask & c_bitMaskHData))
		{
			for (int i = 0; i < c_dataAxisLen; i++)
			{
				for (int j = 0; j < c_dataAxisLen; j++)
					bw.Write(hData[i][j]);
			}
		}
		if (0 != (bitMask & c_bitMaskGData))
		{
			for (int i = 0; i < c_dataAxisLen; i++)
			{
				for (int j = 0; j < c_dataAxisLen; j++)
					bw.Write(gData[i][j]);
			}
		}
		if (0 != (bitMask & c_bitMaskTData))
		{
			for (int i = 0; i < c_dataAxisLen; i++)
			{
				for (int j = 0; j < c_dataAxisLen; j++)
					bw.Write((byte)tData[i][j]);
			}
		}
		desc.dataLen = (int)(bw.BaseStream.Position - desc.pos);
		bw.BaseStream.Seek(desc.pos - DataOfs, SeekOrigin.Begin);
		desc.WriteDescToCache(bw);
		return desc;
	}
}
public class VFTile
{
	// change to zxy in order to use less memory
	public byte[][][] terraVoxels; //zyx for compatiability with ocl mc algo
	public byte[][][] waterVoxels;
	public int[][] nTerraYLens;
	public int[][] nWaterYLens;
	public int tileX;
	public int tileZ;
	public int tileL;	// lod
	public int tileH;	// height
	public static int DataLenZ { get { return VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; } }
	public static int DataLenX { get { return VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; } }
	public static int DataSize(int h) { return DataLenX * (h + 1) * DataLenZ; }
	public int MaxDataLenY { get { return (tileH + 1) * VFVoxel.c_VTSize; } }	// +1 in order to compute gradient
	public VFTile(int lod, int maxHight)
	{
		tileX = int.MinValue;
		tileZ = int.MinValue;
		tileL = lod;
		tileH = maxHight;
		terraVoxels = new byte[DataLenZ][][];
		waterVoxels = new byte[DataLenZ][][];
		nTerraYLens = new int[DataLenZ][];
		nWaterYLens = new int[DataLenZ][];
		int lenY = MaxDataLenY;
		for (int i = 0; i < DataLenZ; i++){
			terraVoxels[i] = new byte[DataLenX][];
			waterVoxels[i] = new byte[DataLenX][];
			nTerraYLens[i] = new int[DataLenX];
			nWaterYLens[i] = new int[DataLenX];
			for(int j = 0; j < DataLenX; j++){
				terraVoxels[i][j] = new byte[lenY];
				waterVoxels[i][j] = new byte[lenY];
			}
		}
	}
}

// VF Data Runtime Generation
public partial class VFDataRTGen : IVxDataLoader
{
	public const byte FLOOR_TYPE = 45;
	public const byte BLOCK_TOWN = 46;
	public const byte BlOCK_TOWN_CONNECTION = 71;
	const float HalfPiTo1000 = (float)(2000.0 / Math.PI);
	const byte BLOCK_AIR = 0;
	const byte BLOCK_STONE = 1;
	const float TAN45 = 0.727f;	
	const int c_noMonsterRadius = 96;
	const int TownConnectionDepth = 4;
	const int BiomaDepth = 8;
	public static IntVector2 noTownStartPos;
	public static List<RandomMapTypePoint> BiomaDistList = new List<RandomMapTypePoint> ();



	static RegionDescArrayCLS[] regionArray;
//	static RegionDescArrayCLS region;
//	static int terTypeCnt;
//	static RandomMapType terrainType;
	public static ClimateType sceneClimate;
	public const int GRASS_REGION = 0;		//RandomMapType - 1
	public const int FOREST_REGION = 1;
	public const int DESERT_REGION = 2;
	public const int REDSTONE_REGION = 3;
	public const int RAINFOREST_REGION = 4;	
	public const int TERRAIN_REGION_CNT = 6;//staticmark
	const float c_fTerTypeMin = -0.39f;
	const float c_seasideStart = -0.32f;//-0.32f;
	const float c_plainStart = 0.078125f;// 0.078125f;=1/12.8
	const float c_hillStart = 0.15625f;//0.15625f=1/6.4
	const float c_highlandStart = 0.8f;//0.5859375f;//0.5859375f=1/1.067
	const float c_fTerTypeMax = 1.15f;//1.15f
	
	const float c_plainLakeStartTerType = 0.07f;
	const float c_topMountainStart = 0.5859375f;
	const float c_topLakeStartTerType = 0.8f;//0.8
	const float c_topLakeBankStartTerType = 0.82f;
	
	const int GRASS_REGION_CNT = 5;//staticmark
	static readonly float[] GRASS_REGION_THRESHOLD = new float[GRASS_REGION_CNT + 1]{
		c_fTerTypeMin,			//TERRAIN_SEA_START -0.39f,
		c_seasideStart,			//TERRAIN_SEASIDEROCKY_START -0.32f,	
		c_plainStart,		//TERRAIN_GRASSLAND_START 0.078125f,
		c_hillStart,		//TERRAIN_HILLCOUNTRY_START 0.15625f,
		c_highlandStart,		//TERRAIN_HIGHLAND_START 0.5859375f,
		c_fTerTypeMax			//TERRAIN_HIGHLAND_END 1.15f
	};
	
	static readonly int[][] S_VoxelIndex = new int[][]{ //VoxelTerrainConstants.MAX_LOD-1
		new int[]{
			0x000,0x001,0x002,0x003,0x004,0x005,0x006,0x007,0x008,0x009,0x00a,0x00b,0x00c,0x00d,0x00e,0x00f,0x010,0x011,0x012,0x013,0x014,0x015,0x016,0x017,0x018,0x019,0x01a,0x01b,0x01c,0x01d,0x01e,0x01f,0x020,0x021,0x022,},
		new int[]{
			0x000,0x001,0x003,0x005,0x007,0x009,0x00b,0x00d,0x00f,0x011,0x013,0x015,0x017,0x019,0x01b,0x01d,0x01f,
			0x101,0x103,0x105,0x107,0x109,0x10b,0x10d,0x10f,0x111,0x113,0x115,0x117,0x119,0x11b,0x11d,0x11f,0x121,0x122,},//31,33,34
		new int[]{
			0x000,0x001,0x005,0x009,0x00d,0x011,0x015,0x019,0x01d,
			0x101,0x105,0x109,0x10d,0x111,0x115,0x119,0x11d,
			0x201,0x205,0x209,0x20d,0x211,0x215,0x219,0x21d,
			0x301,0x305,0x309,0x30d,0x311,0x315,0x319,0x31d,0x321,0x322,},
		new int[]{
			0x000,0x001,0x009,0x011,0x019,
			0x101,0x109,0x111,0x119,
			0x201,0x209,0x211,0x219,
			0x301,0x309,0x311,0x319,
			0x401,0x409,0x411,0x419,
			0x501,0x509,0x511,0x519,
			0x601,0x609,0x611,0x619,
			0x701,0x709,0x711,0x719,0x721,0x722,},
	};
	
	static int TEST_REGION_CNT;//staticmark
	static float[] TEST_REGION_THRESHOLD;
	
	public static int s_noiseHeight = 128; // readonly can not be default parameter
	public static int s_seaDepth = 5;
	public float PlainThickness;
	public const int c_hillThickness = 256;//128
	public static float PlainMax(float plainThickness)
	{
		return s_seaDepth + plainThickness > s_noiseHeight-1 ? s_noiseHeight-1 : s_seaDepth + plainThickness;
	}
	public float SpawnHeight(float PlainThickness)
	{
		return PlainMax(PlainThickness) + 20 > s_noiseHeight - 1 ? s_noiseHeight - 1 : PlainMax(PlainThickness) + 20;
	}
	public static int HillMax
	{
		get
		{
			return s_seaDepth + c_hillThickness > s_noiseHeight-1 ? s_noiseHeight-1 : s_seaDepth + c_hillThickness;
		}
	}
	
	public static int MountainThickness = 256;
	public static int MountainMax
	{
		get
		{
			return s_seaDepth + MountainThickness > s_noiseHeight-1 ? s_noiseHeight-1 : s_seaDepth + MountainThickness;
		}
	}
	
	static int s_noiseWidth = 128;
	public static float s_detailScale = 1.0f / s_noiseWidth; // hill density in 2d
	static float HeightScale { get { return 1.0f / s_noiseHeight; } }
	static float HeightScale_1 { get { return 0.32f / s_noiseHeight; } }//0.32/NoiseHeight
	static int HeightScalePivot { get { return s_noiseHeight / 2; } }//64
	static int DensityClampPivot { get { return s_noiseHeight - 8; } }//120
	
	static float DensityDelta = 0.02f;
	static float DensityDeltaHalf255Reci = 0.5f * 255 / DensityDelta;
	static float DensityThreshold = -0.215f; //-(Mathf.Abs((float)myNoise[9].Noise2DFBM( VoxelX, VoxelZ, 1) )*0.1f + 0.6f);
	static float DensityThresholdMinusDelta = DensityThreshold - DensityDelta;
	static float DensityThresholdPlusDelta = DensityThreshold + DensityDelta;
	
	const float TerBaseNoisePara = 0.08f;//0.02
	const float _rockyPara = 0.1f;
	const float _terTypeCoef=1.0f;//1.2//1
	const float _terDecreaseLarge = 1.2f;//1.2//1
	const float _terDecreaseSmall = 1.0f;
	
	#region terrain parameters
	//terrain Type
//	static float seasideStart = 0.55f;//0.15
//	static float plainStart = 0.65f;//0.30
//	static float hillStart = 0.90f;//0.7
//	static float MountainTopStart = 0.98f;//0.85

	const int SeasideStartIndex = 0;
	const int PlainStartIndex = 1;
	const int HillStartIndex = 2;
	const int MountainStartIndex = 3;
	const int MountainEndIndex = 4;

	//static int GrasslandChance = 25;
	//static int ForestChance = 25;
	//static int DesertChance = 25;
	//static int RedstoneChance = 25;
	static RandomMapType CurrentMapType;
	//static int MapTypeOffset = 5;
	static int[] MapTypeChance;
	static int[] MapTypeValue;
	static List<int> mapTypeList;
	//static float mapTypeFrequency0 = 0.04f;//bioma
	//static float mapTypeFrequencyX = 0.04f;//bioma
	//static float mapTypeFrequencyZ = 0.04f;
	public static void SetMapTypeFrequency(float scale)
	{
		//mapTypeFrequencyX = mapTypeFrequency0 * scale;
		//mapTypeFrequencyZ = mapTypeFrequency0 * scale;
	}
	static float changeMapTypeFrequency = 16f;
	static float terrainFrequency0 = 0.035f;
	static float terrainFrequencyX = 0.035f;//hill,plain,0.05f
	static float terrainFrequencyZ = 0.035f;
	static float MountainFrequencyFactor = 1f;
	static float SierraFrequencyFactor = 0.125f;

	public static void SetTerrainFrequency(float scale)
	{
		
		terrainFrequencyX = terrainFrequency0 * scale;
		terrainFrequencyZ = terrainFrequency0 * scale;
	}
	
	static float HASStart { get { return s_seaDepth; } }//the Y start to render mountain
	static float HASEnd;
	static float HASEnd2;
	static float HASFilterFrequency =terrainFrequencyX;// 0.00875f;//0.0175f;
	static float HAS2FilterFrequency = HASFilterFrequency*2;
	public static int HASMin = 5;//5
	public static int HASMid = 30;//30
	public static int HASMax = 192;//168//192
	const int HASMidBottom = 5;
	const int HASMidTop = 256;
	const int HASMinMin = 5;
	const int HASMaxMax = 256;
	public static void SetPlainThickness(float plainHeight){
		int top;
		if(s_noiseHeight<256)
			top = 100;
		else if(s_noiseHeight<512)
			top = 200;
		else 
			top = 256;
		
		HASMid =  HASMidBottom+Mathf.RoundToInt((top-HASMidBottom)*(plainHeight-1)/99);
	}
	
	//new height above seaParam
	static float HASChangeFrequency {
		get{return terrainFrequencyX*2f;}
	}
	static float HASChangeValue=0.15f;//0.3f;
	static float HASChangeValueSierra=0.3f;
	
	//new islandSize
	const float islandStandard = 80;
	const float islandMin = 60;
	const float islandMax = 100;
	static float islandFactor = 80;
	public static void SetIslandSize(int scale){
		float factor = scale/100.0f;
		islandFactor = factor*(islandMax-islandMin)+islandMin;
	}
	
	//	public static void SetPlainMin(float value){
	//		HASMin = Mathf.FloorToInt((HASMid-HASMinMin)*value+HASMinMin);
	//	}
	//	public static void SetPlainMax(float value){
	//		HASMax = Mathf.CeilToInt((HASMaxMax-HASMid)*value+HASMid);
	//	}
	//	static float HAS2FilterFrequency = 0.42f;
	//	const int HAS2Min = -10;//5
	//	const int HAS2Max = 10;//168//192
	

	//flatness
	public static float flatMin = 0.03f;//0.25
	public static float flatMid = 0.25f;
	public static float flatMax = 2f;//2
	public static float flatFrequency = 1.0f / 128;
	const float flatMidBottom = 0.05f;
	const float flatMidMid = 0.5f;
	const float flatMidTop =3f;
	const float flatMinMin = 0.03f;
	const float flatMaxMax = 3.5f;
	public static void SetFlatness(float flatness){
		flatMid = GetSlideBarValue((101-flatness),flatMidBottom,flatMidMid,flatMidTop);
	}
	public static void SetFlatMin(float value){
		flatMin = (flatMid-flatMinMin)*value+flatMinMin;
	}
	public static void SetFlatMax(float value){
		flatMax = (flatMaxMax-flatMid)*value+flatMid;
	}
	//NoiseIndex
	const int DensityClampFilter01 = 0;
	const int FTerTypeIndex = 1;
	const int DensityClampFilter02 = 2;
	const int HillTerIndex01 = 3;
	const int FTerTypeIndex01 = 4;
	const int MountainParamIndex01 = 5;
	const int MountainParamIndex011 = 6;
	const int SierraIndex = 7;
	const int HillTerIndex02 = 8;
	const int FlatIndex = 9;
	const int TownChangeIndex = 10;
	const int Volume3DNoiseIndex = 11;
	const int MountainParamIndex02 =12;
	const int MountainParamIndex022 =13;
	const int FTerTypeIndex02 = 14;
	const int HASFilterIndex = 15;//height above sea
	const int TopCorrectionIndex = 16;
	const int HASChangeIndex = 17;
 	//town
	const float TownChangeFrequency = 0.25f;
	const float TownChangeFactor = 2;
	public static float TownConnectionAreaWidth{
		get{return (TownConnectionWidth+Mathf.Max(TownConnectionHillDistance,TownConnectionWaterDistance))*(1+TownChangeFactor);}
	}
	public static float TownConnectionAreaTypeWidth{
		get{return TownConnectionWidth*(1+TownChangeFactor);}
	}
	public static float TownChangeMaxDistance{
		get{return Mathf.Max(TownConnectionHillDistance,TownConnectionWaterDistance);}
	}
	public static float TownChangeMaxFactor{
		get{return 1+TownChangeFactor;}
	}
	//bioma
	const float BiomaTerrainChangeRadius = 48;
	const float BiomaTerrainTopRadius = 32;
	const float BiomaTerrainChangeFrequency = 0.25f;
	const float BiomaTerrainChangeFactor = 2;
	#endregion
	
	#region river&bridge & cointinent
	//water
	public static int waterSeed = 1;
	public static float WaterHeightBase { get { return 3.5f + s_seaDepth; } }
	public static float waterHeight;
	public const int TEMP_WATER_PLUS_MIN = 5;
	public const int TEMP_WATER_PLUS_MAX = 20;
	public const int WET_WATER_PLUS_MIN = 50;
	public const int WET_WATER_PLUS_MAX = 80;
	public const float FILL_VOLUME = 160f;	
	//river&bridge
	static float riverFrequency1 = 1.0f / 128;//1.0f/128
	static float riverFrequency100 = 1.0f / 4;//1.0f / 16;//1.0f/4
	static float riverFrequencyNow;	
	static float riverFrequencyX = 0.0625f;//0.0625=1/16
	static float riverFrequencyZ = 0.0625f;	
	static float riverBottomPercent1 = 0.1f;
	static float riverBottomPercent100 = 0.7f;
	static float riverBottomPercentNow;	
	static float riverThreshold1 = 1.0f/32;//1.0f/32
	static float riverThreshold100 = 1.0f/8;//1.0f / 8;//1.0f/4	
	static float riverWidth1 = riverThreshold1 / riverFrequency1;//4//4
	static float riverWidth100 = riverThreshold100 / riverFrequency1;//16//32
	static float riverWidthNow;
	
	static float bridgeFrequency1 = 1.0f/8;//riverFrequency100/2
	static float bridgeFrequency100 = 1.0f / 256;//riverFrequency1/2	
	static float bridgeFrequencyX = riverFrequency1 + (riverFrequency100 - riverFrequency1) * 0.125f;
	static float bridgeFrequencyZ = bridgeFrequencyX;	
	static float bridgeThreshold1 = 0.0375f;
	static float bridgeThreshold100 = 0.0375f / 2;	
	static float bridgeThreshold = 0.15f;
	static float bridgeCof = 1;	
	static float bridgeMaxHeight = 0.7f;//(0,1)
	const float bridgeStart = 0.98f;//0.95
	const float bridgeEdge = 0.93f;//0.95
	const float bridgeEnd = 0.5f;//0.88
	static float bridgeTopValue{
		get{return 0.9f;}//plainStart+(1-plainStart)*bridgeMaxHeight;}//plainStart*3/4+seasideStart*1/4;}
	}
	//static float bridgeTopThreshold=0.5f;
	
	static float riverThreshold = riverThreshold1;//0.25
	static float riverBankPercent = 0.9f;
	static int densityMin = 1;
	static int densityMax = 100;
	static int widthMin = 1;
	static int widthMax = 100;
	static float continentBoundFrequency=2;
	const int RiverIndex = 0;
	const int LakeIndex = 1;
	const int LakeBottomHeightIndex = 2;
	const int ContinentBoundIndex = 3;
	const int BridgeIndex = 4;
	const int RiverBottomChangeIndex = 5;
	const int River2DChangeIndex = 6;
	const int LakeChangeIndex = 7;
	//change
	static float lakeChangeFrequency = 1.0f / 3;
	
	public static void SetRiverDensity(int riverDensity)
	{
		if (riverDensity < densityMin)		riverDensity = densityMin;
		if (riverDensity > densityMax)		riverDensity = densityMax;
		//float densityValue = riverFrequency1 + (riverFrequency100 - riverFrequency1) * (riverDensity - densityMin) / (densityMax - densityMin);
		float densityValue = GetSlideBarValue(riverDensity,riverFrequency1,riverFrequency100);
		riverFrequencyNow = densityValue;
		riverFrequencyX = densityValue;
		riverFrequencyZ = densityValue;
	}
	
	public static void SetRiverWidth(int riverWidth)
	{
		if (riverWidth < widthMin)			riverWidth = widthMin;
		if (riverWidth > widthMax)			riverWidth = widthMax;
		riverWidthNow = riverWidth1 + (riverWidth100 - riverWidth1) * (riverWidth - widthMin) / (widthMax - widthMin);//new
		riverThreshold = riverWidthNow * riverFrequencyX;
		riverBottomPercentNow = riverBottomPercent1 + (riverBottomPercent100 - riverBottomPercent1) * (riverWidthNow - riverWidth1) / (riverWidth100 - riverWidth1);//new
		
		float bridgeFrequencyNow = bridgeFrequency1 + (bridgeFrequency100 - bridgeFrequency1) * (riverWidth - widthMin) / (widthMax - widthMin);
		bridgeFrequencyX = bridgeFrequencyNow;
		bridgeFrequencyZ = bridgeFrequencyNow;
		bridgeThreshold = bridgeThreshold1 + (bridgeThreshold100 - bridgeThreshold1) * (riverWidth - widthMin) / (widthMax - widthMin);
		if (Application.isEditor)
		{
			Debug.Log("<color=red>" + riverWidthNow + "</color>");
			Debug.Log("<color=red>" + riverThreshold + "</color>");
			Debug.Log("<color=red>" + "riverBottomPercentNow:" + riverBottomPercentNow + "</color>");
			Debug.Log("<color=red>" + "bridgeFrequencyNow:" + bridgeFrequencyNow + "</color>");
		}
	}
	
	public static void SetBridgeMaxHeight(int bridgeValue){
		bridgeMaxHeight = bridgeValue/100f;
	}
	#endregion
	
	//lake
	static float lakeFrequency = 1.0f / 16;
	static float lakeThreshold = 0.875f;//
	static float lakeBankPercent = 0.05f;//0.8
	static float lakeBottomHeightMax0 { get { return s_seaDepth + 5; } }
	//top lake
	static int lakeAddedWaterTop { get { return s_noiseHeight - 100; } }
	//continent
//	static float angleScale = 1 / 64f;//1/64
//	static float angleScaleStart = 1 / 72f;
//	static float angleScaleEnd = 1 / 42f;
//	static float angleStart = 0f;//1024
//	static float endCircle = 22.5f;//
	//bioma
	//static float selectedBiomaPlus = 0.6f;
	//static float climateDryPlus = 0.3f;
	//static float changeBiomaThreshold = 0.15f;
	public static float changeBiomaDiff = 24;
	public static float biomaChangeFTerTypeDiff = 30;
	//rocky param
	static int rockyStart0(float PlainThickness) { return  Mathf.FloorToInt(PlainMax(PlainThickness) - 20); }
	static int rockyStart1(float  PlainThickness) { return Mathf.FloorToInt(PlainMax(PlainThickness)); }
	static int rockyStart2 { get { return HillMax; } }
	//top
	static int HillTopCorrection { get { return HillMax - 2; } }//128+128-2;
	static int MountainTopCorrection { get { return s_noiseHeight; } }
	static float disturbFrequency = 12;
	
//	private static float[] terTypeChance;//staticmark
//	private static float[] terTypeChanceInc;//staticmark
	private static List<float[]> terTypeChanceIncList;
	const float terTypeChangeDist = 64;
	private static float regionMineChance;//staticmark
//	private static MineChanceArrayCLS[] regionMinePercentDesc;//staticmark
	private static bool isTownTile; // If the last tile has a town.
	private static double[][] dTileNoiseBuf;
	private static float[][] fTileHeightBuf;//staticmark
	private static float[][] fTileGradTanBuf;//staticmark
	private static RandomMapType[][] tileMapType;
	private static float[] fTerDensityClamp;//staticmark
	private static float[] fTerDensityClampBase;
	private static float[] fTerNoiseHeight;//staticmark
	
	const int MAX_TILES_IN_CACHE = 30;	// normal max 8+7 , not implemented yet
	static readonly byte[] SolidBottomVoxel = new byte[VFVoxel.c_VTSize]{255, 10};
	static readonly byte[] NoiseSeedsPlus = new byte[]{0,1,2,3,4,5,6,7,8,9,
		100,101,102,103,104,105,106,107,108,109}; //terrain
	static readonly byte[] CaveSeedsPlus = new byte[] {90,91,92,93,94,95};
	static readonly byte[] MineSeedsPlus = new byte[]{
		10,11,12,13,14,15, //minebase
		16,17,18,19,20,21,22,23, 24,25,26,27,
		28,29,30,31,32,33,34,35, 36,37,38,39,
		
		40,41,42,43,44,45,46,47, 48,49,50,51//mine3
	};
	static readonly byte[] RiverSeedsPlus = new byte[] { 
		64//river
		,65//lake
		,66//top lake
		,67//continent bound
		,68//bridge
		,69//riverchange
		,70//river2d
		,71//lakechange
	};
	static readonly byte[] BiomaSeedsPlus = new byte[] { 
		80//grassland
		,81//forrest
		,82//desert
		,83//redstone
		,84//change
		,85//rainforest
	};
	
	//private System.Object obj4Lock = new System.Object();
	private static SimplexNoise[] myNoise = null;//staticmark
	private static SimplexNoise[] myCaveNoise = null;
	private static SimplexNoise[] myMineNoise = null;
	private static SimplexNoise[] myRiverNoise = null;//staticmark
	private static SimplexNoise[] myBiomaNoise = null;//staticmark
	private static int staticSeed = -1;
	#region cave
	const float CaveXZFrequency = 1.0f / 8;//1.0f/4
	const float CaveHeightFrequency = 1.0f / 8;//1/8
	const float CaveThicknessFrequency = 1.0f;//1/4
	float CaveHeightMax(float PlainThickness)
	{
		return PlainMax(PlainThickness);
	}
	const int CaveHeightMin = 5;
	const int CaveThicknessMax = 30;
	const float CaveXZThreshold = 0.05f;//0.1f
	const int CaveHeightTerValue = 80;
	const int CaveThicknessTerValue = 20;
	const float CaveThresholdTerValue = 0.2f;
	const float CaveHillFrequency = 1.0f / 8;
	const float CaveHillHeightFrequency = 1.0f / 8;
	const float CaveHillThicknessFrequency = 1.0f / 4;
	static int CaveHillHeightMax { get { return s_noiseHeight - 100; } }
	float CaveHillHeightMin(int PlainThickness)
	{
		return PlainMax(PlainThickness) - 20;
	}
	const int CaveHillThicknessMax = 25;
	const float CaveHillThreshold = 0.08f;
	const float CaveHillFloorPer = 0.5f;
	#endregion
	
	#region mine
	const float MineFrequency = 1.0f / 2;
	const float MineChance = 0.5f;
	float MineMaxHeight(float PlainThickness)
	{
		return PlainMax(PlainThickness);
	}
	const int MineThickness = 50;//30
	static int[] MineStartHeightList; //new int[]{ 30, 70, 55,40,80,20,90,90};//512
	
	const int MineStartNoiseIndex = 6;
	
	static double minePerturbanceFrequency0 = 0.25;
	
	public static double MineFrequency0
	{
		get { return minePerturbanceFrequency0; }
		set { minePerturbanceFrequency0 = value; }
	}
	
	static double minePerturbanceFrequency1 = 0.0625;
	public static double MineFrequency1
	{
		get { return minePerturbanceFrequency1; }
		set { minePerturbanceFrequency1 = value; }
	}
	static bool metalReduceSwitch = true;
	public static bool MetalReduceSwitch
	{
		get { return metalReduceSwitch; }
		set { metalReduceSwitch = value; }
	}
	static int metalReduceArea = 3000;
	public static int MetalReduceArea
	{
		get { return metalReduceArea; }
		set { metalReduceArea = value; }
	}
	
	public static float mineHeightFrequency = 0.5f;
	public static float mineThicknessFrequency = 2;
	public static float mineQuantityFrequency = 8;
	const int HEIGHT_INDEX = 3;
	const int THICKNESS_INDEX = 4;
	const int QUANTITY_INDEX = 5;
	const int HeightOffsetMax = 30;//30
	static int HeightOffsetTer = 64;
	const int ThicknessOffsetMax = 50;
	#endregion
	
	#region terrain layer
	static int PlainTopHeight(float PlainThickness)
	{
		return Mathf.CeilToInt( PlainMax(PlainThickness) + 64 > s_noiseHeight-1 ? s_noiseHeight-1 : PlainMax(PlainThickness) + 64);
	}
	static int HillBottomHeight(float PlainThickness)
	{
		return Mathf.CeilToInt( PlainMax(PlainThickness) + 128 > s_noiseHeight-1 ? s_noiseHeight-1 : PlainMax(PlainThickness) + 128);
	}
	static int HillTopHeight(float PlainThickness)
	{
		return Mathf.CeilToInt( PlainMax(PlainThickness) + 256 > s_noiseHeight-1 ? s_noiseHeight-1 : PlainMax(PlainThickness) + 256);
	}
	//DensityClamp
	
	static float DensityClampPlainTopValue = 0.3f;
	static float DensityClampAllTopValue = 2.3f;
	static float DensityClampBaseBlendFactor = 0.2f;
	
	static float DensityClampLinerTopValue = 2.3f;
	//static float DensityClampLinerBlendSeaSide = 0.05f;
	static float DensityClampLinerBlend = 0.5f;

	#endregion
	#region optimise
	const int JumpCount = 2;
	const byte GenVolumeThreshold = 168;
	#endregion
	
	const int GrasslandIndex = 0;
	const int ForestIndex = 1;
	const int DesertIndex = 2;
	const int RedstoneIndex = 3;
	const int ChangeIndex = 4;
	const int RainforestIndex = 5;
	
	private byte[] _tmpInterTerraVoxels1 = null;
	private byte[] _tmpInterTerraVoxels2 = null;
	private byte[] _tmpInterWaterVoxels1 = null;
	private byte[] _tmpInterWaterVoxels2 = null;
	private byte[] _tmpBridgeVoxels = null;
	private VFTile _curTile = null;
	private VFTile _lodTile = null;
	private VFDataRTGenFileCache _tileFileCache;
	private byte[] _lrbfVol = new byte[4];
	private byte[] _lrbfVolDn = new byte[4];
	private IntVector2 _tmpVec2 = new IntVector2();
	private IntVector3 _tmpVec3 = new IntVector3();
	private IntVector4 _tmpVec4 = new IntVector4();
	private IntVector2 _tmpTileIndex = new IntVector2 ();
	private List<int> _tmpTownIdList = new List<int>();
	private List<VArtifactUnit> _tmpNewTowns = new List<VArtifactUnit> ();
	private bool bImmMode;
	
	private static VoxelPaintXMLParser paintConfig;
	public static Dictionary<IntVector2, List<TreeInfo>> s_dicTreeInfoList = new Dictionary<IntVector2, List<TreeInfo>>();
	public static Dictionary<IntVector2, List<VoxelGrassInstance>> s_dicGrassInstList = new Dictionary<IntVector2, List<VoxelGrassInstance>>();
	
	public static Dictionary<IntVector3, byte> townFloorVoxelByte = new Dictionary<IntVector3, byte>();
	public System.Random TownRand = new System.Random();
	
	#region counter
	//static int renderedCount = 0;
	//static long sumTick = 0;
	#endregion
	public VFDataRTGen(int seed)
	{
		staticSeed = seed;
		string strToDigest = RandomMapConfig.GetModeInt().ToString() + "|" +
			((int)RandomMapConfig.RandomMapID).ToString() + "|" +
				((int)RandomMapConfig.vegetationId).ToString() + "|" +
				((int)RandomMapConfig.ScenceClimate).ToString() + "|" +
				RandomMapConfig.mapSize.ToString() + "|" +
				RandomMapConfig.TerrainHeight.ToString() + "|" +
				RandomMapConfig.riverWidth.ToString() + "|" +
				RandomMapConfig.riverDensity.ToString() + "|" +
				RandomMapConfig.plainHeight.ToString()+"|"+
				RandomMapConfig.flatness.ToString()+"|"+
				RandomMapConfig.bridgeMaxHeight.ToString()+"|"+
				RandomMapConfig.allyCount.ToString()+"|"+
				RandomMapConfig.mirror.ToString()+"|"+
				RandomMapConfig.rotation.ToString()+"|"+
				RandomMapConfig.pickedLineIndex.ToString()+"|"+
				RandomMapConfig.pickedLevelIndex.ToString()+"|"+
				staticSeed.ToString();
		string cacheFilePathName = VFDataRTGenFileCache.GetCacheFilePathName(strToDigest);
		_tileFileCache = new VFDataRTGenFileCache(cacheFilePathName + ".terra");
		_curTile = new VFTile (0, s_noiseHeight);
		_lodTile = new VFTile (1, s_noiseHeight);
		
		int maxHeight = (s_noiseHeight + 1) * VFVoxel.c_VTSize;
		_tmpBridgeVoxels = new byte[maxHeight];
		_tmpInterTerraVoxels1 = new byte[maxHeight];
		_tmpInterTerraVoxels2 = new byte[maxHeight];
		_tmpInterWaterVoxels1 = new byte[maxHeight];
		_tmpInterWaterVoxels2 = new byte[maxHeight];
		
		if (myNoise == null)
			InitStaticParam(seed);
		
		s_dicTreeInfoList.Clear ();
		s_dicGrassInstList.Clear ();
	}
	
	public static void InitStaticParam(int seed){
		staticSeed = seed;
		// Init noise and buffers for noise
		NoiseSeedsPlus[0] = (byte)RandomMapConfig.RandomMapID;
		int nNoise = NoiseSeedsPlus.Length;
		myNoise = new SimplexNoise[nNoise];
		for (int i = 0; i < nNoise; i++) 	myNoise[i] = new SimplexNoise(staticSeed + NoiseSeedsPlus[i]);
		
		int cNoise = CaveSeedsPlus.Length;
		myCaveNoise = new SimplexNoise[cNoise];
		for (int i = 0; i < cNoise; i++) 	myCaveNoise[i] = new SimplexNoise(staticSeed + CaveSeedsPlus[i]);
		
		int mNoise = MineSeedsPlus.Length;
		myMineNoise = new SimplexNoise[mNoise];
		for (int i = 0; i < mNoise; i++) 	myMineNoise[i] = new SimplexNoise(staticSeed + MineSeedsPlus[i]);
		
		if (s_noiseHeight == 512){
			MineStartHeightList =new int[] {130, 130, 120, 110, 100, 90, 80, 70,70,60,50,40}; 
			HeightOffsetTer = 64;
		}
		else
		{
			MineStartHeightList = new int[] { 28,28,25,25,20,18,18,15,15,11,8,6};
			HeightOffsetTer = 0;
		}
		
		int rNoise = RiverSeedsPlus.Length;
		myRiverNoise = new SimplexNoise[rNoise];
		for (int i = 0; i < rNoise; i++) 	myRiverNoise[i] = new SimplexNoise(staticSeed + RiverSeedsPlus[i]);
		
		int bNoise = BiomaSeedsPlus.Length;
		myBiomaNoise = new SimplexNoise[bNoise];
		for (int i = 0; i < bNoise; i++) 	myBiomaNoise[i] = new SimplexNoise(staticSeed + BiomaSeedsPlus[i]);
		
		isTownTile = false;
		dTileNoiseBuf = new double[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE][];
		fTileHeightBuf = new float[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE][];
		fTileGradTanBuf = new float[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE][];
		tileMapType = new RandomMapType[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE][];
		for (int vz = 0; vz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; vz++)
		{
			dTileNoiseBuf[vz] = new double[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE];
			fTileHeightBuf[vz] = new float[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE];
			fTileGradTanBuf[vz] = new float[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE];
			tileMapType[vz] = new RandomMapType[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE];
		}
		
		InitPlanetParam();
		InitDensityClamp();
		InitNoiseHeight();
		
		LoadPaintConfig();
	}
	
	public static void InitPlanetParam()
	{
//		System.Random randomSeed = new System.Random(RandomMapConfig.RandSeed);
//		MapTypeOffset = randomSeed.Next(7, 10);//7~9
//		
//		//random waterHeight
//		Debug.Log("<color=red>" + "Test: " + RandomMapConfig.ScenceClimate + "</color>");
//		Debug.Log("<color=red>" + "Test: waterHeight:" + waterHeight + "</color>");
//		Debug.Log("<color=red>" + "Test: c_fWaterLvl:" + VFVoxelWater.c_fWaterLvl + "</color>");
//		//random detail
//		float value = (float)randomSeed.NextDouble();
//		angleScale = (angleScaleEnd - angleScaleStart) * value + angleScaleStart;
//		angleStart = randomSeed.Next(10000);
		System.Random randomSeed = new System.Random(RandomMapConfig.MineGenSeed);
		mineGenChanceFactor = (float)randomSeed.NextDouble()*1.2f+1;
		//Debug.LogError("mineFactor:"+mineGenChanceFactor);
	}
	public static void LoadPaintConfig()
	{
		BiomaDistList= VATownGenerator.Instance.InitBiomaPos();
		noTownStartPos = BiomaDistList[(int)RandomMapConfig.RandomMapID-1].posList[0];
		paintConfig = new VoxelPaintXMLParser();
		paintConfig.LoadXMLInResources("RandomMapXML/tmpPaintVxMat", "RandomMapXML/", 79);
		regionArray = paintConfig.prms.RegionDescArrayValues;

		SetTerrainParam();
		lock (s_dicTreeInfoList)			s_dicTreeInfoList = new Dictionary<IntVector2, List<TreeInfo>>();
		lock (s_dicGrassInstList)			s_dicGrassInstList = new Dictionary<IntVector2, List<VoxelGrassInstance>>();
	}
	
	private static void InitDensityClamp()
	{
		fTerDensityClamp = new float[s_noiseHeight];
		fTerDensityClampBase = new float[s_noiseHeight];
		
		float topValue =2.3f;
		float bottomValue = 0;
		for (int vy = 1; vy < s_noiseHeight; vy++)
		{
			if (vy < s_seaDepth - 2)
			{
				fTerDensityClampBase[vy] = 0;
			}else {
				float result00 = GetDensityClampValue(s_noiseHeight-1,s_seaDepth-2,topValue,bottomValue,vy);
				fTerDensityClampBase[vy]= result00;
			}
		}
	}
	
	private static float GetDensityClampValue(float top,float bottom,float topValue,float bottomValue,float vy){
		float length = top-bottom;
		float nowPercent = (vy-bottom)/length;
		float actualValue = 1-nowPercent;
		float aResult = Mathf.Asin(actualValue);
		float fResult =(Mathf.PI/2-aResult)/(Mathf.PI/2)*(topValue-bottomValue)+bottomValue;
		return fResult;
	}
	
	//for generation
	private void ReGenDensityClamp(int x, int z)
	{	
		PlainThickness = HASMid;
		//		float HASFactor = (float)myNoise[HASFilterIndex].Noise(x * s_detailScale * HASFilterFrequency, z * s_detailScale * HASFilterFrequency);
		////		float HASFactor2 = (float)myNoise[HASFilterIndex].Noise(x * s_detailScale * HAS2FilterFrequency, z * s_detailScale * HAS2FilterFrequency);
		//		float factorResult = GetHASParamFromFactor(HASFactor);
		//		if(factorResult>=0){
		//			PlainThickness = factorResult* (HASMax - HASMid) + HASMid;
		//		}else {
		//			PlainThickness = (1+factorResult)*(HASMid-HASMin)+ HASMin;
		//		}
		//		if(HASFactor2>=0){
		//			PlainThickness += HASFactor2*HAS2Max;
		//		}else{
		//			PlainThickness += -HASFactor2*HAS2Min;
		//		}
		//		PlainThickness = Mathf.Clamp(PlainThickness,HASMin,HASMax);
		
		float HASFactor = ((float)myNoise[DensityClampFilter01].Noise(x * s_detailScale * HASFilterFrequency, z * s_detailScale * HASFilterFrequency)+1)*0.5f;
		float HASFactor2 = ((float)myNoise[DensityClampFilter02].Noise(x * s_detailScale * HAS2FilterFrequency, z * s_detailScale * HAS2FilterFrequency)+1)*0.5f;
		HASEnd = PlainMax(PlainThickness);
		HASEnd2 = PlainMax(PlainThickness/2);

		//test
		float result01;
		float result02;
		//float result03;
		fTerDensityClamp[0] = 0;
		for (int vy = 1; vy < s_noiseHeight; vy++)
		{
			if(vy<s_seaDepth-2){
				fTerDensityClamp[vy] = fTerDensityClampBase[vy];
			}else if(vy<=HASEnd2)
			{
				result01=GetDensityClampValue(HASEnd,s_seaDepth-2,DensityClampPlainTopValue,0,vy);
				result02=GetDensityClampValue(HASEnd2,s_seaDepth-2,DensityClampPlainTopValue,0,vy);
				fTerDensityClamp[vy] = result01*HASFactor2+result02*(1-HASFactor2);
			}else if(vy<=HASEnd){
				result01=GetDensityClampValue(HASEnd,s_seaDepth-2,DensityClampPlainTopValue,0,vy);
				result02=GetDensityClampValue(s_noiseHeight-1,HASEnd2,DensityClampAllTopValue,DensityClampPlainTopValue,vy);
				fTerDensityClamp[vy] = result01*HASFactor2+result02*(1-HASFactor2);
//				result02=GetLinerValue(vy,s_seaDepth-2,HASEnd,0,DensityClampPlainTopValue);
//				fTerDensityClamp[vy] = result02*DensityClampLinerBlendSeaSide+ fTerDensityClamp[vy]*(1-DensityClampLinerBlendSeaSide);
			}else if(vy > HASEnd){
				float result00= GetLinerValue(vy,HASEnd,s_noiseHeight-1,DensityClampPlainTopValue,DensityClampLinerTopValue);
				result01 = GetDensityClampValue(s_noiseHeight-1,HASEnd,DensityClampAllTopValue,DensityClampPlainTopValue,vy);
				fTerDensityClamp[vy] = result00*DensityClampLinerBlend+ result01*(1-DensityClampLinerBlend);
				result02 = GetDensityClampValue(s_noiseHeight-1,HASEnd2,DensityClampAllTopValue,DensityClampPlainTopValue,vy);
				fTerDensityClamp[vy] = fTerDensityClamp[vy]*HASFactor2+result02*(1-HASFactor2);
			}
			fTerDensityClamp[vy] =fTerDensityClamp[vy]*(1-HASFactor*DensityClampBaseBlendFactor)+ fTerDensityClampBase[vy]*HASFactor*DensityClampBaseBlendFactor;
		}
	}
	private static float GetLinerValue(float vy,float bottomX,float topX,float bottomValue,float topValue){
		float result=0;
		result = bottomValue+(vy-bottomX)/(topX-bottomX)*(topValue-bottomValue);
		return result;
	}
	//for interface
	private static void ReGenDensityClampStatic(IntVector2 worldXZ, out float[] fTerDensityClamp)
	{
		fTerDensityClamp = new float[s_noiseHeight];
		
		float HASFactor = ((float)myNoise[DensityClampFilter01].Noise(worldXZ.x * s_detailScale * HASFilterFrequency, worldXZ.y * s_detailScale * HASFilterFrequency)+1)*0.5f;
		float HASFactor2 = ((float)myNoise[DensityClampFilter02].Noise(worldXZ.x * s_detailScale * HAS2FilterFrequency, worldXZ.y * s_detailScale * HAS2FilterFrequency)+1)*0.5f;

		float HASEnd = PlainMax(HASMid);
		float HASEnd2 = PlainMax (HASMid/2);
		//test
		float result01;
		float result02;
		for (int vy = 1; vy < s_noiseHeight; vy++)
		{
			if(vy<s_seaDepth-2)
			{
				fTerDensityClamp[vy] = fTerDensityClampBase[vy];
			}else if(vy<=HASEnd2)
			{
				result01=GetDensityClampValue(HASEnd,s_seaDepth-2,DensityClampPlainTopValue,0,vy);
				result02=GetDensityClampValue(HASEnd2,s_seaDepth-2,DensityClampPlainTopValue,0,vy);
				fTerDensityClamp[vy] = result01*HASFactor2+result02*(1-HASFactor2);
			}else if(vy<=HASEnd){
				result01=GetDensityClampValue(HASEnd,s_seaDepth-2,DensityClampPlainTopValue,0,vy);
				result02=GetDensityClampValue(s_noiseHeight-1,HASEnd2,DensityClampAllTopValue,DensityClampPlainTopValue,vy);
				fTerDensityClamp[vy] = result01*HASFactor2+result02*(1-HASFactor2);
				//				result02=GetLinerValue(vy,s_seaDepth-2,HASEnd,0,DensityClampPlainTopValue);
				//				fTerDensityClamp[vy] = result02*DensityClampLinerBlendSeaSide+ fTerDensityClamp[vy]*(1-DensityClampLinerBlendSeaSide);
			}else if(vy > HASEnd){
				float result00= GetLinerValue(vy,HASEnd,s_noiseHeight-1,DensityClampPlainTopValue,DensityClampLinerTopValue);
				result01 = GetDensityClampValue(s_noiseHeight-1,HASEnd,DensityClampAllTopValue,DensityClampPlainTopValue,vy);
				fTerDensityClamp[vy] = result00*DensityClampLinerBlend+ result01*(1-DensityClampLinerBlend);
				result02 = GetDensityClampValue(s_noiseHeight-1,HASEnd2,DensityClampAllTopValue,DensityClampPlainTopValue,vy);
				fTerDensityClamp[vy] = fTerDensityClamp[vy]*HASFactor2+result02*(1-HASFactor2);
			}
			fTerDensityClamp[vy] =fTerDensityClamp[vy]*(1-HASFactor*DensityClampBaseBlendFactor)+ fTerDensityClampBase[vy]*HASFactor*DensityClampBaseBlendFactor;
		}
	}
	
	private static void InitNoiseHeight()
	{
		fTerNoiseHeight = new float[s_noiseHeight];
		float scaledY = (s_noiseHeight - 2) * HeightScale;	//(NoiseHeight-1-vy)*HeightScale;
		for (int vy = 1; vy < s_noiseHeight; vy++)
		{
			fTerNoiseHeight[vy] = scaledY;
			scaledY -= vy >= HeightScalePivot ? HeightScale_1 : HeightScale;
		}
	}
	
	#if !NOISE_3D //simple version
	private void GenerateTileData(int x, int z, VFTerTile terTile)
	{	
		terTile.tileX = x;
		terTile.tileZ = z;
		int vxStart = (x<<VoxelTerrainConstants._shift) - VoxelTerrainConstants._numVoxelsPrefix;
		int vzStart = (z<<VoxelTerrainConstants._shift) - VoxelTerrainConstants._numVoxelsPrefix;
		float[][] hData = new float[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE][];
		for(int hz = 0; hz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; hz++)
		{
			float[] hDataSub = new float[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE];
			for(int hx = 0; hx < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; hx++)
			{
				hDataSub[hx] = (float)((myNoise[0].Noise2DFBM((hx+vxStart) * DetailScale, (hz+vzStart) * DetailScale, 5)+1)*0.5f);
				hDataSub[hx] *= hDataSub[hx];	// make it steep
				hDataSub[hx] *= NoiseHeight;
			}
			hData[hz] = hDataSub;
		}
		float[][] nhData = new float[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE][];
		nhData[0] = new float[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE];
		nhData[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-1] = new float[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE];
		for(int nhz = 1; nhz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-1; nhz++)
		{
			float[] nhDataSub = new float[VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE];
			for(int nhx = 1; nhx < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-1; nhx++)
			{
				nhDataSub[nhx] = Math.Max(Math.Max(hData[nhz][nhx-1],hData[nhz][nhx+1]),Math.Max(hData[nhz-1][nhx],hData[nhz+1][nhx]));
			}
			nhData[nhz] = nhDataSub;
		}
		for(int vz = 0; vz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; vz++)
		{
			byte[][] yxVoxels = terTile.voxels[vz];
			for(int vx2 = 0, vx = 0; vx2 < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT; vx2+=VFVoxel.c_VTSize, vx++)
			{
				float h = hData[vz][vx];
				int ny = (int)(h+0.5f);
				int vy = 0;
				for(; vy < ny-1; vy++)
				{
					yxVoxels[vy][vx2] = 255;
					yxVoxels[vy][vx2+1] = 3;
				}
				byte type = paintConfig.GetVxMatByGradient(h, 
				                                           vx==0?h:hData[vz][vx-1], vx==VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-1?h:hData[vz][vx+1],
				                                           vz==0?h:hData[vz-1][vx], vz==VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-1?h:hData[vz+1][vx],0,0);
				if(vy < ny)
				{
					yxVoxels[vy][vx2] = 255;
					yxVoxels[vy][vx2+1] = type;	
					vy++;
				}
				
				float fDec = h - (int)h;
				byte volume = (byte)(fDec < 0.5f ? (128/(1-fDec)) : (256-128/fDec));
				yxVoxels[vy][vx2] = volume;
				yxVoxels[vy][vx2+1] = type;
				if(h+1 < nhData[vz][vx])
				{
					vy++;
					int nny = (int)(nhData[vz][vx] - h);
					byte deltaVol = (byte)(volume/(nny));
					byte vol = (byte)(volume - deltaVol);
					for(int nn = 0; nn < nny; nn++)
					{
						yxVoxels[vy][vx2] = vol;
						yxVoxels[vy][vx2+1] = type;
						vy++;
						vol -= deltaVol;
					}					
				}
			}
		}
	}
	#else
	private void FillTileDataWithNoise(VFTile tile)
	{
		//Profiler.BeginSample ("init");
		townFloorVoxelByte.Clear();
		bool isNewTownTile = false;
		List<VArtifactUnit> newTownList = new List<VArtifactUnit>();
		
		//long tick = System.DateTime.Now.Ticks;
		int x = tile.tileX;
		int z = tile.tileZ;
		int lod = tile.tileL;
		int[] voxelIndex = S_VoxelIndex[lod];
		_tmpTileIndex.x = x;
		_tmpTileIndex.y = z;
		
		//long findTownStartTick = System.DateTime.Now.Ticks;
		
		if(lod <= 2 && VArtifactTownManager.Instance != null){
			int ijEndIndex = 1 << lod;
			for(int i=0 ; i < ijEndIndex ; i++)
			{
				_tmpVec2.x = x+i;
				for(int j=0 ; j < ijEndIndex ; j++){
					_tmpVec2.y = z+j;
					if (VArtifactTownManager.Instance.TileContainsTown(_tmpVec2))
					{
						if(VArtifactTownManager.Instance.GetTileTown(_tmpVec2)!=null&&!VArtifactTownManager.Instance.GetTileTown(_tmpVec2).isEmpty){
							isNewTownTile = true;
							newTownList = VArtifactTownManager.Instance.OutputTownData(_tmpVec2);
							break;
						}
					}
				}
				if(isNewTownTile)
					break;
			}
		}
		//Profiler.EndSample ();
		
		//long findTownEndTick = System.DateTime.Now.Ticks;
		isTownTile = false;
		
		// Fill tile voxels, now y_data(only lod0) will be resampled to lodx in FillchunkDataLod
		float fWaterHeightDeci = waterHeight - Mathf.Floor(waterHeight);
		int nWaterHeight = (int)Mathf.Floor(waterHeight + 0.5f);
		byte volWaterHeight = (byte)(fWaterHeightDeci >= 0.5f ? (255 - 127.5f / fWaterHeightDeci) : (128 / (1 - fWaterHeightDeci)));
		int vxStart = (x << VoxelTerrainConstants._shift) - VoxelTerrainConstants._numVoxelsPrefix;//the x one before the first voxel in this tile 
		int vzStart = (z << VoxelTerrainConstants._shift) - VoxelTerrainConstants._numVoxelsPrefix;//the z one before the first voxel in this tile
		
		byte[] yTerraVoxels;
		byte[] yWaterVoxels;
		int maxHeight;
		RandomMapType mapType;
		//Profiler.BeginSample ("vz_vx");
		for (int vz = 0; vz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; vz++)
		{
			int worldZ= vzStart + (voxelIndex[vz] & 0x0ff) + ((voxelIndex[vz] >> 8) << VoxelTerrainConstants._shift);
			float scaledZ = worldZ * s_detailScale;
			
			for (int vx = 0; vx < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; vx++)
			{
				if(vz>1&&vz<VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-2
				   &&vx>1&&vx<VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-2){
					if(vz%3==2){
						if(vx%3==0)
							vx+=2;
					}else{
						vx=VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-2;
					}
				}
				yTerraVoxels = tile.terraVoxels[vz][vx];
				yWaterVoxels = tile.waterVoxels[vz][vx];
				Array.Clear(yTerraVoxels, 0, yTerraVoxels.Length);
				Array.Clear(yWaterVoxels, 0, yWaterVoxels.Length);
				
				int worldX = vxStart + (voxelIndex[vx] & 0x0ff) + ((voxelIndex[vx] >> 8) << VoxelTerrainConstants._shift);
				IntVector2 WorldXZ= new IntVector2 (worldX,worldZ);
				//gen DensityClamp
				ReGenDensityClamp(worldX, worldZ);
				float scaledX = worldX * s_detailScale;
				maxHeight = tile.tileH;//the height need to compute
				//bool isTownPoint = false;
				if (isNewTownTile && townAvailable&&lod<=0)
				{
					//1.random the town data
					_tmpTownIdList.Clear();
					_tmpTownIdList.Add(newTownList[0].vat.townId);
					for (int ntl = 1; ntl < newTownList.Count; ntl++)
					{
						if (!_tmpTownIdList.Contains(newTownList[ntl].vat.townId))
						{
							_tmpTownIdList.Add(newTownList[ntl].vat.townId);
						}
					}
					for (int til = 0; til < _tmpTownIdList.Count; til++)
					{
						VArtifactTownManager.Instance.RandomArtifactTown(_tmpTownIdList[til]);
					}
					//2.add artifactUnit to render
					for (int ntl = 0; ntl < newTownList.Count; ntl++)
					{
						if (!newTownList[ntl].isAddedToRender){
							//Debug.Log("VFDataRTGen AddedToRender: " + newTownList[ntl].vat.townId);
							VArtifactTownManager.Instance.ArtifactAddToRender(newTownList[ntl], _tmpTileIndex);
						}
					}
				}
//				float mapTypeDiff;
//				RandomMapType firstType;
//				RandomMapType secondType;
//				mapType = GetMapType(scaledX, scaledZ,out firstType,out secondType,out mapTypeDiff);
//				mapType = GetMapType(scaledX, scaledZ);
				
				//new
				//				float fNoise12D1ten = GetfNoise12D1ten(scaledX,scaledZ);
				//				float fNoise12D1tenRaw = fNoise12D1ten/50-1;
				//
				//				// Get TerType
				//				int nTerType = 0;
				//				float fTerType = fNoise12D1ten / 100;//[0,1]
				//				float continentBound = GetContinentValue(worldX, worldZ);
				//				//fTerType -= continentBound;
				//				fTerType = BlendContinentBound(fTerType,continentBound);
				//				if (fTerType < 0)
				//					fTerType = 0;
				//				float baseFTerType = fTerType;//for terrain type judge
				//				float bridgeValue;
				//				bool caveEnable = true;
				//				bool riverArea =false;
				//				bool lakeArea = false;
				//				float riverValue = GetRiverValue(worldX, worldZ, fTerType,ref caveEnable, ref lakeArea,out bridgeValue);
				//				if(riverValue<0.85f)
				//					riverArea = true;
				//				float fTerTypeRiver = fTerType * riverValue;
				//				float fTerTypeBridge = -1;
				//
				//				if (bridgeValue != -1)								fTerTypeBridge = fTerType * bridgeValue;
				//				
				//				fTerType = fTerTypeRiver;
				//				if (fTerType < 0)									fTerType = 0;
				//				if (fTerType > 1)									fTerType = 1;
				
				bool caveEnable = true;
				bool riverArea =false;
				bool lakeArea = false;
				float fTerTypeBridge;
				float riverValue;
				float bridgeValue;
				float bridge2dFactor;
				float[] terTypeInc;
				float fTerTypeRiver = GetFinalFterType(WorldXZ,out fTerTypeBridge,out caveEnable,out riverArea,out lakeArea,out riverValue,out bridgeValue,out bridge2dFactor,out mapType,out terTypeInc);
				tileMapType[vz][vx]=mapType;
				float fTerType = fTerTypeRiver;
				float fTerTypeOrigin = fTerTypeRiver/riverValue;
				float fTerNoise =(fTerType*2-1) * 0.05f;
				int nTerType;
				GetFTerTypeAndZnTerType(ref fTerType,out nTerType,terTypeInc);//,out znTerType[vx]);
				float fTerTypeFactor = fTerType;//fmin~fmax
				// Compute ter Noise/Smooth level based on TerType
				ComputeTerNoise(ref fTerNoise, nTerType, fTerType);
				
				float flatFactor = (float)myNoise[FlatIndex].Noise(scaledX * flatFrequency, scaledZ * flatFrequency);
				float flatParam = GetFlatParamFromFactor(flatFactor,WorldXZ.x,WorldXZ.y,mapType);
				#region compute terrain
				float maxHeightValue;
				float bottomVy = s_seaDepth + 1;
				//1. get start max Height
				//2. optimise max Height
				//3. compute surface voxel
				//bool isHill = IsHillTerrain(nTerType);
				//step1.maxheight
				#region new start
//				if(worldX>-2650 && worldX<-2640 && worldZ>=3800&&worldZ<3810)
//					Debug.Log("test point");
				if(!IsWaterTerrain(nTerType))
				{
					if(IsHillTerrain(nTerType)){
						maxHeightValue = s_noiseHeight;
						maxHeight = s_noiseHeight;
					}
					else if (!IsSeaSideTerrain(nTerType))
					{
							maxHeightValue = PlainTopHeight(PlainThickness) + (fTerTypeRiver - terTypeInc[PlainStartIndex]) / (terTypeInc[HillStartIndex] - terTypeInc[PlainStartIndex]) * (HillBottomHeight(PlainThickness) - PlainTopHeight(PlainThickness));
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
						maxHeightValue = PlainTopHeight(PlainThickness);
						maxHeight = PlainTopHeight(PlainThickness);
					}
					
					#region interpolation Compute
					//step2.optimise maxheight
					OptimiseMaxHeight(ref maxHeight, scaledX, scaledZ, flatParam, fTerType, nTerType, fTerNoise, bottomVy, fTerNoiseHeight, fTerDensityClamp, PlainThickness);
					
					//step3.set top, bottom
					int topvy0 = maxHeight - 1;
					int topvy2 = Mathf.FloorToInt(bottomVy+1);
					int xzJump=1;
					if(IsSeaSideTerrain(nTerType))
						xzJump = 2;
					
					//step4.genvoxelonly
					float fNoiseXZ = -10f;
					for (int vy = topvy0, vy2 = vy * VFVoxel.c_VTSize; vy > bottomVy; vy-=xzJump, vy2 -= VFVoxel.c_VTSize*xzJump)
					{
						if (GenTileVoxelOnly(scaledX, fTerNoiseHeight[vy], scaledZ, flatParam,
						                     fTerDensityClamp[vy], fTerType, nTerType, fTerNoise,
						                     ref yTerraVoxels[vy2], ref yTerraVoxels[vy2 + 1], PlainThickness))
						{
							topvy2 = vy;
							break;
						}
					}					
					//step5.genheight
					//Debug.Log ("compute hill:"+(topvy0-topvy2));
					for (int vy = topvy2 - 1, vy2 = vy * VFVoxel.c_VTSize; vy > 0; vy--, vy2 -= VFVoxel.c_VTSize)
					{
						GenTileVoxelWithHeightMap(scaledX, fTerNoiseHeight[vy], scaledZ, fTerNoise, ref fNoiseXZ, ref yTerraVoxels[vy2], ref yTerraVoxels[vy2 + 1], MountainTopCorrection, vy);
					}
					//2
					if(xzJump>1)
						for(int vy=topvy0-1,vy2=vy * VFVoxel.c_VTSize;vy>=topvy2;vy-=xzJump,vy2-=VFVoxel.c_VTSize*xzJump)
					{
						float upValuef = (float)yTerraVoxels[vy2+VFVoxel.c_VTSize];
						float downValuef = vy-1>bottomVy?(float)yTerraVoxels[vy2-VFVoxel.c_VTSize]:255f;
						yTerraVoxels[vy2]= (byte)(upValuef/2+downValuef/2);
						if(yTerraVoxels[vy2]>0)		yTerraVoxels[vy2+1] = BLOCK_STONE;
						else						yTerraVoxels[vy2+1]= BLOCK_AIR;
					}
					//step6.complete top
					if (maxHeightValue - maxHeight < 1 && yTerraVoxels[(maxHeight - 1) * VFVoxel.c_VTSize] > 175 && yTerraVoxels[(maxHeight - 1) * VFVoxel.c_VTSize + 1] == BLOCK_STONE)
					{
						yTerraVoxels[maxHeight * VFVoxel.c_VTSize] = (byte)((maxHeightValue - maxHeight) * 255);
						yTerraVoxels[maxHeight * VFVoxel.c_VTSize + 1] = BLOCK_STONE;
						maxHeight++;
					}
					if(xzJump !=1){
						if (topvy2 == Mathf.FloorToInt(bottomVy) && (bottomVy - topvy2) > 0 && topvy2 > 0)
						{
							yTerraVoxels[topvy2 * VFVoxel.c_VTSize] = (byte)((bottomVy - topvy2) * 255);
							yTerraVoxels[topvy2 * VFVoxel.c_VTSize + 1] = BLOCK_STONE;
							yTerraVoxels[(topvy2 - 1) * VFVoxel.c_VTSize] = 255;
						}
					}
					#endregion
				}else{
					float zeroPoint = terTypeInc[SeasideStartIndex] / 16;
					float onePoint =  terTypeInc[SeasideStartIndex]*17/16;//(terTypeInc[PlainStartIndex] - terTypeInc[SeasideStartIndex]) / 3 + terTypeInc[SeasideStartIndex];
					float floatHeight;
					if (fTerTypeRiver < zeroPoint)
					{
						floatHeight = 0;
					}
					else
					{
						float sinFactor = (Mathf.Sin(((fTerTypeRiver - zeroPoint) / (onePoint - zeroPoint) - 0.5f) * Mathf.PI) + 1) / 2;
						floatHeight = sinFactor * (s_seaDepth + 1);
					}
					int vyTop = Mathf.FloorToInt(floatHeight);
					maxHeight = vyTop + 1;
					
					byte topVolume = (byte)Mathf.RoundToInt((floatHeight - vyTop) * 255);
					yTerraVoxels[(vyTop + 1) * VFVoxel.c_VTSize] = topVolume;
					yTerraVoxels[(vyTop + 1) * VFVoxel.c_VTSize + 1] = BLOCK_STONE;
					
					//byte stuffVolume = topVolume;
					for (int vy = vyTop, vy2 = vy * VFVoxel.c_VTSize; vy > 0; vy--, vy2 -= VFVoxel.c_VTSize)
					{
						yTerraVoxels[vy2] = (byte)255;
						yTerraVoxels[vy2 + 1] = BLOCK_STONE;
					}
				}
				#endregion
				
				#endregion
				
				bool bridgeAvail=true;// riverWidthNow < (riverWidth1 + riverWidth100) * 0.7f;
				#region bridge
				if (bridgeAvail && fTerTypeBridge != -1)
				{
					Array.Clear(_tmpBridgeVoxels, 0, _tmpBridgeVoxels.Length);
					fTerType = fTerTypeBridge;
					if (fTerType < 0)								fTerType = 0;
					
					fTerNoise = (fTerTypeBridge*2-1) * 0.05f;
					GetFTerTypeAndZnTerType(ref fTerType,out nTerType,terTypeInc);
					// Compute ter Noise/Smooth level based on TerType
					ComputeTerNoise(ref fTerNoise, nTerType, fTerType);
					
					#region  new Bridge
					if(!IsWaterTerrain(nTerType))
					{
						if(IsHillTerrain(nTerType)){
							maxHeightValue = s_noiseHeight;
							maxHeight = s_noiseHeight;
						}
						else if (!IsSeaSideTerrain(nTerType))
						{
								maxHeightValue = PlainTopHeight(PlainThickness) + (fTerTypeBridge - terTypeInc[PlainStartIndex]) / (terTypeInc[HillStartIndex] - terTypeInc[PlainStartIndex]) * (HillBottomHeight(PlainThickness) - PlainTopHeight(PlainThickness));
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
							maxHeightValue = PlainTopHeight(PlainThickness);
							maxHeight = PlainTopHeight(PlainThickness);
						}
						
						//step2.optimise maxheight
						OptimiseMaxHeight(ref maxHeight, scaledX, scaledZ, flatParam, fTerType, nTerType, fTerNoise, bottomVy, fTerNoiseHeight, fTerDensityClamp, PlainThickness);
						
						//step3.set top, bottom
						int topvy0 = maxHeight - 1;
						int topvy2 = Mathf.FloorToInt(bottomVy+1);
						int xzJump=1;
						if(IsSeaSideTerrain(nTerType))
							xzJump = 2;
						
						//step4.genvoxelonly
						float fNoiseXZ = -10f;
						for (int vy = topvy0, vy2 = vy * VFVoxel.c_VTSize; vy > bottomVy; vy-=xzJump, vy2 -= VFVoxel.c_VTSize*xzJump)
						{
							if (GenTileVoxelOnly(scaledX, fTerNoiseHeight[vy], scaledZ, flatParam,
							                     fTerDensityClamp[vy], fTerType, nTerType, fTerNoise,
							                     ref _tmpBridgeVoxels[vy2], ref _tmpBridgeVoxels[vy2 + 1], PlainThickness))
							{
								topvy2 = vy;
								break;
							}
						}
						//--interpolation
						//step5.genheight
						//Debug.Log ("compute hill:"+(topvy0-topvy2));
						for (int vy = topvy2 - 1, vy2 = vy * VFVoxel.c_VTSize; vy > 0; vy--, vy2 -= VFVoxel.c_VTSize)
						{
							GenTileVoxelWithHeightMap(scaledX, fTerNoiseHeight[vy], scaledZ, fTerNoise, ref fNoiseXZ, ref _tmpBridgeVoxels[vy2], ref _tmpBridgeVoxels[vy2 + 1], MountainTopCorrection, vy);
						}
						//2
						if(xzJump>1)
							for(int vy=topvy0-1,vy2=vy * VFVoxel.c_VTSize;vy>=topvy2;vy-=xzJump,vy2-=VFVoxel.c_VTSize*xzJump)
						{
							float upValuef = (float)_tmpBridgeVoxels[vy2+VFVoxel.c_VTSize];
							float downValuef = vy-1>bottomVy?(float)_tmpBridgeVoxels[vy2-VFVoxel.c_VTSize]:255f;
							_tmpBridgeVoxels[vy2]= (byte)(upValuef/2+downValuef/2);
							if(_tmpBridgeVoxels[vy2]>0)					_tmpBridgeVoxels[vy2+1] = BLOCK_STONE;
							else									_tmpBridgeVoxels[vy2+1]= BLOCK_AIR;
						}
						//step6.complete top
						if (maxHeightValue - maxHeight < 1 && _tmpBridgeVoxels[(maxHeight - 1) * VFVoxel.c_VTSize] > 175 && _tmpBridgeVoxels[(maxHeight - 1) * VFVoxel.c_VTSize + 1] == BLOCK_STONE)
						{
							_tmpBridgeVoxels[maxHeight * VFVoxel.c_VTSize] = (byte)((maxHeightValue - maxHeight) * 255);
							_tmpBridgeVoxels[maxHeight * VFVoxel.c_VTSize + 1] = BLOCK_STONE;
							maxHeight++;
						}
						
						if(xzJump !=1){
							if (topvy2 == Mathf.FloorToInt(bottomVy) && (bottomVy - topvy2) > 0 && topvy2 > 0)
							{
								_tmpBridgeVoxels[topvy2 * VFVoxel.c_VTSize] = (byte)((bottomVy - topvy2) * 255);
								_tmpBridgeVoxels[topvy2 * VFVoxel.c_VTSize + 1] = BLOCK_STONE;
								_tmpBridgeVoxels[(topvy2 - 1) * VFVoxel.c_VTSize] = 255;
							}
						}
					}else{
						float zeroPoint = terTypeInc[SeasideStartIndex] / 16;
						float onePoint =  terTypeInc[SeasideStartIndex]*17/16;//(terTypeInc[PlainStartIndex] - terTypeInc[SeasideStartIndex]) / 3 + terTypeInc[SeasideStartIndex];
						float floatHeight;
						if (fTerTypeBridge < zeroPoint)
						{
							floatHeight = 0;
						}
						else
						{
							float sinFactor = (Mathf.Sin(((fTerTypeBridge - zeroPoint) / (onePoint - zeroPoint) - 0.5f) * Mathf.PI) + 1) / 2;
							floatHeight = sinFactor * (s_seaDepth + 1);
						}
						int vyTop = Mathf.FloorToInt(floatHeight);
						maxHeight = vyTop + 1;
						
						byte topVolume = (byte)Mathf.RoundToInt((floatHeight - vyTop) * 255);
						_tmpBridgeVoxels[(vyTop + 1) * VFVoxel.c_VTSize] = topVolume;
						_tmpBridgeVoxels[(vyTop + 1) * VFVoxel.c_VTSize + 1] = BLOCK_STONE;
						
						//byte stuffVolume = topVolume;
						for (int vy = vyTop, vy2 = vy * VFVoxel.c_VTSize; vy > 0; vy--, vy2 -= VFVoxel.c_VTSize)
						{
							_tmpBridgeVoxels[vy2] = (byte)255;
							_tmpBridgeVoxels[vy2 + 1] = BLOCK_STONE;
						}
					}
					#endregion 
					
					float bridgeParam = 1-bridge2dFactor;
					//					if(bridgeParam>0.5f)
					//						bridgeParam = 1;
					//					else{
					//						bridgeParam = 1-(0.5f-bridgeParam)/(1-0.5f);
					//					}
					int heightStartMax = 40;
					int heightStart;
					
					//dig hole
					if(fTerTypeBridge<=terTypeInc[PlainStartIndex]){
						heightStart = Mathf.RoundToInt(heightStartMax *riverValue*riverValue*(1 - fTerTypeOrigin * 0.5f) * Mathf.Pow(bridgeParam, 0.5f));
					}
						else if (fTerTypeBridge <= terTypeInc[HillStartIndex]){
						heightStart = Mathf.RoundToInt(heightStartMax *riverValue*riverValue*(1 - fTerTypeOrigin * 0.5f) * Mathf.Pow(bridgeParam, 0.5f));
					} else {
						heightStart = Mathf.RoundToInt(heightStartMax *riverValue*riverValue*(1 - fTerTypeOrigin * 0.5f) * Mathf.Pow(bridgeParam, 0.5f));
					}
					int bridgeHeight = 0;
					if (heightStart >= 0)
					{
						for (int vy = maxHeight; vy > 0; vy--)
						{
							int vy2 = vy * VFVoxel.c_VTSize;
							if (_tmpBridgeVoxels[vy2] > 127.5)
							{
								bridgeHeight = vy;
								break;
							}
						}
						//int riverBottomHeight = 0;
						int maxY = Math.Min(maxHeight, yTerraVoxels.Length >> VFVoxel.c_shift) - 1;
						for (int vy = maxY, vy2 = vy * VFVoxel.c_VTSize; vy > 0; vy--, vy2 -= VFVoxel.c_VTSize)
						{
							if (yTerraVoxels[vy2] > 127.5)
							{
								//riverBottomHeight = vy;
								break;
							}
						}
						
						float bridgeStandard;
						if(fTerTypeOrigin>terTypeInc[PlainStartIndex])
						{
							bridgeStandard = PlainMax(PlainThickness/2);//+32*(fTerTypeOrigin-terTypeInc[PlainStartIndex]);
						}else{
							bridgeStandard = (PlainThickness/2)*(fTerTypeOrigin-terTypeInc[SeasideStartIndex])/(terTypeInc[PlainStartIndex]-terTypeInc[SeasideStartIndex])+s_seaDepth;
						}
						
						if (bridgeHeight != 0)
						{
							//bridge part
							for (int vy = maxY, vy2 = vy * VFVoxel.c_VTSize; vy > bridgeStandard - heightStart ; vy--, vy2 -= VFVoxel.c_VTSize)
							{
								if (vy >= 0)
								{
									yTerraVoxels[vy2] 			= _tmpBridgeVoxels[vy2];
									yTerraVoxels[vy2 + 1] 		= _tmpBridgeVoxels[vy2+1];
								}
							}
							//							if (bridgeHeight - heightStart > riverBottomHeight)
							//							{
							//								//transition part
							//								maxY = Math.Min(bridgeHeight - heightStart, (yTerraVoxels.Length >> VFVoxel.c_shift) - 1);
							//								for (int vy = maxY, vy2 = vy * VFVoxel.c_VTSize; vy > riverBottomHeight; vy--, vy2 -= VFVoxel.c_VTSize)
							//								{
							//									float bridgeScale = (vy - riverBottomHeight) / (bridgeHeight - heightStart - riverBottomHeight);
							//									yTerraVoxels[vy2] = (byte)(_tmpBridgeVoxels[vy2] * bridgeScale + yTerraVoxels[vy2] * (1 - bridgeScale));
							//								}
							//							}
						}
					}
				}
				#endregion
				// Fill solid bottom
				yTerraVoxels[0] = SolidBottomVoxel[0];
				yTerraVoxels[1] = SolidBottomVoxel[1];
				
				//gen sea
				FillWaterVoxels(yWaterVoxels, nWaterHeight, yTerraVoxels, maxHeight, volWaterHeight,fTerType,riverArea,lakeArea);
				
				#region gen cave
				//plain cave
				if(caveEnable){
					float caveNoiseXZ = (float)myCaveNoise[0].Noise(scaledX * CaveXZFrequency, scaledZ * CaveXZFrequency);
					float absCaveNoiseXZ = Mathf.Abs(caveNoiseXZ);
					float caveThresholdChangeFactor = (float)myCaveNoise[3].Noise(scaledX * CaveXZFrequency * 3, scaledZ * CaveXZFrequency * 3);
					float caveChangeParam = (caveThresholdChangeFactor + 1) * 0.75f;
					float ThresholdValue = (CaveXZThreshold + CaveThresholdTerValue * (c_fTerTypeMax - fTerTypeFactor)) * caveChangeParam;
					if (absCaveNoiseXZ < ThresholdValue)
					{
						float caveHeightNoise = (float)myCaveNoise[1].Noise(scaledX * CaveHeightFrequency, scaledZ * CaveHeightFrequency);
						float CaveHeightMaxValue = CaveHeightMax(PlainThickness) + CaveHeightTerValue * (fTerTypeFactor);
						float caveHeight = (caveHeightNoise + 1) * 0.5f * (CaveHeightMaxValue - CaveHeightMin) + CaveHeightMin;
						float caveThicknessFactor = (float)myCaveNoise[2].Noise(scaledX * CaveThicknessFrequency, scaledZ * CaveThicknessFrequency);
						float caveThicknessMaxValue = CaveThicknessMax + CaveThicknessTerValue * (c_fTerTypeMax - fTerTypeFactor);
						float caveThickness = GetThicknessParamFromFactor(caveThicknessFactor) * caveThicknessMaxValue;
						float thresholdDecrease;
						float thresholdFactor = (ThresholdValue - absCaveNoiseXZ) / CaveXZThreshold;
						if (thresholdFactor > 0.3f)					thresholdDecrease = 1;
						else										thresholdDecrease = Mathf.Pow(thresholdFactor / 0.3f, 0.5f);
						
						caveThickness *= thresholdDecrease;
						if (caveThickness > 0)
						{
							GenCave(caveHeight, caveThickness, yTerraVoxels);
						}
					}
				}
				#endregion
				
				#region gen mine
				//gen mine
				float mineNoise = (float)myMineNoise[0].Noise(scaledX * MineFrequency, scaledZ * MineFrequency);
				float absMineNoise = Mathf.Abs(mineNoise);
				if (absMineNoise > 1 - MineChance)
				{
					float mineChanceFactor = (absMineNoise - (1 - MineChance)) / MineChance;
					GeneralMineral(yTerraVoxels, scaledX, fTerNoiseHeight, scaledZ, mineChanceFactor,fTerType,mapType);
				}
				#endregion
				
				// Fill solid bottom
				yTerraVoxels[0] = SolidBottomVoxel[0];
				yTerraVoxels[1] = SolidBottomVoxel[1];
				#if USE_HEIGHT_BUFF
				// save height
				
				tile.nTerraYLens[vz][vx] = (maxHeight+1)*VFVoxel.c_VTSize;
				tile.nWaterYLens[vz][vx] = (nWaterHeight+1)*VFVoxel.c_VTSize;
				#endif
			}
		}
		//Profiler.EndSample ();
		
		#region interpolation
		//long interpolationTick = System.DateTime.Now.Ticks;
		byte[][] zDownArray;
		byte[][] zUpArray;
		byte[][] xTerraArray;
		byte[][] zWaterDownArray;
		byte[][] zWaterUpArray;
		byte[][] xWaterArray;
		byte[] leftArray;
		byte[] rightArray;
		byte[] downArray;
		byte[] upArray;
		int leftLength;
		int rightLength;
		int downLength;
		int leftValue;
		int rightValue;
		int downValue;
		int upValue;
		byte leftType;
		byte downType;
		byte[] leftWaterArray;
		byte[] rightWaterArray;
		byte[] downWaterArray;
		byte[] upWaterArray;
		int leftWaterLength;
		int rightWaterLength;
		int downWaterLength;
		int upWaterLength;
		int maxWaterHeight;
		int upLength;
		//Profiler.BeginSample("interp_0");
		for(int vz = 2;vz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-2;vz+=3)
		{
			int worldZ = vzStart + (voxelIndex[vz] & 0x0ff) + ((voxelIndex[vz] >> 8) << VoxelTerrainConstants._shift);
			float scaledZ = worldZ*s_detailScale;
			xTerraArray = tile.terraVoxels[vz];
			xWaterArray = tile.waterVoxels[vz];			
			for(int vx= 3;vx< VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-2;vx+=3)
			{
				int worldX =vxStart + (voxelIndex[vx] & 0x0ff) + ((voxelIndex[vx] >> 8) << VoxelTerrainConstants._shift);
				float scaledX = worldX*s_detailScale;
				
				leftArray = xTerraArray[vx-1];
				rightArray = xTerraArray[vx+2];
				#if USE_HEIGHT_BUFF
				leftLength = tile.nTerraYLens[vz][vx-1];//leftArray.Length;
				rightLength = tile.nTerraYLens[vz][vx+2];//rightArray.Length;
				maxHeight = tile.nTerraYLens[vz][vx] = tile.nTerraYLens[vz][vx+1] = leftLength;
				if(rightLength>maxHeight)
					maxHeight = tile.nTerraYLens[vz][vx] = tile.nTerraYLens[vz][vx+1] = rightLength;
				#else
				leftLength = leftArray.Length;
				rightLength = rightArray.Length;
				maxHeight = leftLength;
				if(rightLength>maxHeight)
					maxHeight = rightLength;
				#endif
				
				Array.Clear(_tmpInterTerraVoxels1, 0, _tmpInterTerraVoxels1.Length);
				Array.Clear(_tmpInterTerraVoxels2, 0, _tmpInterTerraVoxels2.Length);
				for(int i=0;i<maxHeight;i+=VFVoxel.c_VTSize){
					leftValue = leftLength>i?(int)leftArray[i]:0;
					rightValue = rightLength>i?(int)rightArray[i]:0;
					_tmpInterTerraVoxels2[i] = (byte)((leftValue*2+rightValue*4)/6);
					_tmpInterTerraVoxels1[i] = (byte)((leftValue*4+rightValue*2)/6);
					leftType = leftValue==0?BLOCK_AIR:leftArray[i+1];
					
					_tmpInterTerraVoxels1[i+1] = leftType;
					_tmpInterTerraVoxels2[i+1] = leftType;
				}
				Array.Copy(_tmpInterTerraVoxels1, tile.terraVoxels[vz][vx+0], _tmpInterTerraVoxels1.Length);
				Array.Copy(_tmpInterTerraVoxels2, tile.terraVoxels[vz][vx+1], _tmpInterTerraVoxels2.Length);
				
				leftWaterArray = xWaterArray[vx-1];
				rightWaterArray = xWaterArray[vx+2];
				#if USE_HEIGHT_BUFF
				leftWaterLength = tile.nWaterYLens[vz][vx-1];//leftWaterArray.Length;
				rightWaterLength = tile.nWaterYLens[vz][vx+2];//rightWaterArray.Length;
				maxWaterHeight = tile.nWaterYLens[vz][vx] = tile.nWaterYLens[vz][vx+1] = leftWaterLength;
				if(rightWaterLength>maxWaterHeight)
					maxWaterHeight = tile.nWaterYLens[vz][vx] = tile.nWaterYLens[vz][vx+1] = rightWaterLength;
				#else
				leftWaterLength = leftWaterArray.Length;
				rightWaterLength = rightWaterArray.Length;
				maxWaterHeight = leftWaterLength;
				if(rightWaterLength>maxWaterHeight)
					maxWaterHeight = rightWaterLength;
				#endif
				
				Array.Clear(_tmpInterWaterVoxels1, 0, _tmpInterWaterVoxels1.Length);
				Array.Clear(_tmpInterWaterVoxels2, 0, _tmpInterWaterVoxels2.Length);
				for(int i=0;i<maxWaterHeight;i+=VFVoxel.c_VTSize){
					leftValue = leftWaterLength>i?(int)leftWaterArray[i]:0;
					rightValue = rightWaterLength>i?(int)rightWaterArray[i]:0;
					_tmpInterWaterVoxels2[i] = (byte)((leftValue*2+rightValue*4)/6);
					_tmpInterWaterVoxels1[i] = (byte)((leftValue*4+rightValue*2)/6);
					leftType = leftValue==0?BLOCK_AIR:leftWaterArray[i+1];
					_tmpInterWaterVoxels1[i+1] = leftType;
					_tmpInterWaterVoxels2[i+1] = leftType;
				}
				Array.Copy(_tmpInterWaterVoxels1, tile.waterVoxels[vz][vx+0], _tmpInterWaterVoxels1.Length);
				Array.Copy(_tmpInterWaterVoxels2, tile.waterVoxels[vz][vx+1], _tmpInterWaterVoxels2.Length);
				
				mapType = GetMapType(scaledX, scaledZ);
				tileMapType[vz][vx] = mapType;
				tileMapType[vz][vx+1] = mapType;
			}
		}
		//Profiler.EndSample();
		
		//Profiler.BeginSample("interp_1");
		for(int vz = 3;vz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-2;vz+=3)
		{
			int worldZ = vzStart + (voxelIndex[vz] & 0x0ff) + ((voxelIndex[vz] >> 8) << VoxelTerrainConstants._shift);
			float scaledZ = worldZ*s_detailScale;
			zDownArray = tile.terraVoxels[vz-1];
			zUpArray = tile.terraVoxels[vz+2];
			zWaterDownArray = tile.waterVoxels[vz-1];
			zWaterUpArray = tile.waterVoxels[vz+2];
			
			for(int vx= 2;vx< VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE-2;vx+=1)
			{
				int worldX =vxStart + (voxelIndex[vx] & 0x0ff) + ((voxelIndex[vx] >> 8) << VoxelTerrainConstants._shift);
				float scaledX = worldX*s_detailScale;
				
				downArray = zDownArray[vx];
				upArray = zUpArray[vx];
				#if USE_HEIGHT_BUFF
				downLength = tile.nTerraYLens[vz-1][vx];//downArray.Length;
				upLength = tile.nTerraYLens[vz+2][vx];//upArray.Length;
				maxHeight = tile.nTerraYLens[vz][vx] = tile.nTerraYLens[vz+1][vx] = downLength;
				if(upLength>maxHeight)	
					maxHeight = tile.nTerraYLens[vz][vx] = tile.nTerraYLens[vz+1][vx] = upLength;
				#else
				downLength = downArray.Length;
				upLength = upArray.Length;
				maxHeight = downLength;
				if(upLength>maxHeight)	
					maxHeight = upLength;
				#endif
				
				Array.Clear(_tmpInterTerraVoxels1, 0, _tmpInterTerraVoxels1.Length);
				Array.Clear(_tmpInterTerraVoxels2, 0, _tmpInterTerraVoxels2.Length);
				for(int i=0;i<maxHeight;i+=VFVoxel.c_VTSize){
					downValue = downLength>i?(int)downArray[i]:0;
					upValue = upLength>i?(int)upArray[i]:0;
					_tmpInterTerraVoxels1[i] = (byte)((downValue*2+upValue)/3);
					_tmpInterTerraVoxels2[i] = (byte)((downValue+upValue*2)/3);					
					downType = downValue==0?BLOCK_AIR:downArray[i+1];
					_tmpInterTerraVoxels1[i+1] = downType;
					_tmpInterTerraVoxels2[i+1] = downType;
				}
				Array.Copy(_tmpInterTerraVoxels1, tile.terraVoxels[vz+0][vx], _tmpInterTerraVoxels1.Length);
				Array.Copy(_tmpInterTerraVoxels2, tile.terraVoxels[vz+1][vx], _tmpInterTerraVoxels2.Length);
				
				downWaterArray = zWaterDownArray[vx];
				upWaterArray = zWaterUpArray[vx];
				#if USE_HEIGHT_BUFF
				downWaterLength = tile.nWaterYLens[vz-1][vx];//downWaterArray.Length;
				upWaterLength = tile.nWaterYLens[vz+2][vx];//upWaterArray.Length;
				maxWaterHeight = tile.nWaterYLens[vz][vx] = tile.nWaterYLens[vz+1][vx] = downWaterLength;
				if(upWaterLength>maxWaterHeight)
					maxWaterHeight = tile.nWaterYLens[vz][vx] = tile.nWaterYLens[vz+1][vx] = upWaterLength;
				#else
				downWaterLength = downWaterArray.Length;
				upWaterLength = upWaterArray.Length;
				maxWaterHeight = downWaterLength;
				if(upWaterLength>maxWaterHeight)
					maxWaterHeight = upWaterLength;
				#endif
				
				Array.Clear(_tmpInterWaterVoxels1, 0, _tmpInterWaterVoxels1.Length);
				Array.Clear(_tmpInterWaterVoxels2, 0, _tmpInterWaterVoxels2.Length);
				for(int i=0;i<maxWaterHeight;i+=VFVoxel.c_VTSize){
					downValue = downWaterLength>i?(int)downWaterArray[i]:0;
					upValue = upWaterLength>i?(int)upWaterArray[i]:0;
					_tmpInterWaterVoxels1[i] = (byte)((downValue*2+upValue)/3);
					_tmpInterWaterVoxels2[i] = (byte)((downValue+upValue*2)/3);
					downType = downValue==0?BLOCK_AIR:downWaterArray[i+1];
					_tmpInterWaterVoxels1[i+1] = downType;
					_tmpInterWaterVoxels2[i+1] = downType;
				}
				Array.Copy(_tmpInterWaterVoxels1, tile.waterVoxels[vz+0][vx], _tmpInterWaterVoxels1.Length);
				Array.Copy(_tmpInterWaterVoxels2, tile.waterVoxels[vz+1][vx], _tmpInterWaterVoxels2.Length);
				
				mapType = GetMapType(scaledX, scaledZ);
				tileMapType[vz][vx] = mapType;				
				tileMapType[vz+1][vx] = mapType;
			}
		}
		//Profiler.EndSample();
		//long  interpolationTick2= System.DateTime.Now.Ticks;
		#endregion
		
		//long townStartTick = System.DateTime.Now.Ticks;
		#region town
		//Profiler.BeginSample("town");
		if (isNewTownTile)
		{
			for(int vz=0;vz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE;vz++)
			{
				int worldZ = vzStart + (voxelIndex[vz] & 0x0ff) + ((voxelIndex[vz] >> 8) << VoxelTerrainConstants._shift);
				for(int vx=0;vx<VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE;vx++){
					int worldX =vxStart + (voxelIndex[vx] & 0x0ff) + ((voxelIndex[vx] >> 8) << VoxelTerrainConstants._shift);
					
					_tmpVec2.x = worldX>>VoxelTerrainConstants._shift;
					_tmpVec2.y = worldZ>>VoxelTerrainConstants._shift;
					newTownList = VArtifactTownManager.Instance.OutputTownData(_tmpVec2);
					if(newTownList==null||newTownList.Count==0)
						continue;
					
					yTerraVoxels = tile.terraVoxels[vz][vx];
					yWaterVoxels = tile.waterVoxels[vz][vx];
					mapType = tileMapType[vz][vx];
					_tmpVec3.x = worldX;
					_tmpVec3.z = worldZ;
					_tmpNewTowns.Clear();
					for (int newTownIndex = 0; newTownIndex < newTownList.Count; newTownIndex++)
					{
						if (newTownList[newTownIndex].IsInTown(worldX,worldZ))
						{
							_tmpNewTowns.Add(newTownList[newTownIndex]);
						}
					}
					int yLen = tile.nTerraYLens[vz][vx];
					for (int i = 0; i < _tmpNewTowns.Count; i++)
					{
						//1.check voxel to remove
						//2.remove
						//3.mix
						
						VArtifactUnit _tmpVau = _tmpNewTowns[i];
						Dictionary<IntVector3, VFVoxel> townVoxelData = _tmpVau.townVoxel;
						int townStartHeight = Mathf.FloorToInt(_tmpVau.worldPos.y+10);
						int townMaxHeight = Mathf.CeilToInt(_tmpVau.worldPos.y + _tmpVau.vaSize.z);
						if ((townMaxHeight + 1) * VFVoxel.c_VTSize > yLen){
							yLen = tile.nTerraYLens[vz][vx] = (townMaxHeight + 1) * VFVoxel.c_VTSize;
						}
						bool townVoxelFlag = false;
						bool terrainAlreadyCut = false;//!_tmpVau.NeedCut(worldX,worldZ);
						int townVoxelBottom = s_noiseHeight - 1;
						for (int vy = townMaxHeight - 1, vy2 = vy * VFVoxel.c_VTSize; vy >= 0; vy--, vy2 -= VFVoxel.c_VTSize)
						{
							_tmpVec3.y = vy;
							if (townVoxelData.ContainsKey(_tmpVec3))
							{
								if(!terrainAlreadyCut){
									Array.Clear(yTerraVoxels, townStartHeight* VFVoxel.c_VTSize, yTerraVoxels.Length-townStartHeight* VFVoxel.c_VTSize);
									terrainAlreadyCut = true;
								}
								
								townVoxelFlag = true;
								townVoxelBottom = vy;
								VFVoxel vfvoxel = townVoxelData[_tmpVec3];
								if ((yTerraVoxels[vy2] + vfvoxel.Volume) > 255)
								{
									yTerraVoxels[vy2] = 255;
								}
								else
								{
									yTerraVoxels[vy2] += vfvoxel.Volume;
								}
								byte terrainType;
								//--to do: triplanar
								if (vfvoxel.Type < VArtifactUtil.isos[_tmpNewTowns[i].isoGuId].m_Materials.Length)
								{
									terrainType = (byte)VArtifactUtil.isos[_tmpNewTowns[i].isoGuId].m_Materials[vfvoxel.Type].m_Guid;
									if (terrainType >= 240)
									{
										int index = terrainType % 240;
										string[] valueStr = VArtifactUtil.triplaner[(int)mapType - 1].Split(',');
										terrainType = Convert.ToByte(valueStr[index]);
									}
								}
								else
								{
									terrainType = vfvoxel.Type;
								}
								townFloorVoxelByte[new IntVector3(vx, vy, vz)] = terrainType;
							}
						}
						if (townVoxelFlag)
						{
							for (int vy = townVoxelBottom, vy2 = vy * VFVoxel.c_VTSize; vy >= 0; vy--, vy2 -= VFVoxel.c_VTSize)
							{
								if (yTerraVoxels[vy2] != 255)
								{
									yTerraVoxels[vy2] = 255;
									if (yTerraVoxels[vy2 + 1] == BLOCK_AIR)
										yTerraVoxels[vy2 + 1] = BLOCK_STONE;
									
								}
							}
							for (int vy = townMaxHeight - 1, vy2 = vy * VFVoxel.c_VTSize; vy >= 0; vy--, vy2 -= VFVoxel.c_VTSize)
							{
								if (nWaterHeight+1 > vy)
								{
									yWaterVoxels[vy2] = (byte)(255 - yTerraVoxels[vy2]);
									if (yWaterVoxels[vy2] < 128)
										yWaterVoxels[vy2 + 1] = BLOCK_AIR;
								}
							}
						}
					}
				}
			}
		}
		//Profiler.EndSample();
		#endregion
		//long townEndTick = System.DateTime.Now.Ticks;
		
		//Profiler.BeginSample ("VType");
		SetVoxelType(tile, isNewTownTile, lod);
		//Profiler.EndSample ();
		//long interpolationTick4 = System.DateTime.Now.Ticks;
		if (isNewTownTile)
		{
			//Profiler.BeginSample("townTile");
			foreach (KeyValuePair<IntVector3, byte> tfPos in townFloorVoxelByte)
			{
				IntVector3 pos= tfPos.Key;
				//byte type = tfPos.Value;
				tile.terraVoxels[pos.z][pos.x][pos.y * VFVoxel.c_VTSize + 1] = tfPos.Value;
			}
			//Profiler.EndSample();
		}
		
//		long interpolationTick5 = System.DateTime.Now.Ticks;
		
//		renderedCount++;
//		long tickTime = System.DateTime.Now.Ticks - tick;
//		long millisecond = tickTime/10000L;
//		sumTick += millisecond;
//		Debug.Log("GenerateTile(" + x + "," + z + "):" + millisecond + 
//		          "AverageMillisecond: " + sumTick / renderedCount+
//		          ",interpolation: "+(interpolationTick2-interpolationTick)/10000L+
//		          ",town: "+(townEndTick-townStartTick)/10000L+
//		          ",findTown: "+(findTownEndTick-findTownStartTick)/10000L);
		//Debug.Log(isTownArea);
	}
	
	#region continent bound
	static float GetContinentValue(int worldx, int worldz)
	{
		float maxDistance = RandomMapConfig.Instance.BoudaryEdgeDistance - RandomMapConfig.Instance.boundOffset;
		float startDistance = RandomMapConfig.Instance.BoudaryEdgeDistance - RandomMapConfig.Instance.boundStart;
		float angle = Mathf.Atan2(Mathf.Abs(worldz),Mathf.Abs(worldx));		
		//new
		float distance = Mathf.Sqrt(Mathf.Pow(worldx, 2) + Mathf.Pow(worldz, 2));
		float noise2DValue = (float)(myRiverNoise[ContinentBoundIndex].Noise(worldx/distance*continentBoundFrequency,worldz/distance*continentBoundFrequency)+1)*0.5f;
		
		float end1 = RandomMapConfig.Instance.BoudaryEdgeDistance ;
		float end0 = maxDistance-noise2DValue*RandomMapConfig.Instance.boundChange;
		float start0 = startDistance-noise2DValue*RandomMapConfig.Instance.boundChange;
		if(Mathf.Abs(worldz)<Mathf.Abs(worldx)){
			end1 = end1/Mathf.Cos(angle);
			end0 = end0/Mathf.Cos(angle);
			start0 = start0/Mathf.Cos(angle);
		}else{
			end1 = end1/Mathf.Sin(angle);
			end0 = end0/Mathf.Sin(angle);
			start0 = start0/Mathf.Sin(angle);
		}


		if(distance>end1)
			return 2;
		if (distance > end0)
			return 1+(distance-end0)/(end1-end0);
		if (distance < start0)
			return 0;
		float value = (distance-start0)/(end0-start0);
		return value;
	}
	
	static float BlendContinentBound(float origin,float bound,float[] terTypeInc){
		float endPoint = -1;
		float endvalue=0;
		float bottomPoint =0;
		float bottomValue=terTypeInc[SeasideStartIndex];
		float setPoint0 = 0.4f;
		float setValue0 = terTypeInc[PlainStartIndex];
		float setPoint1 = 0.45f;
		float setValue1 = terTypeInc[HillStartIndex];
		//float StartPoint = 1;
		
		float bv = 1-bound;
		if(bv>setPoint1)
			bv=(bv-setPoint1)/(1-setPoint1)*(1-setValue1)+setValue1;
		else if(bv>setPoint0)
			bv =(bv-setPoint0)/(setPoint1-setPoint0)*(setValue1-setValue0)+setValue0;
		else if(bv>bottomPoint)
			bv = (bv-bottomPoint)/(setPoint0-bottomPoint)*(setValue0-bottomValue)+bottomValue;
		else
			bv = (bv-endPoint)/(bottomPoint-endPoint)*(bottomValue-endvalue)+endvalue;

		if(origin>bv)
			return bv;
		else
			return origin;
		//		return origin-bound;
	}
	static void RoundToSquare(ref float maxDistance, ref float startDistance, float angle)
	{
		float absAngle = Mathf.Abs(angle);
		if (absAngle > 0 && absAngle <= Mathf.PI / 4)
		{
			maxDistance /= Mathf.Cos(absAngle);
			startDistance /= Mathf.Cos(absAngle);
		}
		else if (absAngle > Mathf.PI / 4 && absAngle < Mathf.PI / 2)
		{
			float x = Mathf.PI / 2 - absAngle;
			maxDistance /= Mathf.Cos(x);
			startDistance /= Mathf.Cos(x);
		}
		else if (absAngle > Mathf.PI / 2 && absAngle <= Mathf.PI * 3 / 4)
		{
			float x = absAngle - Mathf.PI / 2;
			maxDistance /= Mathf.Cos(x);
			startDistance /= Mathf.Cos(x);
		}
		else if (absAngle > Mathf.PI * 3 / 4)
		{
			float x = Mathf.PI - absAngle;
			maxDistance /= Mathf.Cos(x);
			startDistance /= Mathf.Cos(x);
		}
	}
	#endregion
	
	#region lake
//	static float GetLakeValue(int worldX, int worldZ, float fTerType,float plainThickness,ref bool caveEnable,ref bool lakeArea,bool isTownPoint){
//		int lakeHeight = 0;
//		float lakeBottom = 0;
//		float bottom = 0;
//		float threshold0 = 0;
//		float lake2D = 0;
//		float scaledX = worldX * s_detailScale;
//		float scaledZ = worldZ * s_detailScale;
//		float value = 1;
//		if (RandomMapConfig.ScenceClimate != ClimateType.CT_Wet )//&& IsLakeAvailable(fTerType) && !isTownPoint)
//		{					
//			lake2D = (float)myRiverNoise[LakeIndex].Noise(scaledX * lakeFrequency, scaledZ * lakeFrequency);
//			float lakeBottomHeightFactor = (float)myRiverNoise[LakeBottomHeightIndex].Noise(scaledX * lakeFrequency * 0.1f, scaledZ * lakeFrequency * 0.1f);
//			lakeBottomHeightFactor = (lakeBottomHeightFactor + 1) * 0.5f;
//			threshold0 = ResetLakeThreshold(lakeThreshold, fTerType, scaledX, scaledZ);
//			if(Mathf.Abs(lake2D) > threshold0-0.01f)		caveEnable = false;
//			if(Mathf.Abs(lake2D) > threshold0-0.008f)		lakeArea = true;
//			if (Mathf.Abs(lake2D) > threshold0){
//				float bottomThreshold = ResetLakeBottomWidth(threshold0, fTerType);;
//				float p = 0.7f * (1 - fTerType * 0.5f);//0.70f * (1 - fTerType * 0.80f);
//				if (Mathf.Abs(lake2D) > bottomThreshold)
//				{
//					value = p;
//				}
//				else
//				{
//					float per = (Mathf.Abs(lake2D) - bottomThreshold) / (threshold0 - bottomThreshold);
//					float paramPer = 1;
//					if (fTerType > hillStart)
//					{
//						paramPer = 1 / (Mathf.Pow((fTerType - hillStart) / (1 - hillStart), 0.25f) * 8 + 1);
//					}
//					Mathf.Pow(per, paramPer);
//					value = per * (1 - p) + p;
//				}
//			}
//		}
//		return value;
//	} 
//	
//	void DebuffTerrain(bool isBottom, ref byte volume, ref byte type, float factor)
//	{
//		if (!isBottom)
//		{
//			volume = 0;
//			type = BLOCK_AIR;
//		}
//		else
//		{
//			volume = (byte)(255 * factor);
//			if (volume < 127.5f)
//				type = BLOCK_AIR;
//		}
//	}
//	
	//lake reset
//	static float ResetLakeBottomHeight(float lakeBottomMax, float fTerType)
//	{
//		return lakeBottomMax;
//	}
	static float ResetLakeBottomWidth(float lakeThreshold, float fTerType)
	{
		return 1 - (1 - lakeThreshold) * lakeBankPercent;
	}
	static float ResetLakeThreshold(float lakeThreshold, float fTerType, float scaledX, float scaledZ)
	{
		float value = 0;
		float changeFactor = (float)myRiverNoise[LakeChangeIndex].Noise(scaledX * lakeChangeFrequency, scaledZ * lakeChangeFrequency);
		float changeParam = (changeFactor + 1) * 0.75f;
		value = 1 - (1 - lakeThreshold) * changeParam;
		return value;
	}
	
	#endregion
	void FillWater(int vx, int vx2, int nWaterHeight, int fillWaterHeight, byte volWaterHeight, byte[][] yxVoxels, byte[][] yxWaterVoxels)
	{
		byte[] xWaterVoxels;
		int vy = fillWaterHeight;
		
		if (vy > 0)
		{ //vy == nWaterHeightLod
			if (yxVoxels[vy + 1][vx2] < 128 || yxVoxels[vy][vx2] < 128)
			{
				xWaterVoxels = yxWaterVoxels[vy];
				xWaterVoxels[vx2] = volWaterHeight;
				xWaterVoxels[vx2 + 1] = VFVoxelWater.c_iSeaWaterType;
				vy--;
				xWaterVoxels = yxWaterVoxels[vy];
				xWaterVoxels[vx2] = 255;
				xWaterVoxels[vx2 + 1] = VFVoxelWater.c_iSeaWaterType;
				vy--;
			}
			else
			{
				xWaterVoxels = yxWaterVoxels[vy];
				xWaterVoxels[vx2] = 0;
				xWaterVoxels[vx2 + 1] = BLOCK_AIR;
				vy--;
			}
		}
		for (; vy > nWaterHeight - 2; vy--)
		{
			xWaterVoxels = yxWaterVoxels[vy];
			//if(yxVoxels[vy+1][vx2] < 128 || yxVoxels[vy][vx2] < 128) 
			if (yxVoxels[vy + 1][vx2] < 128 || yxVoxels[vy][vx2] < 160)
			{
				xWaterVoxels[vx2] = 255;
				xWaterVoxels[vx2 + 1] = VFVoxelWater.c_iSeaWaterType;
			}
			else
			{
				xWaterVoxels[vx2] = 0;
				xWaterVoxels[vx2 + 1] = BLOCK_AIR;
			}
		}
	}
	// Gen water tile based on terrain tile. all lod tile compatable
	public void FillWaterVoxels(byte[] yWaterVoxels, int nWaterHeight, byte[] yTerraVoxels, int nTerraHeight, byte volWaterHeight,float fTerType,bool riverArea,bool lakeArea)
	{
		int yLen = yTerraVoxels.Length;
		int vy2 = nWaterHeight * VFVoxel.c_VTSize;
		int permeateDepth = 15;
		//1.voxeltop
		int voxelTop = nTerraHeight; //yLen/2-1;
		for(int i=voxelTop, vy = i*VFVoxel.c_VTSize; i > 0; i--,vy-=VFVoxel.c_VTSize){
			voxelTop = i;
			if(yTerraVoxels[vy]>127.5)
				break;
		}
		int voxelTopInWater = nWaterHeight+1;
		for(int i=voxelTopInWater,vy = i*VFVoxel.c_VTSize;i>0;i--,vy-=VFVoxel.c_VTSize){
			voxelTopInWater = i;
			if(vy + VFVoxel.c_VTSize < yLen &&yTerraVoxels[vy + VFVoxel.c_VTSize] > 128&&yTerraVoxels[vy]>128)
				break;
		}
		if (voxelTopInWater <= nWaterHeight) {
			yWaterVoxels [vy2] = volWaterHeight;
			yWaterVoxels [vy2 + 1] = VFVoxelWater.c_iSeaWaterType;
			vy2 -= VFVoxel.c_VTSize;
			yWaterVoxels [vy2] = 255;
			yWaterVoxels [vy2 + 1] = VFVoxelWater.c_iSeaWaterType;
			vy2 -= VFVoxel.c_VTSize;
			for (; vy2>voxelTopInWater*VFVoxel.c_VTSize; vy2-=VFVoxel.c_VTSize) {
				yWaterVoxels [vy2] = 255;
				yWaterVoxels [vy2 + 1] = VFVoxelWater.c_iSeaWaterType;
			}
			for (int i=0; vy2>0&&i<permeateDepth; i++,vy2-=VFVoxel.c_VTSize) {
				yWaterVoxels [vy2] = 255;
				yWaterVoxels [vy2 + 1] = VFVoxelWater.c_iSeaWaterType;
			}
		} else {
			yWaterVoxels [vy2] = 0;
			yWaterVoxels [vy2 + 1] = BLOCK_AIR;
			vy2 -= VFVoxel.c_VTSize;
		}
		
		if(lakeArea||(riverArea))//&&fTerType<(_plainStart+_seasideStart)/2))
		{
			for (; vy2 >= 0; vy2 -= VFVoxel.c_VTSize)
			{
				yWaterVoxels[vy2] = 255;
				yWaterVoxels[vy2 + 1] = VFVoxelWater.c_iSeaWaterType;
			}
		}else{
			for (; vy2 >= 0; vy2 -= VFVoxel.c_VTSize)
			{
				if (vy2 + VFVoxel.c_VTSize >= yLen || yTerraVoxels[vy2 + VFVoxel.c_VTSize] < 128 || yTerraVoxels[vy2] < FILL_VOLUME)
				{
					yWaterVoxels[vy2] = 255;
					yWaterVoxels[vy2 + 1] = VFVoxelWater.c_iSeaWaterType;
				}
				else
				{
					yWaterVoxels[vy2] = 0;
					yWaterVoxels[vy2 + 1] = BLOCK_AIR;
				}
			}
		}
	}
	
	private void PaintTile_Simple(VFTile terTile)
	{
		// Set voxel type based on volume/gradient
		for (int vz = 1; vz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - 1; vz++)
		{
			byte[][] yxVoxelsBk = terTile.terraVoxels[vz + 1];
			byte[][] yxVoxelsFt = terTile.terraVoxels[vz - 1];
			byte[][] yxVoxels = terTile.terraVoxels[vz];
			for (int vy = 1; vy < terTile.tileH - 1; vy++)
			{
				byte[] xVoxelsBk = yxVoxelsBk[vy];
				byte[] xVoxelsFt = yxVoxelsFt[vy];
				byte[] xVoxelsBkDn = yxVoxelsBk[vy - 1];
				byte[] xVoxelsFtDn = yxVoxelsFt[vy - 1];
				byte[] xVoxelsBkUp = yxVoxelsBk[vy + 1];
				byte[] xVoxelsFtUp = yxVoxelsFt[vy + 1];
				byte[] xVoxelsDn = yxVoxels[vy - 1];
				byte[] xVoxelsUp = yxVoxels[vy + 1];
				byte[] xVoxels = yxVoxels[vy];
				for (int vx2 = VFVoxel.c_VTSize, vx = 1; vx < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - 1; vx2 += VFVoxel.c_VTSize, vx++)
				{
					if (xVoxels[vx2] < 128 && xVoxelsDn[vx2] >= 128)
					{
						int dif0, dif1, dif2, dif3;
						if (xVoxels[vx2 + VFVoxel.c_VTSize] == 0) dif0 = xVoxels[vx2] + 255 - xVoxelsDn[vx2 + VFVoxel.c_VTSize];
						else if (xVoxels[vx2 + VFVoxel.c_VTSize] == 255) dif0 = xVoxelsUp[vx2 + VFVoxel.c_VTSize] + 255 - xVoxels[vx2];
						else dif0 = Math.Abs(xVoxels[vx2 + VFVoxel.c_VTSize] - xVoxels[vx2]);
						
						if (xVoxels[vx2 - VFVoxel.c_VTSize] == 0) dif1 = xVoxels[vx2] + 255 - xVoxelsDn[vx2 - VFVoxel.c_VTSize];
						else if (xVoxels[vx2 - VFVoxel.c_VTSize] == 255) dif1 = xVoxelsUp[vx2 - VFVoxel.c_VTSize] + 255 - xVoxels[vx2];
						else dif1 = Math.Abs(xVoxels[vx2 - VFVoxel.c_VTSize] - xVoxels[vx2]);
						
						if (xVoxelsBk[vx2] == 0) dif2 = xVoxels[vx2] + 255 - xVoxelsBkDn[vx2];
						else if (xVoxelsBk[vx2] == 255) dif2 = xVoxelsBkUp[vx2] + 255 - xVoxels[vx2];
						else dif2 = Math.Abs(xVoxelsBk[vx2] - xVoxels[vx2]);
						
						if (xVoxelsFt[vx2] == 0) dif3 = xVoxels[vx2] + 255 - xVoxelsFtDn[vx2];
						else if (xVoxelsFt[vx2] == 255) dif3 = xVoxelsFtUp[vx2] + 255 - xVoxels[vx2];
						else dif3 = Math.Abs(xVoxelsFt[vx2] - xVoxels[vx2]);
						
						int maxDif = Math.Max(Math.Max(dif0, dif1), Math.Max(dif2, dif3));
						if (maxDif < 40)
						{
							xVoxelsDn[vx2 + 1] = 3;
						}
					}
				}
			}
		}
	}
	
	private void SetVoxelType(VFTile tile, bool isTownTile, int lod = 0)
	{
		int[] voxelIndex = S_VoxelIndex[lod];
		// Set voxel type based on volume/gradient
		int len = VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE - 1;
		int vxStart = (tile.tileX << VoxelTerrainConstants._shift) - VoxelTerrainConstants._numVoxelsPrefix;//the x one before the first voxel in this tile 
		int vzStart = (tile.tileZ << VoxelTerrainConstants._shift) - VoxelTerrainConstants._numVoxelsPrefix;//the z one before the first voxel in this tile
		
		for (int vz = 0; vz < len; vz++)
		{
			int worldZ = vzStart + (voxelIndex[vz] & 0x0ff) + ((voxelIndex[vz] >> 8) << VoxelTerrainConstants._shift);
			double[] xNoiseBuf = dTileNoiseBuf[vz];
			float[] xHeightBuf = fTileHeightBuf[vz];//change the fTileHeightBuf one by one
			float[] xGradTanBuf = fTileGradTanBuf[vz];
			byte[][] xyVoxels = tile.terraVoxels[vz];
			for (int vx = 0; vx < len; vx++)
			{
				int worldX = vxStart + (voxelIndex[vx] & 0x0ff) + ((voxelIndex[vx] >> 8) << VoxelTerrainConstants._shift);
				int regionId = VoxelPaintXMLParser.MapTypeToRegionId(tileMapType[vz][vx]);
				
				bool bUnderGround = false;
				double noise = paintConfig.GetNoise(worldX, worldZ);//the true pos
				xNoiseBuf[vx] = noise;
				xGradTanBuf[vx] = 0f;
				xHeightBuf[vx] = 0f;
				
				byte[] yVoxels = xyVoxels[vx];
				int maxVY2 = tile.nTerraYLens[vz][vx];
				for (int vy = (maxVY2 >> VFVoxel.c_shift), vy2 = maxVY2; vy > 1; vy--, vy2 -= VFVoxel.c_VTSize)
				{
					int vy3 = vy2 - VFVoxel.c_VTSize;	//Dn
					byte vol = yVoxels[vy2];
					byte volDn = yVoxels[vy3];
					if (vol < 128 && volDn >= 128)
					{
						float volDif = (float)(vol - volDn);
						float p = (128 - volDn) / volDif; // Based on ocl kernel f0->this, f1->other
						if (vz == 0 || vx == 0)
						{
							if (!bUnderGround)
							{
								xHeightBuf[vx] = vy - 1 + p;
								bUnderGround = true;
							}
						}
						else
						{
							byte[][] xyVoxelsBk = tile.terraVoxels[vz + 1];
							byte[][] xyVoxelsFt = tile.terraVoxels[vz - 1];
							byte[] yVoxelsBk = xyVoxelsBk[vx];
							byte[] yVoxelsFt = xyVoxelsFt[vx];
							_lrbfVol[0] = xyVoxels[vx-1][vy2];
							_lrbfVol[1] = xyVoxels[vx+1][vy2];
							_lrbfVol[2] = yVoxelsBk[vy2];
							_lrbfVol[3] = yVoxelsFt[vy2];
							_lrbfVolDn[0] = xyVoxels[vx-1][vy3];
							_lrbfVolDn[1] = xyVoxels[vx+1][vy3];
							_lrbfVolDn[2] = yVoxelsBk[vy3];
							_lrbfVolDn[3] = yVoxelsFt[vy3];
							
							
							float maxGrad = 0f;
							for (int i = 0; i < 4; i++)
							{
								float grad = _lrbfVolDn[i] < 128 ? ((_lrbfVolDn[i] - volDn) / volDif)
									: _lrbfVol[i] < 128 ? Math.Abs(p - (128 - _lrbfVolDn[i]) / (float)(_lrbfVol[i] - _lrbfVolDn[i]))
										: (1 - p) * (_lrbfVol[i] - vol) / (128.0f - vol);
								if (maxGrad < grad) maxGrad = grad;
							}
							
							if(IsTownConnectionType(worldX,worldZ))
							{
								yVoxels[vy2 - VFVoxel.c_VTSize + 1] =BlOCK_TOWN_CONNECTION;
								
								if(vy>TownConnectionDepth)
								{
									for (int i = vy - 1; i > vy - TownConnectionDepth; i--)
									{
										yVoxels[((i - 1) << VFVoxel.c_shift) + 1] = BlOCK_TOWN_CONNECTION;
									}
								}
								else{
									for (int i = vy - TownConnectionDepth; i > 1; i--)
									{
										yVoxels[((i - 1) << VFVoxel.c_shift) + 1] = BlOCK_TOWN_CONNECTION;
									}
								}
							}
							else{
								yVoxels[vy2 - VFVoxel.c_VTSize + 1] = paintConfig.GetVoxelType(maxGrad, vy, noise, regionId); // Here posX/posZ not be used in this function through they are incorrect
								
								
								if (vy > BiomaDepth)
								{
									for (int i = vy - 1; i > vy - BiomaDepth; i--)
									{
										yVoxels[((i - 1) << VFVoxel.c_shift) + 1] = paintConfig.GetVoxelType(maxGrad, i, noise, regionId);
									}
								}
								else
								{
									for (int i = vy - 1; i > 1; i--)
									{
										yVoxels[((i - 1) << VFVoxel.c_shift) + 1] = paintConfig.GetVoxelType(maxGrad, i, noise, regionId);
									}
								}
							}
							if (!bUnderGround)
							{
								xGradTanBuf[vx] = maxGrad;
								xHeightBuf[vx] = vy - 1 + p;
								bUnderGround = true;
							}
						}
					}
					else if (vz != 0 && vx != 0 && vol >= 128 && volDn < 128 && ((yVoxels[vy2 + VFVoxel.c_VTSize] >= 128) || (vol >= 128 && volDn < 128 && vy == tile.tileH - 1)))
					{
						yVoxels[vy2 + 1] = paintConfig.GetVoxelType(999, vy, noise, regionId); // Here posX/posZ not be used in this function through they are incorrect
					}
					else if (vz != 0 && vx != 0 && vol >= 128 && volDn >= 128 && ((yVoxels[vy2 + VFVoxel.c_VTSize] >= 128) || (vol >= 128 && volDn < 128 && vy == tile.tileH - 1)))
					{
						byte[][] xyVoxelsBk = tile.terraVoxels[vz + 1];
						byte[][] xyVoxelsFt = tile.terraVoxels[vz - 1];
						byte[] yVoxelsBk = xyVoxelsBk[vx];
						byte[] yVoxelsFt = xyVoxelsFt[vx];
						_lrbfVol[0] = xyVoxels[vx-1][vy2];
						_lrbfVol[1] = xyVoxels[vx+1][vy2];
						_lrbfVol[2] = yVoxelsBk[vy2];
						_lrbfVol[3] = yVoxelsFt[vy2];
						for (int i = 0; i < 4; i++)
						{
							if (_lrbfVol[i] < 128)
							{
								yVoxels[vy2 + 1] = paintConfig.GetVoxelType(999, vy, noise, regionId); // Here posX/posZ not be used in this function through they are incorrect
								break;
							}
						}
					}
				}
			}
		}
	}
	
	const int c_radiusPlants = 10;
	static readonly int[] s_ofsToCheckPlants = new int[]{
		-c_radiusPlants,-c_radiusPlants,
		-c_radiusPlants,0, 
		-c_radiusPlants,+c_radiusPlants, 
		0,-c_radiusPlants,
		0,+c_radiusPlants, 
		+c_radiusPlants,-c_radiusPlants,
		+c_radiusPlants,0, 
		+c_radiusPlants,+c_radiusPlants, 
	};
	private void GenTreesOnTile(VFTile terTile, int szCell)
	{
		if (SystemSettingData.Instance.RandomTerrainLevel > 0 && terTile.tileL != 1)
			return;
		
		if (terTile.tileL == 0) {
			if((terTile.tileX&1) != 0 || (terTile.tileZ&1) != 0)
				return;
			FillTileData (terTile.tileX, terTile.tileZ, 1, _lodTile);
			terTile = _lodTile;
		}
		
		_tmpVec2.x = terTile.tileX;
		_tmpVec2.y = terTile.tileZ;
		lock (s_dicTreeInfoList) {
			if(s_dicTreeInfoList.ContainsKey(_tmpVec2))
				return;
		}
		paintConfig.RandSeed = 79;
		List<TreeInfo> lstTreeInfo = new List<TreeInfo>();
		paintConfig.PlantTrees(terTile, dTileNoiseBuf, fTileHeightBuf, fTileGradTanBuf, lstTreeInfo, tileMapType, szCell);
		
		// remove one out of range and add new one
		int n = s_ofsToCheckPlants.Length;
		lock (s_dicTreeInfoList) {
			IntVector2 keyPosTrees = new IntVector2 ();
			List<TreeInfo> tis;
			for(int i = 0; i < n; i+=2){
				keyPosTrees.x = terTile.tileX + (s_ofsToCheckPlants[i]<<terTile.tileL);	
				keyPosTrees.y = terTile.tileZ + (s_ofsToCheckPlants[i+1]<<terTile.tileL);
				if(s_dicTreeInfoList.TryGetValue(keyPosTrees, out tis)){
					s_dicTreeInfoList.Remove(keyPosTrees);
					TreeInfo.FreeTIs(tis);
					break;
				}
			}
			keyPosTrees.x = terTile.tileX;	
			keyPosTrees.y = terTile.tileZ;
			s_dicTreeInfoList.Add (keyPosTrees, lstTreeInfo);
		}
	}
	private void GenGrassOnTile(VFTile terTile, int szCell)
	{
		if (terTile.tileL != 0)
			return;
		
		_tmpVec2.x = terTile.tileX;
		_tmpVec2.y = terTile.tileZ;
		lock (s_dicGrassInstList) {
			if(s_dicGrassInstList.ContainsKey(_tmpVec2))
				return;
		}
		paintConfig.RandSeed = 79;
		List<VoxelGrassInstance> lstGrassInst = new List<VoxelGrassInstance>();
		paintConfig.PlantGrass(terTile, dTileNoiseBuf, fTileHeightBuf, fTileGradTanBuf, lstGrassInst, tileMapType, szCell);
		
		// remove one out of range and add new one
		int n = s_ofsToCheckPlants.Length;
		lock (s_dicGrassInstList) {
			IntVector2 keyPosGrass = new IntVector2 ();
			for(int i = 0; i < n; i+=2){
				keyPosGrass.x = terTile.tileX + (s_ofsToCheckPlants[i]<<terTile.tileL);	
				keyPosGrass.y = terTile.tileZ + (s_ofsToCheckPlants[i+1]<<terTile.tileL);
				if(s_dicGrassInstList.Remove(keyPosGrass))
					break;
			}
			keyPosGrass.x = terTile.tileX;	
			keyPosGrass.y = terTile.tileZ;
			s_dicGrassInstList.Add (keyPosGrass, lstGrassInst);
		}
	}
	#endif
	// Fill chunk data with tile(s) of lod 0
	private void FillChunkDataByTile0(VFVoxelChunkData chunk, VFTile terTile, int vyStart, bool bWater)
	{
		//Profiler.BeginSample ("Fill0");
		int idxDst = 0;
		byte[] vdata = VFVoxelChunkData.s_ChunkDataPool.Get();
		Array.Clear(vdata, 0, vdata.Length);
		
		vyStart -= VoxelTerrainConstants._numVoxelsPrefix;
		int vyEnd = vyStart + VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE;
		if (vyEnd > terTile.tileH)			vyEnd = terTile.tileH;
		if (vyStart < 0){					vyStart = 0; idxDst += VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;	}
		int vy2End = vyEnd * VFVoxel.c_VTSize;
		int vy2Start = vyStart * VFVoxel.c_VTSize;
		byte[][][] voxels = bWater ? terTile.waterVoxels : terTile.terraVoxels;
		for (int vz = 0; vz < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; vz++,idxDst += VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED_VT)
		{
			for (int vx = 0, vx2 = 0; vx < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; vx++, vx2 += VFVoxel.c_VTSize)
			{
				byte[] yVoxels = voxels[vz][vx];
				int idxDst0 = idxDst+vx2;
				for (int vy2 = vy2Start; vy2 < vy2End; vy2 += VFVoxel.c_VTSize, idxDst0 += VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT)
				{
					vdata[idxDst0] = yVoxels[vy2];
					vdata[idxDst0 + 1] = yVoxels[vy2 + 1];
				}
			}
		}
		chunk.OnDataLoaded(vdata, true);
		//Profiler.EndSample ();
		return;
	}
	private void FillTerraChunkDataByTileLOD(VFVoxelChunkData chunk, VFTile tile, int vyStart)
	{
		int lod = tile.tileL;
		if (lod > S_VoxelIndex.Length - 1) { Debug.LogWarning("[VFDataRTGen]Error: Unexpected lod " + lod); return; }
		if (lod == 0) {
			FillChunkDataByTile0(chunk, tile, vyStart, false);
			return;
		}
		
		int[] voxelIndex = S_VoxelIndex[lod];		
		int idxDst = 0;
		byte[] vdata = VFVoxelChunkData.s_ChunkDataPool.Get();
		Array.Clear(vdata, 0, vdata.Length);
		
		vyStart -= VoxelTerrainConstants._numVoxelsPrefix;
		//zx----- tile order
		//zyx for compatiability with ocl mc algo
		for (int z = 0; z < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; z++)
		{
			int idxDst0 = idxDst;
			for (int x = 0; x < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; x++, idxDst0 += VFVoxel.c_VTSize)
			{
				int idxDst1 = idxDst0;
				byte[] yVoxels = tile.terraVoxels[z][x];
				int maxy = yVoxels.Length >> VFVoxel.c_shift;
				for (int y = 0; y < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; y++)
				{
					int curY = vyStart + (voxelIndex[y] & 0xff) + ((voxelIndex[y] >> 8) << VoxelTerrainConstants._shift);
					if (curY >= 0 && curY < maxy)
					{
						int idx = curY << VFVoxel.c_shift;
						vdata[idxDst1] = yVoxels[idx];
						vdata[idxDst1 + 1] = yVoxels[idx + 1];
					}
					idxDst1 += VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
				}
			}
			idxDst += VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SQUARED_VT;
		}
		chunk.OnDataLoaded(vdata, true);
		return;
	}
	private void FillWaterChunkDataByTileLOD(VFVoxelChunkData chunk, VFTile tile, int vyStart)
	{
		int lod = tile.tileL;
		if (lod > S_VoxelIndex.Length - 1) { Debug.LogWarning("[VFDataRTGen]Error: Unexpected lod " + lod); return; }
		if (lod == 0) {
			FillChunkDataByTile0(chunk, tile, vyStart, true);
			return;
		}
		
		byte[] vdata = new byte[VFVoxel.c_VTSize];
		if (waterHeight < vyStart)
		{
			vdata[0] = 0;
			vdata[1] = 0;
		}
		else if (waterHeight >= (vyStart + (1 << (VoxelTerrainConstants._shift + lod))))
		{
			vdata[0] = 255;
			vdata[1] = VFVoxelWater.c_iSeaWaterType;
		}
		else
		{
			vdata[0] = VFVoxelWater.c_iSurfaceVol;
			vdata[1] = VFVoxelWater.c_iSeaWaterType;
		}
		if (null != (System.Object)VFVoxelWater.self)
		{
			VFVoxelWater.self.OnWaterDataLoad(chunk, vdata, false);
		}
		return;
	}
	private int FillTileData(int tileX, int tileZ, int lod, VFTile tile)	// return 0: not hit caches; 1: rectcache hit; 2: filecache hit
	{
		#if VOXEL_CACHE_ENABLE
		//Profiler.BeginSample ("GetFromCache");
		_tmpVec4.x = tileX;
		_tmpVec4.y = tileZ;
		_tmpVec4.z = lod;
		_tmpVec4.w = s_noiseHeight;
		//if(tileX==56 && tileZ==-50 && lod == 1)
		{
			VFTerTileCacheDesc cacheFileDesc = _tileFileCache.FillTileDataWithFileCache(_tmpVec4, tile, dTileNoiseBuf, fTileHeightBuf, fTileGradTanBuf, tileMapType);
			//Profiler.EndSample ();
			if (cacheFileDesc != null)
			{
				isTownTile = (0 != (cacheFileDesc.bitMask & VFTerTileCacheDesc.c_bitMaskBTown));
				if (lod == 0 && VArtifactTownManager.Instance!=null)
				{
					//Profiler.BeginSample("GenTown");
					_tmpTileIndex.x = tileX;
					_tmpTileIndex.y = tileZ;
					VArtifactTownManager.Instance.GenTownFromTileIndex(_tmpTileIndex);
					//Profiler.EndSample();
				}
				return 1;
			}
		}
		#endif
		
		//Profiler.BeginSample ("GenTileData");
		tile.tileX = tileX;
		tile.tileZ = tileZ;
		tile.tileL = lod;
		FillTileDataWithNoise(tile);
		//Profiler.EndSample ();
		
		#if VOXEL_CACHE_ENABLE
		// save all data for lod 1 in order to use them in palnt trees
		int terraBitMask = (isTownTile ? VFTerTileCacheDesc.c_bitMaskBTown : 0) | (lod <= 1 ? VFTerTileCacheDesc.c_bitMaskAllData : VFTerTileCacheDesc.c_bitMaskVoxData);
		_tileFileCache.SaveDataToFileCaches(terraBitMask, tile, dTileNoiseBuf, fTileHeightBuf, fTileGradTanBuf, tileMapType);
		#endif
		return 0;
	}
	
	class RTGenChunkReq
	{
		public VFVoxelChunkData terraChunk;
		public VFVoxelChunkData waterChunk;
		public int terraStamp;
		public int waterStamp;
		public bool TerraOutOfDate { get { return terraChunk == null || !terraChunk.IsStampIdentical(terraStamp); } }
		public bool WaterOutOfDate { get { return waterChunk == null || !waterChunk.IsStampIdentical(waterStamp); } }
	}
	
	private Dictionary<IntVector4, RTGenChunkReq> _chunkReqList = new Dictionary<IntVector4, RTGenChunkReq>();
	// Interface implementation
	public bool IsIdle  { 	get { return _chunkReqList.Count == 0; 			} }
	public bool ImmMode { 	get { return bImmMode; } set{ bImmMode = value; } }
	public void Close() {	_tileFileCache.Close (); 	}	
	public void AddRequest(VFVoxelChunkData chunkData)
	{
		int vyStart = (chunkData.ChunkPosLod.y << VoxelTerrainConstants._shift);
		if (vyStart < 0 || vyStart >= s_noiseHeight + 0.5f)	// this NoiseTerrain's height <0 || >tileH
		{
			chunkData.OnDataLoaded(VFVoxelChunkData.S_ChunkDataAir);
			return;
		}
		
		bool bWater = chunkData.SigOfType == VFVoxelWater.c_Sig;
		if (true)//(chunkData.LOD != 0)			// Process lod chunk
		{
			RTGenChunkReq req = null;
			if (!_chunkReqList.TryGetValue(chunkData.ChunkPosLod, out req))
			{
				req = new RTGenChunkReq();
				_chunkReqList[chunkData.ChunkPosLod] = req;
			}
			if (bWater) { 	req.waterChunk = chunkData; req.waterStamp = chunkData.StampOfUpdating; }
			else { 			req.terraChunk = chunkData; req.terraStamp = chunkData.StampOfUpdating; }
		}
		//else
		//{
		//	VFTile tile = _curTile;
		//	//Profiler.BeginSample("GetTileData");
		//	FillTileData(chunkData.ChunkPosLod.x, chunkData.ChunkPosLod.z, chunkData.LOD, tile);	// it would take much time to get tile for each chunk
		//	//Profiler.EndSample();			
			
		//	//Profiler.BeginSample("GenPlants");
		//	GenGrassOnTile(tile, 1);
		//	GenTreesOnTile(tile, 1);
		//	//Profiler.EndSample();
			
		//	//Profiler.BeginSample("Fill");
		//	FillChunkDataByTile0(chunkData, tile, vyStart, bWater);
		//	//Profiler.EndSample();
		//}
	}
	#if GEN_LOD_WITH_LOD0
	private void FillChunkDataByTile0(VFVoxelChunkData chunk, List<VFTerTile> terTiles, int lod, int vyStart)
	{
		if(lod == 0){						FillChunkDataByTile0(terTiles[0], vyStart, ref vdata);			return;	}
		if(lod > S_VoxelIndex.Length-1){	Debug.LogWarning("[VFDataRTGen]Error: Unexpected lod "+lod);	return;	}
		
		int[] voxelIndex = S_VoxelIndex[lod];
		int maxy = terTiles[0].tileH;
		int idxDst = 0;
		byte[] vdata = VFVoxelChunkData.s_ChunkDataPool.Get();
		vyStart -= VoxelTerrainConstants._numVoxelsPrefix;
		//zx----- tile order
		//zyx for compatiability with ocl mc algo
		for(int z = 0; z < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; z++)
		{
			int curZ = voxelIndex[z]&0xff;
			int tileOfs = (voxelIndex[z]>>8)<<lod;
			for(int y = 0; y < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; y++)
			{
				int curY = vyStart + (voxelIndex[y]&0xff) + ((voxelIndex[y]>>8)<<VoxelTerrainConstants._shift);
				if(curY < 0 || curY >= maxy)
				{
					System.Array.Clear(vdata, idxDst, VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT);
					idxDst += VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE_VT;
				}
				else
				{
					for(int x = 0; x < VoxelTerrainConstants.VOXEL_ARRAY_AXIS_SIZE; x++,idxDst+=VFVoxel.c_VTSize)
					{
						VFTerTile curTile = terTiles[tileOfs+(voxelIndex[x]>>8)];
						byte[] xLine = curTile.voxels[curZ][curY];
						int idxInLine = (voxelIndex[x]&0xff)<<VFVoxel.c_shift;
						vdata[idxDst] = xLine[idxInLine];
						vdata[idxDst+1] = xLine[idxInLine+1];
					}
				}
			}
		}
		chunk.OnDataLoaded(vdata, true);
		return;
	}
	public void ProcessReqs()
	{
		if(bIdle)
		{
			bool bLoop = false;
			do{
				bLoop = false;
				IntVector4 chunkPosLod = null;
				foreach(KeyValuePair<IntVector4, RTGenChunkReq> req in _chunkReqList)
				{
					RTGenChunkReq reqValue = req.Value;
					chunkPosLod = req.Key;
					if(reqValue.TerraOutOfDate && reqValue.WaterOutOfDate){		bLoop = true; break;				}
					
					int max = 1<<chunkPosLod.w;
					List<VFTerTile> terraTiles = new List<VFTerTile>();
					List<VFTerTile> waterTiles = new List<VFTerTile>();
					for(int z = 0; z < max; z++)
					{
						for(int x = 0; x < max; x++)
						{
							IntVector2 iv2TilePos = new IntVector2(chunkPosLod.x+x, chunkPosLod.z+z);
							VFTerTile terraTile = null, waterTile = null;
							GetTileData(iv2TilePos, 0, out terraTile, out waterTile);
							terraTiles.Add(terraTile);
							waterTiles.Add(waterTile);
						}
					}
					// All lod0 data is ready to be used to compute lod data 
					int vyStart = (chunkPosLod.y << VoxelTerrainConstants._shift);
					if(!reqValue.TerraOutOfDate)
					{
						FillChunkDataByTile0(reqValue.terraChunk, terraTiles, chunkPosLod.w, vyStart);
					}
					if(!reqValue.WaterOutOfDate)
					{
						FillChunkDataByTile0(reqValue.waterChunk, waterTiles, chunkPosLod.w, vyStart);
					}
				}
				if(chunkPosLod != null)	_chunkReqList.Remove(chunkPosLod);
			}while(bLoop);
		}
		bIdle = true;
	}
	#else
	private int CompareChunkPosLodXZ(IntVector4 cpos0, IntVector4 cpos1)
	{
		if (cpos0.w != cpos1.w) {
			return cpos0.w - cpos1.w;
		}
		
		Vector3 center = LODOctreeMan.self.LastRefreshPos;
		float dx0 = center.x - cpos0.x;
		float dz0 = center.z - cpos0.z;
		float dx1 = center.x - cpos1.x;
		float dz1 = center.z - cpos1.z;
		float sqrDist0 = dx0 * dx0 + dz0 * dz0;
		float sqrDist1 = dx1 * dx1 + dz1 * dz1;
		return (int)(sqrDist0 - sqrDist1);
	}
	public void ProcessReqs()
	{
		if (_chunkReqList.Count > 0) {
			//Profiler.BeginSample("ProcReq");
			IntVector4 chunkPosLod = null;
			List<IntVector4> sortedCPos = _chunkReqList.Keys.ToList<IntVector4> ();
			sortedCPos.Sort (CompareChunkPosLodXZ);
			for (int i = 0; i < sortedCPos.Count; i++) {
				chunkPosLod = sortedCPos [i];
				RTGenChunkReq reqValue = _chunkReqList [chunkPosLod];
				if (!reqValue.TerraOutOfDate || !reqValue.WaterOutOfDate) {
					VFTile tile = _curTile;
					if (tile.tileX != chunkPosLod.x || tile.tileZ != chunkPosLod.z || tile.tileL != chunkPosLod.w) {
						//Profiler.BeginSample ("GetTile");
						FillTileData (chunkPosLod.x, chunkPosLod.z, chunkPosLod.w, tile);
						//Profiler.EndSample ();	
						
						//Profiler.BeginSample("GenPlants");
						GenGrassOnTile(tile, 1);
						GenTreesOnTile(tile, 1);
						//Profiler.EndSample();
					}
					int vyStart = (chunkPosLod.y << VoxelTerrainConstants._shift);
					if (!reqValue.TerraOutOfDate) {				FillTerraChunkDataByTileLOD (reqValue.terraChunk, tile, vyStart);			}
					if (!reqValue.WaterOutOfDate) {				FillWaterChunkDataByTileLOD (reqValue.waterChunk, tile, vyStart);			}
				}
				_chunkReqList.Remove (chunkPosLod);
			}
			//Profiler.EndSample();
		}
	}
	#endif
	
	public static void SetTerrainParam()
	{
		terTypeChanceIncList = new List<float[]> ();
		for(int i=0;i<regionArray.Length;i++){
			RegionDescArrayCLS region = regionArray[i];
			float[] terTypeChance = new float[GRASS_REGION_CNT];
			float[] terTypeChanceInc = new float[GRASS_REGION_CNT];
			for (int j = 0; j < GRASS_REGION_CNT; j++)
			{
				terTypeChance[j] = region.TerrainDescArrayValues[j].terChance;
				if(j==0)
					terTypeChanceInc[j] = terTypeChance[j];
				else
					terTypeChanceInc[j] = terTypeChance[j] + terTypeChanceInc[j-1];
				//Debug.LogError("terTypeChanceInc["+i+ "]: " + terTypeChanceInc[i + 1]);
//				switch (j)
//				{
//				case 0: seasideStart = terTypeChanceInc[j + 1];		break;
//				case 1: plainStart = terTypeChanceInc[j + 1];		break;
//				case 2: hillStart = terTypeChanceInc[j + 1];		break;
//				case 3: MountainTopStart = terTypeChanceInc[j + 1];	break;
//				}
			}
			TEST_REGION_CNT = GRASS_REGION_CNT;
			TEST_REGION_THRESHOLD = GRASS_REGION_THRESHOLD;
			terTypeChanceIncList.Add(terTypeChanceInc);
		}
	}
	
	
	public static void ComputeTerNoise(ref float fTerNoise, int nTerType, float fTerType)
	{
		switch (nTerType)
		{
		default:
			fTerNoise += 0.8f - (-0.32f - fTerType) * 0.376766f;
			break;
		case 0://water
		case 4://top of mountain
			fTerNoise += 0.65f;
			break;
		case 1://seaside
			fTerNoise += 0.8f - (-0.32f - fTerType) * 0.376766f;
			break;
		case 3://mountain
			fTerNoise += 0.8f - (fTerType - 0.15625f) * 0.3491f;
			break;
		case 2:	// grass mid value
			fTerNoise += 0.9f - Math.Abs(fTerType - 0.1171875f) * 2.56f;
			break;
		}
	}
	public static RandomMapType GetXZMapType(int x, int z) 
	{
		if (myNoise == null)
		{
			LogManager.Error("The Class VFDataRtGen.cs not initialized!!!");
			return RandomMapType.GrassLand;
		}
		float scaledX = x * s_detailScale;
		float scaledZ = z * s_detailScale;
		
		//maptype
		RandomMapType mapType = GetMapType(scaledX, scaledZ);
		return mapType;
	}
	public static RandomMapType GetXZMapType(IntVector2 worldXZ)
	{
		int x = worldXZ.x;
		int z = worldXZ.y;
		return GetXZMapType(x, z);
	}
	
	
	public static bool townAvailable{	get;set;	}

//	public static float GetRiverScale(int x, int z, float fTerType, bool isTownPoint)
//	{
//		float scaledX = x * s_detailScale;
//		float scaledZ = z * s_detailScale;
//		float river2D = (float)myRiverNoise[RiverIndex].Noise(scaledX * riverFrequencyX, scaledZ * riverFrequencyZ);
//		float threshold = ResetThreshold(riverThreshold, fTerType);
//		if (Mathf.Abs(river2D) < threshold && !isTownPoint)
//		{
//			return Mathf.Abs(river2D) / threshold;
//		}
//		return 1;
//	}
	public static float GetfNoise12D1ten(int worldx,int worldz){
		float scaledX = worldx*s_detailScale;
		float scaledZ = worldz*s_detailScale;
		return GetfNoise12D1ten(scaledX,scaledZ);
	}
	public static float GetfNoise12D1ten(float scaledX,float scaledZ){
		float fNoise12D1ten;
		float fNoise12D1ten01 = ((float)myNoise[FTerTypeIndex].Noise(scaledX * terrainFrequencyX, scaledZ * terrainFrequencyZ)+1)*50;//50;
		float fNoise12D1ten02 = ((float)myNoise[FTerTypeIndex01].Noise(scaledX * terrainFrequencyX, scaledZ * terrainFrequencyZ)+1)*50;//50;
		float fNoise12D1ten03 = ((float)myNoise[FTerTypeIndex02].Noise(scaledX * terrainFrequencyX*3, scaledZ * terrainFrequencyZ*3)+1)*50;

//		float maxBlendDef = 20f;
		//float blendCoef = 0.1f;

		float maxIslandDef = 10f;//changable
		
		float ignoreParam = 50;//seasideStart*100;
		float islandStartThreshold = ignoreParam/2;
		if(fNoise12D1ten01>ignoreParam)
			fNoise12D1ten01 = (fNoise12D1ten01-ignoreParam)*((islandFactor-ignoreParam)/(100-ignoreParam))+ignoreParam;
		if(fNoise12D1ten02>ignoreParam)
			fNoise12D1ten02 = (fNoise12D1ten02-ignoreParam)*((islandFactor-ignoreParam)/(100-ignoreParam))+ignoreParam;
		fNoise12D1ten03=fNoise12D1ten03*(ignoreParam/100)+(0.75f*100-ignoreParam)/2;
		fNoise12D1ten03 = Mathf.Clamp(fNoise12D1ten03,0,100);
		float fNoise12D1tenMax = Mathf.Max(fNoise12D1ten01,fNoise12D1ten02);
		//float fNoise12D1tenMin = Mathf.Min(fNoise12D1ten01,fNoise12D1ten02);
		if(fNoise12D1tenMax<=ignoreParam)
		{
			if(fNoise12D1tenMax<=islandStartThreshold){
				if(fNoise12D1ten03>fNoise12D1tenMax){
					float dif = islandStartThreshold-fNoise12D1tenMax;
					if(dif<maxIslandDef)
						fNoise12D1ten=fNoise12D1tenMax*(1-dif/maxIslandDef)+fNoise12D1ten03*dif/maxIslandDef;
					else
						fNoise12D1ten = fNoise12D1ten03;
				}else{
					fNoise12D1ten=fNoise12D1tenMax;
				}
			}else{
				fNoise12D1ten=fNoise12D1tenMax;
			}
		}else// if(fNoise12D1tenMax>ignoreParam&&fNoise12D1tenMin<=ignoreParam)
		{
			fNoise12D1ten=fNoise12D1tenMax;
		}
//		else{
//			float dif = fNoise12D1tenMax-fNoise12D1tenMin;
//			if(dif<maxBlendDef)
//			{
//				fNoise12D1ten = fNoise12D1tenMax+(fNoise12D1tenMin-ignoreParam)*(blendCoef*Mathf.Sqrt(1-dif/maxBlendDef));
//			}else
//			{
//				fNoise12D1ten = fNoise12D1tenMax;
//			}
//		}

		//HASChange
		float factorHASC = 6;//24;
		float HAS00 = (float)myNoise[HASChangeIndex].Noise(scaledX * terrainFrequencyX, scaledZ * terrainFrequencyZ);
		float finalHASC = HAS00*factorHASC+2;
		//		if(finalHill<factorHill/8)
		//			finalHill = 0;
		//		else
		//			finalHill = (finalHill-factorHill/8)*8/7;
		fNoise12D1ten+=finalHASC;



		//hill
		float factorHill = 20;//24;
		float hill01 = (float)myNoise[HillTerIndex01].Noise(scaledX * terrainFrequencyX*6f, scaledZ * terrainFrequencyZ*6f);
		float hill02 = (float)myNoise[HillTerIndex02].Noise(scaledX * terrainFrequencyX*4f, scaledZ * terrainFrequencyZ*4f);
		float h1 = hill01;
		float h2 = (hill02+1)*0.5f;
		float finalHill = h1*h2*factorHill+4;
		fNoise12D1ten+=finalHill;

		//mountain
		float mountainValue00 =GetMountain(scaledX,scaledZ,MountainParamIndex01,30,1);
//		float mountainValue01 =GetMountain(scaledX,scaledZ,FTerTypeIndex+1,30,1.5f);
		float mountainValue02 =GetMountain(scaledX,scaledZ,MountainParamIndex02,20,2f);
//		if(fNoise12D1ten<ignoreParam&&sceneClimate!=ClimateType.CT_Dry){
////			mountainValue01 = mountainValue01*(fNoise12D1ten/ignoreParam);
			mountainValue02 = mountainValue02*Mathf.Pow(fNoise12D1ten/ignoreParam,2);
//			mountainValue01 = mountainValue01*(fNoise12D1ten-ignoreParam)/(plainStart*100-ignoreParam);
//			mountainValue02 = mountainValue02*Mathf.Pow((fNoise12D1ten-ignoreParam)/(plainStart*100-ignoreParam),2);
//		}

		fNoise12D1ten+=Mathf.Max(mountainValue00,mountainValue02);

		//Sierra
		float finalSierra = GetFinalSierra(scaledX,scaledZ);
		fNoise12D1ten+=finalSierra;
		
//		fNoise12D1ten = Mathf.Clamp(fNoise12D1ten,0,100);
		return fNoise12D1ten;
	}
}