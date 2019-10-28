using UnityEngine;
using System.Collections;

public class CSConst
{
    public const int NONELECTICS = 20;

    // Data Type
    public const int dtDefault = 0;
    public const int dtAssembly = 1;
    public const int dtStorage = 2;
    public const int dtEngineer = 3;
    public const int dtEnhance = 4;
    public const int dtRepair = 5;
    public const int dtRecyle = 6;
    public const int dtFarm = 7;
    public const int dtFactory = 8;
    public const int dtProcessing = 9;
    public const int dtTrade = 10;
    public const int dtTrain = 11;
    public const int dtCheck = 12;
    public const int dtTreat = 13;
    public const int dtTent = 14;
	//public const int dtTransaction = 11;

    public const int dtDwelling = 21;
    public const int dtPowerPlant = 32;	// (32 ~ 64)
    // Power planet type
    public const int dtppCoal = 33;
    public const int dtppSolar = 34;
	public const int dtppFusion = 35;
    // Persennel 
    public const int dtPersonnel = 50;


    // <CETC> Entity Type
    public const int etAssembly = dtAssembly;
    public const int etStorage = dtStorage;
    public const int etEngineer = dtEngineer;
    public const int etEnhance = dtEnhance;
    public const int etRepair = dtRepair;
    public const int etRecyle = dtRecyle;
    public const int etFarm = dtFarm;
    public const int etFactory = dtFactory;
    public const int etProcessing = dtProcessing;
    public const int etTrade = dtTrade;
    public const int etTrain = dtTrain;
    public const int etCheck = dtCheck;
    public const int etTreat = dtTreat;
    public const int etTent = dtTent;

    public const int etDwelling = dtDwelling;
    public const int etPowerPlant = dtPowerPlant;
    // Power planet type
    public const int etppCoal = dtppCoal;
	public const int etppSolar = dtppSolar;
	public const int etppFusion = dtppFusion;

    public const int etUnknow = 12121;


    public enum ObjectType
    {
        Assembly = etAssembly,
        Storage = etStorage,
        Engineer = etEngineer,
        Enhance = etEnhance,
        Repair = etRepair,
        Recyle = etRecyle,
        Farm = etFarm,
        Factory = etFactory,
        Processing = etProcessing,
        Dwelling = etDwelling,
        PowerPlant = etPowerPlant,
        PowerPlant_Coal = etppCoal,
		PowerPlant_Solar = etppSolar,
		PowerPlant_Fusion = etppFusion,
        Trade = etTrade,
        Train = etTrain,
        Check = etCheck,
        Treat = etTreat,
        Tent = etTent,
        None = etUnknow
    }

    public enum PowerPlantType
    {
        Coal = etppCoal,
        Solar = etppSolar,
		Fusion = etppFusion
    }

    public enum CreatorType
    {
        Managed,
        NoManaged
    }

    // ID for Special Entiti
    public const int etID_Farm = -100;

    // UI Menu Type

    // Register Result Type
    public const int rrtNoAssembly = 0;
    public const int rrtHasAssembly = 1;
    public const int rrtOutOfRadius = 2;
    public const int rrtOutOfRange = 3;
    public const int rrtSucceed = 4;
    public const int rrtUnkown = 5;
    public const int rrtTooCloseToNativeCamp = 6;
    public const int rrtTooCloseToNativeCamp1 = 7;
    public const int rrtTooCloseToNativeCamp2 = 8;
	public const int rrtAreaUnavailable = 9;

    // Personnel State Type
    
    public const int pstUnknown = 0;
    public const int pstPrepare = 1;
    public const int pstIdle = 2;
    public const int pstRest = 3;
    public const int pstWork = 4;
    public const int pstFollow = 5;
    public const int pstDead = 6;
    public const int pstAtk = 7;
    public const int pstPatrol = 8;
    //public const int pstStore = 9;
    public const int pstPlant = 9;
    public const int pstWatering = 10;
    public const int pstWeeding = 11;
    public const int pstGain = 12;
    //public const int pstGuard = 9;

    // Personnel occupational type
    public const int potUnknown = -1;
    public const int potDweller = 0;
    public const int potWorker = 1;
    public const int potSoldier = 2;
    public const int potFarmer = 3;
    public const int potFollower = 4;
    public const int potProcessor = 5;
    public const int potDoctor = 6;
    public const int potTrainer = 7;

    // Personnel work mode -- Key Works<PWT>
    public const int pwtUnknown = -1;
    public const int pwtNoWork = 0;
    public const int pwtNormalWork = 1;
    public const int pwtWorkWhenNeed = 2;
    public const int pwtWorkaholic = 3;
    public const int pwtFarmForMag = 4;
    public const int pwtFarmForHarvest = 5;
    public const int pwtFarmForPlant = 6;
    public const int pwtPatrol = 7;
    public const int pwtGuard = 8;
    public const int pwtStandby = 9;
    public const int pwtAssigned = 10;

    // Entity Event Type
    public const int eetDestroy = 1;
    public const int eetHurt = 2;

    // Entity event type
    public const int ehtHurt = 1 << 1;
    public const int ehtRestore = 2 << 2;
    public const int eetAssembly_AddBuilding = 2001;
    public const int eetAssembly_RemoveBuilding = 2002;
    public const int eetAssembly_Upgraded = 2003;
    public const int eetStorage_HistoryDequeue = 3001;
    public const int eetStorage_HistoryEnqueue = 3002;
    public const int eetStorage_PackageRemoveItem = 3003;
    public const int eetDwellings_ChangeState = 4001;
    public const int eetCommon_ChangeAssembly = 5001;
    public const int eetFarm_OnPlant = 6001;

    // Entity Grade type
    public const int egtUrgent = 0;
    public const int egtHigh = 1;
    public const int egtMedium = 2;
    public const int egtLow = 3;
    public const int egtTotal = 4;

    #region CREAROR
    // Creator ID 
    public const int ciDefMgCamp = 0;

    public const int ciDefNoMgCamp = 10000;

    // Creator Event Type
    public const int cetAddEntity = 1001;
    public const int cetRemoveEntity = 1002;
    public const int cetAddPersonnel = 1003;
    public const int cetRemovePersonnel = 1004;



    // Monster Type
    public enum EMonsterType
    {
        Land,
        Sky,
        Water
    }

    #endregion
}
