using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

using Pathea;


public enum RSceneMode
{
    Adventure,
    Build,
    Max
}
public enum RGameType
{
    Cooperation = 0,
    VS,
    Survive,
    Max
}

public class RandomMapConfig
{
    static RandomMapConfig mInstance;
    public static RandomMapConfig Instance
    {
        get
        {
            if (mInstance == null)
                mInstance = new RandomMapConfig();
            return mInstance;
        }
    }

    public static RSceneMode mSceneMode;
    public static RGameType mGameType;
    public static int cacheModeInt;

    public static RandomMapType RandomMapID = RandomMapType.GrassLand;
	public static int RandomMapTypeCount{
		get{return Enum.GetValues(typeof(RandomMapType)).Length;}
	}
    //随机草树种子
    public static string SeedString = "";
    public static int RandSeed = 666;
    //随机草树ID 1草原 2深林 3红石 4 沙漠
    public static RandomMapType vegetationId = RandomMapType.GrassLand;

    public static ClimateType ScenceClimate = ClimateType.CT_Dry;

    //随机地图大小
    //0:40k*40k 1:20k*20k 2:8k*8k 3:4K*4k 4:2k*2k
    public static int mapSize=0;
    public static int riverDensity;
    public static int riverWidth;
	public static int plainHeight = 50;
	public static int flatness =50;
	public static int bridgeMaxHeight = 50;
	public static int TownGenInitSeed{
		get{return RandSeed+(int)RandomMapID+mapSize+riverDensity+riverWidth+plainHeight+flatness+bridgeMaxHeight;}
	}
	public static int BiomaInitSeed{
		get{return TownGenInitSeed+1;}
	}
	public static int TownGenSeed{
		get{return TownGenInitSeed+2;}
	}
	public static int AllyGenSeed{
		get{return TownGenInitSeed+3;}
	}
	public static int MineGenSeed{
		get{return RandSeed+1;}
	}
	// 使不使用Sill tree
	public static bool useSkillTree = false;

    //lz-2017.03.14 开启全脚本
    public static bool openAllScripts = false;

    public static bool mirror=false;
	public static int rotation = 0;
	public static int pickedLineIndex = 0;
	public static int pickedLevelIndex = 0;

    public int boundaryWest = -20000;
    public int boundaryEast = 20000;
    public int boundarySouth = -20000;
    public int boundaryNorth = 20000;
    public int mapRadius = 20000;
    public float boundStart = 450;//0.5
    public int boundOffset = 150;
	public int boundChange = 500;
	public int BorderOffset=300;
	public int BoudaryEdgeDistance{
		get{return mapRadius+BorderOffset;}
	}

    public static int terrainHeight = 128;
    public static int TerrainHeight
    {
        set
        {
            terrainHeight = value;
        }
        get
        {
            return terrainHeight;
        }
    }

	public static int allyCount=8;

	public Vector2 MapSize {get {return new Vector2(BoudaryEdgeDistance*2 ,  
			                                                BoudaryEdgeDistance*2 );}}
    public void SetMapParam()
    {
		WeatherConfig.SetClimateType(ScenceClimate,vegetationId);
		SetTerrainHeight(terrainHeight,ScenceClimate);
		VFDataRTGen.SetRiverDensity(riverDensity);
        VFDataRTGen.SetRiverWidth(riverWidth);
		System.Random randTool = new System.Random(RandSeed);
		VFDataRTGen.SetPlainThickness(plainHeight);
//		VFDataRTGen.SetPlainMin((float)randTool.NextDouble());
//		VFDataRTGen.SetPlainMax((float)randTool.NextDouble());
		VFDataRTGen.SetFlatness(flatness);
		VFDataRTGen.SetFlatMin((float)randTool.NextDouble());
		VFDataRTGen.SetFlatMax((float)randTool.NextDouble());
		VFDataRTGen.SetBridgeMaxHeight(bridgeMaxHeight);
        //Debug.LogError(Mathf.Atan(1)*4);
        switch (mapSize)
        {
            case 0:
                //boundary
                SetBoundary(-20000, 20000, -20000, 20000, 200);
                //VFDataRTGen mineral
                VFDataRTGen.MetalReduceSwitch = false;
                VFDataRTGen.MetalReduceArea = 4000;
                //VFDataRTGen.MineFrequency0 = 0.5;
                //VFDataRTGen.MineFrequency1 = 2;

                VFDataRTGen.SetMapTypeFrequency(1f);
                //VArtifactTownManager town level campDistance detectarea
                //SetTownBoundary(-19200, 19200, -19200, 19200);
                //VArtifactTownManager.Instance.LevelRadius = 4000;
                //SetTownDistance(1, 1);
                //VArtifactTownManager.Instance.DetectedChunkNum = 32;
				//allyCount=8;
                break;
            case 1:
                //boundary
                SetBoundary(-10000, 10000, -10000, 10000, 100);
                //VFDataRTGen mineral
                VFDataRTGen.MetalReduceSwitch = false;
                VFDataRTGen.MetalReduceArea = 2000;
                //VFDataRTGen.MineFrequency0 = 0.5;
                //VFDataRTGen.MineFrequency1 = 2;

                VFDataRTGen.SetMapTypeFrequency(1f);
                //VArtifactTownManager town level
                //SetTownBoundary(-9600, 9600, -9600, 9600);
                //VArtifactTownManager.Instance.LevelRadius = 2000;
                //SetTownDistance(1, 1);
				//VArtifactTownManager.Instance.DetectedChunkNum = 32;
				//allyCount=8;
                break;
            case 2:
                //boundary
                SetBoundary(-4000, 4000, -4000, 4000, 40);
                //VFDataRTGen mineral
                VFDataRTGen.MetalReduceSwitch = false;
                //VFDataRTGen.MineFrequency0 = 0.25;
                //VFDataRTGen.MineFrequency1 = 0.25;
                //maptype
                VFDataRTGen.SetMapTypeFrequency(1.5f);
                //VFDataRTGen.SetTerrainFrequency(1.5f);
                //VArtifactTownManager town level campDistance detectarea
                //SetTownBoundary(-3860, 3860, -3860, 3860);
                //VArtifactTownManager.Instance.LevelRadius = 800;
                //SetTownDistance(0.5f, 0.8f);
				//VArtifactTownManager.Instance.DetectedChunkNum = 16;
				//allyCount=8;
                break;
            case 3:
                //boundary
                SetBoundary(-2000, 2000, -2000, 2000, 20);
                //VFDataRTGen mineral
                VFDataRTGen.MetalReduceSwitch = false;
                //VFDataRTGen.MineFrequency0 = 1;
                //VFDataRTGen.MineFrequency1 = 2;
                VFDataRTGen.SetMapTypeFrequency(3f);
                //VFDataRTGen.SetTerrainFrequency(3f);
                //VArtifactTownManager town level campDistance detectarea
                //SetTownBoundary(-1920, 1920, -1920, 1920);
                //VArtifactTownManager.Instance.LevelRadius = 400;
                //SetTownDistance(0.5f, 0.8f);
				//VArtifactTownManager.Instance.DetectedChunkNum = 12;
				//allyCount=4;
                break;
            case 4:
                //boundary
                SetBoundary(-1000, 1000, -1000, 1000, 10);
                //VFDataRTGen mineral
                VFDataRTGen.MetalReduceSwitch = false;
                //VFDataRTGen.MineFrequency0 = 1;
                //VFDataRTGen.MineFrequency1 = 2;
                VFDataRTGen.SetMapTypeFrequency(3f);
                //VFDataRTGen.SetTerrainFrequency(3f);
                //VArtifactTownManager town level campDistance detectarea
                //SetTownBoundary(-960, 960, -960, 960);
                //VArtifactTownManager.Instance.LevelRadius = 200;
                //SetTownDistance(0.25f, 0.8f);
				//VArtifactTownManager.Instance.DetectedChunkNum = 6;
				//allyCount=4;
                break;
        }
        //VFDataRTGen.TestMapBound();
		
		Debug.Log("<color=red>SeedString:" + SeedString 
		          + "terrainHeight:" + terrainHeight 
		          + "mapsize: " + mapSize 
		          + ", riverdensity: " + riverDensity 
		          + ", riverwidth: " + riverWidth 
		          + "plainHeight:" +plainHeight
		          +"flatness:"+flatness
		          +"bridgemaxheight:"+bridgeMaxHeight
		          +"allyCount"+allyCount
		          +"</color>");


        VFVoxelWater.c_fWaterLvl = VFDataRTGen.WaterHeightBase;
        if (ScenceClimate == ClimateType.CT_Wet)
        {
            System.Random waterseed = new System.Random(RandSeed);
            int waterplus = waterseed.Next(VFDataRTGen.WET_WATER_PLUS_MIN, VFDataRTGen.WET_WATER_PLUS_MAX);
            VFVoxelWater.c_fWaterLvl += waterplus;
        }
        else if (ScenceClimate == ClimateType.CT_Temperate)
        {
            System.Random waterseed = new System.Random(RandSeed);
            int waterplus = waterseed.Next(VFDataRTGen.TEMP_WATER_PLUS_MIN, VFDataRTGen.TEMP_WATER_PLUS_MAX);
            VFVoxelWater.c_fWaterLvl += waterplus;
        }
		VFDataRTGen.sceneClimate = ScenceClimate;
        VFDataRTGen.waterHeight = VFVoxelWater.c_fWaterLvl;
        VFDataRTGen.InitStaticParam(RandSeed);
		SetGlobalFogHeight(VFDataRTGen.waterHeight);

    }


    public void SetBoundary(int west, int east, int south, int north, int offset)
    {
        boundaryWest = west;
        boundaryEast = east;
        boundarySouth = south;
        boundaryNorth = north;
        mapRadius = east;
        boundOffset = offset;
    }

	public static void InitTownAreaPara(){
		System.Random myRand = new System.Random (TownGenSeed);
		mirror = myRand.NextDouble()>=0.5;
		rotation = myRand.Next(4);
		pickedLineIndex = myRand.Next(TownGenData.GenerationLine.Length);
		pickedLevelIndex = myRand.Next(TownGenData.AreaLevel.Length);
	}

	private void SetTerrainHeight(int terrainHeight,ClimateType scenceClimate)
	{
        if(terrainHeight<256)
        {
            TerrainHeight=128;
            VFDataRTGen.s_noiseHeight = 128;
            VFDataRTGen.s_seaDepth = 5;
        }else if(terrainHeight<512){
            TerrainHeight = 256;
            VFDataRTGen.s_noiseHeight = 256;
            VFDataRTGen.s_seaDepth = 5;
        }else{
            TerrainHeight = 512;
            VFDataRTGen.s_noiseHeight = 512;
//			if(scenceClimate==ClimateType.CT_Dry)
//            	VFDataRTGen.s_seaDepth = 64;
//			else
				VFDataRTGen.s_seaDepth = 128;
        }
    }


    #region interface
    public static int GetModeInt(){
        if (PeGameMgr.IsSingleAdventure)
        {
            return 1;
        }
        if (PeGameMgr.IsMultiAdventure)
        {
            return 2;
        }
        
        return 3;
    }

	public static void SetGlobalFogHeight(float height){
		UnityStandardAssets.ImageEffects.GlobalFog gf = Camera.main.GetComponent<UnityStandardAssets.ImageEffects.GlobalFog>();
		if(gf!=null)
			gf.height = height;
	}
	public static void SetGlobalFogHeight(){
		UnityStandardAssets.ImageEffects.GlobalFog gf = Camera.main.GetComponent<UnityStandardAssets.ImageEffects.GlobalFog>();
		if(gf!=null)
			gf.height = VFDataRTGen.waterHeight;
	}
    #endregion
}