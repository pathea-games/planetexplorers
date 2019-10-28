using UnityEngine;
using System.Collections;
using Pathea;

public class OperatableItemRepairMachine : OperatableItem
{
    public override bool Operate()
    {
        if (!base.Operate())
        {
            return false;
        }

        if (!TutorialData.AddActiveTutorialID(TutorialData.RepairMachineId))
        {
            CSRepairObject repm = GetComponent<CSRepairObject>();
            if (null != repm && !GameUI.Instance.mRepair.isShow)
            {
                GameUI.Instance.mRepair.OpenWnd(repm);
            }
        }

        return true;
    }

    public override bool Init(int id)
    {
        if (!base.Init(id))
        {
            return false;
        }
        if (CSMain.GetCreator(CSConst.ciDefNoMgCamp) == null)
        {
            CSMain.InitOperatItemEvent += PostInit;
            return true;
        }
        else
        {
            CSRepairObject repm = gameObject.GetComponent<CSRepairObject>();
            repm.transform.localScale = Vector3.one;
            return CSConst.rrtSucceed == repm.Init(id, CSMain.GetCreator(CSConst.ciDefNoMgCamp), false);
        }
    }

    public void PostInit()
    {
        CSRepairObject repm = gameObject.GetComponent<CSRepairObject>();
        repm.transform.localScale = Vector3.one;
        repm.Init(m_id, CSMain.GetCreator(CSConst.ciDefNoMgCamp), false);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        CSMain.InitOperatItemEvent -= PostInit;
    }
}