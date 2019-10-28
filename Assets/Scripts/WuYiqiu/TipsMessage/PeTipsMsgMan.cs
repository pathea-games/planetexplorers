using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using ItemAsset;


public class PeTipsMsgMan : MonoLikeSingleton<PeTipsMsgMan> 
{
	public const int c_MaxTipsMsgCnt = 20;

	List<PeTipMsg> m_TipsMsg;

	public delegate void DNotify (PeTipMsg msg);
	public event DNotify onAddTipMsg;

	public void AddTipMsg (PeTipMsg msg)
	{
		m_TipsMsg.Add(msg);

		if (onAddTipMsg != null)
			onAddTipMsg(msg);
	}

	protected override void OnInit()
	{
		base.OnInit();
		m_TipsMsg = new List<PeTipMsg>();




	}

	bool _playerEventAdded = false;

	public override void Update ()
	{
		base.Update ();

		if (!_playerEventAdded)
		{
			if (PeCreature.Instance != null && PeCreature.Instance.mainPlayer != null)
			{
				PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<PlayerPackageCmpt>();
				if (pkg == null)
					return;
				
				pkg.getItemEventor.Subscribe(OnPlayerItemPackageEvent);
				_playerEventAdded = true;
			}
		}
	}
	
	public override void OnDestroy()
	{
		base.OnDestroy();
	}

	void OnPlayerItemPackageEvent (object sender, PlayerPackageCmpt.GetItemEventArg e)
	{
//		ItemPackage.EventArg.Op type = e.;


		PlayerPackageCmpt pkg = PeCreature.Instance.mainPlayer.GetCmpt<Pathea.PlayerPackageCmpt>();
		if (pkg == null)
			return;

//		ItemObject io = e.itemObj;
//		if (io == null)
//			return;
		ItemProto item = ItemProto.Mgr.Instance.Get(e.protoId);
		if (item == null)
			return;

        //lz-2018.1.19 获取东西的时候显示背包数量
        string msg = string.Format("{0} X {1} ({2})", item.GetName(), e.count, pkg.GetItemCount(e.protoId));
        new PeTipMsg(msg, item.icon[0], PeTipMsg.EMsgLevel.Norm, PeTipMsg.EMsgType.Misc);
//		PeTipMsg tips = new PeTipMsg(msg, PeTipMsg.EMsgLevel.Warning, PeTipMsg.EMsgType.Misc);
	}
}
