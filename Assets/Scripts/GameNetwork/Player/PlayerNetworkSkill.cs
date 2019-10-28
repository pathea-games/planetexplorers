using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CustomData;
using System.IO;
using SkillAsset;

public partial class PlayerNetwork
{
	#region Action Callback APIs
	void RPC_S2C_SkillCast(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int _skillID;
		uLink.NetworkViewID _viewID;

		stream.TryRead<int>(out _skillID);
		stream.TryRead<uLink.NetworkViewID>(out _viewID);

		SkillRunner caster = Runner as SkillRunner;
		if (null != caster)
		{
			ISkillTarget target = null;
			uLink.NetworkView view = uLink.NetworkView.Find(_viewID);
			if (null != view)
			{
				NetworkInterface network = view.GetComponent<NetworkInterface>();
				target = (null == network) ? null : network.Runner;
			}

			caster.RunEffOnProxy(_skillID, target);
		}
	}

	void RPC_S2C_SkillCastShoot(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int skillID;
		Vector3 pos;

		stream.TryRead<int>(out skillID);
		stream.TryRead<Vector3>(out pos);

		DefaultPosTarget target = new DefaultPosTarget(pos);

		SkillRunner caster = Runner as SkillRunner;
		if (null != caster)
			caster.RunEffOnProxy(skillID, target);
	}

	//学习合成技能
    void RPC_S2C_MergeSkillList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        int[] skillIdList = stream.Read<int[]>();
		if(PlayerNetwork.mainPlayer == null || PlayerNetwork.mainPlayer.entity == null)
			return;
		Pathea.ReplicatorCmpt cmpt =  PlayerNetwork.mainPlayer.entity.GetCmpt<Pathea.ReplicatorCmpt>();
		if (null != cmpt)
		{
			foreach (int id in skillIdList)
				cmpt.replicator.AddFormula(id);
		}

        //for (int i = 0; i < skillIdList.Length;i++ )
        //    PlayerFactory.mMainPlayer.AddPrescriptionID(skillIdList[i]);
    }

	void RPC_S2C_MetalScanList(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int[] metalScan = stream.Read<int[]>();
        bool openWnd = stream.Read<bool>();

		MetalScanData.AddMetalScan(metalScan, openWnd);

        //if (null != PlayerFactory.mMainPlayer)
        //    PlayerFactory.mMainPlayer.ApplyMetalScan(metalScan.ToList());
	}

    //合成成功后更新合成窗口
    void RPC_S2C_ReplicateSuccess(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
//        GameGui_N.Instance.mCompoundGui.UpdateItemList();
//        GameGui_N.Instance.mCompoundGui.UpdateRightWnd(GameGui_N.Instance.mCompoundGui.getMCurrentMergeId());
//		GameUI.Instance.mUIMainMidCtrl.UpdateLink();
    }

//	[RPC]//因为某些原因无法合成,取消合成
//	void RPC_S2C_CancelMerge(uLink.BitStream stream)
//	{
//		string _errorMsg;
//		stream.TryRead<string>(out _errorMsg);

//		Debug.Log(_errorMsg);
////        GameGui_N.Instance.mCompoundGui.setMInMerging(false);
//		PlayerFactory.mMainPlayer.CancelMerge();
//	}

	#endregion
}
