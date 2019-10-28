using ItemAsset;
using System.Collections.Generic;
using UnityEngine;

public class CmdList : IEnumerable<string>
{
    public delegate void CmdExe();
    public class Cmd
    {
        public string name;
        public CmdExe exe;
    }

    List<Cmd> mList = new List<Cmd>(2);

    public void Add(string cmdName, CmdExe cmdExe)
    {
        mList.Add(new Cmd() { name = cmdName, exe = cmdExe });
    }

	public void Add(Cmd cmd)
	{
		mList.Add(cmd);
	}

    public int count
    {
        get
        {
            return mList.Count;
        }
    }

	public Cmd Get(int index)
	{
		if (index >= mList.Count)
		{
			return null;
		}

		return mList[index];
	}
    public bool Remove(string name)
    {
        return 0 < mList.RemoveAll((item) =>
        {
            if (name == item.name)
            {
                return true;
            }

            return false;
        });
    }

    public void Clear()
    {
        mList.Clear();
    }

    public void ExecuteCmd(string name)
    {
        Cmd cmd = mList.Find((item) =>
        {
            if (item.name == name)
            {
                return true;
            }

            return false;
        });

        if (null == cmd)
        {
            return;
        }

        cmd.exe();
    }

    IEnumerator<string> IEnumerable<string>.GetEnumerator()
    {
        return mList.ConvertAll<string>((item) =>
        {
            return item.name;
        })
        .GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return mList.GetEnumerator();
    }
}

public class DragItemMousePick : MousePickableChildCollider
{
	protected GameObject rootGameObject{        get{            return gameObject;        }    }
	protected int id	{		
		get{
			ItemScript script = GetScript();
			if (null == script)
			{
				return Pathea.IdGenerator.Invalid;
			}
			return script.id;
		}
	}
    protected Vector3 GetPos()
    {
        DragItemAgent agent = DragItemAgent.GetById(id);
        if (agent == null)
        {
            return Vector3.zero;
        }
        return agent.position;
    }
    protected ItemScript GetScript()
    {
        return GetComponent<ItemScript>();
    }

    protected int itemObjectId
    {
        get
        {
            ItemScript script = GetScript();
            if (null == script)
            {
                return Pathea.IdGenerator.Invalid;
            }
            return script.itemObjectId;
        }
    }

	ItemObject m_ItemObj;

    protected ItemAsset.ItemObject itemObj
    {
        get
        {
			if(null == m_ItemObj)
				m_ItemObj = ItemAsset.ItemMgr.Instance.Get(itemObjectId);
			return m_ItemObj;
        }
    }
	
	PlayerPackage mPkg = null;
	protected PlayerPackage pkg
	{
		get
		{
			if (null == mPkg)
			{
				Pathea.PlayerPackageCmpt p = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
				mPkg = p.package;
			}
			
			return mPkg;
		}
	}

    void Awake()
    {
        priority = MousePicker.EPriority.Level2;
        if (MissionManager.Instance != null)
        {
            if (MissionManager.Instance.m_PlayerMission.isRecordCreation && this.name.StartsWith("Creation"))
                Invoke("CheckCanCmd", 0.5f);
        }
    }

    void CheckCanCmd()
    {
		if(null == itemObj || null == itemObj.protoData || itemObj.protoData.itemClassId == (int)WhiteCat.CreationItemClass.Aircraft)
            cancmd = false;
    }

    protected override void OnStart ()
	{
		base.OnStart ();
		
		Pathea.SkAliveEntity alive = GetComponent<Pathea.SkAliveEntity> ();
		if (alive != null) {
			alive.deathEvent += OnDeath;
		}
	}
	protected override void OnDestroy()
	{
        base.OnDestroy();
		if(GameUI.Instance != null && null != GameUI.Instance.mItemOp)
			GameUI.Instance.mItemOp.GetItem(null, this);
	}

    #region Cmd    
    protected void HideItemOpGui()
    {
        GameUI.Instance.mItemOp.Hide();
    }

    protected virtual void InitCmd(CmdList cmdList)
    {
        cmdList.Add("Turn", Turn90Degree);
        cmdList.Add("Get", OnGetBtn);
    }

    Pathea.PeTrans mView = null;
    Vector3 playerPos
    {
        get
        {
            if (null == mView){
				if(Pathea.PeCreature.Instance.mainPlayer != null)
               		mView = Pathea.PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PeTrans>();
            }

            if (null == mView){
                return Vector3.zero;
            }

            return mView.position;
        }
    }

    [HideInInspector]
    public bool cancmd = true;
    public virtual bool CanCmd()
    {
        if (null != SelectItem_N.Instance && SelectItem_N.Instance.HaveOpItem()) {
            return false;
        }
        //float dis = Vector3.Distance(playerPos, GetPos());
		if (DistanceInRange(playerPos, operateDistance))
        {			
			Pathea.SkAliveEntity alive = GetComponent<Pathea.SkAliveEntity> ();
			if (alive != null && alive.isDead) {
				return false;
			}
            if (!cancmd)
                return false; 
            return true;
        }
        return false;
    }

    public virtual void DoGetItem()
    {
		if(null == itemObj) return;

        if (!GameConfig.IsMultiMode)
        {
            if (Pathea.PlayerPackageCmpt.LockStackCount
                && !ItemMgr.IsCreationItem(itemObj.protoId))
            {
                ItemMgr.Instance.DestroyItem(itemObj.instanceId);
            }
            else if (null != pkg)
            {
				if(ItemPackage.InvalidIndex == pkg.AddItem(itemObj))
				{
					PeTipMsg.Register(PELocalization.GetString(9500312), PeTipMsg.EMsgLevel.Warning);
                	return;
				}
                if(MissionManager.Instance != null && Pathea.PeCreature.Instance != null && Pathea.PeCreature.Instance.mainPlayer != null)
                    MissionManager.Instance.ProcessUseItemMissionByID(itemObj.protoId, Pathea.PeCreature.Instance.mainPlayer.position, -1);
            }

            DragItemAgent agent = DragItemAgent.GetById(id);
            if (agent != null)
            {                
                DragItemAgent.Destory(agent);
            }
            
            GameUI.Instance.mItemPackageCtrl.ResetItem();
        }
        else
        {
			if (null != PlayerNetwork.mainPlayer)
				PlayerNetwork.mainPlayer.RequestGetItemBack(itemObjectId);
        }

        HideItemOpGui();
    }

    public virtual void OnGetBtn()
    {
        //GameUI.Instance.mItemOp.SetOjbect(this);
        GameUI.Instance.mItemOp.GetItem(DoGetItem, this);

		if (Pathea.PeGameMgr.IsMulti)
			PlayerNetwork.PreRequestGetItemBack(itemObjectId);
	}

    public virtual void Turn90Degree()
    {
        if (GameConfig.IsMultiMode)
        {
            if (null != PlayerNetwork.mainPlayer)
            {
                PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_Turn, itemObjectId);
            }
        }
        else
        {
            DragItemAgent agent = DragItemAgent.GetById(id);
            if (agent != null)
            {
                agent.Rotate(new Vector3(0, 90f, 0));
            }
        }
    }
    #endregion

    #region Mouse pick
    protected override bool CheckPick(Ray camMouseRay, out float dis)
    {
        if (CanCmd())
        {
            return base.CheckPick(camMouseRay, out dis);
        }

        dis = float.MaxValue;
        return false;
    }

    protected override string tipsText
    {
        get
        {
            if (null != itemObj && null != itemObj.protoData)
            {
				return "[5CB0FF]" + itemObj.protoData.dragName + "[-]"
                    + "\n" + PELocalization.GetString(8000129);
            }
            return "";
        }
    }

    protected override void CheckOperate()
    {
        if (PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && CanCmd())
        {
            CmdList cmdList = new CmdList();

            InitCmd(cmdList);

            //GameUI.Instance.mItemOp.SetOjbect(this);
            GameUI.Instance.mItemOp.SetCmdList(this, cmdList);
        }
    }
    #endregion
	#region DeathHandle	
	void OnDeath(SkillSystem.SkEntity a, SkillSystem.SkEntity b){		
		GameUI.Instance.mItemOp.Hide();
	}
	#endregion
}