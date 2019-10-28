using UnityEngine;
using System.Collections;

public class DragItemMousePickLadder : DragItemMousePick
{
	ItemScript_ClimbLadder mClimb;	
	ItemScript_ClimbLadder climb
	{
		get
		{
			if (mClimb == null)
			{
				mClimb = GetComponent<ItemScript_ClimbLadder>();
			}			
			return mClimb;
		}
	}
	
	public bool rightMouse = true;
    bool CanPlayerClimb()
    {
        return true;
    }

    public void TryClimbLadder(Pathea.PeCmpt who)
    {
		Pathea.OperateCmpt oper = who.Entity.operateCmpt;
		if (oper == null || oper.HasOperate)
			return;

		if(null == oper.Entity.motionMgr)
			return;

		if(oper.Entity.motionMgr.IsActionRunning(Pathea.PEActionType.Climb))
		{
			climb.opClimb.StopOperate(oper, Pathea.Operate.EOperationMask.ClimbLadder);
		}
		else
		{
			climb.opClimb.StartOperate(oper, Pathea.Operate.EOperationMask.ClimbLadder);
		}

//		if (climb.opClimb.IsIdle ()) {
//			climb.opClimb.StartOperate(oper, Pathea.Operate.EOperationMask.ClimbLadder);
//		} else {
//			climb.opClimb.StopOperate(oper, Pathea.Operate.EOperationMask.ClimbLadder);
//		}
    }

	public override bool CanCmd()
    {
		return base.CanCmd() && climb.opClimb.IsIdle ();
	}

    protected override void CheckOperate()
    {
		if(rightMouse == true)
			base.CheckOperate();
    }

    protected override string tipsText
    {
        get
        {
            if (CanPlayerClimb())
            {
                if (base.tipsText != "")
                    return base.tipsText + "\n" + PELocalization.GetString(8000128);
                else
                    return PELocalization.GetString(8000128);
            }

            return base.tipsText;
        }
    }
}
