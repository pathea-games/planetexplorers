using UnityEngine;
using System.Collections;
using SkillAsset;
using Pathea;
using Pathea.Operate;


public class DragItemMousePickOperate : DragItemMousePick
{
	[SerializeField] EOperationMask m_Mask = EOperationMask.Sleep;
	[SerializeField] PEActionType m_ActionType = PEActionType.Sleep;
	[SerializeField] string m_ButtonName = "Sleep";
	[SerializeField] int m_AddTipsID = 8000120;

	Operation m_Operation;
	protected Operation operation
	{
		get
		{
			if (m_Operation == null)
				m_Operation = GetComponent<Operation>();
			return m_Operation;
		}
	}

	protected IOperator operater{
        get {
            if (Pathea.PeCreature.Instance.mainPlayer != null)
                return Pathea.PeCreature.Instance.mainPlayer.operateCmpt;
            else
                return null;
        }
    }

	protected virtual void AddOperateCmd(CmdList cmdList)
	{
		cmdList.Add(m_ButtonName, OnDoOperate);
	}

	protected virtual void OnDoOperate()
	{
		if (Operatable ()) 
		{
			operation.StartOperate (operater, m_Mask);
			GameUI.Instance.mItemOp.Hide();
		}
	}

    protected override void InitCmd(CmdList cmdList)
    {
		cmdList.Add("Turn", Turn90Degree);
		cmdList.Add ("Get", OnGetBtn);
		if (Operatable())
			AddOperateCmd(cmdList);
    }

    public override bool CanCmd()
    {
        return base.CanCmd()
            && Operatable();
    }

    protected bool Operatable()
    {
		if (operation == null)
        {
            return false;
        }

        if (!operation.CanOperateMask(m_Mask))
        {
            return false;
        }

		Pathea.OperateCmpt operateCmpt = Pathea.MainPlayer.Instance.entity.operateCmpt;
		if(null != operateCmpt && operateCmpt.HasOperate)
			return false;
		
		Pathea.MotionMgrCmpt mmc = Pathea.MainPlayer.Instance.entity.motionMgr;
		
		if (null != mmc && (mmc.IsActionRunning(m_ActionType) || !mmc.CanDoAction(m_ActionType)))
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
            if (!Operatable())
            {
                return;
            }

			OnDoOperate();
        }
    }

	protected virtual void Update()
	{
		if (null != operater && !operater.IsActionRunning (m_ActionType)) 
			operation.StopOperate(operater, m_Mask);
	}
	
	protected override string tipsText
	{
		get
		{
			if (Operatable())
			{
				return base.tipsText + "\n" + PELocalization.GetString(m_AddTipsID);
			}
			
			return "";
		}
	}
}
