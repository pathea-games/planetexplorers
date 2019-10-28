
using UnityEngine;
using System.Collections;
using SkillAsset;

public class DragItemMousePickBed : DragItemMousePick
{
    ItemScript_Bed mBedView;

    ItemScript_Bed bedView
    {
        get
        {
            if (mBedView == null)
            {
                mBedView = GetComponent<ItemScript_Bed>();
            }

            return mBedView;
        }
    }

    protected override void InitCmd(CmdList cmdList)
    {
		cmdList.Add("Turn", Turn90Degree);
		cmdList.Add ("Get", OnGetBtn);

        //if (!GameConfig.IsMultiMode)
        {
            if (bedView.peSleep.CanOperateMask(Pathea.Operate.EOperationMask.Sleep))
            {
                cmdList.Add("Sleep", () =>
                {
					if(EntityMonsterBeacon.IsRunning())
					{						
						PeTipMsg.Register(PELocalization.GetString(8000596), PeTipMsg.EMsgLevel.Warning);
						return;
					}
                    if (!bedView.peSleep.CanOperateMask(Pathea.Operate.EOperationMask.Sleep))
                    {
                        return;
                    }
					
					Pathea.OperateCmpt operateCmpt = Pathea.MainPlayer.Instance.entity.operateCmpt;
					if(null != operateCmpt && operateCmpt.HasOperate)
						return;

                    Pathea.MotionMgrCmpt mmc = Pathea.MainPlayer.Instance.entity.GetCmpt<Pathea.MotionMgrCmpt>();

                    if (null != mmc && (mmc.IsActionRunning(Pathea.PEActionType.Sleep) || !mmc.CanDoAction(Pathea.PEActionType.Sleep)))
                    {
                        return;
                    }

                    //if (GameConfig.IsMultiMode)
                   //     GameUI.Instance.mItemOp.SleepImmediately(bedView.peSleep, Pathea.MainPlayer.Instance.entity);
                    //else
                        GameUI.Instance.mItemOp.ShowSleepWnd(true,this,bedView.peSleep, Pathea.MainPlayer.Instance.entity);
                });

            }
        }
    }

    public override bool CanCmd()
    {
        return base.CanCmd()
            && Operatable();
    }

    bool Operatable()
    {
		if (bedView == null || bedView.peSleep == null)
        {
            return false;
        }

        if (!bedView.peSleep.CanOperateMask(Pathea.Operate.EOperationMask.Sleep))
        {
            return false;
        }

        return true;
    }

    protected override void CheckOperate()
    {
        base.CheckOperate();

		if (PeInput.Get(PeInput.LogicFunction.InteractWithItem) && CanCmd())
        {
			if(EntityMonsterBeacon.IsRunning())
			{
				PeTipMsg.Register(PELocalization.GetString(8000596), PeTipMsg.EMsgLevel.Warning);				
				return;
			}

            if (!Operatable())
            {
                return;
            }
			
			Pathea.OperateCmpt operateCmpt = Pathea.MainPlayer.Instance.entity.operateCmpt;
			if(null != operateCmpt && operateCmpt.HasOperate)
				return;

            Pathea.MotionMgrCmpt mmc = Pathea.MainPlayer.Instance.entity.motionMgr;

            if (null != mmc && (mmc.IsActionRunning(Pathea.PEActionType.Sleep) || !mmc.CanDoAction(Pathea.PEActionType.Sleep)))
            {
                return;
            }

            GameUI.Instance.mItemOp.ShowSleepWnd(true,this,bedView.peSleep, Pathea.MainPlayer.Instance.entity);
           // GameUI.Instance.mItemOp.SleepImmediately(bedView.peSleep, Pathea.MainPlayer.Instance.entity);
		}
    }

    protected override string tipsText
    {
        get
        {
            if (Operatable())
            {
                return base.tipsText + "\n" + PELocalization.GetString(8000120);
            }

            return "";
        }
    }
}
