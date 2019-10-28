/// <summary>
/// 2016-10-20 22:14:43 
/// by Pugee
/// </summary>
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Pathea;
using Pathea.Operate;
using CSRecord;
using TownData;
using Mono.Data.SqliteClient;
using ItemAsset;
using ItemAsset.PackageHelper;
using System.IO;

#region model
//[Obsolete]
public class TradeObj
{
    public int protoId;
    public int count;
    public int max;

    public TradeObj(int protoId,int count)
    {
        this.protoId = protoId;
        this.count = count;
    }
    public TradeObj(CSUI_ItemInfo ci)
    {
        this.protoId = ci.protoId;
        this.count = ci.Number;
    }
    public TradeObj(int protoId,int count,int max)
    {
        this.protoId = protoId;
        this.count = count;
        this.max = max;
    }

    #region ulink
    public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
    {
        try
        {
            int protoId = stream.ReadInt32();
            int count = stream.ReadInt32();
            int max = stream.ReadInt32();
            TradeObj to = new TradeObj(protoId, count, max);
            return to;
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }

    public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
    {
        try
        {
            TradeObj to = value as TradeObj;
            stream.WriteInt32(to.protoId);
            stream.WriteInt32(to.count);
            stream.WriteInt32(to.max);
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }
    #endregion
}

//[Obsolete]
public class TownTradeItemInfo
{
    public int campId = -2;
    public List<TradeObj> needItems = new List<TradeObj>();
    public List<TradeObj> rewardItems = new List<TradeObj>();
    public CounterScript cs;
    public float m_CurTime;
    public float m_Time;
    public IntVector2 pos;
    public string name="undefined";
    public CSTradeInfoData csti;
    public string Icon
    {
        get { return csti.icon; }
    }

    public float TimeLeft
    {
        get
        {
            if (cs != null)
            {
                return cs.FinalCounter-cs.CurCounter;
            }
            else
            {
                return m_Time;
            }
        }
    }


    public delegate void RefreshEvent(TownTradeItemInfo task,CSTradeInfoData cstid);
    public RefreshEvent RefreshCome;
    public TownTradeItemInfo(IntVector2 pos)
    {
        this.pos = pos;
    }

    public TownTradeItemInfo(DetectedTown dt)
    {
        this.pos = dt.PosCenter;
        name = dt.name;
        campId = dt.campId;
    }

    public TownTradeItemInfo(DetectedTown dt, int tradeId,float currentTime, TradeObj[] needItems, TradeObj[] rewardItems)
    {
        this.pos = dt.PosCenter;
        name = dt.name;
        campId = dt.campId;
        this.needItems = needItems.ToList();
        this.rewardItems = rewardItems.ToList();
        csti = CSTradeInfoData.GetData(tradeId);
        m_CurTime = currentTime;
        m_Time = csti.refreshTime;
        cs = null;
    }

    public void InitRewardItem()
    {
        //int needItemPriceSum = GetNeedItemSumPrice();
        //int rewardItemsCount = rewardItems.Count;
        //int eachRewardPrice = needItemPriceSum / rewardItemsCount;
        //for (int i = 0; i < rewardItemsCount; i++)
        //{
        //    rewardItems[i].count = eachRewardPrice / ItemAsset.ItemProto.GetPrice(rewardItems[i].protoId);
        //}
        //------
        for (int i = 0; i < rewardItems.Count; i++)
        {
            rewardItems[i].count = 0;
        }
    }


    public void setNeedNum(int protoId, int num)
    {
        TradeObj to = needItems.Find(it => it.protoId == protoId);
        if (to != null)
        {
            to.count = Mathf.Min(num, to.max);
        }
    }
    public void SetNeedItems(TradeObj[] needItems)
    {
        this.needItems = needItems.ToList();
    }

    public void setRewardNum(int protoId, int num)
    {
        TradeObj to = rewardItems.Find(it => it.protoId == protoId);
        if (to != null)
        {
            to.count = Mathf.Min(num, to.max);
        }
    }
    public void SetRewardItems(TradeObj[] rewardItems)
    {
        this.rewardItems = rewardItems.ToList();
    }

    public int GetNeedItemSumPrice()
    {
        int needItemSumPrice = 0;
        for (int i = 0; i < needItems.Count; i++)
        {
            needItemSumPrice += ItemAsset.ItemProto.GetPrice(needItems[i].protoId) * needItems[i].count;
        }
        return needItemSumPrice;
    }

    public int GetRewardItemSumPrice()
    {
        int rewardItemSumPrice = 0;
        for (int i = 0; i < needItems.Count; i++)
        {
            rewardItemSumPrice += ItemAsset.ItemProto.GetPrice(rewardItems[i].protoId) * rewardItems[i].count;
        }
        return rewardItemSumPrice;
    }

    public void StartUpdateCounter()
    {
        StartCounter(0,m_Time);
    }

    public void StartCounter(float curTime, float finalTime)
    {
        if (finalTime < 0F)
            return;

        if (cs == null)
        {
            cs = CSMain.Instance.CreateCounter("Trade", curTime, finalTime);
        }
        else
        {
            cs.Init(curTime, finalTime);
        }
        if (!GameConfig.IsMultiMode)
        {
            cs.OnTimeUp = NeedRefresh;
        }
    }
    public void ContinueCounter()
    {
        StartCounter(m_CurTime, m_Time);
    }
    public void NeedRefresh()
    {
        if(RefreshCome!=null)
            RefreshCome(this,csti);
    }

    public void BeRefreshed(TradeObj[] needItems,TradeObj[] rewardItems)
    {
        this.needItems = needItems.ToList();
        this.rewardItems = rewardItems.ToList();
        m_Time = csti.refreshTime;
        StartUpdateCounter();
    }

    public void Update()
    {
        if (cs != null)
        {
            m_CurTime = cs.CurCounter;
            m_Time = cs.FinalCounter;
        }
        else
        {
            m_CurTime = 0F;
            m_Time = -1F;
        }
    }
    public void DoTrade(ICollection<TradeObj> need)
    {
        foreach (TradeObj to in need)
        {
            TradeObj ttiiTo = needItems.Find(it => it.protoId == to.protoId);
            if (ttiiTo != null )
            {
                ttiiTo.count -= to.count;
                if (ttiiTo.count < 0)
                    ttiiTo.count = 0;
            }
        }
    }

    #region uLink
    public static object Deserialize(uLink.BitStream stream, params object[] codecOptions)
    {
        try
        {
            IntVector2 pos = new IntVector2(stream.ReadInt32(), stream.ReadInt32());
			TownTradeItemInfo ttii = new TownTradeItemInfo(pos);
			ttii.csti = CSTradeInfoData.GetData(stream.ReadInt32());
            ttii.m_CurTime = stream.ReadSingle();
            ttii.m_Time = stream.ReadSingle();
            int needItemsCount = stream.ReadInt32();
            for (int m = 0; m < needItemsCount; m++)
            {
                ttii.needItems.Add((TradeObj)TradeObj.Deserialize(stream));
            }
            int rewardItemsCount = stream.ReadInt32();
            for (int m = 0; m < rewardItemsCount; m++)
            {
                ttii.rewardItems.Add((TradeObj)TradeObj.Deserialize(stream));
            }
            return ttii;
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }

    public static void Serialize(uLink.BitStream stream, object value, params object[] codecOptions)
    {
        try
        {
            TownTradeItemInfo ttii = value as TownTradeItemInfo;
            stream.WriteInt32(ttii.pos.x);
            stream.WriteInt32(ttii.pos.y);
            stream.WriteInt32(ttii.csti.id);
            stream.WriteSingle(ttii.m_CurTime);
			stream.WriteSingle(ttii.m_Time);
            stream.WriteInt32(ttii.needItems.Count);
            foreach (TradeObj to in ttii.needItems)
            {
                TradeObj.Serialize(stream, to);
            }
            stream.WriteInt32(ttii.rewardItems.Count);
            foreach (TradeObj to in ttii.rewardItems)
            {
                TradeObj.Serialize(stream, to);
            }
        }
        catch (System.Exception e)
        {
            throw e;
        }
    }
    #endregion
}

//[Obsolete]
public class CSTradeInfoData
{
    public const int RANDOM_TOWN_ID=1;
    public int id;
    public List<TradeObj> needItemList = new List<TradeObj>();
    public int needTypeAmountMin;
    public int needTypeAmountMax;
    public float needRandomVariate;
    public List<TradeObj> rewardItemList = new List<TradeObj>();
    public int rewardTypeAmountMin;
    public int rewardTypeAmountMax;
    public float refreshTime;
    public string icon;

    public static Dictionary<int, CSTradeInfoData> mTradeInfo = new Dictionary<int, CSTradeInfoData>();


    public static void LoadData(){
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("tradePost");

        while (reader.Read())
        {
            CSTradeInfoData cstid = new CSTradeInfoData();
            cstid.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("tradeID")));
            string[] needitems = reader.GetString(reader.GetOrdinal("need")).Split(';');
            foreach(string item in needitems){
                string[] protoIdNumStr = item.Split(',');
                TradeObj pob= new TradeObj (Convert.ToInt32(protoIdNumStr[0]),Convert.ToInt32(protoIdNumStr[1]));
                cstid.needItemList.Add(pob);
            }
            string[] typeAmount1 = reader.GetString(reader.GetOrdinal("typeAmount1")).Split(',');
            cstid.needTypeAmountMin=Convert.ToInt32(typeAmount1[0]);
            cstid.needTypeAmountMax = Convert.ToInt32(typeAmount1[1]);
            cstid.needRandomVariate = reader.GetFloat(reader.GetOrdinal("randomMax"));
            string[] rewarditems = reader.GetString(reader.GetOrdinal("reward")).Split(';');
            foreach(string item in rewarditems){
                string[] protoIdNumStr = item.Split(',');
                TradeObj pob= new TradeObj (Convert.ToInt32(protoIdNumStr[0]),Convert.ToInt32(protoIdNumStr[1]));
                cstid.rewardItemList.Add(pob);
            }
            string[] typeAmount2 = reader.GetString(reader.GetOrdinal("typeAmount2")).Split(',');
            cstid.rewardTypeAmountMin=Convert.ToInt32(typeAmount2[0]);
            cstid.rewardTypeAmountMax = Convert.ToInt32(typeAmount2[1]);
            cstid.refreshTime = Convert.ToSingle(reader.GetString(reader.GetOrdinal("refreshTime")));
            if(Application.isEditor)
                cstid.refreshTime = 40;
            cstid.icon = reader.GetString(reader.GetOrdinal("icon"));
            cstid.CheckData();
            mTradeInfo.Add(cstid.id, cstid);
        }
    }
    public void CheckData()
    {
        if (needTypeAmountMax > needItemList.Count)
        {
            Debug.LogError(id + ":typeAmount1 too large!");
        }
        if (rewardTypeAmountMax > rewardItemList.Count)
        {
            Debug.LogError(id + ":typeAmount2 too large!");
        }
    }

    public static CSTradeInfoData GetData(int id)
    {
        if (mTradeInfo.ContainsKey(id))
            return mTradeInfo[id];
        else
            return null;
    }
}

//[Obsolete]
public class CampTradeIdData
{
	public const int StoryDetectTrade = 1;
	public const int RandomDetectTrade = 2;
	public const int StoryMissionTrade = 3;
    public int id;
    public List<int> campId= new List<int>();
    public int mode;
    public int tradePostId;

    public static Dictionary<int, CampTradeIdData> campTradeIdInfo = new Dictionary<int, CampTradeIdData>();
    public static void LoadData()
    {
        SqliteDataReader reader = LocalDatabase.Instance.ReadFullTable("trademanager");

        while (reader.Read())
        {
            CampTradeIdData ctid = new CampTradeIdData();
            ctid.id = Convert.ToInt32(reader.GetString(reader.GetOrdinal("id")));
            string[] campIdStrs = reader.GetString(reader.GetOrdinal("campid")).Split(',');
            foreach (string campIdStr in campIdStrs)
            {
                ctid.campId.Add(Convert.ToInt32(campIdStr));
            }
            ctid.mode = Convert.ToInt32(reader.GetString(reader.GetOrdinal("mode")));
            ctid.tradePostId = Convert.ToInt32(reader.GetString(reader.GetOrdinal("tradepostid")));
            campTradeIdInfo.Add(ctid.id, ctid);
        }
    }
	public static bool IsStoryDetectTradeCamp(int campId){
		foreach (KeyValuePair<int, CampTradeIdData> pair in campTradeIdInfo) {
			if(pair.Value.mode == StoryDetectTrade && pair.Value.campId.Contains(campId)){
				return true;
			}
		}
		return false;
	}
	public static bool IsStoryMissionTradeCamp(int campId){
		foreach (KeyValuePair<int, CampTradeIdData> pair in campTradeIdInfo) {
			if(pair.Value.mode == StoryMissionTrade && pair.Value.campId.Contains(campId)){
				return true;
			}
		}
		return false;
	}

	public static bool IsDetectTradeCamp(int campId){
		return campTradeIdInfo.ContainsKey(campId)&&(campTradeIdInfo[campId].mode==StoryDetectTrade||campTradeIdInfo[campId].mode==RandomDetectTrade);
	}
}

#endregion

public class CSTrade:CSElectric
{

    public Dictionary<int, int> campToTrade = new Dictionary<int, int>();
    public CSTrade(CSCreator creator)
    {
        m_Creator = creator;
        m_Type = CSConst.etTrade;
        m_Workers = new CSPersonnel[1];

        m_WorkSpaces = new PersonnelSpace[1];
        for (int i = 0; i < m_WorkSpaces.Length; i++)
        {
            m_WorkSpaces[i] = new PersonnelSpace(this);
        }

        m_Grade = CSConst.egtLow;
        if (IsMine)
		{
			BindEvent();
        }
    }
    public override bool IsDoingJob()
    {
        return IsRunning;
    }
    public override GameObject gameLogic
    {
        get { return base.gameLogic; }
        set
        {
            base.gameLogic = value;

            if (gameLogic != null)
            {
                PEMachine workmachine = gameLogic.GetComponent<PEMachine>();
                if (workmachine != null)
                {
                    for (int i = 0; i < m_WorkSpaces.Length; i++)
                    {
                        m_WorkSpaces[i].WorkMachine = workmachine;
                    }
                }
            }
        }
    }
    public CSUI_TradingPost uiObj;
    public CSBuildingLogic BuildingLogic
    {
        get { return gameLogic.GetComponent<CSBuildingLogic>(); }
    }
    public CSTradeInfo m_TInfo;
    public CSTradeInfo Info
    {
        get
        {
            if (m_TInfo == null)
                m_TInfo = m_Info as CSTradeInfo;
            return m_TInfo;
        }
    }
    private CSTradeData m_TData;
    public CSTradeData Data
    {
        get
        {
            if (m_TData == null)
                m_TData = m_Data as CSTradeData;
            return m_TData;
        }
    }

    public override void CreateData()
    {
        CSDefaultData ddata = null;
        bool isNew;
        if (GameConfig.IsMultiMode)
        {
            isNew = MultiColonyManager.Instance.AssignData(ID, CSConst.dtTrade, ref ddata, _ColonyObj);
        }
        else
        {
            isNew = m_Creator.m_DataInst.AssignData(ID, CSConst.dtTrade, ref ddata);
        }
        m_Data = ddata as CSTradeData;

        if (isNew)
        {
            Data.m_Name = CSUtils.GetEntityName(m_Type);
            Data.m_Durability = Info.m_Durability;
			InitTradeData();
        }
        else
        {
            StartRepairCounter(Data.m_CurRepairTime, Data.m_RepairTime, Data.m_RepairValue);
            StartDeleteCounter(Data.m_CurDeleteTime, Data.m_DeleteTime);
			InitTradeDataWithData();
		}
	}
    public override void DestroySelf()
    {
        base.DestroySelf();
		DestroySomeData();
    }

    public override void Update()
    {
        base.Update();
        
    }

    public override void RemoveData()
    {
        m_Creator.m_DataInst.RemoveObjectData(ID);
    }

    public List<IntVector2> GetTownInfo()
    {
        return DetectedTownMgr.Instance.detectedTowns;
    }

    public PeEntity GetTownNpc(VArtifactTown vat)
    {
        PeEntity npc = new PeEntity();
        //List<VATownNpcInfo> npcList = vat.npcList;
        //foreach (VATownNpcInfo vaNpc in npcList)
        //{
        //    //--to do:
        //}

        return npc;
    }

    public CounterScript updateCounter;

	public void InitTradeData(){
		UpdateTradeData();
	}
	public void InitTradeDataWithData(){
		UpdateTradeData();
	}
	public void UpdateTradeData(){
		bool needRefresh = false;
		if(m_MgCreator!=null){
			foreach(int id in m_MgCreator.AddedStoreId){
				if(mShopList.ContainsKey(id))
					continue;
				else{
					mShopList.Add(id,new stShopData(-1,-1));
					needRefresh = true;
				}
			}
			List<int> removeList = new List<int> ();
			foreach(int key in mShopList.Keys)
			{
				if(!m_MgCreator.AddedStoreId.Contains(key))
					removeList.Add(key);
			}
			foreach(int removeId in removeList){
				mShopList.Remove(removeId);
				needRefresh = true;
			}
		}
		if(needRefresh)
			UpdateBuyDataToUI();
	}

    #region ui event
	void UnbindEvent(){
		if (uiObj!=null)
		{
			if(m_MgCreator!=null)
			{
				m_MgCreator.UnRegistStoreIdAddedEvent(AddSellItems);
				m_MgCreator.UnRegistUpdateAddedStoreIdEvent(UpdateTradeData);
				m_MgCreator.UnRegistUpdateMoneyEvent(UpdateMoneyToUI);
			}
			uiObj.BuyItemEvent-=BuyItem;
			uiObj.SellItemEvent-=SellItem;
			uiObj.RepurchaseItemEvent-=RepurchaseItem;
			uiObj.RequestRefreshUIEvent -= UpdateShop;
			uiObj=null;
		}
	}
	void BindEvent(){
		if (uiObj==null&&CSUI_MainWndCtrl.Instance != null)
		{
			if(m_MgCreator!=null)
			{
				m_MgCreator.RegistStoreIdAddedEvent(AddSellItems);
				m_MgCreator.RegistUpdateAddedStoreIdEvent(UpdateTradeData);
				m_MgCreator.RegistUpdateMoneyEvent(UpdateMoneyToUI);
			}
			uiObj = CSUI_MainWndCtrl.Instance.TradingPostUI;
			uiObj.BuyItemEvent+=BuyItem;
			uiObj.SellItemEvent+=SellItem;
			uiObj.RepurchaseItemEvent+=RepurchaseItem;
			uiObj.RequestRefreshUIEvent += UpdateShop;
		}
	}

	void UpdateDataToUI(object Obj)
    {
		if(Obj!=null)
			(Obj as CSUI_TradingPost).UpdateUIData(mBuyItemList,mRepurchaseList,colonyMoney);
	}

	void UpdateBuyDataToUI(object Obj){
		if(Obj!=null){
			(Obj as CSUI_TradingPost).UpdateBuyItemList(mBuyItemList);
			(Obj as CSUI_TradingPost).UpdateCurrency(colonyMoney);
		}
	}

	void UpdateRepurchaseDataToUI(object Obj){
		if(Obj!=null){
			(Obj as CSUI_TradingPost).UpdateRepurchaseList(mRepurchaseList);
			(Obj as CSUI_TradingPost).UpdateCurrency(colonyMoney);
		}
	}
	void UpdateMoneyToUI(object Obj){
		if(Obj!=null){
			(Obj as CSUI_TradingPost).UpdateCurrency(colonyMoney);
		}
	}

	public override void DestroySomeData(){
		if(IsMine)
			UnbindEvent();
	}
	public override void UpdateDataToUI(){

		if(IsMine){
			//1.link
			BindEvent();
			//2.updateData

			//3.updateUI
			UpdateDataToUI(uiObj);
		}
	}
	public void UpdateBuyDataToUI(){
		
		if(IsMine){
			//1.link
			BindEvent();
			//2.updateData

			//3.updateUI
			UpdateBuyDataToUI(uiObj);
		}
	}
	public void UpdateRepurchaseDataToUI(){
		
		if(IsMine){
			//1.link
			BindEvent();
			//2.updateData
			
			//3.updateUI
			UpdateRepurchaseDataToUI(uiObj);
		}
	}
	public void UpdateMoneyToUI(){
		if(IsMine){
			//1.link
			BindEvent();
			//2.updateData
			
			//3.updateUI
			UpdateMoneyToUI(uiObj);
		}
	}
    #endregion

	List<ItemObject> mBuyItemList=new List<ItemObject>();
	List<ItemObject> mRepurchaseList=new List<ItemObject> ();

	//save
	Dictionary<int, stShopData> mShopList=new Dictionary<int, stShopData>();
	public Dictionary<int,stShopData> ShopList{
		get{return mShopList;}
	}
	int colonyMoney{
		set{
			if(m_MgCreator!=null)
				m_MgCreator.ColonyMoney=value;
		}
		get{
			if(m_MgCreator==null)
				return 0;
			return m_MgCreator.ColonyMoney;
		}
	}


	public Money PlayerMoney
	{
		get
		{	
			PackageCmpt cmpt = PeCreature.Instance.mainPlayer.GetCmpt<PackageCmpt>();
			return cmpt.money;
		}
	}


	public void AddSellItems(List<int> addStoreId){
		foreach(int addId in addStoreId){
			if(!mShopList.ContainsKey(addId))
			{
				mShopList[addId] = new stShopData (-1,-1);
			}
		}
		UpdateShop();
	}

	public void UpdateShop(){
		if(PeGameMgr.IsMulti)
		{
			UpdateDataToUI();
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRD_RequestShop);
			return;
		}

		mBuyItemList.Clear();
		bool bPass = true;
		foreach (int key in mShopList.Keys)
		{
			ShopData data = ShopRespository.GetShopData(key);
			if (data == null)
				continue;
			
			bPass = true;
			if(PeGameMgr.IsStory){
				for (int i = 0; i < data.m_LimitMisIDList.Count; i++)
				{
					if (data.m_LimitType == 1)
					{
						if (MissionManager.Instance.HadCompleteMission(data.m_LimitMisIDList[i]))
							break;
					}
					else
					{
						if (!MissionManager.Instance.HadCompleteMission(data.m_LimitMisIDList[i]))
						{
							bPass = false;
							break;
						}
					}
				}
			}

			if (!bPass)
				continue;
			
			if(mShopList[key].CreateTime<0){
				ItemObject itemObj = ItemAsset.ItemMgr.Instance.CreateItem(data.m_ItemID); // single
				itemObj.stackCount = data.m_LimitNum;
				mShopList[key].ItemObjID = itemObj.instanceId;
				mShopList[key].CreateTime = GameTime.Timer.Second;
				mBuyItemList.Add(itemObj);
			}
			else if (GameTime.Timer.Second - mShopList[key].CreateTime > data.m_RefreshTime)
			{
				ItemObject itemObj;
				if (mShopList[key].ItemObjID >= 0){
					itemObj = ItemMgr.Instance.Get(mShopList[key].ItemObjID); 
					if(itemObj == null)
						itemObj = ItemMgr.Instance.CreateItem(data.m_ItemID);
					itemObj.stackCount = data.m_LimitNum;
				}else{
					itemObj = ItemMgr.Instance.CreateItem(data.m_ItemID); // single
					itemObj.stackCount = data.m_LimitNum;
				}
				mShopList[key].ItemObjID = itemObj.instanceId;
				mShopList[key].CreateTime = GameTime.Timer.Second;
				mBuyItemList.Add(itemObj);
			}
			else
			{
				if (mShopList[key].ItemObjID < 0)
					continue;
				else
				{
					ItemObject itemObj = ItemMgr.Instance.Get(mShopList[key].ItemObjID);
					if(itemObj==null)
						itemObj = ItemMgr.Instance.CreateItem(data.m_ItemID);
					mShopList[key].ItemObjID = itemObj.instanceId;
					mBuyItemList.Add(itemObj);
				}
			}
		}

		UpdateDataToUI();
	}

	#region buy
	public void BuyItem(int instanceId,int count){
		int shopID = -1;
		int cost = 0;
		ItemObject itemObj = ItemAsset.ItemMgr.Instance.Get(instanceId);
		if(itemObj==null)
		{
			UpdateShop();
			return;
		}


		if (PeGameMgr.IsMulti)
		{
			cost = count * Mathf.RoundToInt(ShopRespository.GetPriceBuyItemId(itemObj.protoId)*(1+ColonyConst.TRADE_POST_CHARGE_RATE));
			if (PlayerMoney.current < cost)
			{
				PeTipMsg.Register(PELocalization.GetString(Money.Digital? 8000092:8000073), PeTipMsg.EMsgLevel.Warning);
				return;
			}

			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRD_BuyItem,instanceId,count);
			return;
		}
		else{
			foreach(KeyValuePair<int,stShopData> kvp in mShopList){
				if(kvp.Value.ItemObjID==instanceId){
					shopID=kvp.Key;
					break;
				}
			}
			if(shopID<0){
				UpdateShop();
				return;
			}

			ShopData data = ShopRespository.GetShopData(shopID);
			if (data != null){
				cost = Mathf.RoundToInt(data.m_Price*(1+ColonyConst.TRADE_POST_CHARGE_RATE))*count;
			}
			else {
				PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
				return;
			}
			if(!Buy(itemObj,count,data,false))
				return;
		}

		colonyMoney +=cost;
		RemoveBuyItem(itemObj,count);

		//--to do: updateUi;
		UpdateBuyDataToUI();
	}

	public void RemoveBuyItem(ItemObject itemObj, int count){
		if (itemObj.stackCount< count)
			Debug.LogError("Remove num is big than item you have.");
		else if (itemObj.GetCount() > count)
			itemObj.DecreaseStackCount(count);
		else
		{
			mBuyItemList.Remove(itemObj);
		}
	}
	#endregion
	
	#region repurchase
	public void RepurchaseItem(int instanceId,int count){
		ItemObject itemObj = ItemAsset.ItemMgr.Instance.Get(instanceId);
		if(itemObj==null)
		{
			UpdateShop();
			return;
		}
		if(!mRepurchaseList.Contains(itemObj)){
			UpdateShop();
			return;
		}
		int cost = itemObj.GetSellPrice();
		if (PeGameMgr.IsMulti)
		{
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRD_RepurchaseItem,instanceId,count);
			return;
		}
		else{
			if(!Buy(itemObj,count,null,true))
				return;
		}
		cost *= count;
		
		colonyMoney +=cost;
		RemoveRepurchase(itemObj,count);

		//--to do: update UI
		UpdateRepurchaseDataToUI();
	}
	
	public void RemoveRepurchase(ItemObject itemObj, int count)
	{
		if (itemObj.GetCount() < count)
		{
			Debug.LogError("Remove num is big than item you have.");
			return;
		}
		
		if (itemObj.GetCount() > count)
			itemObj.DecreaseStackCount(count);
		else
			mRepurchaseList.Remove(itemObj);
	}
	#endregion

	#region func
	public bool Buy(ItemObject itemObj,int count, ShopData data, bool isRepurchase){
		int price;
		if (!isRepurchase)
			price = Mathf.RoundToInt(data.m_Price*(1+ColonyConst.TRADE_POST_CHARGE_RATE));
		else
			price = itemObj.GetSellPrice();
		int cost = price * count;
		if (PlayerMoney.current < cost)
		{
			PeTipMsg.Register(PELocalization.GetString(Money.Digital? 8000092:8000073), PeTipMsg.EMsgLevel.Warning);
			return false;
		}
		
		ItemObject addItem = null;
		PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (null == pkg)
		{
			PeTipMsg.Register(PELocalization.GetString(8000496), PeTipMsg.EMsgLevel.Warning);
			return false;
		}

		if(!pkg.package.CanAdd(itemObj.protoId,count)){
			PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
			return false;
		}

		if (itemObj.protoData.maxStackNum == 1)
		{
			int NUM = count;
			for (int i = 0; i < NUM; i++)
			{
				count = 1;
				if (count < itemObj.GetCount())
				{
					addItem = ItemMgr.Instance.CreateItem(itemObj.protoId); // single
					addItem.SetStackCount(count);
				}
				else
				{
					addItem = itemObj;
					if(!isRepurchase)
						mShopList[data.m_ID].ItemObjID=-1;
				}
				pkg.package.AddItem(addItem,!isRepurchase);
			}
			PlayerMoney.current-=cost;
			
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
		else
		{
			if(count == itemObj.GetCount()){
				if(!isRepurchase)
					mShopList[data.m_ID].ItemObjID=-1;
			}
			pkg.package.Add(itemObj.protoId,count,!isRepurchase);
			PlayerMoney.current-=cost;
			GameUI.Instance.mItemPackageCtrl.ResetItem();
		}
		
		return true;
	}
	public void ParseData(byte[] data,CSTradeData tradeData)
	{
		using (MemoryStream ms = new MemoryStream(data))
		using (BinaryReader reader = new BinaryReader(ms))
		{
			int count = BufferHelper.ReadInt32(reader);
			for(int i=0;i<count;i++){
				int storeId = BufferHelper.ReadInt32(reader);
				int instanceId = BufferHelper.ReadInt32(reader);
				double createTime = BufferHelper.ReadDouble(reader);
				tradeData.mShopList.Add(storeId,new stShopData(instanceId,createTime));
			}
		}
	}
	#endregion

	#region sell
	public void SellItem(int instanceId,int count){
		ItemObject itemObj = ItemAsset.ItemMgr.Instance.Get(instanceId);
		if(itemObj==null)
		{
			Debug.LogError("Sell item is null");
			return;
		}

		if(count>itemObj.GetCount())
		{
			Debug.LogError("not enough count");
			return;
		}

		PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
		if (null == pkg)
		{
			return;
		}

		int protoId = itemObj.protoId;
		int cost = itemObj.GetSellPrice()*count;

		if(colonyMoney<cost)
		{
            //lz-2016.10.31 The TradePost has no money to pay for you!
            new PeTipMsg(PELocalization.GetString(8000858), PeTipMsg.EMsgLevel.Error);
			return;
		}
		

		if (PeGameMgr.IsMulti)
		{
			_ColonyObj._Network.RPCServer(EPacketType.PT_CL_TRD_SellItem,instanceId,count);
			return;
		}
		else
		{
			colonyMoney -= cost;
			ItemObject SellItemObj;
			if(count==itemObj.stackCount){
				SellItemObj = itemObj;
				pkg.Remove(itemObj);
			}else{
				SellItemObj = ItemMgr.Instance.CreateItem(protoId);
				SellItemObj.SetStackCount(count);
				pkg.DestroyItem(instanceId, count);
			}
			 
			AddRepurchase(SellItemObj);
			pkg.money.current += cost;//Add money to player
		}

		//--to do: updateUI
		UpdateRepurchaseDataToUI();
	}

	public void AddRepurchase(ItemObject item)
	{
		mRepurchaseList.Add(item);
	}


	//update money
	#endregion

	#region multiMode
	public void UpdateBuyResultMulti(int instanceId){
		mBuyItemList.RemoveAll(it=>it.instanceId==instanceId);
		UpdateBuyDataToUI();
	}
	public void UpdateSellResultMulti(int instanceId){
		ItemObject repurchaseItem = ItemMgr.Instance.Get(instanceId);
		mRepurchaseList.Add(repurchaseItem);
		UpdateRepurchaseDataToUI();
	}
	public void UpdateRepurchaseResultMulti(int instanceId){
		mRepurchaseList.RemoveAll(it=>it.instanceId==instanceId);
		UpdateRepurchaseDataToUI();
	}

	public void UpdateBuyItemMulti(List<int> instanceIdList){
		bool needRefresh = false;
		if(mBuyItemList.Count==instanceIdList.Count){
			foreach(ItemObject itemObj in mBuyItemList){
				if(!instanceIdList.Contains (itemObj.instanceId))
				{	
					needRefresh = true;
					break;
				}
			}
		}else
			needRefresh = true;

		if(!needRefresh){
			return;
		}

		mBuyItemList.Clear();
		foreach(int instanceId in instanceIdList){
			ItemObject itemObj = ItemMgr.Instance.Get(instanceId);
			if(itemObj!=null)
				mBuyItemList.Add(itemObj);
		}
		UpdateBuyDataToUI();
	}


	public void UpdateRepurchaseMulti(List<int> instanceIdList)
	{
		bool needRefresh = false;
		if(mRepurchaseList.Count==instanceIdList.Count){
			foreach(ItemObject itemObj in mRepurchaseList){
				if(!instanceIdList.Contains (itemObj.instanceId))
				{	
					needRefresh = true;
					break;
				}
			}
		}else
			needRefresh = true;
		if(!needRefresh){
			return;
		}
		mRepurchaseList.Clear();
		foreach(int instanceId in instanceIdList){
			ItemObject itemObj = ItemMgr.Instance.Get(instanceId);
			if(itemObj!=null)
				mRepurchaseList.Add(itemObj);
		}
		UpdateRepurchaseDataToUI();
	}

	public void UpdateMoneyMulti(int money){
		colonyMoney = money;
		UpdateMoneyToUI();
	}
	#endregion
}
