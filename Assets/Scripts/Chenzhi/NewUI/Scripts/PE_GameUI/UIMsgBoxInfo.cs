using UnityEngine;
using System.Collections;



public class UIMsgBoxInfo 
{
	// ID 820XXXXXX for ChengZhi
    public static MsgBoxValue mRoomIsClose = new MsgBoxValue(8000437, "The server is down or unavailable!");
    public static MsgBoxValue mDisconnectedServer = new MsgBoxValue(8000438, "The network connection with the server has been lost. Please try connecting again.");
    public static MsgBoxValue mCZ_WorkShopUpNeedSeletedIso = new MsgBoxValue(8000439, "You need selected one ISO!");
    public static MsgBoxValue mCZ_WorkShopUpDeleteIso = new MsgBoxValue(8000440, "Delete Sucess!");
    public static MsgBoxValue mCZ_WorkShopUpDeleteIsoFailed = new MsgBoxValue(8000441, "Delete Failed!");
    public static MsgBoxValue mCZ_WorkShopUpLoadIso = new MsgBoxValue(8000442, "Upload Sucess!");
    public static MsgBoxValue mCZ_WorkShopUpLoadIsoFailed = new MsgBoxValue(8000443, "Upload Failed!");
    public static MsgBoxValue mCZ_DeleteSrever = new MsgBoxValue(8000444, "Are you sure to delete the server?");
    public static MsgBoxValue mCZ_InvitePlayer = new MsgBoxValue(8000445, "invite you to join his game.");
	
	// ID 821XXXXXX for LiLingJun
	public static MsgBoxValue NotEnoughItem = new MsgBoxValue(821000001, "Not enough materials to make this!");
	public static MsgBoxValue CannotPut = new MsgBoxValue(821000002, "Cannot put it here!");
	public static MsgBoxValue DistanceNotMatch = new MsgBoxValue(821000003, "Out of range from the last joint! (60m)");
	public static MsgBoxValue ConnectError = new MsgBoxValue(821000004, "Obstacle between joints!");
	public static MsgBoxValue CannotSleep = new MsgBoxValue(821000005, "You can't sleep right now.");

	public static MsgBoxValue RouteUnstopNotice = new MsgBoxValue(821000006, "You must stop this route first.");
	public static MsgBoxValue RouteDeleteNotice = new MsgBoxValue(821000007, "The unremove train will be lose, are you sure?");
	public static MsgBoxValue ReNameNotice = new MsgBoxValue(821000008, "This name already exist.");
	public static MsgBoxValue RailwayDeleteNotice = new MsgBoxValue(821000009, "Can't delete when the train has passengers.");

	//Speciale
	public static MsgBoxValue ClearPlant = new MsgBoxValue(8000134, "Clear");
	public static MsgBoxValue GetPlant = new MsgBoxValue(8000135, "Get");
	public static MsgBoxValue WaterPlant = new MsgBoxValue(8000132, "Water");
	public static MsgBoxValue CleanPlant = new MsgBoxValue(8000133, "Clean");


	// ID 822XXXXXX for WuYiQiu
	public static MsgBoxValue mOpenColonyTips 	  	= new MsgBoxValue(82200001, "Use \"Alt+B\" to quickly open the Colony UI.");
	public static MsgBoxValue mStartToUpgrade     	= new MsgBoxValue(82200002, "Start to upgrade the $A$.");
	public static MsgBoxValue mStartToCharge	  	= new MsgBoxValue(82200003, "Start to charge the $A$.");
	public static MsgBoxValue mStartToEnhance	  	= new MsgBoxValue(82200004, "Start to enhance the $A$.");
	public static MsgBoxValue mStartToRepair	  	= new MsgBoxValue(82200005, "Start to repair the $A$.");
	public static MsgBoxValue mStartToRecycle 	  	= new MsgBoxValue(82200006, "Start to recycle the $A$");
	public static MsgBoxValue mPlantSequence	  	= new MsgBoxValue(82200007, "The Workers will plant in the sequence.");
	public static MsgBoxValue mPutIntoMachine     	= new MsgBoxValue(82200008, "You put the $A$ into the $B$.");
	public static MsgBoxValue mTakeAwayFromMachine	= new MsgBoxValue(82200009, "You take away the $A$ from the $B$.");
	public static MsgBoxValue mFullFuelTips			= new MsgBoxValue(82200010, "The $A$ is full with fuel now.");
	public static MsgBoxValue mStartToDelete		= new MsgBoxValue(82200011, "Start to delete the $A$." );
	public static MsgBoxValue mResortTheItems		= new MsgBoxValue(82200012, "The Storage start to resort the items");
	public static MsgBoxValue mSplitItems			= new MsgBoxValue(82200013, "The Item $A$ successfully splinter off $B$");
	public static MsgBoxValue mNeedStaticField		= new MsgBoxValue(82200014, "You need to use this item within the static field of a settlement.");
	public static MsgBoxValue mNeedAssembly			= new MsgBoxValue(82200015, "You need to place an assembly core to make it work!");
	public static MsgBoxValue mNotToBeCharged		= new MsgBoxValue(82200016, "The $A$ is not need to be charged.");
	public static MsgBoxValue mNotPlantSeed			= new MsgBoxValue(82200017, "The $A$ is not a plant seed.");
	public static MsgBoxValue mOnlyCanPutWater 		= new MsgBoxValue(82200018, "This grid only can put water!");
	public static MsgBoxValue mOnlyCanPutInsecticider		= new MsgBoxValue(82200019, "This grid only can put Insecticider!");
	public static MsgBoxValue mNotEnoughGrid				= new MsgBoxValue(82200020, "There are not enough grid in your package, check please.");
	public static MsgBoxValue mCantWorkWithoutElectricity	= new MsgBoxValue(82200021, "This $A$ cannot find work without electricity!");
	public static MsgBoxValue mHasBeenEnhancingTheItem		= new MsgBoxValue(82200022, "This enhance machine already has been enhancing the item.");
	public static MsgBoxValue mHasBeenRepairingTheItem		= new MsgBoxValue(82200023, "This repair machine already has been repairing the item.");
	public static MsgBoxValue mHasBeenRecyclingTheItem 		= new MsgBoxValue(82200024, "This recycle machine already has been recycling the item.");
	public static MsgBoxValue mCantWorkForItem				= new MsgBoxValue(82200025, "The $A$ cant work for $B$!");
	public static MsgBoxValue mNotRequireRepair				= new MsgBoxValue(82200026, "This $A$ does not require repair.");
	public static MsgBoxValue mCantRecycle					= new MsgBoxValue(82200027, "This $A$ cannot be recycled.");
	public static MsgBoxValue mDeleteItem					= new MsgBoxValue(82200028, "You throw away the $A$ from $B$");
	public static MsgBoxValue mPutItemToNpc					= new MsgBoxValue(82200029, "Put the $A$ into the $B$'s bag.");
	public static MsgBoxValue mTakeAwayItemFromNpc			= new MsgBoxValue(82200030, "Take away $A$ from the $B$'s bag.");
	public static MsgBoxValue mProfessionForWorker			= new MsgBoxValue(82200031, "$A$ become a worker.  The worker can make some machine efficiently.");
	public static MsgBoxValue mProfessionForFarmer			= new MsgBoxValue(82200032, "$A$ become a farmer,  The farmer can Help you better manage the farm.");
	public static MsgBoxValue mProfessionForSolider			= new MsgBoxValue(82200033, "$A$ become a soldier, The soldier can protect your colony. ");
	public static MsgBoxValue mProfessionForDweller			= new MsgBoxValue(82200034, "$A$ become a dweller, The dweller does not do anything for you. ");
	public static MsgBoxValue mProfessionForFollower		= new MsgBoxValue(82200035, "$A$ become a follower, You let him be your follower.");
    public static MsgBoxValue mProfessionForDoctor          = new MsgBoxValue(82200053, "$A$ become a doctor.");
    public static MsgBoxValue mProfessionForInstructor      = new MsgBoxValue(82200054, "$A$ become a instructor.");
    public static MsgBoxValue mProfessionFailed             = new MsgBoxValue(82200055, "Can't change $A$'s work at the moment.");
	public static MsgBoxValue mWorkerForNormal				= new MsgBoxValue(82200036, "The mode \"Normal\" means: the worker will do the work step-by-step.");
	public static MsgBoxValue mWorkerForWorkaholic			= new MsgBoxValue(82200037, "The mode \"Workaholic\" means: the worker will be working until exhaustion.");
	public static MsgBoxValue mWorkerForWorkWhenNeed		= new MsgBoxValue(82200038, "The mode \"WorkWhenNeed\" means: the worker will work when the machine need.");
	public static MsgBoxValue mFarmerForManage				= new MsgBoxValue(82200039, "The mode \"Manage\" means: the farmer will be watering , cleaning and clear the plants.");
	public static MsgBoxValue mFarmerForHarvest				= new MsgBoxValue(82200040, "The mode \"Harvest\" means: the farmer will harvest the plants");
	public static MsgBoxValue mFarmerForPlant				= new MsgBoxValue(82200041, "The mode \"Plant\" means: the farmer will plant when there are enough plants and clods.");
	public static MsgBoxValue mSoldierForPatrol				= new MsgBoxValue(82200042, "The mode \"Patrol\" means: the soldier will patrol around in colony, protect all the machines.");
	public static MsgBoxValue mSoldierForGuard				= new MsgBoxValue(82200043, "The mode \"Guard\" means: the soldier will protect colony in a certain range.");
	public static MsgBoxValue mSetWorkRoom					= new MsgBoxValue(82200044, "$A$ work for $B$ now.");
	public static MsgBoxValue mGuardPos						= new MsgBoxValue(82200045, "You set the guard center where you stand.");
	public static MsgBoxValue mGuardArea					= new MsgBoxValue(82200046, "The protected area is showing.");

    //lz-2016.06.08 不能工作，电和基地核心分开提示
    public static MsgBoxValue mCantWorkWithoutAssembly = new MsgBoxValue(8000194, "The $A$ cannot work without Assembly Core!");
    public static MsgBoxValue mCantWorkAssemblyLevelInsufficient = new MsgBoxValue(8000195, "The $A$ cannot work because Assembly level insufficient!");

	public static MsgBoxValue mStorageHistory_1				= new MsgBoxValue(82201046, "$A$ add $B$ into storage.");
	public static MsgBoxValue mStorageHistory_2				= new MsgBoxValue(82201047, "$A$ take away $B$ from storage. and use it.");
	public static MsgBoxValue mCantHandlePersonnel			= new MsgBoxValue(82201048, "不能操作这个NPC， 他床位不在基地内");
    public static MsgBoxValue mNpdDeadNotChangeProfession = new MsgBoxValue(82201085, "NPC已经死亡，不可以改变NPC的职业！");

    public static MsgBoxValue  mIsCompounding				= new MsgBoxValue(82201049, "$A$ 还未合成完毕, 不能取走.");
	public static MsgBoxValue  mTakeAwayCompoundItem		= new MsgBoxValue(82201050, "从合成工厂里面取走$A$.");
	public static MsgBoxValue  mJoinCompoudingQueue			= new MsgBoxValue(82201051, "$A$ 加入合成队列.");


    //farmPlant--puji
    public static MsgBoxValue mRemovePlantConfirm = new MsgBoxValue(8000155, "Still alive, Sure to remove?");

	//luwei
	public static MsgBoxValue mProfessionForProcessor		= new MsgBoxValue(82200052, "$A$ become a Processor");

    //lz-2016.06.22  把使用MessageBox_N Show的info的部分全部转移过来


}

public struct MsgBoxValue
{
	public int mStrId;
	public string mDefaultValue;
	public string GetString()
	{
		string str = PELocalization.GetString(mStrId);
		return (str == "") ? mDefaultValue : str;
	}
	public MsgBoxValue(int _mStrId,string _mDefaultValue)
	{
		mStrId = _mStrId;
		mDefaultValue = _mDefaultValue;
	}
}
